namespace PlatformWars
{
	partial class RoundManager
	{
		void HandleWaitForPlayer()
		{
			var game = Game.Current as PlatformWars.Game;
			var players = game.GetPlayers();

			if ( players.Count < 2 )
				return;



			// Begin round setup.
			SetState( RoundState.TerrainGen );
		}
	}
}
