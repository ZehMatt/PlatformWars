using Sandbox;
using System.Collections.Generic;

namespace PlatformWars
{
	enum Team
	{
		Spectator = 0,
		Red,
		Blue,
		Green,
		Yellow,
		Magenta,
	}

	class TemporaryClient : Sandbox.Client
	{
		public IReadOnlyList<Client> All { get => Sandbox.Client.All; }
		public ulong SteamId { get { return 0; } }
		public string Name { get { return ""; } }
		public int NetworkIdent { get => 0; }
		public ICamera Camera { get => null; set { } }
		public ICamera DevCamera { get => null; set { } }
		public PvsConfig Pvs { get => null; }
		public Entity Pawn { get; set; }

		public T GetScore<T>( string key, T defaultValue = default ) { return defaultValue; }
		public string GetUserString( string key, string defaultValue = null ) { return defaultValue; }
		public bool HasPermission( string v ) { return false; }
		public void SendCommandToClient( string command ) { }
		public void SetScore( string key, object value ) { }
	}

	partial class Player : Entity
	{
		static readonly Color32[] TeamColors = { Color32.Transparent, Color32.Red, Color32.Green, Color32.Cyan, Color32.Yellow, Color32.Magenta };

		[Net]
		Entity _client { get; set; }

		public Client Client { get => _client as Client; }

		[Net]
		public List<Pawn> Pawns { get; set; }

		[Net]
		Team CurrentTeam { get; set; }

		public string Name { get => Client.Name; }

		public ulong SteamId { get => Client.SteamId; }

		Stack<Cameras.Mode> CameraStack = new();

		public Player( Client cl )
		{
			Transmit = TransmitType.Always;

			Host.AssertServer();

			_client = cl as Entity;

			Spectate();
		}

		public Player()
		{
		}

		public Pawn GetControlledPawn()
		{
			return Client.Pawn as Pawn;
		}

		void ResetPawns()
		{
			Host.AssertServer();

			for ( int i = 0; i < Pawns.Count; i++ )
			{
				var pawn = Pawns[i];
				if ( pawn == null )
					continue;

				pawn.Delete();
			}

			Pawns.Clear();
			Client.Pawn = null;
		}

		public void SetupPawns( int count )
		{
			Host.AssertServer();

			ResetPawns();

			for ( int i = 0; i < count; i++ )
			{
				var pawn = new Pawn();
				pawn.AssignPlayer( this );
				pawn.RenderColor = TeamColors[(int)CurrentTeam % TeamColors.Length];

				Pawns.Add( pawn );
			}
		}

		public void RemovePawn( Pawn pawn )
		{
			for ( int i = 0; i < Pawns.Count; i++ )
			{
				var ent = Pawns[i];
				if ( ent == pawn )
				{
					Pawns.RemoveAt( i );
					break;
				}
			}
		}

		public List<Pawn> GetPawns()
		{
			var res = new List<Pawn>();
			for ( int i = 0; i < Pawns.Count; i++ )
			{
				var pawn = Pawns[i];
				if ( pawn == null )
					continue;

				res.Add( pawn as Pawn );
			}
			return res;
		}

		public void Spectate()
		{
			Host.AssertServer();

			SetCameraMode( Cameras.Mode.Spectate );
		}

		public Pawn GetPawn( int index )
		{
			var pawn = Pawns[index];

			if ( pawn == null )
				return null;

			return pawn;
		}

		public void RemoveControlled()
		{
			var pawn = Client.Pawn;
			if ( pawn != null && pawn.IsValid() )
			{
				var wep = pawn.ActiveChild;
				if ( wep != null && wep.IsValid() )
				{
					wep.Parent = null;
					//wep.OnCarryDrop()
				}
				pawn.ActiveChild = null;
			}

			Client.Pawn = null;
			SetCameraMode( Cameras.Mode.Spectate );
		}

		public void ControllPawn( Pawn pawn )
		{
			Client.Pawn = pawn;
			SetCameraMode( Cameras.Mode.FPS );
			pawn.ActiveChild = null;
		}

		public void SetTeam( Team team )
		{
			Host.AssertServer();
			CurrentTeam = team;
		}

		public void PushCameraMode( Cameras.Mode newMode )
		{
			var current = Client.Camera as Cameras.Base;
			Assert.NotNull( current );

			CameraStack.Push( current.Mode );

			SetCameraMode( newMode );
		}

		public void PopCameraMode()
		{
			Assert.True( CameraStack.Count > 0 );

			var oldMode = CameraStack.Pop();
			SetCameraMode( oldMode );
		}

		void SetCamera( Camera cam )
		{
			Client.Camera = cam;
		}

		public void SetCameraMode( Cameras.Mode mode )
		{
			switch ( mode )
			{
				case Cameras.Mode.Spectate:
					SetCamera( new Cameras.Spectate() );
					break;
				case Cameras.Mode.PawnDeath:
					SetCamera( new Cameras.PawnDeathCam() );
					break;
				case Cameras.Mode.FPS:
					SetCamera( new Cameras.FPS() );
					break;
				default:
					Assert.True( false );
					break;
			}
		}

		[Event( "server.tick" )]
		void ServerTick()
		{
			var curPawn = GetControlledPawn();
			foreach ( var pawn in GetPawns() )
			{
				if ( pawn != curPawn )
				{
					// Ugly work around to keep simulating forces with no input.
					var tempCl = new TemporaryClient() { Pawn = pawn };
					pawn.Simulate( null );
				}
			}
		}
	}
}
