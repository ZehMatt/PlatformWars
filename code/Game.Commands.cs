using Sandbox;

namespace PlatformWars
{
    partial class Game
    {
        [ServerCmd("pos", Help = "Returns some information about the player position")]
        public static void PosCommand()
        {
            var target = ConsoleSystem.Caller;
            if (target == null) return;
            Log.Info($"Pos: {target.WorldPos}");
            Log.Info($"Ang: {target.WorldAng}");
            Log.Info($"Vel: {target.Velocity}");
        }

        [ServerCmd("platforms", Help = "Generate platforms")]
        public static void PlatformsCommand()
        {
            var game = GameBase.Current as PlatformWars.Game;
            if (game == null)
                return;

            var roundMgr = game.GetRoundManager();
            if (roundMgr == null)
                return;

            roundMgr.GeneratePlatforms();
        }
    }
}
