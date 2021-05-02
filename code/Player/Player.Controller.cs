using Sandbox;

namespace PlatformWars
{
    class PlayerController : WalkController
    {
        public override void Tick()
        {
            var roundMgr = RoundManager.Get();

            base.Tick();

            // If the round is in progress we need to check if we are allowed to move.
            if (roundMgr != null)
            {
                // Only allow movement if the its the players turn.
                if (!roundMgr.HasPlayerControl(this.Player as PlatformWars.Player))
                {
                    WishVelocity = Vector3.Zero;
                    return;
                }
            }
        }

        public override void BuildInput(ClientInput input)
        {
            Host.AssertClient();

            var roundMgr = RoundManager.Get();

            // If the round is in progress we need to check if we are allowed to move.
            if (roundMgr != null)
            {
                // Only allow movement if the its the players turn.
                if (!roundMgr.HasPlayerControl(this.Player as PlatformWars.Player))
                {
                    input.Clear();
                    input.StopProcessing = true;
                    return;
                }
            }

            base.BuildInput(input);
        }
    }
}
