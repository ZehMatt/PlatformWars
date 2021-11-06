using Sandbox;

namespace PlatformWars.Weapons
{
	[Library( "weapon_pw_pistol", Title = "Pistol" )]
	partial class Pistol : Base
	{
		public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";

		public override string ModelPath => "weapons/rust_pistol/rust_pistol.vmdl";

		public override void Simulate( Client player )
		{
			if ( !Owner.IsValid() )
				return;

			if ( Input.Pressed( InputButton.Attack1 ) )
			{
				AttackPrimary();
			}
		}

		public virtual void Reload()
		{
		}

		public virtual void AttackPrimary()
		{
			ShootAnimation();
			ShootEffectsPredicted();
			ShootEffects();

			PlaySound( "rust_pistol.shoot" );

			if ( Host.IsServer )
			{
				var bullet = new Projectiles.Bullet();
				bullet.LaunchTowards( Owner.EyePos, Owner.EyeRot );
				bullet.Owner = Owner;
			}
		}

		public virtual void AttackSecondary()
		{

		}
	}

}
