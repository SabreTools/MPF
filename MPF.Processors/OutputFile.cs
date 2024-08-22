namespace MPF.Processors
{
    /// <summary>
    /// Represents a single output file
    /// </summary>
    internal class OutputFile
    {
        /// <summary>
        /// Set of all filename variants
        /// </summary>
        public string[] Filenames { get; private set; }

        /// <summary>
        /// Indicates if the file is a required output
        /// </summary>
        public bool Required { get; private set; }

        /// <summary>
        /// Indicates if the file should be included as an artifact
        /// </summary>
        public bool IsArtifact { get; private set; }

        /// <summary>
        /// Indicates if the file can be deleted after processing is finished
        /// </summary>
        public bool IsDeleteable { get; private set; }

        /// <summary>
        /// Indicates if the file can be zipped after processing is finished
        /// </summary>
        public bool IsZippable { get; private set; }

        public OutputFile(string filename, bool required, bool isArtifact, bool isDeleteable, bool isZippable)
        {
            Filenames = [filename];
            Required = required;
            IsArtifact = isArtifact;
            IsDeleteable = IsDeleteable;
            IsZippable = IsZippable;
        }
    }
}