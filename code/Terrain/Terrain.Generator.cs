using Sandbox;

namespace PlatformWars.Terrain
{
	class Generator
	{
		Manager Manager;
		bool Generating = false;

		public Generator( Manager manager )
		{
			Manager = manager;
		}

		public bool IsGenerating { get => Generating; }

		void CreatePlatform( Vector3 pos )
		{
			var crate = Manager.Create<PlatformWars.Entities.Platform>();
			crate.Position = pos;

			var phys = crate.PhysicsBody;
			if ( phys != null )
			{
				phys.MotionEnabled = false;
			}
		}

		public async void Generate( Vector3 pos, int w, int l, int h, int seed = 1 )
		{
			if ( Generating )
				return;

			Generating = true;

			var iter = 0;

			var prng = new System.Random( seed );

			float offsetX = (float)prng.NextDouble() * l;
			float offsetY = (float)prng.NextDouble() * l;

			foreach ( var ent in Entity.All )
			{
				if ( ent.ClassInfo.Name != "ent_platform" )
					continue;
				ent.Delete();
			}

			// Ground layer.
			iter = 0;
			float resolution = 5.0f;
			for ( int x = 0; x < w; x++ )
			{
				for ( int y = 0; y < l; y++ )
				{
					float z = h * 1.5f; // Fixed plane.
					float x1 = (float)x / (float)w;
					float y1 = (float)y / (float)l;
					float z1 = (float)z;

					float value1 = Noise.Perlin( offsetX + (x1 * resolution), offsetY + (y1 * resolution), z * resolution );
					float value2 = Noise.Fractal( x, offsetX + (x1 * resolution), offsetY + (y1 * resolution), z * resolution );
					float value = value1;
					//float value = value1;

					var actualZ = MathX.FloorToInt( value * h );
					//Log.Info( $"Value ({x}, {y}, {actualZ}): {value}" );

					int voxelX = x;
					int voxelY = y;

					if ( actualZ != 0 )
						continue;

					var voxelPos = pos + new Vector3( voxelX, voxelY, 0 );
					Manager.Set( voxelPos, Voxel.Solid );

					Manager.AddSpawn( voxelPos );

					if ( ++iter % 100 == 0 )
						await Manager.Task.Delay( 1 );
				}
			}

			// Second layer is walls/obstacles.
			iter = 0;
			resolution = 3.0f;

			offsetX = (float)prng.NextDouble() * l;
			offsetY = (float)prng.NextDouble() * l;

			for ( int x = 0; x < w; x++ )
			{
				for ( int y = 0; y < l; y++ )
				{
					float z = h * 0.5f; // Fixed plane.
					float x1 = (float)x / (float)w;
					float y1 = (float)y / (float)l;
					float z1 = (float)z;

					float value1 = Noise.Perlin( offsetX + (x1 * resolution), offsetY + (y1 * resolution), z * resolution );
					float value2 = Noise.Fractal( x, offsetX + (x1 * resolution), offsetY + (y1 * resolution), z * resolution );
					float value = value1;
					//float value = value1;

					var actualZ = MathX.FloorToInt( value * h );
					//Log.Info( $"Value ({x}, {y}, {actualZ}): {value}" );

					int voxelX = x;
					int voxelY = y;

					if ( actualZ < 1 )
						continue;

					var voxelPos = pos + new Vector3( voxelX, voxelY, 0 );
					var ground = Manager.Get( voxelPos );
					if ( ground.Type != TerrainType.Air )
						continue;

					for ( int i = 1; i < actualZ; i++ )
					{
						Manager.Set( pos + new Vector3( voxelX, voxelY, i ), Voxel.Solid );
					}

					if ( ++iter % 100 == 0 )
						await Manager.Task.Delay( 1 );
				}
			}

			// Ceilings/Floating platforms.
			iter = 0;
			resolution = 3.0f;

			offsetX = (float)prng.NextDouble() * l;
			offsetY = (float)prng.NextDouble() * l;

			for ( int x = 0; x < w; x++ )
			{
				for ( int y = 0; y < l; y++ )
				{
					float z = h; // Fixed plane.
					float x1 = (float)x / (float)w;
					float y1 = (float)y / (float)l;
					float z1 = (float)z;

					float value1 = Noise.Perlin( offsetX + (x1 * resolution), offsetY + (y1 * resolution), z );
					float value2 = Noise.Fractal( x, offsetX + (x1 * resolution), offsetY + (y1 * resolution), z * resolution );
					float value = value1;
					//float value = value1;

					var actualZ = MathX.FloorToInt( value + 0.5f ) * h;
					//Log.Info( $"Value ({x}, {y}, {actualZ}): {value}" );

					int voxelX = x;
					int voxelY = y;

					if ( actualZ != h )
						continue;

					Manager.Set( pos + new Vector3( voxelX, voxelY, h ), Voxel.Solid );

					if ( ++iter % 100 == 0 )
						await Manager.Task.Delay( 1 );
				}
			}

			Log.Info( "Generating Platforms" );
			iter = 0;
			for ( int z = 0; z < h; z++ )
			{
				for ( int x = 0; x < w; x++ )
				{
					for ( int y = 0; y < l; y++ )
					{
						int x1 = x;
						int y1 = y;
						int z1 = z;

						var voxelPos = pos + new Vector3( x1, y1, z1 );
						var voxel = Manager.Get( voxelPos );
						if ( voxel.Type == TerrainType.Air )
						{
							continue;
						}

						var platformPos = new Vector3( voxelPos.x * Voxel.SizeX, voxelPos.y * Voxel.SizeY, voxelPos.z * Voxel.SizeZ );

						//Log.Info( $"Generating platform at {platformPos}" );

						CreatePlatform( platformPos );

						if ( ++iter % 50 == 0 )
							await Manager.Task.Delay( 1 );
					}
				}
			}

			Generating = false;
		}
	}
}
