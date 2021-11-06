using Sandbox;

namespace PlatformWars
{
	partial class RoundManager
	{
		World.Generator TerrainGen;

		// Public for command.
		public void HandleTerrainGeneration()
		{
			if ( !IsAuthority )
				return;

			var terrain = World.Manager.Get();
			if ( terrain == null )
			{
				Log.Error( "Terrain Manager does not exist" );
				return;
			}

			if ( TerrainGen == null )
			{
				TerrainGen = new( terrain );

				int w = World.Manager.Width;
				int l = World.Manager.Length;
				int h = World.Manager.MaxHeight;

				Vector3 pos = new Vector3( -(w / 2), -(l / 2), 16 );
				TerrainGen.Generate( pos, w, l, h, WorldSeed );
			}

			if ( TerrainGen.IsGenerating )
				return;

			SetState( RoundState.Setup );
		}
	}
}
