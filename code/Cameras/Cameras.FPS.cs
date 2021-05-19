using Sandbox;

namespace PlatformWars.Cameras
{
	class FPS : Base
	{
		Vector3 lastPos;

		public FPS() : base( Mode.FPS )
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
			var player = Local.Pawn;
			if ( player == null ) return;

			Pos = Vector3.Lerp( player.EyePos.WithZ( lastPos.z ), player.EyePos, 20.0f * Time.Delta );
			Rot = player.EyeRot;

			FieldOfView = 80;

			Viewer = player;
			lastPos = Pos;
		}
	}
}
