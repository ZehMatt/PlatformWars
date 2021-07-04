using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;

namespace PlatformWars.UI
{
	[Library]
	partial class PlayerItems : Panel
	{
		Panel Canvas;
		List<PlayerItem> Items = new();

		public PlayerItems()
		{
			StyleSheet.Load( "/ui/PlayerItems.scss" );

			Canvas = Add.Panel( "items" );
			PopulateEntries();
		}

		[Event.Hotload]
		void PopulateEntries()
		{
			Canvas.DeleteChildren();
			Items.Clear();

			for ( int i = 0; i < 32; i++ )
			{
				var x = new PlayerItem( i, Canvas );
				x.AddEventListener( "onclick", () => { OnClickItem( x ); } );

				Items.Add( x );
			}
		}

		private void OnClickItem( PlayerItem item )
		{
			if ( item.Entity == null )
				return;

			Log.Info( $"Change weapon to {item.Entity}" );

			ConsoleSystem.Run( "platformwars_inv", item.Entity.ClassInfo.Name );
		}

		private void UpdateItems()
		{
			var pawn = Local.Pawn as Pawn;
			if ( pawn == null )
				return;

			var ply = pawn.GetPlayer();
			if ( ply == null )
				return;

			var items = ply.GetItems();
			for ( int i = 0; i < Items.Count; i++ )
			{
				if ( i >= items.Count )
				{
					Items[i].Entity = null;
				}
				else
				{
					Items[i].Entity = items[i] as Weapons.Base;
				}
			}
		}

		public override void Tick()
		{
			base.Tick();

			if ( Input.Down( InputButton.Menu ) )
				Parent.SetClass( "playeritemsopen", true );
			else
				Parent.SetClass( "playeritemsopen", false );

			UpdateItems();
		}
	}
}
