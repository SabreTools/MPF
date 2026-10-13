using MPF.Frontend.Tools;
using Xunit;

namespace MPF.Frontend.Test.Tools
{
    public class TerminalOutputProcessorTests
    {
        #region Process

        [Fact]
        public void Process_ShorterLineAfterLongerLine_HasNoLeftovers()
        {
            var processor = new TerminalOutputProcessor();

            processor.Process("a long first line\nab\n");

            Assert.Equal(new[] { "a long first line", "ab" }, processor.CommittedLines);
            Assert.Equal(string.Empty, processor.CurrentLine);
        }

        [Fact]
        public void Process_CarriageReturn_OverwritesLine()
        {
            var processor = new TerminalOutputProcessor();

            processor.Process("50%\r100%\n");

            Assert.Equal(new[] { "100%" }, processor.CommittedLines);
        }

        [Fact]
        public void Process_EraseEntireLine_ClearsLine()
        {
            var processor = new TerminalOutputProcessor();

            processor.Process("progress 50%\r\x1b[2Kdone\n");

            Assert.Equal(new[] { "done" }, processor.CommittedLines);
        }

        [Fact]
        public void Process_ColorSequences_AreIgnored()
        {
            var processor = new TerminalOutputProcessor();

            processor.Process("\x1b[32mgreen\x1b[0m\n");

            Assert.Equal(new[] { "green" }, processor.CommittedLines);
        }

        #endregion

        #region Flush

        [Fact]
        public void Flush_AfterCommittedLine_DoesNotCommitAgain()
        {
            var processor = new TerminalOutputProcessor();

            processor.Process("done\n");
            processor.Flush();

            Assert.Equal(new[] { "done" }, processor.CommittedLines);
        }

        [Fact]
        public void Flush_PartialLine_CommitsIt()
        {
            var processor = new TerminalOutputProcessor();

            processor.Process("no newline");
            processor.Flush();

            Assert.Equal(new[] { "no newline" }, processor.CommittedLines);
            Assert.Equal(string.Empty, processor.CurrentLine);
        }

        #endregion
    }
}
