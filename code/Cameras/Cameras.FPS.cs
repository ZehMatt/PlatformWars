using Sandbox;

namespace PlatformWars.Cameras
{
	class FPS : Base
	{
		Vector3 TargetPos;

		public FPS() : base( Mode.FPS )
		{ }

		public override void Activated()
		{
			var pawn = Local.Pawn;
			if ( pawn == null ) return;

			Pos = pawn.EyePos;
			Rot = pawn.EyeRot;
		}

		public override void Build( ref CameraSetup camSetup )
		{
			base.Build( ref camSetup );

			camSetup.ZFar = 20000;
			camSetup.ZNear = 10;

		}

		public override void Update()
		{
			var pawn = Local.Pawn as AnimEntity;
			if ( pawn == null ) return;

			TargetPos = pawn.EyePos;

			Pos = pawn.EyePos;
			Rot = pawn.EyeRot;

			FieldOfView = 75;

			Viewer = pawn;
		}
	}
}
