using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MPF.Core.Data;
using RedumpLib.Data;

namespace MPF.Modules.Redumper
{
    /// <summary>
    /// Represents a generic set of Redumper parameters
    /// </summary>
    public class Parameters : BaseParameters
    {
        #region Generic Dumping Information

        /// <inheritdoc/>
        public override string InputPath => string.Empty;

        /// <inheritdoc/>
        public override string OutputPath => string.Empty;

        /// <inheritdoc/>
        public override int? Speed
        {
            get { return 1; }
            set { }
        }

        #endregion

        #region Metadata

        /// <inheritdoc/>
        public override InternalProgram InternalProgram => InternalProgram.Redumper;

        #endregion

        #region Flag Values

        // TODO: Fill out

        #endregion

        /// <inheritdoc/>
        public Parameters(string parameters) : base(parameters) { }

        /// <inheritdoc/>
        public Parameters(RedumpSystem? system, MediaType? type, char driveLetter, string filename, int? driveSpeed, Options options)
            : base(system, type, driveLetter, filename, driveSpeed, options)
        {
        }

        #region BaseParameters Implementations

        /// <inheritdoc/>
        public override (bool, List<string>) CheckAllOutputFilesExist(string basePath, bool preCheck)
        {
            // TODO: Fill out
            return (true, new List<string>());
        }

        /// <inheritdoc/>
        public override void GenerateSubmissionInfo(SubmissionInfo info, string basePath, Drive drive, bool includeArtifacts)
        {
            // TODO: Fill in submission info specifics for Redumper
            string outputDirectory = Path.GetDirectoryName(basePath);

            switch (this.Type)
            {
                // Determine type-specific differences
            }

            switch (this.System)
            {
                case RedumpSystem.KonamiPython2:
                    if (GetPlayStationExecutableInfo(drive?.Letter, out string pythonTwoSerial, out Region? pythonTwoRegion, out string pythonTwoDate))
                    {
                        // Ensure internal serial is pulled from local data
                        info.CommonDiscInfo.CommentsSpecialFields[SiteCode.InternalSerialName] = pythonTwoSerial ?? string.Empty;
                        info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? pythonTwoRegion;
                        info.CommonDiscInfo.EXEDateBuildDate = pythonTwoDate;
                    }

                    info.VersionAndEditions.Version = GetPlayStation2Version(drive?.Letter) ?? "";
                    break;

                case RedumpSystem.SonyPlayStation:
                    if (GetPlayStationExecutableInfo(drive?.Letter, out string playstationSerial, out Region? playstationRegion, out string playstationDate))
                    {
                        // Ensure internal serial is pulled from local data
                        info.CommonDiscInfo.CommentsSpecialFields[SiteCode.InternalSerialName] = playstationSerial ?? string.Empty;
                        info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? playstationRegion;
                        info.CommonDiscInfo.EXEDateBuildDate = playstationDate;
                    }

                    break;

                case RedumpSystem.SonyPlayStation2:
                    if (GetPlayStationExecutableInfo(drive?.Letter, out string playstationTwoSerial, out Region? playstationTwoRegion, out string playstationTwoDate))
                    {
                        // Ensure internal serial is pulled from local data
                        info.CommonDiscInfo.CommentsSpecialFields[SiteCode.InternalSerialName] = playstationTwoSerial ?? string.Empty;
                        info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? playstationTwoRegion;
                        info.CommonDiscInfo.EXEDateBuildDate = playstationTwoDate;
                    }

                    info.VersionAndEditions.Version = GetPlayStation2Version(drive?.Letter) ?? "";
                    break;

                case RedumpSystem.SonyPlayStation4:
                    info.VersionAndEditions.Version = GetPlayStation4Version(drive?.Letter) ?? "";
                    break;

                case RedumpSystem.SonyPlayStation5:
                    info.VersionAndEditions.Version = GetPlayStation5Version(drive?.Letter) ?? "";
                    break;
            }
        }

        /// <inheritdoc/>
        public override string GenerateParameters()
        {
            List<string> parameters = new List<string>();

            // TODO: Fill out

            return string.Join(" ", parameters);
        }

        /// <inheritdoc/>
        public override Dictionary<string, List<string>> GetCommandSupport()
        {
            return new Dictionary<string, List<string>>();
        }

        /// <inheritdoc/>
        public override string GetDefaultExtension(MediaType? mediaType) => ".bin"; // TODO: Fill out

        /// <inheritdoc/>
        public override bool IsDumpingCommand()
        {
            switch (this.BaseCommand)
            {
                // TODO: Fill out
                default:
                    return true;
            }
        }

        /// <inheritdoc/>
        protected override void ResetValues()
        {
            BaseCommand = string.Empty; // TODO: Fill out

            flags = new Dictionary<string, bool?>();
        }

        /// <inheritdoc/>
        protected override void SetDefaultParameters(char driveLetter, string filename, int? driveSpeed, Options options)
        {
            // TODO: Fill out
        }

        /// <inheritdoc/>
        protected override bool ValidateAndSetParameters(string parameters)
        {
            // TODO: Fill out
            return true;
        }

        #endregion
    }
}
