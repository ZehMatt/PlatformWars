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
			Animator = new StandardPlayerAnimator();

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

	}
}
