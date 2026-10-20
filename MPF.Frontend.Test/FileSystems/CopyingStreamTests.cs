using System;
using System.IO;
using MPF.Frontend.FileSystems;
using Xunit;

namespace MPF.Frontend.Test.FileSystems
{
    public class CopyingStreamTests
    {
        [Fact]
        public void Read_ContinuingStart_Written()
        {
            var output = new MemoryStream();
            var stream = new CopyingStream(new MemoryStream([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]), output);
            var buffer = new byte[4];

            Assert.Equal(4, stream.Read(buffer, 0, 4));
            stream.Seek(8, SeekOrigin.Begin);
            Assert.Equal(2, stream.Read(buffer, 0, 2));
            stream.Seek(2, SeekOrigin.Begin);
            Assert.Equal(4, stream.Read(buffer, 0, 4));
            stream.Seek(0, SeekOrigin.Begin);
            Assert.Equal(2, stream.Read(buffer, 0, 2));

            Assert.Equal(6, stream.Copied);
            Assert.Equal(new byte[] { 1, 2, 3, 4, 5, 6 }, output.ToArray());
        }

        [Fact]
        public void Read_SourceFails_ErrorKept()
        {
            var stream = new CopyingStream(new FailingStream(10, failFrom: 0), new MemoryStream());

            Assert.Throws<IOException>(() => stream.Read(new byte[4], 0, 4));
            Assert.IsType<IOException>(stream.Error);
        }

        [Fact]
        public void Read_OutputFails_ErrorKept()
        {
            // Like a full disk, the output cannot take more than 4 bytes
            var stream = new CopyingStream(new MemoryStream(new byte[10]), new MemoryStream(new byte[4]));

            Assert.Throws<NotSupportedException>(() => stream.Read(new byte[10], 0, 10));
            Assert.IsType<NotSupportedException>(stream.Error);
        }
    }
}
