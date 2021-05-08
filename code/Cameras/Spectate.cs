using Sandbox;

namespace PlatformWars.Cameras
{

	partial class Spectate : BaseCamera
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

		SpectateMode Mode = SpectateMode.Pawn;

		Vector3 OverviewOffset = new Vector3( 0, 0, 300 );

		public override void Activated()
		{
			base.Activated();

			Host.AssertClient();

			LookDistance = Cookie.Get( "LookDistance", LookDistance );
			Mode = Cookie.Get( "Mode", Mode );

			LookAngles = Rot.Angles();
			FovOverride = 80;

			// Update new positions also.
			Update();
		}

		public override void Deactivated()
		{
			base.Deactivated();
		}

		public override void Update()
		{
			switch ( Mode )
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
			Rot = Rotation.Slerp( Rot, TargetRot, LerpSpeed * RealTime.Delta );
			FieldOfView = FovOverride;
		}

		void CycleMode()
		{
			switch ( Mode )
			{
				case SpectateMode.Pawn:
					Mode = SpectateMode.Free;
					break;
				case SpectateMode.Free:
					Mode = SpectateMode.Overview;
					break;
				case SpectateMode.Overview:
					Mode = SpectateMode.Pawn;
					break;
			}
			Cookie.Set( "Mode", Mode );
		}

		public override void BuildInput( ClientInput input )
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
			var player = Player.Local;
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
					var pos = pawn.WorldPos;
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

			return pawn.GetBoneTransform( pawn.GetBoneIndex( "spine2" ) ).Pos;
		}

		void SpectateMove()
		{
			var player = Player.Local as BasePlayer;
			if ( player == null )
				return;

			var pawnPos = GetPawnPos();
			Pos = pawnPos + GetViewOffset();

			var tr = Trace.Ray( pawnPos, Pos )
				.Ignore( player )
				.WorldOnly()
				.Radius( 4 )
				.Run();

			//
			// Doing a second trace at the half way point is a little trick to allow a larger camera collision radius
			// without getting initially stuck
			//
			tr = Trace.Ray( pawnPos + tr.Direction * (tr.Distance * 0.5f), tr.EndPos )
				.Ignore( player )
				.WorldOnly()
				.Radius( 8 )
				.Run();

			var delta = pawnPos - tr.EndPos;
			TargetPos = tr.EndPos;
			TargetRot = Rotation.From( delta.EulerAngles );
		}

		Vector3 GetViewOffset()
		{
			if ( Player.Local is not BasePlayer player )
				return Vector3.Zero;

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
