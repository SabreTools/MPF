namespace MPF.Frontend
{
    /// <summary>
    /// Abstraction for a live tool-output console shown by a GUI frontend while a
    /// dumping program runs.
    /// </summary>
    /// <remarks>
    /// Implemented by a frontend that can display the tool's redirected output in its
    /// own surface (for example, a separate window on Linux, where the dumping tool has
    /// no console window of its own). Frontends that rely on the tool drawing its own
    /// console window (Windows) leave this null, preserving the original behavior.
    /// </remarks>
    public interface IToolOutputConsole
    {
        /// <summary>
        /// Show the console. Called on the UI thread just before the tool starts.
        /// </summary>
        public void Open();

        /// <summary>
        /// Receive a raw chunk of the tool's standard output or error.
        /// </summary>
        /// <param name="chunk">Raw output chunk, with carriage returns preserved</param>
        /// <remarks>
        /// Thread-safe: called from background reader threads, so implementations must
        /// not touch UI state directly here.
        /// </remarks>
        public void Append(string chunk);

        /// <summary>
        /// Signal that the tool process has exited. Called on the UI thread.
        /// </summary>
        /// <remarks>
        /// The implementation closes the console or leaves it open per the user's
        /// preference.
        /// </remarks>
        public void NotifyToolExited();
    }
}
