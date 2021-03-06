using Sandbox;
using System;
using System.Collections.Generic;

namespace PlatformWars
{
	partial class RoundManager
	{
		const int PawnCount = 10;

		void SetupTeams()
		{
			Host.AssertServer();

			ActivePlayers.Clear();

			// Setup the Teams.
			int playerIdx = 0;
			var game = Game.Current as PlatformWars.Game;
			var players = game.GetPlayers();
			foreach ( var p in players )
			{
				var ply = p as Player;
				ply.SetTeam( Team.Red + playerIdx );

				playerIdx++;
				ActivePlayers.Add( ply );
			}
		}

		void SetupPawns()
		{
			var players = GetActivePlayers();
			for ( int i = 0; i < players.Count; i++ )
			{
				var ply = players[i];
				ply.SetupPawns( PawnCount );
			}
		}

		struct SpawnPos
		{
			public Vector3 pos;
			public float averageDist;

			public int CompareDistance( SpawnPos y )
			{
				return (int)(averageDist - y.averageDist);
			}
		}

		void ReorganizePawns()
		{
			Host.AssertServer();

			var players = GetActivePlayers();

			List<Pawn> pawns = new List<Pawn>();
			foreach ( var p in players )
			{
				var ply = p as Player;
				pawns.AddRange( ply.GetPawns() );
			}

			var mgr = World.Manager.Get();
			var totalDim = World.Manager.Width * World.Manager.Length;
			var dividedDim = MathF.Sqrt( totalDim / pawns.Count );

			var spawns = new List<SpawnPos>();
			foreach ( var spawn in mgr.GetSpawns() )
			{
				spawns.Add( new SpawnPos() { pos = spawn, averageDist = 0.0f } ); ;
			}

			for ( int i = 0; i < spawns.Count; ++i )
			{
				var spawn = spawns[i];
				for ( int j = 0; j < spawns.Count; j++ )
				{
					if ( j == i )
						continue;

					var dist = Vector3.DistanceBetween( spawn.pos, spawns[j].pos );
					if ( dist < dividedDim )
					{
						// We have to make sure we got enough spawns.
						if ( spawns.Count - 1 <= pawns.Count )
							break;

						spawns.RemoveAt( j );
						if ( j < i )
							i--;
						j--;
					}
				}
			}

			var prng = new System.Random( WorldSeed );

			int spawnCount = pawns.Count;
			foreach ( var pawn in pawns )
			{
				int spawnPick = prng.Next( 0, spawns.Count - 1 );

				var spawn = spawns[spawnPick];
				var pos = spawn.pos;
				var spawnPos = new Vector3( pos.x * World.Voxel.SizeX, pos.y * World.Voxel.SizeY, 200 + (pos.z * World.Voxel.SizeZ) );

				pawn.Reset( spawnPos );

				spawns.RemoveAt( spawnPick );
			}
		}

		void SetupLoadout()
		{
			var players = GetActivePlayers();
			foreach ( var ply in players )
			{
				ply.AddItem( new Weapons.Pistol() );
				ply.AddItem( new Weapons.SMG() );
			}
		}

		void HandleSetup()
		{
			if ( IsServer )
			{
				SetupTeams();
				SetupPawns();
				ReorganizePawns();
				SetupLoadout();
			}

			SetState( RoundState.Starting );
		}

	}
}
