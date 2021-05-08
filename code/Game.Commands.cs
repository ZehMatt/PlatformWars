using Sandbox;

namespace PlatformWars
{
	partial class Game
	{
		[ServerCmd( "pos", Help = "Returns some information about the player position" )]
		public static void PosCommand()
		{
			var target = ConsoleSystem.Caller;
			if ( target == null )
				return;
			Log.Info( $"Pos: {target.WorldPos}" );
			Log.Info( $"Ang: {target.WorldAng}" );
			Log.Info( $"Vel: {target.Velocity}" );
		}

		[ServerCmd( "terrain_gen", Help = "Generate Terrain" )]
		public static void TerrainGen()
		{
			var terrain = Terrain.Manager.Get();
			if ( terrain == null )
			{
				Log.Error( "Terrain Manager does not exist" );
				return;
			}

			terrain.Clear();

			Vector3 pos = new Vector3( -(Terrain.Manager.Width / 2), -(Terrain.Manager.Length / 2), 16 );

			Terrain.Generator gen = new( terrain );
			gen.Generate( pos, Terrain.Manager.Width, Terrain.Manager.Length, Terrain.Manager.MaxHeight );
		}

	}
}
