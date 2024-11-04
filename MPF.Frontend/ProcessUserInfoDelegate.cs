using SabreTools.RedumpLib.Data;

namespace MPF.Frontend
{
    /// <summary>
    /// Determines how user information is processed, if at all
    /// </summary>
    /// <param name="info">Submission info that may be overwritten</param>
    /// <returns>True for successful updating, false or null otherwise</returns>
    public delegate bool? ProcessUserInfoDelegate(ref SubmissionInfo? info);
}