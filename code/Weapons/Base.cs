using Sandbox;
using System.Linq;

namespace PlatformWars.Weapons
{
	partial class Base : BaseCarriable
	{
		public virtual string ModelPath => null;

		[ServerCmd( "platformwars_inv" )]
		public static void ChangeWeapon( string entClass )
		{
			var target = ConsoleSystem.Caller.Pawn as Pawn;
			if ( target == null )
				return;

			var ply = target.GetPlayer();
			if ( ply == null )
				return;

			var ent = ply.GetItems().First( x => x.ClassInfo.Name == entClass );
			if ( ent == null )
				return;

			target.SwitchToWeapon( ent );
		}

		public override void CreateViewModel()
		{
			Host.AssertClient();

			if ( string.IsNullOrEmpty( ViewModelPath ) )
				return;

			ViewModelEntity = new ViewModel();
			ViewModelEntity.Position = Position;
			ViewModelEntity.Owner = Owner;
			ViewModelEntity.EnableViewmodelRendering = true;
			ViewModelEntity.SetModel( ViewModelPath );
		}

		public override void Spawn()
		{
			base.Spawn();

			SetModel( ModelPath );
		}

		protected void ShootAnimation()
		{
			(Owner as AnimEntity)?.SetAnimBool( "b_attack", true );

			ViewModelEntity?.SetAnimBool( "fire", true );
			CrosshairPanel?.CreateEvent( "fire" );
		}

		protected void ShootEffectsPredicted()
		{
			if ( IsLocalPawn )
			{
				_ = new Sandbox.ScreenShake.Perlin();
			}
		}

		[ClientRpc]
		protected void ShootEffects()
		{
			Host.AssertClient();

			Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );
		}
	}

}
