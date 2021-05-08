using Sandbox;

namespace PlatformWars
{
	partial class RoundManager
	{
		Terrain.Generator TerrainGen;

		// Public for command.
		public void HandleTerrainGeneration()
		{
			if ( !IsAuthority )
				return;

			var terrain = Terrain.Manager.Get();
			if ( terrain == null )
			{
				Log.Error( "Terrain Manager does not exist" );
				return;
			}

			if ( TerrainGen == null )
			{
				TerrainGen = new( terrain );

				int w = Terrain.Manager.Width;
				int l = Terrain.Manager.Length;
				int h = Terrain.Manager.MaxHeight;

				Vector3 pos = new Vector3( -(w / 2), -(l / 2), 16 );
				TerrainGen.Generate( pos, w, l, h );
			}

			if ( TerrainGen.IsGenerating )
				return;

			SetState( RoundState.Setup );
		}
	}
}
