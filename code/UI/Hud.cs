using Sandbox;
using Sandbox.UI;

namespace PlatformWars.UI
{
	[Library]
	partial class Hud : HudEntity<RootPanel>
	{
		private Huds.DamageInfo DamageInfo;

		public Hud()
		{
			if ( !IsClient )
				return;

			RootPanel.StyleSheet.Load( "/ui/Hud.scss" );

			RootPanel.AddChild<Huds.PawnTags>();
			RootPanel.AddChild<Huds.RoundInfo>();
			DamageInfo = RootPanel.AddChild<Huds.DamageInfo>();
			RootPanel.AddChild<CrosshairCanvas>();
			RootPanel.AddChild<ChatBox>();
			RootPanel.AddChild<VoiceList>();
			RootPanel.AddChild<KillFeed>();
			RootPanel.AddChild<Scoreboard<ScoreboardEntry>>();
		}

		public static Hud Get()
		{
			var game = Game.Current as PlatformWars.Game;
			if ( game == null )
				return null;

			return game.PlayerHud;
		}


		[ClientRpc]
		public void DisplayDamageValue( Pawn pawn, float damage )
		{
			Log.Info( $"Pawn damage {damage} to {pawn}" );

			DamageInfo.AddDamageDisplay( pawn, damage );
		}
	}
}
