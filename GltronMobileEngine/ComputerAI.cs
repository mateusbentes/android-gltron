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
        
        // AI constants from Java version
        private const float AI_LOOKAHEAD_DISTANCE = 15.0f;
        private const float AI_TURN_PROBABILITY = 0.3f;
        private const float AI_AGGRESSIVE_DISTANCE = 25.0f;
        private const float AI_EVASIVE_DISTANCE = 10.0f;
        
        // AI timing
        private static readonly Random _random = new Random();
        
        public static void InitAI(ISegment[] walls, IPlayer[] players, float gridSize)
        {
            try
            {
                _walls = walls;
                _players = players;
                _gridSize = gridSize;
                
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
            
            try
            {
                // Get player position and direction
                float x = player.getXpos();
                float y = player.getYpos();
                int direction = player.getDirection();
                
                // Calculate distances to obstacles
                float[] distances = CalculateDistances(player);
                
                // Determine AI action based on distances
                int action = DetermineAIAction(player, distances, ownPlayerIndex);
                
                if (action != 0) // 0 = no turn
                {
                    player.doTurn(action, _currentTime);
                    
                    try
                    {
#if ANDROID
                        Android.Util.Log.Debug("GLTRON", $"AI Player {playerIndex} turned {(action == Player.TURN_LEFT ? "LEFT" : "RIGHT")} at ({x:F1},{y:F1})");
#endif
                    }
                    catch { }
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
            float step = 1.0f;
            float maxDistance = AI_LOOKAHEAD_DISTANCE;
            
            float dx = dirX[direction];
            float dy = dirY[direction];
            
            for (float d = step; d <= maxDistance; d += step)
            {
                float checkX = startX + dx * d;
                float checkY = startY + dy * d;
                
                // Check wall collision
                if (CheckWallCollision(checkX, checkY))
                {
                    return d;
                }
                
                // Check trail collision
                if (CheckTrailCollision(checkX, checkY))
                {
                    return d;
                }
                
                distance = d;
            }
            
            return distance;
        }
        
        private static bool CheckWallCollision(float x, float y)
        {
            if (_walls == null) return false;
            
            // Simple boundary check
            if (x <= 0 || x >= _gridSize || y <= 0 || y >= _gridSize)
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
                
                // Simple distance check to player trails
                float playerX = player.getXpos();
                float playerY = player.getYpos();
                float distance = (float)Math.Sqrt((x - playerX) * (x - playerX) + (y - playerY) * (y - playerY));
                
                if (distance < 2.0f) // Trail width approximation
                {
                    return true;
                }
            }
            
            return false;
        }
        
        private static int DetermineAIAction(IPlayer player, float[] distances, int ownPlayerIndex)
        {
            float forwardDist = distances[1]; // forward
            float leftDist = distances[0];    // left
            float rightDist = distances[2];   // right
            
            // Emergency avoidance - turn away from immediate danger
            if (forwardDist < AI_EVASIVE_DISTANCE)
            {
                if (leftDist > rightDist)
                {
                    return Player.TURN_LEFT;
                }
                else if (rightDist > leftDist)
                {
                    return Player.TURN_RIGHT;
                }
                else
                {
                    // Both sides equal, choose randomly
                    return _random.NextDouble() < 0.5 ? Player.TURN_LEFT : Player.TURN_RIGHT;
                }
            }
            
            // Aggressive behavior - try to get closer to human player
            if (_players != null && ownPlayerIndex < _players.Length)
            {
                var ownPlayer = _players[ownPlayerIndex];
                if (ownPlayer != null && ownPlayer.getSpeed() > 0.0f)
                {
                    float ownX = ownPlayer.getXpos();
                    float ownY = ownPlayer.getYpos();
                    float myX = player.getXpos();
                    float myY = player.getYpos();
                    
                    float distanceToOwn = (float)Math.Sqrt((ownX - myX) * (ownX - myX) + (ownY - myY) * (ownY - myY));
                    
                    if (distanceToOwn > AI_AGGRESSIVE_DISTANCE && forwardDist > AI_LOOKAHEAD_DISTANCE * 0.7f)
                    {
                        // Try to move toward human player
                        float angleToOwn = (float)Math.Atan2(ownY - myY, ownX - myX);
                        int currentDir = player.getDirection();
                        
                        // Simple direction adjustment toward target
                        if (_random.NextDouble() < AI_TURN_PROBABILITY)
                        {
                            return _random.NextDouble() < 0.5 ? Player.TURN_LEFT : Player.TURN_RIGHT;
                        }
                    }
                }
            }
            
            // Random movement to avoid predictability
            if (forwardDist > AI_LOOKAHEAD_DISTANCE * 0.8f && _random.NextDouble() < AI_TURN_PROBABILITY * 0.3f)
            {
                return _random.NextDouble() < 0.5 ? Player.TURN_LEFT : Player.TURN_RIGHT;
            }
            
            return 0; // No turn
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
