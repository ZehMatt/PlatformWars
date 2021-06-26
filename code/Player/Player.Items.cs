using Sandbox;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlatformWars
{
	// Replacement for the Inventory thing, our pawns share one inventory from
	// the player.
	partial class Player
	{
		[Net]
		public List<Entity> Items { get; set; }

		public void AddItem( Entity ent )
		{
			if ( Items.Count == 0 )
			{
				Items.Add( ent );
				return;
			}

			for ( int i = 0; i < Items.Count; ++i )
			{
				if ( Items[i] == null )
				{
					Items[i] = ent;
					return;
				}
			}

			Items.Add( ent );
		}

		public void RemoveItem( Entity ent )
		{
			// Don't invalidate the indices of the items, swap with null.
			for ( int i = 0; i < Items.Count; ++i )
			{
				if ( Items[i] == ent )
				{
					Items[i] = null;
					break;
				}
			}

			// Shrink list if the last entries are null.
			while ( Items.Count > 0 && Items.Last() == null )
			{
				Items.RemoveAt( Items.Count - 1 );
			}
		}

		public ReadOnlyCollection<Entity> GetItems()
		{
			return new ReadOnlyCollection<Entity>( Items );
		}

		public bool HasItems()
		{
			return Items.Count > 0;
		}
	}
}
