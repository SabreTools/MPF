using SabreTools.RedumpLib.Data;
using Xunit;

namespace MPF.Frontend.Test
{
    public class InterfaceConstantsTests
    {
        [Theory]
        [MemberData(nameof(GenerateGetAllowedDriveSpeedForPhysicalMediaTypeTestData))]
        public void GetAllowedDriveSpeedForPhysicalMediaTypeTest(PhysicalMediaType? mediaType, int maxExpected)
        {
            var actual = InterfaceConstants.GetSpeedsForPhysicalMediaType(mediaType);
            Assert.Equal(maxExpected, actual[^1]);
        }

        /// <summary>
        /// Generate a test set for GetAllowedDriveSpeedForPhysicalMediaTypeTest
        /// </summary>
        public static TheoryData<PhysicalMediaType?, int> GenerateGetAllowedDriveSpeedForPhysicalMediaTypeTestData()
        {
            return new TheoryData<PhysicalMediaType?, int>
            {
                { PhysicalMediaType.CDROM, 72 },
                { PhysicalMediaType.DVD, 24 },
                { PhysicalMediaType.NintendoGameCubeGameDisc, 24 },
                { PhysicalMediaType.NintendoWiiOpticalDisc, 24 },
                { PhysicalMediaType.HDDVD, 24 },
                { PhysicalMediaType.BluRay, 16 },
                { PhysicalMediaType.NintendoWiiUOpticalDisc, 16 },
                { PhysicalMediaType.LaserDisc, 72 },
                { null, 72 },
            };
        }
    }
}
