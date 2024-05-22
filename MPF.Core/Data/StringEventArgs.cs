namespace MPF.Core.Data
{
    /// <summary>
    /// String wrapper for event arguments
    /// </summary>
    public class StringEventArgs : System.EventArgs
    {
        public string? Value { get; set; }

        /// <summary>
        /// Results can be compared to boolean values based on the success value
        /// </summary>
        public static implicit operator string?(StringEventArgs args) => args.Value;
    }
}