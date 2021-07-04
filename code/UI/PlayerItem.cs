using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace PlatformWars.UI
{
	partial class PlayerItem : Panel
	{
		Weapons.Base Ent;

		public Weapons.Base Entity { get => Ent; set => SetEntity( value ); }

		Scene ModelScene;
		Label Name;

		public PlayerItem( int index, Panel parent )
		{
			Parent = parent;
			Name = Add.Label( $"{index}" );
		}

		public void SetEntity( Entity ent )
		{
			if ( Entity == ent )
				return;

			Ent = ent as Weapons.Base;
			if ( ModelScene != null )
			{
				ModelScene.Delete();
				ModelScene = null;
			}

			CreateScene();
		}

		[Event.Hotload]
		public void OnRefreshScene()
		{
			Ent = null;
			//Log.Info( "Refresh scene" );
		}

		private void CreateScene()
		{
			if ( Entity == null )
				return;

			var transform = Entity.Transform;
			transform.Rotation = Rotation.Identity;

			var bbox = Entity.CollisionBounds * 0.8f;
			bbox.Mins = transform.PointToLocal( bbox.Mins );
			bbox.Maxs = transform.PointToLocal( bbox.Maxs );

			var center = (bbox.Maxs - bbox.Mins) * 0.5f;
			var size = bbox.Size;
			var len = size.Length;

			var fov = 75;
			var distance = Math.Max( MathF.Abs( size.x ), MathF.Abs( size.y ) ) * 0.5f / MathF.Tan( MathF.PI * fov / 360 );

			Vector3 camPos = -Vector3.Forward * distance;

			Log.Info( $"Size: {size}, Len: {len}, Mins: {bbox.Mins}, Maxs: {bbox.Maxs}, Center: {center}" );

			using ( SceneWorld.SetCurrent( new SceneWorld() ) )
			{
				var itemModel = Entity.ModelPath;

				var pos = Transform.Zero;
				pos.Position = Vector3.Zero;
				if ( MathF.Abs( size.x ) > MathF.Abs( size.y ) )
					pos.Rotation = Rotation.From( 0, -90, 0 );
				else
					pos.Rotation = Rotation.From( 0, 0, 0 );

				SceneObject.CreateModel( itemModel, pos );

				Light.Point( Vector3.Up * 150.0f, 200.0f, Color.White * 5000.0f );

				ModelScene = Add.Scene( SceneWorld.Current, camPos, Vector3.Forward.EulerAngles, fov );
				ModelScene.Style.Width = 80;
				ModelScene.Style.Height = 80;
			}
		}

	}
}
