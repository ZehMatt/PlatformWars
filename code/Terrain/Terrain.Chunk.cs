using Sandbox;
using System.Collections.Generic;

namespace PlatformWars.Terrain
{
	public struct Voxel
	{
		public const int SizeX = 35;
		public const int SizeY = 35;
		public const int SizeZ = 35;
		public static readonly Vector3 Size = new Vector3( SizeX, SizeY, SizeZ );

		public static readonly Voxel Air = new Voxel();
		public static readonly Voxel Solid = new Voxel( TerrainType.Solid );

		// [ Bit 13 to 16 is type ] [ Bit 0 to 13 is health ]
		public ushort Raw;

		public Voxel( Entity ent )
		{
			Raw = 0;
		}

		public Voxel( ushort val = 0 )
		{
			Raw = val;
		}

		public Voxel( TerrainType type )
		{
			Raw = 0;
			Type = type;
		}

		public TerrainType Type
		{
			get
			{
				return (TerrainType)((Raw & 0b1110000000000000) >> 13);
			}
			set
			{
				Raw &= 0b0001111111111110;
				Raw |= (ushort)((int)value << 13);
			}
		}

		public int Health
		{
			get
			{
				return (Raw & 0b0001111111111110);
			}
			set
			{
				Raw &= 0b1110000000000000;
				Raw |= (ushort)value;
			}
		}
	}

	public class ChunkData : NetworkClass
	{
		public const int SizeX = 32;
		public const int SizeY = 32;
		public const int SizeZ = 8;

		public List<Voxel> Data = new();

		int ToIndex( int x, int y, int z )
		{
			return (((SizeX * SizeY) * z) + (SizeX * y)) + x;
		}

		public Voxel Get( int x, int y, int z )
		{
			int index = ToIndex( x, y, z );
			var val = Data[index];
			return val;
		}

		public void Set( int x, int y, int z, Voxel data )
		{
			int index = ToIndex( x, y, z );
			Data[index] = data;
		}

		public override bool NetWrite( NetWrite write )
		{
			base.NetWrite( write );

			return true;
		}

		public override bool NetRead( NetRead read )
		{
			base.NetRead( read );

			return true;
		}
	}

	partial class Chunk : AnimEntity
	{
		[Net]
		ChunkData Data { get; set; } = new();

		public override void Spawn()
		{
			for ( int z = 0; z < ChunkData.SizeZ; z++ )
			{
				for ( int x = 0; x < ChunkData.SizeX; x++ )
				{
					for ( int y = 0; y < ChunkData.SizeY; y++ )
					{
						Data.Data.Add( Voxel.Air );
					}
				}
			}
		}

		public Voxel Get( Vector3 chunkLocal )
		{
			return Get( (int)chunkLocal.x, (int)chunkLocal.y, (int)chunkLocal.z );
		}

		public Voxel Get( int x, int y, int z )
		{
			return Data.Get( x, y, z );
		}

		public void Set( int x, int y, int z, Voxel data )
		{
			Data.Set( x, y, z, data );
		}
	}
}
