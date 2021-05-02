namespace PlatformWars
{
    partial class RoundManager
    {
        [ServerVar]
        public static float platformwars_start_time { get; set; } = 5.0f;

        void HandleStarting()
        {
            if (StateTime < platformwars_start_time)
                return;

            SetState(RoundState.Transition);
        }
    }
}
