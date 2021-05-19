namespace PlatformWars.Cameras
{
	public enum Mode
	{
		Spectate,
		PawnDeath,
		FPS,
	}

	public class Base : Sandbox.Camera
	{
		Mode _mode = Mode.Spectate;

		public Mode Mode { get { return _mode; } }

		protected Base( Mode mode )
		{
			_mode = mode;
		}

		public override void Update() { }
	}
}
