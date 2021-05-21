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

		// Temporary states
		PawnDeath,
	}

	struct SavedState
	{
		public RoundState State;
		public float StateTime;
	}

	[Library( "ent_roundmanager", Title = "Round Manager", Spawnable = false )]
	partial class RoundManager : Entity
	{
		public delegate void StateActivationDelegate();
		public delegate void StateUpdateDelegate();
		public delegate void StateDeactivationDelegate();

		[ServerVar]
		public static bool platformwars_debug { get; set; } = true;

		[NetPredicted]
		bool StatePaused { get; set; }

		[Net]
		float StateTime { get; set; }

		[NetPredicted]
		RoundState State { get; set; }

		[NetPredicted]
		Player ActivePlayer { get; set; }

		[Net]
		EntityHandle<Pawn> ActivePawn { get; set; }

		[Net]
		int WorldSeed { get; set; } = 1;

		Stack<SavedState> SavedStates = new();

		[Net]
		Network.EntityList ActivePlayers { get; set; } = new();

		Dictionary<RoundState, StateActivationDelegate> ActivationHandler = new();
		Dictionary<RoundState, StateUpdateDelegate> UpdateHandler = new();
		Dictionary<RoundState, StateDeactivationDelegate> DeactivationHandler = new();

		public static RoundManager Get()
		{
			var game = Game.Current as PlatformWars.Game;
			if ( game == null )
				return null;

			return game.GetRoundManager();
		}

		public RoundManager()
		{
			// FIXME: Make this a bit more generic, its kept as delegates so it all can access
			// the round manager without jumping through another hoop.

			UpdateHandler.Add( RoundState.Idle, HandleIdle );

			UpdateHandler.Add( RoundState.WaitForPlayer, HandleWaitForPlayer );

			UpdateHandler.Add( RoundState.TerrainGen, HandleTerrainGeneration );

			UpdateHandler.Add( RoundState.Setup, HandleSetup );

			UpdateHandler.Add( RoundState.Starting, HandleStarting );

			UpdateHandler.Add( RoundState.PrePlayerTurn, HandlePrePlayerTurn );

			UpdateHandler.Add( RoundState.PlayerTurn, HandlePlayerTurn );

			UpdateHandler.Add( RoundState.PostPlayerTurn, HandlePostPlayerTurn );

			UpdateHandler.Add( RoundState.Evaluate, HandleEvaluate );

			UpdateHandler.Add( RoundState.Transition, HandleTransition );

			ActivationHandler.Add( RoundState.PawnDeath, PreHandlePawnDeath );
			UpdateHandler.Add( RoundState.PawnDeath, HandlePawnDeath );
			DeactivationHandler.Add( RoundState.PawnDeath, PostHandlePawnDeath );
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
			StateUpdateDelegate del;
			if ( UpdateHandler.TryGetValue( State, out del ) )
			{
				del();
			}

			if ( !StatePaused && IsAuthority )
				StateTime += Time.Delta;
		}

		void PushState( RoundState newState )
		{
			SavedStates.Push( new SavedState() { State = State, StateTime = StateTime } );
			SetState( newState );
		}

		void PopState()
		{
			if ( SavedStates.Count == 0 )
			{
				Log.Error( "Attempting to pop states on empty list." );
				return;
			}

			StateDeactivationDelegate del;
			if ( DeactivationHandler.TryGetValue( State, out del ) )
			{
				del();
			}

			var saved = SavedStates.Pop();
			State = saved.State;
			StateTime = saved.StateTime;
		}

		void SetState( RoundState newState )
		{
			{
				StateDeactivationDelegate del;
				if ( DeactivationHandler.TryGetValue( State, out del ) )
				{
					del();
				}
			}

			State = newState;
			StateTime = 0;

			{
				StateActivationDelegate del;
				if ( ActivationHandler.TryGetValue( State, out del ) )
				{
					del();
				}
			}
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

		public bool CanPawnMove( Pawn pawn )
		{
			if ( platformwars_debug && State == RoundState.WaitForPlayer )
				return true;

			if ( State != RoundState.PlayerTurn )
				return false;

			if ( ActivePawn != pawn )
				return false;

			return true;
		}

		public List<Player> GetActivePlayers()
		{
			List<Player> res = new();
			for ( int i = 0; i < ActivePlayers.Count; ++i )
			{
				var ent = ActivePlayers.Get( i ) as Player;
				if ( ent == null )
					continue;

				res.Add( ent );
			}
			return res;
		}

		public void OnPawnKilled( Pawn pawn )
		{
			if ( !IsAuthority )
				return;

			var ply = pawn.GetPlayer();
			ply.RemovePawn( pawn );

			if ( ply == GetActivePlayer() && pawn == GetActivePawn() )
			{
				ply.RemoveControlled();
				SetState( RoundState.PostPlayerTurn );
			}

			AddDyingPawn( pawn );
			PushState( RoundState.PawnDeath );
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
