using Sandbox;
using System.Collections.Generic;

namespace PlatformWars.Network
{
    /// <summary>
    /// Like an array, but not really
    /// </summary>
    public class NetList<T> : NetworkClass
    {
        internal List<T> Values = new();

        /// <summary>
        /// Get a value
        /// </summary>
        public T Get(int i)
        {
            return Values[i];
        }

        public void Add(T value)
        {
            Values.Add(value);
            MarkDirty();
        }

        public void Remove(T value)
        {
            Values.Remove(value);
            MarkDirty();
        }

        public void RemoveAt(int index)
        {
            Values.RemoveAt(index);
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
            NetworkDirty("Values", NetVarGroup.Net); // TODO - .Net is wrong here
        }

        public override bool NetWrite(NetWrite write)
        {
            base.NetWrite(write);

            // Amount
            write.Write<short>((short)Values.Count);

            foreach (var entry in Values)
            {
                write.Write(entry);
            }

            return true;
        }

        public override bool NetRead(NetRead read)
        {
            base.NetRead(read);

            int count = read.Read<short>();

            Values.Clear();

            for (int i = 0; i < count; i++)
            {
                Values.Add(read.Read<T>());
            }

            return true;
        }
    }
}
