using Sandbox;

namespace PlatformWars
{
    [Library("platformwars", Title = "PlatformWars")]
    partial class Game : Sandbox.Game
    {
        [Net]
        EntityHandle<RoundManager> RoundManager { get; set; }

        public Game()
        {
            if (IsServer)
            {
                new PlatformWars.UI.Hud();
            }
        }

        public RoundManager GetRoundManager()
        {
            return RoundManager;
        }

        public override Player CreatePlayer() => new PlatformWars.Player();

        public override void PostLevelLoaded()
        {
            base.PostLevelLoaded();

            RoundManager = Create<RoundManager>();
        }


    }

}
