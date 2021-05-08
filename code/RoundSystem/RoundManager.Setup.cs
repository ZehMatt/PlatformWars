using Sandbox;
using System;
using System.Collections.Generic;

namespace PlatformWars
{
	partial class RoundManager
	{
		void SetupTeams()
		{
			Host.AssertServer();

			ActivePlayers.Clear();

			// Setup the Teams.
			int playerIdx = 0;
			foreach ( var p in Player.All )
			{
				var ply = p as Player;
				ply.SetTeam( Team.Red + playerIdx );

				playerIdx++;
				ActivePlayers.Add( ply );
			}
		}

		void SetupPawns()
		{
			for ( int i = 0; i < ActivePlayers.Count; i++ )
			{
				var ply = ActivePlayers.Get( i ).Entity as Player;
				ply.SetupPawns( 4 );
			}
		}

		bool GetSpawnPos( Terrain.Manager mgr, float x, float y, out Vector3 res )
		{
			res = Vector3.Zero;

			int w = Terrain.Manager.Width;
			int l = Terrain.Manager.Length;
			int h = Terrain.Manager.MaxHeight;

			Vector3 pos = new Vector3( -(w / 2), -(l / 2), 16 );

			for ( int x1 = -2; x1 < 2; x1++ )
			{
				for ( int y1 = -2; y1 < 2; y1++ )
				{
					var voxPos = pos + new Vector3( x1, y1, 16 );
					var vox = mgr.Get( voxPos );
					if ( vox.Type == Terrain.TerrainType.Solid )
					{
						res = voxPos * Terrain.Voxel.SizeX;
						return true;
					}
				}
			}

			return false;
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

			List<Pawn> pawns = new List<Pawn>();
			foreach ( var p in Player.All )
			{
				var ply = p as Player;
				pawns.AddRange( ply.GetPawns() );
			}

			var mgr = Terrain.Manager.Get();
			var totalDim = Terrain.Manager.Width * Terrain.Manager.Length;
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
					if ( dist < 16 )
					{
						spawns.RemoveAt( j );
						if ( j < i )
							i--;
						j--;
					}
				}
			}

			int spawnCount = pawns.Count;
			foreach ( var pawn in pawns )
			{
				int spawnPick = Rand.Int( 0, spawns.Count - 1 );

				var spawn = spawns[spawnPick];
				var pos = spawn.pos;
				var spawnPos = new Vector3( pos.x * Terrain.Voxel.SizeX, pos.y * Terrain.Voxel.SizeY, 200 + (pos.z * Terrain.Voxel.SizeZ) );

				pawn.Reset( spawnPos );

				spawns.RemoveAt( spawnPick );
			}
		}

		void SetupPlayers()
		{
			for ( int i = 0; i < ActivePlayers.Count; i++ )
			{
				var ply = ActivePlayers.Get( i ).Entity as Player;
				ply.Respawn();
			}
		}

		void HandleSetup()
		{
			if ( IsServer )
			{
				SetupTeams();
				SetupPlayers();
				SetupPawns();
				ReorganizePawns();
			}

			SetState( RoundState.Starting );
		}

	}
}
