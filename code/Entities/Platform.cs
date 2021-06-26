using Sandbox;
using System;
using System.Threading.Tasks;

namespace PlatformWars.Entities
{

	[Library( "ent_platform", Title = "Procedural Platform", Spawnable = false )]
	public partial class Platform : BasePhysics
	{
		public const int BlockSize = 35;
		public const int MaxHealth = 100;
		private DamageInfo LastDamageInfo;
		private CollisionEventData LastCollisionInfo;
		int explosionCasualties = 0;

		[ServerVar]
		public static bool debug_prop_explosion { get; set; } = false;

		[Property]
		protected int CollisionGroupOverride { get; set; }

		public override void Spawn()
		{
			base.Spawn();

			Host.AssertServer();

			SetModel( "models/citizen_props/crate01.vmdl" );
			//Health = MaxHealth;
			EnableShadowCasting = false;
			EnableShadowReceive = true;

			if ( PhysicsBody != null )
			{
				PhysicsBody.MotionEnabled = false;
			}

			MoveType = MoveType.Physics;
			CollisionGroup = CollisionGroup.Interactive;
			PhysicsEnabled = true;
			UsePhysicsCollision = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;
			Health = MaxHealth;

			RemoveCollisionLayer( CollisionLayer.Trigger );
		}

		public override void OnNewModel( Model model )
		{
			base.OnNewModel( model );

			if ( IsServer )
			{
				UpdatePropData( model );
			}
		}

		protected virtual void UpdatePropData( Model model )
		{
			Host.AssertServer();
		}

		public override void OnKilled()
		{
			if ( LifeState != LifeState.Alive )
				return;

			UpdateColor();

			if ( LastDamageInfo.Flags.HasFlag( DamageFlags.PhysicsImpact ) )
			{
				Velocity = LastCollisionInfo.PreVelocity;
			}

			Breakables.Result pieces = new();
			Breakables.Break( this, pieces );

			// Make pieces also float in the water.
			foreach ( var prop in pieces.Props )
			{
				prop.SetInteractsWith( CollisionLayer.Trigger );
			}

			if ( HasExplosionBehavior() )
			{
				if ( LastDamageInfo.Flags.HasFlag( DamageFlags.Blast ) && LastDamageInfo.Attacker is Platform prop )
				{
					prop.explosionCasualties++;
					_ = ExplodeAsync( 0.1f * prop.explosionCasualties );

					return;
				}
				else
				{
					OnExplosion();
				}
			}

			base.OnKilled();
		}

		protected override void OnPhysicsCollision( CollisionEventData eventData )
		{
			LastCollisionInfo = eventData;
			base.OnPhysicsCollision( eventData );
		}

		public async Task ExplodeAsync( float fTime )
		{
			if ( LifeState != LifeState.Alive )
				return;

			LifeState = LifeState.Dead;

			await Task.DelaySeconds( fTime );
			OnExplosion();

			Delete();
		}

		private bool HasExplosionBehavior()
		{
			var model = GetModel();
			if ( model == null || model.IsError )
				return false;

			return model.HasExplosionBehavior();
		}

		private void OnExplosion()
		{
			var model = GetModel();
			if ( model == null || model.IsError )
				return;

			if ( !PhysicsBody.IsValid() )
				return;

			if ( !model.HasExplosionBehavior() )
				return;

			var explosionBehavior = model.GetExplosionBehavior();

			if ( !string.IsNullOrWhiteSpace( explosionBehavior.Sound ) )
			{
				Sound.FromWorld( explosionBehavior.Sound, PhysicsBody.MassCenter );
			}

			if ( !string.IsNullOrWhiteSpace( explosionBehavior.Effect ) )
			{
				Particles.Create( explosionBehavior.Effect, PhysicsBody.MassCenter );
			}

			if ( explosionBehavior.Radius > 0.0f )
			{
				var sourcePos = PhysicsBody.MassCenter;
				var overlaps = Physics.GetEntitiesInSphere( sourcePos, explosionBehavior.Radius );

				if ( debug_prop_explosion )
					DebugOverlay.Sphere( sourcePos, explosionBehavior.Radius, Color.Orange, true, 5 );

				foreach ( var overlap in overlaps )
				{
					if ( overlap is not ModelEntity ent || !ent.IsValid() )
						continue;

					if ( ent.LifeState != LifeState.Alive )
						continue;

					if ( !ent.PhysicsBody.IsValid() )
						continue;

					if ( ent.IsWorld )
						continue;

					var targetPos = ent.PhysicsBody.MassCenter;

					var dist = Vector3.DistanceBetween( sourcePos, targetPos );
					if ( dist > explosionBehavior.Radius )
						continue;

					var tr = Trace.Ray( sourcePos, targetPos )
						.Ignore( this )
						.WorldOnly()
						.Run();

					if ( tr.Fraction < 1.0f )
					{
						if ( debug_prop_explosion )
							DebugOverlay.Line( sourcePos, tr.EndPos, Color.Red, 5, true );

						continue;
					}

					if ( debug_prop_explosion )
						DebugOverlay.Line( sourcePos, targetPos, 5, true );

					var distanceMul = 1.0f - Math.Clamp( dist / explosionBehavior.Radius, 0.0f, 1.0f );
					var damage = explosionBehavior.Damage * distanceMul;
					var force = (explosionBehavior.Force * distanceMul) * ent.PhysicsBody.Mass;
					var forceDir = (targetPos - sourcePos).Normal;

					ent.TakeDamage( DamageInfo.Explosion( sourcePos, forceDir * force, damage )
						.WithAttacker( this ) );
				}
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
		}

		void UpdateColor()
		{
			float curHealth = Health;
			float health = curHealth / (float)MaxHealth;
			float r = (1.0f - health);
			float g = health - 1.0f;
			float b = 1.0f - (r + g);

			if ( IsClient )
			{
				Log.Info( $"Client Health: {Health}" );
			}

			RenderColor = new Color32( (byte)(r * 255), (byte)(g * 255), (byte)(b * 255) );
		}

		public override void TakeDamage( DamageInfo info )
		{
			LastDamageInfo = info;

			base.TakeDamage( info );

			if ( Health < 0 )
			{
				Log.Info( "Should be dead now" );
			}

			UpdateColor();
		}
	}

}
