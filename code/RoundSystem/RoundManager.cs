using Sandbox;
using System.Collections.Generic;

namespace PlatformWars
{

	enum RoundState
	{
		// Initial.
		Idle = 0,

		// The cycle starts over here.
		WaitForPlayer,
		TerrainGen,
		Setup,
		Starting,
		// -> Round cycle.
		PrePlayerTurn,
		PlayerTurn,
		PostPlayerTurn,
		Evaluate, // Test if the round should finish, next state is Transition or End
		Transition,
		// Round end.
		End,

		// Begin new game.
		Restart,
	}

	[Library( "ent_roundmanager", Title = "Round Manager", Spawnable = false )]
	partial class RoundManager : Entity
	{
		[ServerVar]
		public static bool platformwars_debug { get; set; } = true;

		[NetPredicted]
		bool StatePaused { get; set; }

		[NetPredicted]
		float StateTime { get; set; }

		[NetPredicted]
		RoundState State { get; set; }

		[NetPredicted]
		Player ActivePlayer { get; set; }

		[Net]
		EntityHandle<Pawn> ActivePawn { get; set; }

		[Net]
		Network.NetList<EntityHandle<Player>> ActivePlayers { get; set; } = new();

		public static RoundManager Get()
		{
			var game = GameBase.Current as PlatformWars.Game;
			if ( game == null )
				return null;

			return game.GetRoundManager();
		}

		public RoundManager()
		{
		}

		public override void Spawn()
		{
			Transmit = TransmitType.Always;
		}

		[Event( "tick" )]
		void Tick()
		{
			//DebugOverlay.ScreenText(0, $"Round State: {State.ToString()}");
			//DebugOverlay.ScreenText(1, $"State Time: {StateTime.ToString()}");

			switch ( State )
			{
				case RoundState.Idle:
					HandleIdle();
					break;
				case RoundState.WaitForPlayer:
					HandleWaitForPlayer();
					break;
				case RoundState.TerrainGen:
					HandleTerrainGeneration();
					break;
				case RoundState.Setup:
					HandleSetup();
					break;
				case RoundState.Starting:
					HandleStarting();
					break;
				case RoundState.PrePlayerTurn:
					HandlePrePlayerTurn();
					break;
				case RoundState.PlayerTurn:
					HandlePlayerTurn();
					break;
				case RoundState.PostPlayerTurn:
					HandlePostPlayerTurn();
					break;
				case RoundState.Evaluate:
					HandleEvaluate();
					break;
				case RoundState.Transition:
					HandleTransition();
					break;
				case RoundState.End:
					break;
			}

			if ( StatePaused )
				StateTime += Time.Delta;
		}

		void PushState()
		{

		}

		void PopState()
		{

		}

		void SetState( RoundState newState )
		{
			State = newState;
			StateTime = 0;
		}

		public RoundState GetState()
		{
			return State;
		}

		void SetActivePlayer( Player ply )
		{
			ActivePlayer = ply;
		}

		public Player GetActivePlayer()
		{
			return ActivePlayer;
		}

		public Pawn GetActivePawn()
		{
			if ( !ActivePawn.IsValid )
				return null;

			return ActivePawn.Entity as Pawn;
		}

		public bool HasPlayerControl( Player ply )
		{
			if ( platformwars_debug && State == RoundState.WaitForPlayer )
				return true;

			if ( State != RoundState.PlayerTurn )
				return false;

			if ( ActivePlayer != ply )
				return false;

			var pawn = ply.GetControlledPawn();
			return pawn != null && pawn.IsValid();
		}

		public List<Player> GetActivePlayers()
		{
			List<Player> res = new();
			for ( int i = 0; i < ActivePlayers.Count; ++i )
			{
				var ent = ActivePlayers.Get( i );
				if ( !ent.Entity.IsValid() )
					continue;

				res.Add( ent.Entity as Player );
			}
			return res;
		}

		public void OnPawnKilled( Pawn pawn )
		{
			var ply = pawn.GetPlayer();
			ply.RemovePawn( pawn );

			if ( ply == GetActivePlayer() && pawn == GetActivePawn() )
			{
				ply.RemoveControlled();
				ply.Camera = new SpectateRagdollCamera();

				SetState( RoundState.PostPlayerTurn );
			}
		}

		public float GetStateTime()
		{
			return StateTime;
		}

		public float GetStateTime( RoundState state )
		{
			switch ( state )
			{
				case RoundState.Idle:
				case RoundState.WaitForPlayer:
				case RoundState.Setup:
				case RoundState.Evaluate:
				case RoundState.Transition:
				case RoundState.End:
				case RoundState.Restart:
					break;
				case RoundState.Starting:
					return platformwars_start_time;
				case RoundState.PrePlayerTurn:
					return platformwars_preturn_time;
				case RoundState.PlayerTurn:
					return platformwars_turn_time;
				case RoundState.PostPlayerTurn:
					return platformwars_postturn_time;
			}
			return -1.0f;
		}
	}
}
