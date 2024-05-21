using MPF.Core.Data;
using SabreTools.RedumpLib.Data;

namespace MPF.Core.Modules.XboxBackupCreator
{
    /// <summary>
    /// Represents a generic set of Xbox Backup Creator parameters
    /// </summary>
    public class Parameters : BaseParameters
    {
        #region Metadata

        /// <inheritdoc/>
        public override InternalProgram InternalProgram => InternalProgram.XboxBackupCreator;

        #endregion

        /// <inheritdoc/>
        public Parameters(string? parameters) : base(parameters) { }

        /// <inheritdoc/>
        public Parameters(RedumpSystem? system, MediaType? type, string? drivePath, string filename, int? driveSpeed, Options options)
            : base(system, type, drivePath, filename, driveSpeed, options)
        {
        }
    }
}
