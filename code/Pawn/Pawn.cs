using Sandbox;

namespace PlatformWars
{
	partial class Pawn : Sandbox.Player
	{
		[Net]
		Player PlayerOwner { get; set; }

		[Net]
		ModelEntity Ragdoll { get; set; }

		public const int MaxHealth = 250;

		RealTimeSince DeathTime;

		public Pawn()
		{
			Transmit = TransmitType.Always;
		}

		public override void Spawn()
		{
			LifeState = LifeState.Alive;
			Health = MaxHealth;
		}

		public void AssignPlayer( Player ply )
		{
			PlayerOwner = ply;
		}

		public Player GetPlayer()
		{
			return PlayerOwner;
		}

		public void Reset( Vector3 pos )
		{
			Host.AssertServer();

			SetModel( "models/citizen/citizen.vmdl" );

			Controller = new PawnController();
			Animator = new PawnAnimator();

			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;
			Position = pos;
			Tags.Add( "player" );

			LifeState = LifeState.Alive;
			Velocity = Vector3.Zero;

			CreateHull();
			ResetInterpolation();
			Dress();
		}

		DamageInfo LastDamage;

		public override void TakeDamage( DamageInfo info )
		{
			if ( LifeState != LifeState.Alive )
				return;

			base.TakeDamage( info );

			LastDamage = info;

			UI.Hud.Get().DisplayDamageValue( To.Everyone, this, info.Damage );
		}

		public float GetDeathTime()
		{
			if ( LifeState == LifeState.Alive )
				return -1.0f;

			return DeathTime;
		}

		public void SwitchToWeapon( Entity weapon )
		{
			Log.Info( "Requested weapon change" );

			weapon.Parent = this;
			ActiveChild = weapon;

			weapon.OnCarryStart( this );
		}

		/// <summary>
		/// Called every tick to simulate the player. This is called on the
		/// client as well as the server (for prediction). So be careful!
		/// </summary>
		public override void Simulate( Client cl )
		{
			if ( LifeState == LifeState.Dead )
			{
				return;
			}

			if ( Input.ActiveChild != null )
			{
				SwitchToWeapon( Input.ActiveChild );
			}

			if ( Input.Pressed( InputButton.View ) )
			{
				Log.Info( "Change camera" );

				if ( cl.Camera is Cameras.FPS )
					cl.Camera = new Cameras.TPS();
				else
					cl.Camera = new Cameras.FPS();
			}

			//UpdatePhysicsHull();

			var controller = GetActiveController();
			controller?.Simulate( cl, this, GetActiveAnimator() );

			SimulateActiveChild( cl, ActiveChild );
		}

		public override void FrameSimulate( Client cl )
		{
			base.FrameSimulate( cl );

			var controller = GetActiveController();
			controller?.FrameSimulate( cl, this, GetActiveAnimator() );
		}

		public override void OnKilled()
		{
			if ( LifeState == LifeState.Dead )
				return;

			LifeState = LifeState.Dead;

			Log.Info( "Pawn got killed" );

			var ragdoll = CreateRagdoll( LastDamage.Force, GetHitboxBone( LastDamage.HitboxIndex ) );

			ClearCollisionLayers();
			EnableDrawing = false;

			Ragdoll = ragdoll;
			DeathTime = 0;

			// NOTE: This pawn/entity will keep existing until the round manager decides otherwise.
			var roundMgr = RoundManager.Get();
			if ( roundMgr != null )
			{
				roundMgr?.OnPawnKilled( this );
			}

			base.OnKilled();
		}

		public ModelEntity GetRagdoll()
		{
			return Ragdoll;
		}

		public override void OnAnimEventFootstep( Vector3 pos, int foot, float volume )
		{
			if ( LifeState != LifeState.Alive )
				return;

			if ( !IsClient )
				return;

			DebugOverlay.Box( 1, pos, -1, 1, Color.Red );
			DebugOverlay.Text( pos, $"{volume}", Color.White, 5 );

			var tr = Trace.Ray( pos, pos + Vector3.Down * 7 )
				.Radius( 1 )
				.Ignore( this )
				.Run();

			if ( !tr.Hit ) return;

			tr.Surface.DoFootstep( this, tr, foot, volume );
		}


	}
}
