namespace PlatformWars
{
    partial class RoundManager
    {
        [ServerVar]
        public static float platformwars_preturn_time { get; set; } = 5.0f;

        void HandlePrePlayerTurn()
        {
            if (StateTime < platformwars_preturn_time)
                return;

            SetState(RoundState.PlayerTurn);
        }

        [ServerVar]
        public static float platformwars_turn_time { get; set; } = 99999.0f;

        void HandlePlayerTurn()
        {
            if (StateTime < platformwars_turn_time)
                return;

            SetState(RoundState.PostPlayerTurn);
        }

        [ServerVar]
        public static float platformwars_postturn_time { get; set; } = 5.0f;

        void HandlePostPlayerTurn()
        {
            if (StateTime < platformwars_postturn_time)
                return;

            GetActivePlayer().RemoveControlled();

            // Evaluate the current round cycle.
            SetState(RoundState.Evaluate);
        }
    }
}
