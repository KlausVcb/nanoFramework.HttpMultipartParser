using System;
using System.Collections;

namespace nanoFramework.HttpMultipartParser.Utility
{
    internal class LineBuffer : IDisposable
    {
        Hashtable data = new();
        int length = 0;

        public void Dispose() => data.Clear();

        public void Write(byte[] bytes, int offset, int count)
        {
            var chunk = new byte[count];

            Array.Copy(bytes, offset, chunk, 0, count);
            
            data.Add(data.Count, chunk);
            length += count;
        }

        public int Length => length;

        public byte[] ToArray(bool clear = false)
        {
            var result = new byte[length];
            var pos = 0;

            for (int i = 0; i < data.Count; i++)
            {
                var data = this.data[i];

                if (data is byte)
                    result[pos++] = (byte)data;
                else if (data is byte[])
                {
                    var array = (byte[])data;
                    Array.Copy(array, 0, result, pos, array.Length);
                    pos += array.Length;
                }
                this.data[i] = null;
            }

            if (clear) Clear();

            return result;
        }

        public void Clear()
        {
            data.Clear();
            length = 0;
        }
    }
}
