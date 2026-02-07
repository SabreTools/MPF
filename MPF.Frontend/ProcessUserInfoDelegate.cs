using SabreTools.RedumpLib.Data;

namespace MPF.Frontend
{
    /// <summary>
    /// Determines how user information is processed, if at all
    /// </summary>
    /// <param name="options">Options set that may impact processing</params>
    /// <param name="info">Submission info that may be overwritten</param>
    /// <returns>True for successful updating, false or null otherwise</returns>
    public delegate bool? ProcessUserInfoDelegate(Options? options, ref SubmissionInfo? info);
}
