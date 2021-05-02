namespace PlatformWars
{
    partial class RoundManager
    {
        void HandleWaitForPlayer()
        {
            if (Player.All.Count < 2)
                return;

            // Begin round setup.
            SetState(RoundState.Setup);
        }
    }
}
