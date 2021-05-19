using Sandbox;
using System;
using System.Collections.Generic;

namespace PlatformWars.Terrain
{
	public enum TerrainType
	{
		Air = 0,
		Solid,
	}

	class Manager : Entity
	{
		public static int Width = 64;
		public static int Length = 64;
		public static int MaxHeight = 8;

		List<Entity> PlatformProps = new();
		List<Vector3> Spawns = new();
		Dictionary<Vector3, Chunk> Chunks = new();

		public static Manager Get()
		{
			var game = Game.Current as PlatformWars.Game;
			return game.GetTerrainManager();
		}

		// World space into voxel space. Each unit is divided by the block size.
		public Vector3 WorldToVoxel( Vector3 pos )
		{
			int x = MathX.FloorToInt( pos.x ) / Voxel.SizeX;
			int y = MathX.FloorToInt( pos.y ) / Voxel.SizeY;
			int z = MathX.FloorToInt( pos.z ) / Voxel.SizeZ;

			return new Vector3( x, y, z );
		}

		// Voxel space into world space.
		public Vector3 VoxelToWorld( Vector3 pos )
		{
			return new Vector3( pos.x * Voxel.SizeX, pos.y * Voxel.SizeY, pos.z * Voxel.SizeZ );
		}

		Vector3 VoxelToChunk( Vector3 pos )
		{
			int x = MathX.FloorToInt( pos.x ) / ChunkData.SizeX;
			int y = MathX.FloorToInt( pos.y ) / ChunkData.SizeY;
			int z = MathX.FloorToInt( pos.z ) / ChunkData.SizeZ;

			return new Vector3( x, y, z );
		}

		Vector3 VoxelToChunkLocal( Vector3 voxelPos )
		{
			int x = (int)voxelPos.x % ChunkData.SizeX;
			if ( x < 0 )
				x = Math.Abs( x );

			int y = (int)voxelPos.y % ChunkData.SizeY;
			if ( y < 0 )
				y = Math.Abs( y );

			int z = (int)voxelPos.z % ChunkData.SizeZ;
			if ( z < 0 )
				z = Math.Abs( z );

			return new Vector3( x, y, z );
		}

		Chunk GetOrCreateChunk( Vector3 chunkPos )
		{
			Chunk chunk = null;
			if ( Chunks.TryGetValue( chunkPos, out chunk ) )
			{
				return chunk;
			}

			chunk = Create<Chunk>();
			chunk.Position = new Vector3( chunkPos.x * ChunkData.SizeX, chunkPos.y * ChunkData.SizeY,
				chunkPos.z * ChunkData.SizeZ );

			Chunks.Add( chunkPos, chunk );

			return chunk;
		}

		Chunk GetChunk( Vector3 chunkPos )
		{
			Chunk chunk = null;
			if ( Chunks.TryGetValue( chunkPos, out chunk ) )
			{
				return chunk;
			}
			return null;
		}

		Chunk GetChunk( int x, int y, int z )
		{
			return GetChunk( new Vector3( x, y, z ) );
		}

		public Voxel Get( Vector3 pos )
		{
			return Get( MathX.FloorToInt( pos.x ), MathX.FloorToInt( pos.y ), MathX.FloorToInt( pos.z ) );
		}

		public Voxel Get( int x, int y, int z )
		{
			var vec = new Vector3( x, y, z );
			var chunkPos = VoxelToChunk( vec );

			var chunk = GetChunk( chunkPos );
			if ( chunk == null )
				return new Voxel();

			var chunkLocal = VoxelToChunkLocal( vec );

			return chunk.Get( chunkLocal );
		}

		public void Set( Vector3 pos, Voxel voxel )
		{
			Set( MathX.FloorToInt( pos.x ), MathX.FloorToInt( pos.y ), MathX.FloorToInt( pos.z ), voxel );
		}

		public void Set( int x, int y, int z, Voxel voxel )
		{
			//Log.Info( $"Set Voxel ({x},{y},{z})" );

			var vec = new Vector3( x, y, z );
			var chunkPos = VoxelToChunk( vec );

			var chunk = GetOrCreateChunk( chunkPos );
			var chunkLocal = VoxelToChunkLocal( vec );

			chunk.Set( (int)chunkLocal.x, (int)chunkLocal.y, (int)chunkLocal.z, voxel );
		}

		public void Clear()
		{
			foreach ( var kv in Chunks )
			{
				kv.Value.Delete();
			}
			Chunks.Clear();
			Spawns.Clear();
		}

		public void AddSpawn( Vector3 pos )
		{
			Spawns.Add( pos );
		}

		public List<Vector3> GetSpawns()
		{
			return Spawns;
		}
	}
}
