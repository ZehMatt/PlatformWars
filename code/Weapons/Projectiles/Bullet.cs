using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace PlatformWars.Weapons.Projectiles
{
	[Library( "pw_bullet" )]
	partial class Bullet : Projectile
	{
		public override string ProjectileModel => "weapons/shells/pistol_shell.vmdl";

		public override float LaunchSpeed { get; } = 4200.0f;

		public override float Mass { get; } = 0.002f;

		public override float ImpactDamage { get; set; } = 25.0f;
	}
}
