﻿using Sandbox;

namespace PlatformWars.Weapons
{
	[Library( "weapon_pw_pistol", Title = "Pistol" )]
	partial class Pistol : Base
	{
		public const float Damage = 50.0f;

		public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";

		public override float PrimaryRate => 15.0f;
		public override float SecondaryRate => 1.0f;
		public override float ReloadTime => 3.0f;

		public override int Bucket => 1;

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "weapons/rust_pistol/rust_pistol.vmdl" );
			AmmoClip = 12;
		}

		public override bool CanPrimaryAttack()
		{
			return base.CanPrimaryAttack() && Input.Pressed( InputButton.Attack1 );
		}

		public override void AttackPrimary()
		{
			TimeSincePrimaryAttack = 0;
			TimeSinceSecondaryAttack = 0;

			/*
			if ( !TakeAmmo( 1 ) )
			{
				DryFire();
				return;
			}
			*/

			//
			// Tell the clients to play the shoot effects
			//
			ShootEffects();
			PlaySound( "rust_pistol.shoot" );

			//
			// Shoot the bullets
			//
			ShootBullet( 0.05f, 10.5f, Damage, 3.0f );
		}
	}

}
