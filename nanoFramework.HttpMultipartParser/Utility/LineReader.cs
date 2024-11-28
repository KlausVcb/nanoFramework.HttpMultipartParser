using System;
using System.IO;
using System.Text;

namespace nanoFramework.HttpMultipartParser.Utility
{
    /// <summary>Provides methods to read a stream line by line while still returning the bytes.</summary>
    internal class LineReader
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private readonly LineBuffer lineBuffer = new();
        int availableBytes = -1;
        int pos = 0;

        /// <summary>Initializes a new instance of the <see cref="LineReader"/> class.</summary>
        /// <param name="input">The input stream to read from.</param>
        /// <param name="bufferSize">The buffer size to use for new buffers.</param>
        public LineReader(Stream input, int bufferSize)
        {
            stream = input;
            buffer = new byte[bufferSize];
        }

        /// <summary>
        /// Reads a line from the stack delimited by the newline for this platform.
        /// The newline characters will not be included in the stream.
        /// </summary>
        /// <returns>The <see cref="byte[]" /> containing the line or null if end of stream.</returns>
        public byte[] ReadByteLine()
        {
            while (availableBytes != 0)
            {
                if (pos >= availableBytes)
                {
                    availableBytes = stream.Read(buffer);
                    if (availableBytes == 0) break;

                    stream.Position += availableBytes;
                    pos = 0;
                }

                var previousPos = pos;

                for (int i = pos; i < availableBytes; i++)
                {
                    if (buffer[i] == '\n')
                    {
                        var length = (i > 0 && buffer[i - 1] == '\r' ? i - 1 : i) - previousPos;
                        pos = i + 1;

                        if (lineBuffer.Length > 0)
                        {
                            lineBuffer.Write(buffer, previousPos, length);
                            return lineBuffer.ToArray(true);
                        }

                        var line = new byte[length];
                        Array.Copy(buffer, previousPos, line, 0, length);
                        return line;
                    }
                }

                pos = availableBytes;
                lineBuffer.Write(buffer, previousPos, availableBytes - previousPos);
            }

            return lineBuffer.Length == 0 ? null : lineBuffer.ToArray(true);
        }

        /// <summary>
        /// Reads a line from the stack delimited by the newline for this platform.
        /// The newline characters will not be included in the stream.
        /// </summary>
        /// <returns>The <see cref="string" /> containing the line or null if end of stream.</returns>
        public string ReadLine()
        {
            byte[] data = ReadByteLine();
            return data == null ? null : Encoding.UTF8.GetString(data, 0, data.Length);
        }
    }
}
