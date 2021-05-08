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

	partial class Player : BasePlayer
	{
		static readonly Color32[] TeamColors = { Color32.Transparent, Color32.Red, Color32.Green, Color32.Cyan, Color32.Yellow, Color32.Magenta };

		[Net]
		public Network.NetList<EntityHandle<Pawn>> Pawns { get; set; } = new();

		[NetPredicted]
		EntityHandle<Pawn> ControlledPawn { get; set; }

		[Net]
		Team CurrentTeam { get; set; }

		public override void Spawn()
		{
			Controller = new PlayerController();
			Animator = new StandardPlayerAnimator();
			Camera = new Cameras.Spectate();
			Inventory = new BaseInventory( this );

			if ( IsServer )
			{
				CurrentTeam = Team.Spectator;
				EnableDrawing = false;
				CollisionGroup = CollisionGroup.Never;
				EnableViewmodelRendering = false;

				// Our player is not supposed to be an actual player.
				RemoveCollisionLayer( CollisionLayer.Trigger );
			}
		}

		public Pawn GetControlledPawn()
		{
			return (ControlledPawn.IsValid ? ControlledPawn.Entity : null) as Pawn;
		}

		void ResetPawns()
		{
			Host.AssertServer();

			for ( int i = 0; i < Pawns.Count; i++ )
			{
				var pawn = Pawns.Get( i ).Entity;

				if ( !pawn.IsValid() )
					continue;

				pawn.Delete();
			}

			Pawns.Clear();
			ControlledPawn = null;
		}

		public void SetupPawns( int count )
		{
			Host.AssertServer();

			ResetPawns();

			for ( int i = 0; i < count; i++ )
			{
				var pawn = Create<Pawn>();
				pawn.AssignPlayer( this );
				pawn.RenderColor = TeamColors[(int)CurrentTeam % TeamColors.Length];

				Pawns.Add( pawn );
			}
		}

		public void RemovePawn( Pawn pawn )
		{
			for ( int i = 0; i < Pawns.Count; i++ )
			{
				var ent = Pawns.Get( i );
				if ( ent.Entity == pawn )
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
				var pawn = Pawns.Get( i ).Entity;

				if ( !pawn.IsValid() )
					continue;

				res.Add( pawn as Pawn );
			}
			return res;
		}

		public override Camera GetActiveCamera()
		{
			return base.GetActiveCamera();
		}

		public void Spectate()
		{
			Camera = new Cameras.Spectate();
		}

		public Pawn GetPawn( int index )
		{
			var pawn = Pawns.Get( index );
			if ( pawn == null )
				return null;

			if ( !pawn.IsValid )
				return null;

			return pawn.Entity as Pawn;
		}

		public void RemoveControlled()
		{
			var pawn = GetControlledPawn();
			if ( pawn == null )
				return;

			if ( IsAuthority )
			{
				pawn.SetParent( null );
				pawn.CopyBonesFrom( this );
			}

			EnableDrawing = false;
			EnableViewmodelRendering = false;
			CollisionGroup = CollisionGroup.Never;

			ControlledPawn = null;
		}

		public void ControllPawn( Pawn pawn )
		{
			RemoveControlled();

			ControlledPawn = pawn;

			if ( IsAuthority )
			{
				WorldPos = pawn.WorldPos;
				WorldAng = pawn.WorldAng;

				EnableDrawing = true;
				Camera = new FirstPersonCamera();
				CollisionGroup = CollisionGroup.Weapon;
				EnableViewmodelRendering = true;

				RenderColor = pawn.RenderColor;

				pawn.SetParent( this, true );
				pawn.EnableDrawing = true;
				pawn.EnableAllCollisions = true;
			}
		}

		public void SetTeam( Team team )
		{
			Host.AssertServer();
			CurrentTeam = team;
		}

		public override void Respawn()
		{
			if ( CurrentTeam == Team.Spectator )
				return;

			SetModel( "models/citizen/citizen.vmdl" );

			EnableAllCollisions = false;
			EnableDrawing = false;
			EnableHideInFirstPerson = true;
			EnableViewmodelRendering = false;
			EnableShadowInFirstPerson = true;
			CollisionGroup = CollisionGroup.Weapon;
			EnableSolidCollisions = false;

			Inventory.Add( new Weapons.Pistol(), true );
			GiveAmmo( Weapons.AmmoType.Pistol, 100 );

			base.Respawn();
		}

		public override void OnKilled()
		{
		}

		protected override void Tick()
		{
			base.Tick();

			//
			// Input requested a weapon switch
			//
			if ( Input.ActiveChild != null )
			{
				ActiveChild = Input.ActiveChild;
			}

			if ( LifeState != LifeState.Alive )
				return;

			TickPlayerUse();

			if ( Input.Pressed( InputButton.View ) )
			{
				if ( Camera is ThirdPersonCamera )
				{
					Camera = new FirstPersonCamera();
				}
				else
				{
					Camera = new ThirdPersonCamera();
				}
			}
		}

		public override void StartTouch( Entity other )
		{
			base.StartTouch( other );
		}

		public override void PostCameraSetup( Camera camera )
		{
			base.PostCameraSetup( camera );
		}

		public override void TakeDamage( DamageInfo info )
		{
		}
	}
}
