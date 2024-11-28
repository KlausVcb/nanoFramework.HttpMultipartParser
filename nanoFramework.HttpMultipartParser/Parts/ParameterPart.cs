namespace nanoFramework.HttpMultipartParser.Parts
{
    /// <summary>Represents a single parameter extracted from a multipart/form-data stream.</summary>
    public class ParameterPart
    {
        /// <summary>Initializes a new instance of the <see cref="ParameterPart" /> class.</summary>
        /// <param name="name">The name.</param>
        /// <param name="data">The data.</param>
        public ParameterPart(string name, string data)
        {
            Name = name;
            Data = data;
        }

        /// <summary>Gets the data.</summary>
        public string Data { get; }

        /// <summary>Gets the name.</summary>
        public string Name { get; }
    }
}
