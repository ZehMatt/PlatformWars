using Sandbox;

namespace PlatformWars.Weapons
{
	[Library( "weapon_pw_smg", Title = "SMG" )]
	partial class SMG : Base
	{
		public override string ViewModelPath => "weapons/rust_smg/v_rust_smg.vmdl";

		public override string ModelPath => "weapons/rust_smg/rust_smg.vmdl";
	}

}
