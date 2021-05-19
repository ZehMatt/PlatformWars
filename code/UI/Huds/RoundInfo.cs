using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace PlatformWars.UI.Huds
{
	class Timer : Panel
	{
		public Label CounterLabel;

		public Timer()
		{
			CounterLabel = Add.Label( "0" );
		}
	}

	class Info : Panel
	{
		public Label TextLabel;

		bool DelayedActivate;
		RealTimeSince TextActive;

		public Info()
		{
			TextLabel = Add.Label();
		}

		public void SetText( string newText )
		{
			if ( TextLabel.Text == newText )
				return;

			if ( TextActive < 3.0 )
			{
				TextLabel.SetClass( "active", false );
				DelayedActivate = true;
			}
			else
			{
				TextLabel.SetClass( "active", true );
			}

			TextActive = 0;
			TextLabel.Text = newText;
		}

		public override void Tick()
		{
			base.Tick();

			if ( DelayedActivate && TextActive >= 0.2 )
			{
				TextLabel.SetClass( "active", true );
				DelayedActivate = false;
			}

			if ( TextActive >= 3.0f )
				TextLabel.SetClass( "active", false );
		}
	}

	public class RoundInfo : Panel
	{
		Info InfoPanel;
		Timer TimerPanel;

		public static bool Enabled2 = false;

		public RoundInfo()
		{
			StyleSheet.Load( "/ui/huds/RoundInfo.scss" );

			InfoPanel = new();
			InfoPanel.Parent = this;

			TimerPanel = new();
			TimerPanel.Parent = this;
		}

		public override void Tick()
		{
			base.Tick();

			var roundMgr = RoundManager.Get();
			if ( roundMgr == null )
				return;

			UpdateInfo( roundMgr );
			UpdateTimer( roundMgr );
		}

		void UpdateInfoText( RoundManager roundMgr )
		{
			var state = roundMgr.GetState();
			var activePly = roundMgr.GetActivePlayer();
			var localPly = Local.Client;

			var text = "";
			switch ( state )
			{
				case RoundState.WaitForPlayer:
					text = "Waiting for players...";
					break;
				case RoundState.TerrainGen:
					text = "Generating level...";
					break;
				case RoundState.Setup:
					text = "Initializing";
					break;
				case RoundState.Starting:
					float stateTime = roundMgr.GetStateTime();
					float maxTime = roundMgr.GetStateTime( state );
					int secs = (int)MathF.Round( maxTime - stateTime );
					text = $"Starting Round in {secs}";
					break;
				case RoundState.PrePlayerTurn:
					if ( activePly == localPly )
					{
						text = "Your turn, get prepared.";
					}
					else
					{
						text = $"{activePly.Name} is thinking.";
					}
					break;
				case RoundState.PlayerTurn:
					if ( activePly == localPly )
					{
						text = "Go!";
					}
					else
					{
						text = $"{activePly.Name} is on the move";
					}
					break;
				case RoundState.PostPlayerTurn:
					text = "Turn over";
					break;
				case RoundState.Evaluate:
				case RoundState.Transition:
				case RoundState.End:
				case RoundState.Restart:
				case RoundState.Idle:
					return;
			}

			if ( text == "" )
				return;

			InfoPanel.SetText( text );
		}

		void UpdateInfo( RoundManager roundMgr )
		{
			float stateTime = roundMgr.GetStateTime();

			var curState = roundMgr.GetState();
			UpdateInfoText( roundMgr );
		}

		void UpdateTimer( RoundManager roundMgr )
		{
			float stateTime = roundMgr.GetStateTime();

			bool showTimer = false;
			var state = roundMgr.GetState();
			switch ( state )
			{
				case RoundState.PrePlayerTurn:
				case RoundState.PlayerTurn:
					showTimer = true;
					break;
			}

			TimerPanel.SetClass( "active", showTimer );

			if ( showTimer )
			{
				float maxTime = roundMgr.GetStateTime( state );
				if ( maxTime >= 0.0f )
				{
					// Countdown instead of counter.
					stateTime = maxTime - stateTime;
				}

				TimerPanel.CounterLabel.Text = string.Format( "{0:00.00}", stateTime );
			}

		}
	}
}
