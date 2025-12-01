using System;
using Microsoft.Xna.Framework;
using GltronMobileEngine.Interfaces;

namespace GltronMobileEngine
{
    /// <summary>
    /// Computer AI system for GLTron Mobile - matches Java ComputerAI.java behavior
    /// Compatible with Android, iOS, and other MonoGame platforms
    /// </summary>
    public static class ComputerAI
    {
        private static ISegment[]? _walls;
        private static IPlayer[]? _players;
        private static float _gridSize;
        private static long _currentTime;
        private static long _deltaTime;
        
        // Per-AI turn cooldown tracking
        private static long[]? _lastTurnTime;
        
        // AI constants tuned for better pathfinding
        private const float AI_LOOKAHEAD_DISTANCE = 28.0f; // longer lookahead
        private const float AI_TURN_PROBABILITY = 0.18f;   // less random jitter
        private const float AI_AGGRESSIVE_DISTANCE = 30.0f;
        private const float AI_EVASIVE_DISTANCE = 12.0f;   // react sooner
        
        // AI timing
        private static readonly Random _random = new Random();
        
        public static void InitAI(ISegment[] walls, IPlayer[] players, float gridSize)
        {
            try
            {
                _walls = walls;
                _players = players;
                _gridSize = gridSize;
                _lastTurnTime = new long[players.Length];
                
                System.Diagnostics.Debug.WriteLine($"GLTRON: AI initialized for {players.Length} players on {gridSize}x{gridSize} grid");
                
#if ANDROID
                Android.Util.Log.Info("GLTRON", $"AI initialized for {players.Length} players on {gridSize}x{gridSize} grid");
#endif
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GLTRON: AI initialization failed: {ex}");
            }
        }
        
        public static void UpdateTime(long deltaTime, long currentTime)
        {
            _deltaTime = deltaTime;
            _currentTime = currentTime;
        }
        
        public static void DoComputer(int playerIndex, int ownPlayerIndex)
        {
            if (_players == null || _walls == null) return;
            if (playerIndex >= _players.Length || playerIndex == ownPlayerIndex) return;
            
            var player = _players[playerIndex];
            if (player == null || player.getSpeed() <= 0.0f) return;
            
            // Turn cooldown: avoid oscillations
            if (_lastTurnTime != null)
            {
                long last = _lastTurnTime[playerIndex];
                const long TURN_COOLDOWN_MS = 250;
                if (last != 0 && (_currentTime - last) < TURN_COOLDOWN_MS)
                {
                    return; // skip decision this frame
                }
            }
            
            try
            {
                // Calculate distances to obstacles
                float[] distances = CalculateDistances(player);
                
                // Determine AI action based on distances
                int action = DetermineAIAction(player, distances, ownPlayerIndex);
                
                if (action != 0) // 0 = no turn
                {
                    player.doTurn(action, _currentTime);
                    if (_lastTurnTime != null) _lastTurnTime[playerIndex] = _currentTime;
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GLTRON: AI error for player {playerIndex}: {ex}");
            }
        }
        
        private static float[] CalculateDistances(IPlayer player)
        {
            float x = player.getXpos();
            float y = player.getYpos();
            int direction = player.getDirection();
            
            // Direction vectors: 0=up(-Y), 1=right(+X), 2=down(+Y), 3=left(-X)
            float[] dirX = { 0.0f, 1.0f, 0.0f, -1.0f };
            float[] dirY = { -1.0f, 0.0f, 1.0f, 0.0f };
            
            float[] distances = new float[3]; // forward, left, right
            
            // Calculate distance in each direction
            for (int i = 0; i < 3; i++)
            {
                int checkDir = (direction + i - 1 + 4) % 4; // left, forward, right
                distances[i] = CalculateDistanceInDirection(x, y, checkDir, dirX, dirY);
            }
            
            return distances;
        }
        
        private static float CalculateDistanceInDirection(float startX, float startY, int direction, float[] dirX, float[] dirY)
        {
            float distance = 0.0f;
            float step = 1.0f; // finer sampling for better precision
            float maxDistance = AI_LOOKAHEAD_DISTANCE;
            
            float dx = dirX[direction];
            float dy = dirY[direction];
            
            for (float d = step; d <= maxDistance; d += step)
            {
                float checkX = startX + dx * d;
                float checkY = startY + dy * d;
                
                // Check wall collision first (more important)
                if (CheckWallCollision(checkX, checkY))
                {
                    return d - 0.5f; // small bias to consider collision slightly earlier
                }
                
                // Check trail collision
                if (CheckTrailCollision(checkX, checkY))
                {
                    return d - 0.5f; // small bias
                }
                
                distance = d;
            }
            
            return distance;
        }
        
        private static bool CheckWallCollision(float x, float y)
        {
            if (_walls == null) return false;
            
            // CRITICAL FIX: Match Java wall collision detection
            // Java walls are at the edges of the grid (0,0) to (gridSize, gridSize)
            float margin = 1.0f; // Small margin for collision detection
            
            if (x <= margin || x >= (_gridSize - margin) || 
                y <= margin || y >= (_gridSize - margin))
            {
                return true;
            }
            
            return false;
        }
        
        private static bool CheckTrailCollision(float x, float y)
        {
            if (_players == null) return false;
            
            foreach (var player in _players)
            {
                if (player == null || player.getTrailHeight() <= 0) continue;
                
                // Ignore immediate vicinity of the player's own head to avoid self-hit artifacts
                float headX = player.getXpos();
                float headY = player.getYpos();
                float dx = x - headX;
                float dy = y - headY;
                float distHead = (float)Math.Sqrt(dx * dx + dy * dy);
                if (distHead < 1.0f) continue;
                
                // Approximate trail occupancy by proximity to the player's path
                float distance = (float)Math.Sqrt((x - headX) * (x - headX) + (y - headY) * (y - headY));
                if (distance < 2.0f) // Trail width approximation
                {
                    return true;
                }
            }
            
            return false;
        }
        
        private static int DetermineAIAction(IPlayer player, float[] distances, int ownPlayerIndex)
        {
            // Tron-style: prefer straight, turn decisively when needed
            const float SAFE_STRAIGHT = 20.0f;  // if we have this much free space, keep straight
            const float MIN_WIDTH = 3.0f;       // minimal comfortable corridor width
            const float TURN_CLEAR_MARGIN = 2.0f; // require this much better score to turn

            float forwardDist = DistanceUntilCollision(player, 0);
            float leftLane = DistanceUntilCollision(player, -1);
            float rightLane = DistanceUntilCollision(player, 1);

            float widthForward = CorridorWidth(player, 0);
            float widthLeft = CorridorWidth(player, -1);
            float widthRight = CorridorWidth(player, 1);

            // Emergency: if forward is tight, pick safer side
            if (forwardDist < AI_EVASIVE_DISTANCE || widthForward < (MIN_WIDTH - 0.5f))
            {
                // score sides by lane length and width
                float scoreL = leftLane + widthLeft * 2.0f - EnclosurePenalty(player, -1);
                float scoreR = rightLane + widthRight * 2.0f - EnclosurePenalty(player, 1);
                if (scoreL > scoreR + 0.5f) return Player.TURN_LEFT;
                if (scoreR > scoreL + 0.5f) return Player.TURN_RIGHT;
                // tie-breaker: pick the longer lane
                if (leftLane > rightLane) return Player.TURN_LEFT;
                if (rightLane > leftLane) return Player.TURN_RIGHT;
                return _random.NextDouble() < 0.5 ? Player.TURN_LEFT : Player.TURN_RIGHT;
            }

            // If straight is safe and corridor is decent, stay straight
            if (forwardDist >= SAFE_STRAIGHT && widthForward >= MIN_WIDTH)
            {
                return 0;
            }

            // Evaluate sides using a weighted heuristic
            float scoreForward = forwardDist + widthForward * 2.5f - EnclosurePenalty(player, 0) * 0.5f;
            float scoreLeft = leftLane + widthLeft * 2.5f - EnclosurePenalty(player, -1);
            float scoreRight = rightLane + widthRight * 2.5f - EnclosurePenalty(player, 1);

            float bestSide = Math.Max(scoreLeft, scoreRight);
            if (bestSide > scoreForward + TURN_CLEAR_MARGIN)
            {
                return (scoreLeft > scoreRight) ? Player.TURN_LEFT : Player.TURN_RIGHT;
            }

            // Otherwise, keep steady
            return 0;
        }

        // Distance until collision from player's position in relative direction (-1 left, 0 forward, 1 right)
        private static float DistanceUntilCollision(IPlayer player, int relTurn)
        {
            int dir = (player.getDirection() + (relTurn == -1 ? 3 : (relTurn == 1 ? 1 : 0))) % 4;
            float[] dirX = { 0.0f, 1.0f, 0.0f, -1.0f };
            float[] dirY = { -1.0f, 0.0f, 1.0f, 0.0f };
            float x = player.getXpos();
            float y = player.getYpos();
            float step = 1.0f;
            float distance = 0.0f;
            for (float d = step; d <= AI_LOOKAHEAD_DISTANCE; d += step)
            {
                float cx = x + dirX[dir] * d;
                float cy = y + dirY[dir] * d;
                if (CheckWallCollision(cx, cy) || CheckTrailCollision(cx, cy))
                {
                    return Math.Max(0.0f, d - 0.5f);
                }
                distance = d;
            }
            return distance;
        }

        // Estimate corridor width by sampling perpendicular offsets ahead of the player
        private static float CorridorWidth(IPlayer player, int relTurn)
        {
            int dir = (player.getDirection() + (relTurn == -1 ? 3 : (relTurn == 1 ? 1 : 0))) % 4;
            float[] dirX = { 0.0f, 1.0f, 0.0f, -1.0f };
            float[] dirY = { -1.0f, 0.0f, 1.0f, 0.0f };
            float x = player.getXpos();
            float y = player.getYpos();

            float perpX = dirY[dir];
            float perpY = -dirX[dir];

            // sample at few steps ahead to average corridor
            float[] depths = new float[] { 2f, 4f, 6f };
            float maxPerp = 4.0f;
            float step = 1.0f;
            float totalWidth = 0.0f;
            int samples = 0;
            foreach (var depth in depths)
            {
                float cx = x + dirX[dir] * depth;
                float cy = y + dirY[dir] * depth;
                float width = 0.0f;
                // expand to both sides until hit
                for (float p = step; p <= maxPerp; p += step)
                {
                    float lx = cx + perpX * p;
                    float ly = cy + perpY * p;
                    float rx = cx - perpX * p;
                    float ry = cy - perpY * p;
                    bool leftFree = !CheckWallCollision(lx, ly) && !CheckTrailCollision(lx, ly);
                    bool rightFree = !CheckWallCollision(rx, ry) && !CheckTrailCollision(rx, ry);
                    if (leftFree) width += 0.5f; else break; // stop at first collision on left side
                }
                for (float p = step; p <= maxPerp; p += step)
                {
                    float rx = cx - perpX * p;
                    float ry = cy - perpY * p;
                    if (!CheckWallCollision(rx, ry) && !CheckTrailCollision(rx, ry)) width += 0.5f; else break;
                }
                totalWidth += width;
                samples++;
            }
            return samples > 0 ? totalWidth / samples : 0.0f;
        }

        // Penalize directions that quickly lead into enclosed areas (simple bounded exploration)
        private static float EnclosurePenalty(IPlayer player, int relTurn)
        {
            int dir = (player.getDirection() + (relTurn == -1 ? 3 : (relTurn == 1 ? 1 : 0))) % 4;
            float[] dirX = { 0.0f, 1.0f, 0.0f, -1.0f };
            float[] dirY = { -1.0f, 0.0f, 1.0f, 0.0f };
            float startX = player.getXpos() + dirX[dir] * 2.0f;
            float startY = player.getYpos() + dirY[dir] * 2.0f;

            // Coarse exploration grid
            float step = 2.0f;
            int maxNodes = 40; // bounded search
            int explored = 0;
            // simple FIFO queue using arrays
            int qh = 0, qt = 0;
            const int QSIZE = 96;
            float[] qx = new float[QSIZE];
            float[] qy = new float[QSIZE];

            // visited check via hashing to coarse cells
            System.Collections.Generic.HashSet<long> visited = new System.Collections.Generic.HashSet<long>();
            void Enqueue(float ex, float ey)
            {
                long key = (((long)Math.Floor(ex / step)) << 32) ^ (long)Math.Floor(ey / step);
                if (visited.Contains(key)) return;
                visited.Add(key);
                qx[qt] = ex; qy[qt] = ey; qt = (qt + 1) % QSIZE;
            }
            bool Dequeue(out float ox, out float oy)
            {
                if (qh == qt) { ox = oy = 0; return false; }
                ox = qx[qh]; oy = qy[qh]; qh = (qh + 1) % QSIZE; return true;
            }

            Enqueue(startX, startY);
            while (Dequeue(out float cx, out float cy))
            {
                if (explored++ > maxNodes) break;
                // neighbors 4-dir
                float[,] neigh = new float[,] { { cx + step, cy }, { cx - step, cy }, { cx, cy + step }, { cx, cy - step } };
                for (int i = 0; i < 4; i++)
                {
                    float nx = neigh[i,0];
                    float ny = neigh[i,1];
                    if (nx < 0 || ny < 0 || nx > _gridSize || ny > _gridSize) continue;
                    if (CheckWallCollision(nx, ny) || CheckTrailCollision(nx, ny)) continue;
                    Enqueue(nx, ny);
                }
            }

            // If few nodes are reachable, it's enclosed: return higher penalty
            float openness = explored; // proportional to reachable area estimate
            float penalty = Math.Max(0.0f, 40 - openness) * 0.2f; // 0..~8
            return penalty;
        }
        
        public static IPlayer? GetClosestOpponent(IPlayer player, int ownPlayerIndex)
        {
            if (_players == null) return null;
            
            IPlayer? closest = null;
            float minDistance = float.MaxValue;
            
            float myX = player.getXpos();
            float myY = player.getYpos();
            
            for (int i = 0; i < _players.Length; i++)
            {
                if (i == ownPlayerIndex || _players[i] == null || _players[i].getSpeed() <= 0.0f) continue;
                
                float otherX = _players[i].getXpos();
                float otherY = _players[i].getYpos();
                float distance = (float)Math.Sqrt((otherX - myX) * (otherX - myX) + (otherY - myY) * (otherY - myY));
                
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = _players[i];
                }
            }
            
            return closest;
        }
    }
}
