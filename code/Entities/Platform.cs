using Sandbox;

namespace PlatformWars.Entities
{

    [Library("ent_platform", Title = "Procedural Platform", Spawnable = false)]
    public partial class Platform : Prop
    {
        public const int BlockSize = 35;
        public const int MaxHealth = 500;

        public override void Spawn()
        {
            base.Spawn();

            Host.AssertServer();

            SetModel("models/citizen_props/crate01.vmdl");
            Health = MaxHealth;
            EnableShadowCasting = false;
            EnableShadowReceive = false;

            UpdateColor();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        public override void OnKilled()
        {
            base.OnKilled();
        }

        void UpdateColor()
        {
            float health = (float)Health / (float)MaxHealth;
            float r = (1.0f - health);
            float g = 1.0f;
            float b = 1.0f;

            if (IsClient)
            {
                Log.Info($"Client Health: {Health}");
            }

            RenderColor = new Color32((byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
        }

        public override void TakeDamage(DamageInfo info)
        {
            base.TakeDamage(info);
            UpdateColor();
        }
    }

}
