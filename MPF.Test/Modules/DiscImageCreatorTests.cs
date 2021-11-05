using MPF.Modules.DiscImageCreator;
using Xunit;

namespace MPF.Test.Modules
{
    public class DiscImageCreatorTests
    {
        [Fact]
        public void DiscImageCreatorAudioParametersTest()
        {
            string originalParameters = "audio F \"ISO\\Audio CD\\Audio CD.bin\" 72 -5 0";

            // Validate that a common audio commandline is parsed
            var parametersObject = new Parameters(originalParameters);
            Assert.NotNull(parametersObject);

            // Validate that the same set of parameters are generated on the output
            string newParameters = parametersObject.GenerateParameters();
            Assert.NotNull(newParameters);
            Assert.Equal(originalParameters, newParameters);
        }

        [Fact]
        public void DiscImageCreatorDataParametersTest()
        {
            string originalParameters = "data F \"ISO\\Data CD\\Data CD.bin\" 72 -5 0";

            // Validate that a common audio commandline is parsed
            var parametersObject = new Parameters(originalParameters);
            Assert.NotNull(parametersObject);

            // Validate that the same set of parameters are generated on the output
            string newParameters = parametersObject.GenerateParameters();
            Assert.NotNull(newParameters);
            Assert.Equal(originalParameters, newParameters);
        }
    }
}
