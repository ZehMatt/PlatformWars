using Sandbox;

namespace PlatformWars.Cameras
{
	class TPS : Base
	{
		Vector3 lastPos;

		public TPS() : base( Mode.TPS )
		{ }

		public override void Activated()
		{
			var player = Local.Pawn;
			if ( player == null ) return;

			Pos = player.EyePos;
			Rot = player.EyeRot;

			lastPos = Pos;
		}

		public override void Update()
		{
			var pawn = Local.Pawn as AnimEntity;

			if ( pawn == null )
				return;

			Pos = pawn.Position;
			Vector3 targetPos;

			var center = pawn.Position + Vector3.Up * 64;
			Pos = center;
			Rot = Rotation.FromAxis( Vector3.Up, 4 ) * Input.Rotation;

			float distance = 130.0f * pawn.Scale;
			targetPos = Pos + Input.Rotation.Right * ((pawn.CollisionBounds.Maxs.x + 15) * pawn.Scale);
			targetPos += Input.Rotation.Forward * -distance;

			Pos = targetPos;
			FieldOfView = 70;

			Viewer = null;
		}

		public override void BuildInput( InputBuilder input )
		{
			base.BuildInput( input );
		}
	}
}
