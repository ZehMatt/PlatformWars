using Sandbox;

namespace PlatformWars
{
	partial class RoundManager
	{
		[ServerVar]
		public static float platformwars_transition_time { get; set; } = 1.0f;

		[Net, Predicted]
		int CurrentCycle { get; set; } = 0;

		public Player GetNextPlayer()
		{
			int playerIndex = CurrentCycle % ActivePlayers.Count;
			return GetPlayer( playerIndex );
		}

		public Pawn GetNextPawn( Player player )
		{
			if ( player.Pawns.Count == 0 )
				return null;

			int pawnIndex = CurrentCycle % player.Pawns.Count;
			return player.GetPawn( pawnIndex );
		}

		Player GetPlayer( int index )
		{
			if ( index >= ActivePlayers.Count )
				return null;

			var ent = ActivePlayers[index];
			if ( ent == null )
				return null;

			return ent;
		}

		void HandleTransition()
		{
			var currentPly = GetActivePlayer();
			if ( currentPly != null )
			{
				currentPly.SetCameraMode( Cameras.Mode.Spectate );
			}

			var ply = GetNextPlayer();
			var pawn = GetNextPawn( ply );

			CurrentCycle++;

			SetActivePlayer( ply );
			ActivePawn = pawn;

			ply.ControllPawn( pawn );

			SetState( RoundState.PrePlayerTurn );
		}
	}
}
