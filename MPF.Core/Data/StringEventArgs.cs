namespace MPF.Core.Data
{
    /// <summary>
    /// String wrapper for event arguments
    /// </summary>
    public class StringEventArgs : System.EventArgs
    {
        public string? Value { get; set; }
    }
}