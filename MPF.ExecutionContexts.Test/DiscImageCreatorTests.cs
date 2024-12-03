using System.Collections.Generic;
using MPF.ExecutionContexts.DiscImageCreator;
using Xunit;

namespace MPF.ExecutionContexts.Test
{
    public class DiscImageCreatorTests
    {
        [Fact]
        public void DiscImageCreatorAudioParametersTest()
        {
            string originalParameters = "audio F \"ISO\\Audio CD\\Audio CD.bin\" 72 -5 0";

            // Validate that a common audio commandline is parsed
            var executionContext = new ExecutionContext(originalParameters);
            Assert.NotNull(executionContext);

            // Validate that the same set of parameters are generated on the output
            var newParameters = executionContext.GenerateParameters();
            Assert.NotNull(newParameters);
            Assert.Equal(originalParameters, newParameters);
        }

        [Fact]
        public void DiscImageCreatorDataParametersTest()
        {
            string originalParameters = "data F \"ISO\\Data CD\\Data CD.bin\" 72 -5 0";

            // Validate that a common audio commandline is parsed
            var executionContext = new ExecutionContext(originalParameters);
            Assert.NotNull(executionContext);

            // Validate that the same set of parameters are generated on the output
            var newParameters = executionContext.GenerateParameters();
            Assert.NotNull(newParameters);
            Assert.Equal(originalParameters, newParameters);
        }

        /// <summary>
        /// Generate a HashSet of keys that are considered to be set
        /// </summary>
        /// <param name="executionContext">ExecutionContext object representing how to invoke the internal program</param>
        /// <returns>HashSet representing the strings</returns>
        private static HashSet<string> GenerateUsedKeys(ExecutionContext executionContext)
        {
            var usedKeys = new HashSet<string>();
            if (executionContext?.Keys == null)
                return usedKeys;

            foreach (string key in executionContext.Keys)
            {
                if (executionContext[key] == true)
                    usedKeys.Add(key);
            }

            return usedKeys;
        }
    }
}
