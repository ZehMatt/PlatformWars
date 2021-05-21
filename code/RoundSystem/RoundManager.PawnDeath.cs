using Sandbox;
using System.Collections.Generic;

namespace PlatformWars
{
	struct SavedGlobalState
	{
		public float PhysicsTimeScale;
	}

	partial class RoundManager
	{
		// FIXME: Use stack when whitelisted.
		List<SavedGlobalState> SavedDeathStates = new();

		[Net]
		Network.EntityList DyingPawns { get; set; } = new();

		// TODO: Make this a convar.
		const float MaxPawnDeathTime = 10.0f;

		// Whenever a pawn goes kaboom it should end up calling this function.
		// This round state manages all moving ragdolls and the cameras. We
		// want to resume the game play after the slowmo kaboom has played out.
		void AddDyingPawn( Pawn pawn )
		{
			DyingPawns.Add( pawn );
		}

		void PreHandlePawnDeath()
		{
			if ( !IsAuthority )
				return;

			var cam = new Cameras.PawnDeathCam();

			foreach ( var ply in GetActivePlayers() )
			{
				ply.PushCameraMode( Cameras.Mode.PawnDeath );
			}

			SavedGlobalState saved = new();
			saved.PhysicsTimeScale = Global.PhysicsTimeScale;

			SavedDeathStates.Add( saved );

			Global.PhysicsTimeScale = 0.1f;
		}

		void HandlePawnDeath()
		{
			if ( !IsAuthority )
				return;

			// The minimum time?
			if ( StateTime < 1.0f )
				return;

			if ( DyingPawns.Count == 0 )
			{
				// Everything gone quiet, resume play.
				PopState();
				return;
			}

			// Keep this state alive for as long there is stuff dying and flying around.
			for ( int i = 0; i < DyingPawns.Count; )
			{
				bool shouldRemove = false;

				var pawn = DyingPawns.Get( i ) as Pawn;
				if ( pawn == null )
				{
					// What?
					Log.Error( "Pawn doesn't exist? Logical fuckup." );

					shouldRemove = true;
				}
				else if ( pawn.GetDeathTime() >= MaxPawnDeathTime )
				{
					// Only allow dead pawns to stay in this list for max 10 seconds.
					Log.Info( "Removing pawn from dying list, exceeded maximum time" );
					shouldRemove = true;
				}
				else
				{
					var ragdoll = pawn.GetRagdoll();
					if ( ragdoll == null || ragdoll.PhysicsBody == null )
					{

						shouldRemove = true;
					}
					else
					{
						var phys = ragdoll.PhysicsBody;
						if ( phys.Velocity.IsNearZeroLength )
						{
							Log.Info( "Removing pawn from dying list, ragdoll asleep" );
							shouldRemove = true;
						}

					}
				}

				if ( shouldRemove )
				{
					DyingPawns.RemoveAt( i );
					if ( DyingPawns.Count == 0 )
						break;

					i--;
				}
				else
				{
					i++;
				}
			}
		}

		void PostHandlePawnDeath()
		{
			if ( !IsAuthority )
				return;

			var idx = SavedDeathStates.Count - 1;
			var saved = SavedDeathStates[idx];
			SavedDeathStates.RemoveAt( idx );

			Global.PhysicsTimeScale = saved.PhysicsTimeScale;

			foreach ( var ply in GetActivePlayers() )
			{
				ply.PopCameraMode();
			}
		}

		public Vector3 GetDeathSpectatePos()
		{
			Vector3 pos = Vector3.Zero;
			for ( int i = 0; i < DyingPawns.Count; ++i )
			{
				var ent = DyingPawns.Get( i ) as Pawn;
				if ( ent == null )
					continue;

				var ragdoll = ent.GetRagdoll();
				if ( ragdoll == null )
					continue; // Potentially networked late.

				pos += ragdoll.Position;
			}

			if ( DyingPawns.Count > 0 )
				pos /= DyingPawns.Count;

			return pos;
		}
	}
}
