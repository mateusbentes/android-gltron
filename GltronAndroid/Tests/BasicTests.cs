using System;
using Microsoft.Xna.Framework;
using GltronMonoGame;

namespace GltronAndroid.Tests
{
    /// <summary>
    /// Basic tests to validate crash fixes
    /// </summary>
    public static class BasicTests
    {
        public static bool TestPlayerCreation()
        {
            try
            {
                var player = new Player(0, 100.0f);
                return player != null && player.getSpeed() > 0;
            }
            catch (Exception ex)
            {
                try { Android.Util.Log.Error("GLTRON", $"TestPlayerCreation failed: {ex}"); } catch { }
                return false;
            }
        }

        public static bool TestSegmentIntersection()
        {
            try
            {
                var seg1 = new Segment(new Vec(0, 0), new Vec(10, 0));
                var seg2 = new Segment(new Vec(5, -5), new Vec(0, 10));
                
                var intersection = seg1.Intersect(seg2);
                return intersection != null; // Should intersect at (5, 0)
            }
            catch (Exception ex)
            {
                try { Android.Util.Log.Error("GLTRON", $"TestSegmentIntersection failed: {ex}"); } catch { }
                return false;
            }
        }

        public static bool TestGameInitialization()
        {
            try
            {
                var game = new GLTronGame();
                game.initialiseGame();
                return game.GetOwnPlayerScore() >= 0;
            }
            catch (Exception ex)
            {
                try { Android.Util.Log.Error("GLTRON", $"TestGameInitialization failed: {ex}"); } catch { }
                return false;
            }
        }

        public static void RunAllTests()
        {
            try
            {
                Android.Util.Log.Info("GLTRON", "Running basic tests...");
                
                bool playerTest = TestPlayerCreation();
                bool segmentTest = TestSegmentIntersection();
                bool gameTest = TestGameInitialization();
                
                Android.Util.Log.Info("GLTRON", $"Player creation test: {(playerTest ? "PASS" : "FAIL")}");
                Android.Util.Log.Info("GLTRON", $"Segment intersection test: {(segmentTest ? "PASS" : "FAIL")}");
                Android.Util.Log.Info("GLTRON", $"Game initialization test: {(gameTest ? "PASS" : "FAIL")}");
                
                if (playerTest && segmentTest && gameTest)
                {
                    Android.Util.Log.Info("GLTRON", "All basic tests PASSED");
                }
                else
                {
                    Android.Util.Log.Warn("GLTRON", "Some basic tests FAILED");
                }
            }
            catch (Exception ex)
            {
                try { Android.Util.Log.Error("GLTRON", $"RunAllTests failed: {ex}"); } catch { }
            }
        }
    }
}
