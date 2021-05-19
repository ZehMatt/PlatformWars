using Sandbox;

namespace PlatformWars.Cameras
{

	partial class Spectate : Base
	{
		enum SpectateMode
		{
			Pawn,
			Free,
			Overview,
		}

		Angles LookAngles;
		Vector3 MoveInput;

		public Vector3 TargetPos;
		public Rotation TargetRot;

		float MoveSpeed;
		float FovOverride = 0;
		float LerpSpeed = 20.0f;

		float LookDistance = 400;

		SpectateMode SpectMode = SpectateMode.Pawn;

		Vector3 OverviewOffset = new Vector3( 0, 0, 300 );

		public Spectate() : base( Mode.Spectate )
		{
		}

		public override void Activated()
		{
			base.Activated();

			Host.AssertClient();

			LookDistance = Cookie.Get( "LookDistance", LookDistance );
			SpectMode = Cookie.Get( "Mode", SpectMode );

			LookAngles = Rot.Angles();
			FovOverride = 80;

			Pos = CurrentView.Position;
			Rot = CurrentView.Rotation;

			// Update new positions also.
			//Update();
		}

		public override void Deactivated()
		{
			base.Deactivated();
		}

		public override void Update()
		{
			switch ( SpectMode )
			{
				case SpectateMode.Pawn:
					SpectateMove();
					break;
				case SpectateMode.Free:
					FreeMove();
					break;
				case SpectateMode.Overview:
					OverviewMove();
					break;
			}

			Pos = Vector3.Lerp( Pos, TargetPos, LerpSpeed * RealTime.Delta );
			Rot = TargetRot;
			FieldOfView = FovOverride;
		}

		void CycleMode()
		{
			switch ( SpectMode )
			{
				case SpectateMode.Pawn:
					SpectMode = SpectateMode.Free;
					break;
				case SpectateMode.Free:
					SpectMode = SpectateMode.Overview;
					break;
				case SpectateMode.Overview:
					SpectMode = SpectateMode.Pawn;
					break;
			}
			Cookie.Set( "Mode", SpectMode );
		}

		public override void BuildInput( InputBuilder input )
		{
			MoveInput = input.AnalogMove;
			//DebugOverlay.ScreenText(4, $"MoveInput: {MoveInput.ToString()}");

			MoveSpeed = 1;
			if ( input.Down( InputButton.Run ) )
				MoveSpeed = 5;

			if ( input.Pressed( InputButton.Duck ) )
			{
				CycleMode();
			}

			LookDistance += (input.MouseWheel * 10.0f);

			LookAngles += input.AnalogLook * (FovOverride / 80.0f);
			LookAngles.roll = 0;

			input.Clear();
			input.StopProcessing = true;
		}

		void OverviewMove()
		{
			var player = Local.Pawn;
			if ( player == null )
				return;

			var roundMgr = RoundManager.Get();
			if ( roundMgr == null )
				return;

			var activePlayers = roundMgr.GetActivePlayers();
			var posMin = Vector3.Zero;
			var posMax = Vector3.Zero;
			var posCenter = Vector3.Zero;
			var totalPawns = 0;

			foreach ( var ply in activePlayers )
			{
				var pawns = ply.GetPawns();
				foreach ( var pawn in pawns )
				{
					var pos = pawn.Position;
					posCenter += pos;
					posMin = Vector3.Min( posMin, pos );
					posMax = Vector3.Max( posMax, pos );
					totalPawns++;
				}
			}

			if ( totalPawns > 0 )
				posCenter /= totalPawns;

			TargetPos = OverviewOffset + posCenter + GetViewOffset();
			TargetRot = player.EyeRot;
		}

		Vector3 GetPawnPos()
		{
			var roundMgr = RoundManager.Get();
			if ( roundMgr == null )
				return Vector3.Zero;

			var pawn = roundMgr.GetActivePawn();
			if ( pawn == null )
				return Vector3.Zero;

			return pawn.GetBoneTransform( pawn.GetBoneIndex( "spine2" ) ).Position;
		}

		void SpectateMove()
		{
			var pawnPos = GetPawnPos();
			Pos = pawnPos + GetViewOffset();

			var tr = Trace.Ray( pawnPos, Pos )
				.WorldOnly()
				.Radius( 4 )
				.Run();

			//
			// Doing a second trace at the half way point is a little trick to allow a larger camera collision radius
			// without getting initially stuck
			//
			tr = Trace.Ray( pawnPos + tr.Direction * (tr.Distance * 0.5f), tr.EndPos )
				.WorldOnly()
				.Radius( 8 )
				.Run();

			var delta = pawnPos - tr.EndPos;
			TargetPos = tr.EndPos;
			TargetRot = Rotation.From( delta.EulerAngles );
		}

		Vector3 GetViewOffset()
		{
			return LookAngles.Direction * -LookDistance + Vector3.Up * 20;
		}

		void FreeMove()
		{
			var mv = MoveInput.Normal * 300 * RealTime.Delta * Rot * MoveSpeed;

			TargetRot = Rotation.From( LookAngles );
			TargetPos += mv;
		}
	}
}
