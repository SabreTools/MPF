using System.Collections.Generic;
using SabreTools.RedumpLib.Data;
using Xunit;

namespace MPF.Frontend.Test
{
    public class InterfaceConstantsTests
    {
        public static readonly List<object?[]> GetAllowedDriveSpeedForPhysicalMediaTypeTestData =
        [
            [PhysicalMediaType.CDROM, 72],
            [PhysicalMediaType.DVD, 24],
            [PhysicalMediaType.NintendoGameCubeGameDisc, 24],
            [PhysicalMediaType.NintendoWiiOpticalDisc, 24],
            [PhysicalMediaType.HDDVD, 24],
            [PhysicalMediaType.BluRay, 16],
            [PhysicalMediaType.NintendoWiiUOpticalDisc, 16],
            [PhysicalMediaType.LaserDisc, 72],
            [null, 72],
        ];

        [Theory]
        [MemberData(nameof(GetAllowedDriveSpeedForPhysicalMediaTypeTestData))]
        public void GetAllowedDriveSpeedForPhysicalMediaTypeTest(PhysicalMediaType? mediaType, int maxExpected)
        {
            var actual = InterfaceConstants.GetSpeedsForPhysicalMediaType(mediaType);
            Assert.Equal(maxExpected, actual[^1]);
        }
    }
}
