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
			Inventory = new Inventory( this );
		}

		public override void BuildInput( InputBuilder input )
		{
			var roundMgr = RoundManager.Get();
			if ( roundMgr != null && !roundMgr.CanPawnMove( this ) )
			{
				input.Clear();
				input.ClearButtons();
			}
			else
			{
				base.BuildInput( input );
			}
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

			Controller = new WalkController();
			Animator = new StandardPlayerAnimator();

			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;
			Position = pos;

			LifeState = LifeState.Alive;
			Velocity = Vector3.Zero;

			CreateHull();
			ResetInterpolation();
			Dress();
		}

		DamageInfo LastDamage;

		public override void TakeDamage( DamageInfo info )
		{
			base.TakeDamage( info );

			LastDamage = info;
		}

		public float GetDeathTime()
		{
			if ( LifeState == LifeState.Alive )
				return -1.0f;

			return DeathTime;
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
