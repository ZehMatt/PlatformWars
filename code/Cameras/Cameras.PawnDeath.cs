using Sandbox;

namespace PlatformWars.Cameras
{
	public class PawnDeathCam : Base
	{
		Vector3 FocusPoint;
		Angles LookAngles;
		float FovOverride = 0;

		public Vector3 TargetPos;
		public Rotation TargetRot;

		float LookDistance = 400;
		float CameraSpeed = 10.0f;

		public PawnDeathCam() : base( Mode.PawnDeath )
		{
		}

		public override void Activated()
		{
			base.Activated();

			FocusPoint = CurrentView.Position - GetViewOffset();
			FieldOfView = 70;

			LookAngles = Rot.Angles();
			FovOverride = 80;

			Pos = CurrentView.Position;
			Rot = CurrentView.Rotation;
		}
		public override void BuildInput( InputBuilder input )
		{
			LookAngles += input.AnalogLook * (FovOverride / 80.0f);
			LookAngles.roll = 0;

			input.Clear();
			input.StopProcessing = true;
		}

		public override void Update()
		{
			var targetPos = GetSpectatePoint();

			Pos = targetPos + GetViewOffset();

			var tr = Trace.Ray( GetSpectatePoint(), Pos )
				.WorldOnly()
				.Radius( 4 )
				.Run();

			//
			// Doing a second trace at the half way point is a little trick to allow a larger camera collision radius
			// without getting initially stuck
			//
			tr = Trace.Ray( targetPos + tr.Direction * (tr.Distance * 0.5f), tr.EndPos )
				.WorldOnly()
				.Radius( 8 )
				.Run();

			var delta = targetPos - tr.EndPos;
			TargetPos = tr.EndPos;
			TargetRot = Rotation.From( delta.EulerAngles );

			var speed = Time.Delta * CameraSpeed;
			Pos = Vector3.Lerp( Pos, TargetPos, speed );
			Rot = Rotation.Lerp( Rot, TargetRot, speed );
		}

		public virtual Vector3 GetSpectatePoint()
		{
			var roundMgr = RoundManager.Get();
			if ( roundMgr == null )
				return Vector3.Zero;

			return roundMgr.GetDeathSpectatePos();
		}

		public virtual Vector3 GetViewOffset()
		{
			return LookAngles.Direction * -LookDistance + Vector3.Up * 20;
		}
	}
}
