using Sandbox;

namespace platformwars.Entities
{
	[Library( "ent_loot", Title = "Loot Container", Spawnable = false )]
	public partial class LootContainer : Entity
	{
		private SceneObject SceneModel;

		public override void ClientSpawn()
		{
			base.ClientSpawn();

			var mdl = Model.Load( "models/citizen_props/crate01.vmdl" );
			SceneModel = new SceneObject( mdl, Transform );
		}
	}
}
