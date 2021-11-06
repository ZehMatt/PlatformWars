using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlatformWars;

namespace PlatformWars.Weapons
{
	partial class Projectile : Entity
	{
		private SceneObject ModelObject;

		public virtual string ProjectileModel { get; } = null;

		// Meters in seconds.
		public virtual float LaunchSpeed { get; } = 0.0f;

		// Kilogram.
		public virtual float Mass { get; } = 0.0f;

		public virtual float ImpactDamage { get; set; } = 0.0f;

		public Vector3 CurrentVelocity = Vector3.Zero;

		private bool HitObject = false;

		private const float UpdateRate = 1.0f / 66.0f;
		private float Accumulator = 0.0f;

		private World.Manager World;

		private TimeSince LifeTime;

		public override void Spawn()
		{
			base.Spawn();

			Log.Info( $"[{(IsClient ? "CL" : "SV")}] Projectile spawn [{this.NetworkIdent}]{this}" );

			Transmit = TransmitType.Always;
			LifeState = LifeState.Alive;
			LifeTime = 0;

			World = PlatformWars.World.Manager.Get();
			Assert.NotNull( World );
		}

		public override void ClientSpawn()
		{
			base.ClientSpawn();

			Log.Info( $"[{(IsClient ? "CL" : "SV")}] Projectile spawn [{this.NetworkIdent}]{this}" );

			ModelObject = new SceneObject( Model.Load( ProjectileModel ), Transform );

			World = PlatformWars.World.Manager.Get();
			Assert.NotNull( World );
		}

		public void LaunchTowards( Vector3 pos, Rotation rot )
		{
			Position = pos;
			Rotation = rot;

			CurrentVelocity = rot.Forward * LaunchSpeed;
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			if ( ModelObject != null )
			{
				ModelObject.Delete();
				ModelObject = null;
			}
		}

		public static TraceResult RunTrace( Entity owner, Entity self, Vector3 start, Vector3 end )
		{
			bool InWater = Physics.TestPointContents( start, CollisionLayer.Water );

			var tr = Trace.Ray( start, end )
					.UseHitboxes()
					.HitLayer( CollisionLayer.Water, !InWater )
					.Ignore( owner )
					.Ignore( self )
					.Size( 1.0f )
					.Run();

			return tr;
		}

		[Event.Tick]
		private void Update()
		{
			if ( HitObject )
				return;

			Accumulator += Time.Delta;

			while ( Accumulator >= UpdateRate )
			{
				TickStep( UpdateRate );

				if ( HitObject )
					break;

				Accumulator -= UpdateRate;
			}
		}

		private bool IsValidPosition( Vector3 pos )
		{
			if ( pos.x < -32766 || pos.y < -32766 || pos.z < -32766 )
				return false;
			if ( pos.x > 32766 || pos.y > 32766 || pos.z > 32766 )
				return false;
			return true;
		}

		private void TickStep( float dt )
		{
			var currentPos = Position;

			var gravity = (PhysicsWorld.Gravity * 0.2f) * dt;

			// Apply gravity
			CurrentVelocity += gravity;

			// Apply air friction.
			//CurrentVelocity *= 0.995f; // Yeah this is not accurate at all.
			CurrentVelocity += ((World.GetWindForce() * 10.0f) * dt);

			var speedDelta = CurrentVelocity * dt;
			var nextPos = currentPos + speedDelta;

			if ( !IsValidPosition( nextPos ) )
			{
				if ( IsServer )
				{
					Log.Warning( $"Bullet out of bounds {this}, deleting" );
					Delete();
				}
				return;
			}

			DebugOverlay.Line( currentPos, nextPos, Color.Red, 5.0f );

			var tr = RunTrace( Owner, this, currentPos, nextPos );
			if ( tr.Hit )
			{
				Log.Info( $"[{(IsClient ? "CL" : "SV")}] Projectile [{this.NetworkIdent}]{this} impact with {tr.Entity}" );

				if ( IsServer )
				{
					DeleteAsync( 0.01f );
				}

				EnableDrawing = false;
				HitObject = true;

				Position = tr.EndPos;

				OnImpact( tr, CurrentVelocity );
			}
			else
			{
				Position = nextPos;
			}
		}

		[Event.Frame]
		private void FrameUpdate()
		{
			if ( ModelObject == null )
				return;

			var dst = Transform;
			var src = ModelObject.Transform;

			var speedDelta = 2000.0f * Time.Delta;
			var p = MoveTowards( src.Position, dst.Position, speedDelta );

			src.Position = p;

			ModelObject.Transform = src;
		}

		protected virtual void OnImpact( TraceResult tr, Vector3 velocity )
		{
			var ent = tr.Entity;
			if ( ent == null || !ent.IsValid() )
				return;

			if ( IsServer )
			{
				var dmg = DamageInfo.FromBullet( tr.EndPos, velocity * 10.0f, ImpactDamage )
					.WithAttacker( Owner )
					.UsingTraceResult( tr )
					.WithWeapon( Owner.ActiveChild )
					;

				ent.TakeDamage( dmg );
			}

		}

		public static Vector3 MoveTowards( Vector3 current, Vector3 target, float maxDistanceDelta )
		{
			if ( maxDistanceDelta > Vector3.DistanceBetween( current, target ) )
			{
				return target;
			}
			else
			{
				Vector3 delta = (target - current);
				return current + (delta.Normal * maxDistanceDelta);
			}
		}

	}
}
