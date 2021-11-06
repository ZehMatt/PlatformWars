using Sandbox;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PlatformWars
{
	[Library( "platformwars", Title = "PlatformWars" )]
	partial class Game : Sandbox.Game
	{
		[Net]
		public RoundManager RoundManager { get; set; }

		[Net]
		public World.Manager TerrainManager { get; set; }

		List<Player> Players = new();

		[Net]
		public UI.Hud PlayerHud { get; set; }

		public override void Spawn()
		{
			base.Spawn();

			PlayerHud = Create<UI.Hud>();
			RoundManager = Create<RoundManager>();
			TerrainManager = Create<World.Manager>();
		}

		public override void ClientSpawn()
		{
			base.Spawn();
		}

		public RoundManager GetRoundManager()
		{
			return RoundManager;
		}

		public World.Manager GetTerrainManager()
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

		public Player GetLocalPlayer()
		{
			var cl = Local.Client;
			foreach ( var ply in Players )
			{
				if ( ply.Client == cl )
					return ply;
			}
			return null;
		}

		public override void PostLevelLoaded()
		{
			base.PostLevelLoaded();
		}
	}

}
