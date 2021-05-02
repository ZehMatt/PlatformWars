
namespace PlatformWars
{
    partial class RoundManager
    {
        void HandleIdle()
        {
            if (StateTime < 1.0f)
                return;

            SetState(RoundState.WaitForPlayer);
        }
    }
}
