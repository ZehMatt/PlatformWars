using Sandbox;

namespace PlatformWars
{
	[Library( "ent_stateent" )]
	partial class StateEntity : Prop
	{
		[Net]
		public RoundManager RoundManager { get; set; }

		[Net]
		public Terrain.Manager TerrainManager { get; set; }

		[Net]
		public int IntegerVal { get; set; }

		[Net]
		public float FloatVal { get; set; }

		public override void Spawn()
		{
			base.Spawn();

			RoundManager = Create<RoundManager>();
			TerrainManager = Create<Terrain.Manager>();
			IntegerVal = 0xDEAD;
			FloatVal = 0.3515f;

			if ( RoundManager == null )
				Log.Warning( "SV: RoundManager != null" );
			if ( TerrainManager == null )
				Log.Warning( "SV: TerrainManager != null" );
			if ( IntegerVal != 0xDEAD )
				Log.Warning( "SV: IntegerVal != 0xDEAD" );
			if ( FloatVal != 0.3515f )
				Log.Warning( "SV: FloatVal != 0.3515f" );
		}

		public override void ClientSpawn()
		{
			base.Spawn();

			if ( RoundManager == null )
				Log.Warning( "CL: RoundManager != null" );
			if ( TerrainManager == null )
				Log.Warning( "CL: TerrainManager != null" );
			if ( IntegerVal != 0xDEAD )
				Log.Warning( "CL: IntegerVal != 0xDEAD" );
			if ( FloatVal != 0.3515f )
				Log.Warning( "CL: FloatVal != 0.3515f" );
		}
	}

}
