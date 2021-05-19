using Sandbox;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PlatformWars
{
	[Library( "platformwars", Title = "PlatformWars" )]
	partial class Game : Sandbox.Game
	{
		[Net]
		EntityHandle<RoundManager> RoundManager { get; set; }

		[Net]
		EntityHandle<Terrain.Manager> TerrainManager { get; set; }

		List<Player> Players = new();

		public Game()
		{
			if ( IsServer )
			{
				new PlatformWars.UI.Hud();
			}
		}

		public RoundManager GetRoundManager()
		{
			return RoundManager;
		}

		public Terrain.Manager GetTerrainManager()
		{
			return TerrainManager;
		}

		public override void ClientJoined( Client cl )
		{
			var player = new PlatformWars.Player( cl );
			Players.Add( player );

			// Uh, we should probably not do that?
			cl.Pawn = player;
		}

		public ReadOnlyCollection<Player> GetPlayers()
		{
			return Players.AsReadOnly();
		}

		public override void PostLevelLoaded()
		{
			base.PostLevelLoaded();

			RoundManager = Create<RoundManager>();
			TerrainManager = Create<Terrain.Manager>();
		}


	}

}
