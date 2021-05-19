using Sandbox;

namespace PlatformWars.Cameras
{
	public class PawnDeathCam : Base
	{
		Vector3 FocusPoint;

		public PawnDeathCam() : base( Mode.PawnDeath )
		{
		}

		public override void Activated()
		{
			base.Activated();

			FocusPoint = CurrentView.Position - GetViewOffset();
			FieldOfView = 70;
		}

		public override void Update()
		{
			var player = Local.Pawn;
			if ( player == null ) return;

			// lerp the focus point
			FocusPoint = Vector3.Lerp( FocusPoint, GetSpectatePoint(), Time.Delta * 20.0f );

			Pos = FocusPoint + GetViewOffset();
			Rot = player.EyeRot;

			FieldOfView = FieldOfView.LerpTo( 50, Time.Delta * 20.0f );
			Viewer = null;
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
			var player = Local.Pawn;
			if ( player == null ) return Vector3.Zero;

			return player.EyeRot.Forward * (-130 * player.Scale) + Vector3.Up * (20 * player.Scale);
		}
	}
}
