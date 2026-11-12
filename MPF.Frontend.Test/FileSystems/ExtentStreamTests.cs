using System.IO;
using MPF.Frontend.FileSystems;
using Xunit;

namespace MPF.Frontend.Test.FileSystems
{
    public class ExtentStreamTests
    {
        /// <summary>
        /// Volume of 10 bytes with the values 1 to 10
        /// </summary>
        private static MemoryStream CreateVolume()
            => new([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);

        [Fact]
        public void Read_MultipleExtents_JoinedInOneRead()
        {
            var stream = new ExtentStream(CreateVolume(), [new Extent(6, 2, true), new Extent(1, 3, true)], 5);

            var actual = new byte[8];
            Assert.Equal(5, stream.Read(actual, 0, actual.Length));
            Assert.Equal(0, stream.Read(actual, 0, actual.Length));
            Assert.Equal(new byte[] { 7, 8, 2, 3, 4, 0, 0, 0 }, actual);
        }

        [Fact]
        public void Read_NotRecorded_Zeros()
        {
            var stream = new ExtentStream(CreateVolume(), [new Extent(0, 4, false)], 4);

            var actual = new byte[] { 9, 9, 9, 9 };
            Assert.Equal(4, stream.Read(actual, 0, 4));
            Assert.Equal(new byte[] { 0, 0, 0, 0 }, actual);
        }

        [Fact]
        public void Read_BeyondVolume_Zeros()
        {
            var stream = new ExtentStream(CreateVolume(), [new Extent(8, 4, true)], 4);

            var actual = new byte[] { 9, 9, 9, 9 };
            Assert.Equal(4, stream.Read(actual, 0, 4));
            Assert.Equal(new byte[] { 9, 10, 0, 0 }, actual);
        }

        [Fact]
        public void Seek_End_PositionFromLength()
        {
            var stream = new ExtentStream(CreateVolume(), [new Extent(0, 10, true)], 10);

            stream.Seek(-2, SeekOrigin.End);
            Assert.Equal(9, stream.ReadByte());
        }

        [Fact]
        public void VolumeStream_Length_ReadsLimited()
        {
            var stream = new VolumeStream(CreateVolume(), 4);

            var actual = new byte[8];
            Assert.Equal(4, stream.Read(actual, 0, 8));
            Assert.Equal(0, stream.Read(actual, 0, 8));

            stream.SetLength(6);
            Assert.Equal(2, stream.Read(actual, 0, 8));
            Assert.Equal(new byte[] { 5, 6 }, new[] { actual[0], actual[1] });
        }

        [Fact]
        public void VolumeStream_BlockSize_ReadsWholeBlocksOnce()
        {
            var device = new WholeBlockStream([1, 2, 3, 4, 5, 6, 7, 8, 9, 10], 4);
            var stream = new VolumeStream(device, 10, 4);

            var actual = new byte[4];
            stream.Seek(5, SeekOrigin.Begin);
            Assert.Equal(4, stream.Read(actual, 0, 4));
            Assert.Equal(new byte[] { 6, 7, 8, 9 }, actual);
            Assert.Equal(1, stream.Read(actual, 0, 4));
            Assert.Equal(10, actual[0]);
            Assert.Equal(0, stream.Read(actual, 0, 4));

            stream.Seek(4, SeekOrigin.Begin);
            Assert.Equal(4, stream.Read(actual, 0, 4));
            Assert.Equal(new byte[] { 5, 6, 7, 8 }, actual);
            Assert.Equal(2, device.Reads);
        }

        [Fact]
        public void VolumeStream_BlockSize_EndsWithDevice()
        {
            var device = new WholeBlockStream([1, 2, 3, 4, 5, 6, 7, 8, 9, 10], 4);
            var stream = new VolumeStream(device, 12, 4);

            var actual = new byte[4];
            stream.Seek(8, SeekOrigin.Begin);
            Assert.Equal(2, stream.Read(actual, 0, 4));
            Assert.Equal(new byte[] { 9, 10 }, new[] { actual[0], actual[1] });
        }

        [Fact]
        public void VolumeStream_BlockSize_KeepsLimitedBlocks()
        {
            // 65 blocks of 2 bytes, one more than are kept
            var device = new WholeBlockStream(new byte[130], 2);
            var stream = new VolumeStream(device, 130, 2);

            var actual = new byte[130];
            Assert.Equal(130, stream.Read(actual, 0, 130));
            stream.Seek(0, SeekOrigin.Begin);
            Assert.Equal(2, stream.Read(actual, 0, 2));
            Assert.Equal(66, device.Reads);
        }
    }
}
