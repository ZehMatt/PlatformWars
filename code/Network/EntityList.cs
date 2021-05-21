using Sandbox;
using System.Collections.Generic;

namespace PlatformWars.Network
{
	public class EntityList : NetworkClass
	{
		internal List<EntityHandle<Entity>> Values = new();

		public Entity Get( int i )
		{
			return Values[i].Entity;
		}

		public void Add( Entity value )
		{
			Values.Add( value );
			MarkDirty();
		}

		public void Remove( Entity value )
		{
			Values.Remove( value );
			MarkDirty();
		}

		public void RemoveAt( int index )
		{
			Values.RemoveAt( index );
			MarkDirty();
		}

		public void Clear()
		{
			Values.Clear();
			MarkDirty();
		}

		public int Count { get => Values.Count; }

		public void MarkDirty()
		{
			NetworkDirty( nameof( Values ), NetVarGroup.Net ); // TODO - .Net is wrong here
		}

		public override bool NetWrite( NetWrite write )
		{
			base.NetWrite( write );

			write.Write<short>( (short)Values.Count );

			foreach ( var entry in Values )
			{
				write.Write( entry.EntityId );
			}

			return true;
		}

		public override bool NetRead( NetRead read )
		{
			base.NetRead( read );

			int count = read.Read<short>();
			Values.Clear();

			for ( int i = 0; i < count; i++ )
			{
				int entityId = read.Read<int>();
				Values.Add( new EntityHandle<Entity>( entityId ) );
			}

			return true;
		}
	}
}
