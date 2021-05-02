using Sandbox;
using Sandbox.UI;

namespace PlatformWars.UI
{
    [Library]
    public partial class Hud : Sandbox.Hud
    {
        public Hud()
        {
            if (!IsClient)
                return;

            RootPanel.StyleSheet.Load("/ui/Hud.scss");

            RootPanel.AddChild<Huds.PawnTags>();
            RootPanel.AddChild<Huds.RoundInfo>();
            RootPanel.AddChild<CrosshairCanvas>();
            RootPanel.AddChild<ChatBox>();
            RootPanel.AddChild<VoiceList>();
            RootPanel.AddChild<KillFeed>();
            RootPanel.AddChild<Scoreboard<ScoreboardEntry>>();
        }
    }
}
