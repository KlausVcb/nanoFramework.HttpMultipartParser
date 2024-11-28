using nanoFramework.HttpMultipartParser.Parts;
using nanoFramework.HttpMultipartParser.Utility;
using System;
using System.Collections;
using System.IO;
using System.Text;

namespace nanoFramework.HttpMultipartParser
{
    /// <summary>
    ///     Provides methods to parse a
    ///     <see href="http://www.ietf.org/rfc/rfc2388.txt">
    ///         <c>multipart/form-data</c>
    ///     </see>
    ///     stream into it's parameters and file data.
    /// </summary>
    public class MultipartFormDataParser
	{
        private const int defaultBufferSize = 4096;

		private readonly Stream stream;
		private readonly bool ignoreInvalidParts;
		private string boundary;
        private byte[] boundaryBinary;
        private readonly int binaryBufferSize;
		private bool readEndBoundary;
        private readonly ArrayList _files = new();
        private readonly ArrayList _parameters = new();

		/// <summary>Initializes a new instance of the <see cref="MultipartFormDataParser" /> class</summary>
		/// <param name="stream">The stream containing the multipart data.</param>
		/// <param name="binaryBufferSize">The size of the buffer to use for parsing the multipart form data.</param>
		/// <param name="ignoreInvalidParts">By default the parser will throw an exception if it encounters an invalid part. Set this to true to ignore invalid parts.</param>
		public MultipartFormDataParser(Stream stream, int binaryBufferSize = defaultBufferSize, bool ignoreInvalidParts = false)
		{
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
            this.binaryBufferSize = binaryBufferSize;
			this.ignoreInvalidParts = ignoreInvalidParts;
		}

        /// <summary>Gets the mapping of parameters parsed files. The name of a given field maps to the parsed file data.</summary>
        public FilePart[] Files => _files.ToArray(typeof(FilePart)) as FilePart[];

        /// <summary>Gets the parameters. Several ParameterParts may share the same name.</summary>
        public ParameterPart[] Parameters => _parameters.ToArray(typeof(ParameterPart)) as ParameterPart[];

        /// <summary>Parse the stream into a new instance of the <see cref="MultipartFormDataParser" /> class</summary>
        /// <param name="stream">The stream containing the multipart data.</param>
        /// <param name="encoding">The encoding of the multipart data.</param>
        /// <param name="binaryBufferSize">The size of the buffer to use for parsing the multipart form data.</param>
        /// <param name="ignoreInvalidParts">By default the parser will throw an exception if it encounters an invalid part. Set this to true to ignore invalid parts.</param>
        /// <returns>A new instance of the <see cref="MultipartFormDataParser"/> class.</returns>
        public static MultipartFormDataParser Parse(Stream stream, int binaryBufferSize = defaultBufferSize, bool ignoreInvalidParts = false)
        {
            var parser = new MultipartFormDataParser(stream, binaryBufferSize, ignoreInvalidParts);
			parser.Run();
            return parser;
        }

        private void Run()
        {
            var reader = new LineReader(stream, binaryBufferSize);

            boundary = DetectBoundary(reader);
            boundaryBinary = Encoding.UTF8.GetBytes(boundary);

            //we have read until we encountered the boundary so we should be at the first section => parse it!

            while (!readEndBoundary)
                ParseSection(reader);
        }

        private static string DetectBoundary(LineReader reader)
		{
			var line = string.Empty;
			while (line == string.Empty)
			{
				line = reader.ReadLine();
			}

			if (string.IsNullOrEmpty(line)) throw new Exception("Unable to determine boundary: either the stream is empty or we reached the end of the stream");
			else if (!line.StartsWith("--")) throw new Exception("Unable to determine boundary: content does not start with a valid multipart boundary");

			return line.EndsWith("--") ? line.Substring(0, line.Length - 2) : line;
		}

        private void ParseSection(LineReader reader)
        {
            Hashtable parameters = new();

            string line = reader.ReadLine();
            while (line != string.Empty)
            {
                if (line == null || line.StartsWith(boundary))
                    throw new Exception("Unexpected end of section");

                HeaderUtility.ParseHeaders(line, parameters);

                line = reader.ReadLine();
            }

            if (IsFilePart(parameters))
                ParseFilePart(parameters, reader);
            else if (IsParameterPart(parameters))
                ParseParameterPart(parameters, reader);
            else if (ignoreInvalidParts)
                SkipPart(reader);
            else
                throw new Exception("Unable to determine the section type. Some possible reasons include: section is malformed, required parameters such as 'name', 'content-type' or 'filename' are missing, section contains nothing but empty lines.");
        }

        private bool IsFilePart(Hashtable parameters) => parameters.Contains("filename") ||
                parameters.Contains("content-type") ||
                (!parameters.Contains("name") && parameters.Count > 0);

        private bool IsParameterPart(Hashtable parameters) => parameters.Contains("name");

		private void ParseFilePart(Hashtable parameters, LineReader reader)
		{
			// Read the parameters
			parameters.TryGetValue("name", out string name);
			parameters.TryGetValue("filename", out string filename);
			parameters.TryGetValue("content-type", out string contentType);
			parameters.TryGetValue("content-disposition", out string contentDisposition);

			RemoveWellKnownParameters(parameters);

			// Default values if expected parameters are missing
			contentType ??= "text/plain";
			contentDisposition ??= "form-data";

            MemoryStream stream = new();

			while(true)
            {
                var line = reader.ReadByteLine();

                if (CheckForBoundary(line))
                {
                    stream.Position = 0;
					_files.Add(new FilePart(name, filename, stream, parameters, contentType, contentDisposition));
					break;
                }

				stream.Write(line, 0, line.Length);
            }
        }

        private void ParseParameterPart(Hashtable parameters, LineReader reader)
		{
			var sb = new StringBuilder();

            while (true)
            {
                var line = reader.ReadByteLine();

                if (line == null || CheckForBoundary(line))
                {
                    _parameters.Add(new ParameterPart(parameters["name"].ToString(), sb.ToString()));
                    break;
                }

				sb.Append(Encoding.UTF8.GetString(line, 0, line.Length));
            }
		}

		private void SkipPart(LineReader reader)
		{
            while (true)
            {
                var line = reader.ReadByteLine();
                if (line == null || CheckForBoundary(line))
                    break;
            }
        }

        private bool CheckForBoundary(byte[] line)
        {
            if ((line == null))
            {
                readEndBoundary = true;
                return true;
            }

            var length = boundaryBinary.Length;

            if (line.Length < length) return false;

            for (int i = 0; i < length; i++)
                if (line[i] != boundaryBinary[i])
                    return false;

            //if we get here we have a boundary, check if it is the endboundary
            if (line.Length >= length + 2 && line[length] == '-' && line[length + 1] == '-')
                readEndBoundary = true;

            return true;
        }

        private void RemoveWellKnownParameters(Hashtable parameters)
		{
			var wellKnownParameters = new[] { "name", "filename", "filename*", "content-type", "content-disposition" };

			foreach (var parameter in wellKnownParameters)
				if(parameters.Contains(parameter))
					parameters.Remove(parameter);
		}
	}
}
