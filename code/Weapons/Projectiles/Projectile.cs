using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlatformWars.Weapons
{
	partial class Projectile : AnimEntity
	{
		//private SceneObject ModelObject;

		public virtual string ProjectileModel { get; } = null;

		// Meters in seconds.
		public virtual float Speed { get; } = 0.0f;

		// Kilogram.
		public virtual float Mass { get; } = 0.0f;

		private bool HitObject = false;

		public override void Spawn()
		{
			base.Spawn();

			Log.Info( $"[{(IsClient ? "CL" : "SV")}] Projectile spawn [{this.NetworkIdent}]{this}" );

			Transmit = TransmitType.Always;
			LifeState = LifeState.Alive;

			SetModel( ProjectileModel );
		}

		public override void ClientSpawn()
		{
			base.ClientSpawn();

			Log.Info( $"[{(IsClient ? "CL" : "SV")}] Projectile spawn [{this.NetworkIdent}]{this}" );

			//ModelObject = new SceneObject( Model.Load( ProjectileModel ), Transform );
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			//if ( ModelObject != null )
			{
				//ModelObject.Delete();
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

			var delta = Time.Delta;

			var currentPos = Position;
			var direction = Rotation.Angles().Direction;
			var speedInch = Speed; // * 39.37f;
			var speed = speedInch * delta;
			var nextPos = currentPos + (direction * speed);

			Position = nextPos;

			var tr = RunTrace( Owner, this, currentPos, nextPos );
			if ( tr.Hit )
			{
				Log.Info( $"[{(IsClient ? "CL" : "SV")}] Projectile [{this.NetworkIdent}]{this} impact with {tr.Entity}" );

				if ( IsServer )
				{
					Delete();
				}

				EnableDrawing = false;
				HitObject = true;
			}
		}

		[Event.Frame]
		private void FrameUpdate()
		{
			/*
			if ( ModelObject == null )
				return;

			var dst = Transform;
			var src = ModelObject.Transform;

			ModelObject.Transform = Transform.Lerp( src, dst, Time.Delta * 20.0f, true );
			*/
		}


	}
}
