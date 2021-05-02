namespace PlatformWars
{
    partial class RoundManager
    {
        void HandleEvaluate()
        {
            if (StateTime < 1.0f)
                return;

            SetState(RoundState.Transition);
        }
    }
}
