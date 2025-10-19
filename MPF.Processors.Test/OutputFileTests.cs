using System;
using System.IO;
using MPF.Processors.OutputFiles;
using Xunit;

namespace MPF.Processors.Test
{
    public class OutputFileTests
    {
        #region Properties

        [Theory]
        [InlineData(OutputFileFlags.None, false)]
        [InlineData(OutputFileFlags.Required, true)]
        [InlineData(OutputFileFlags.Artifact, false)]
        [InlineData(OutputFileFlags.Binary, false)]
        [InlineData(OutputFileFlags.Deleteable, false)]
        [InlineData(OutputFileFlags.Zippable, false)]
        [InlineData(OutputFileFlags.Preserve, false)]
        public void IsRequiredTest(OutputFileFlags flags, bool expected)
        {
            var of = new OutputFile("file", flags, "key");
            var cof = new CustomOutputFile("file", flags, "key", File.Exists);
            var rof = new RegexOutputFile("file", flags, "key");

            Assert.Equal(expected, of.IsRequired);
            Assert.Equal(expected, cof.IsRequired);
            Assert.Equal(expected, rof.IsRequired);
        }

        [Theory]
        [InlineData(OutputFileFlags.None, false)]
        [InlineData(OutputFileFlags.Required, false)]
        [InlineData(OutputFileFlags.Artifact, true)]
        [InlineData(OutputFileFlags.Binary, true)]
        [InlineData(OutputFileFlags.Deleteable, false)]
        [InlineData(OutputFileFlags.Zippable, false)]
        [InlineData(OutputFileFlags.Preserve, false)]
        public void IsArtifactTest(OutputFileFlags flags, bool expected)
        {
            var of = new OutputFile("file", flags, "key");
            var cof = new CustomOutputFile("file", flags, "key", File.Exists);
            var rof = new RegexOutputFile("file", flags, "key");

            Assert.Equal(expected, of.IsArtifact);
            Assert.Equal(expected, cof.IsArtifact);
            Assert.Equal(expected, rof.IsArtifact);
        }

        [Theory]
        [InlineData(OutputFileFlags.None, false)]
        [InlineData(OutputFileFlags.Required, false)]
        [InlineData(OutputFileFlags.Artifact, false)]
        [InlineData(OutputFileFlags.Binary, true)]
        [InlineData(OutputFileFlags.Deleteable, false)]
        [InlineData(OutputFileFlags.Zippable, false)]
        [InlineData(OutputFileFlags.Preserve, false)]
        public void IsBinaryArtifactTest(OutputFileFlags flags, bool expected)
        {
            var of = new OutputFile("file", flags, "key");
            var cof = new CustomOutputFile("file", flags, "key", File.Exists);
            var rof = new RegexOutputFile("file", flags, "key");

            Assert.Equal(expected, of.IsBinaryArtifact);
            Assert.Equal(expected, cof.IsBinaryArtifact);
            Assert.Equal(expected, rof.IsBinaryArtifact);
        }

        [Theory]
        [InlineData(OutputFileFlags.None, false)]
        [InlineData(OutputFileFlags.Required, false)]
        [InlineData(OutputFileFlags.Artifact, false)]
        [InlineData(OutputFileFlags.Binary, false)]
        [InlineData(OutputFileFlags.Deleteable, true)]
        [InlineData(OutputFileFlags.Zippable, false)]
        [InlineData(OutputFileFlags.Preserve, false)]
        public void IsDeleteableTest(OutputFileFlags flags, bool expected)
        {
            var of = new OutputFile("file", flags, "key");
            var cof = new CustomOutputFile("file", flags, "key", File.Exists);
            var rof = new RegexOutputFile("file", flags, "key");

            Assert.Equal(expected, of.IsDeleteable);
            Assert.Equal(expected, cof.IsDeleteable);
            Assert.Equal(expected, rof.IsDeleteable);
        }

        [Theory]
        [InlineData(OutputFileFlags.None, false)]
        [InlineData(OutputFileFlags.Required, false)]
        [InlineData(OutputFileFlags.Artifact, false)]
        [InlineData(OutputFileFlags.Binary, false)]
        [InlineData(OutputFileFlags.Deleteable, false)]
        [InlineData(OutputFileFlags.Zippable, true)]
        [InlineData(OutputFileFlags.Preserve, true)]
        public void IsZippableTest(OutputFileFlags flags, bool expected)
        {
            var of = new OutputFile("file", flags, "key");
            var cof = new CustomOutputFile("file", flags, "key", File.Exists);
            var rof = new RegexOutputFile("file", flags, "key");

            Assert.Equal(expected, of.IsZippable);
            Assert.Equal(expected, cof.IsZippable);
            Assert.Equal(expected, rof.IsZippable);
        }

        [Theory]
        [InlineData(OutputFileFlags.None, false)]
        [InlineData(OutputFileFlags.Required, false)]
        [InlineData(OutputFileFlags.Artifact, false)]
        [InlineData(OutputFileFlags.Binary, false)]
        [InlineData(OutputFileFlags.Deleteable, false)]
        [InlineData(OutputFileFlags.Zippable, false)]
        [InlineData(OutputFileFlags.Preserve, true)]
        public void IsPreservedTest(OutputFileFlags flags, bool expected)
        {
            var of = new OutputFile("file", flags, "key");
            var cof = new CustomOutputFile("file", flags, "key", File.Exists);
            var rof = new RegexOutputFile("file", flags, "key");

            Assert.Equal(expected, of.IsPreserved);
            Assert.Equal(expected, cof.IsPreserved);
            Assert.Equal(expected, rof.IsPreserved);
        }

        #endregion

        #region Exists

        [Fact]
        public void Exists_Empty_False()
        {
            string outputDirectory = string.Empty;
            var of = new OutputFile("pic.bin", OutputFileFlags.None, "key");
            var cof = new CustomOutputFile("pic.bin", OutputFileFlags.None, "key", File.Exists);
            var rof = new RegexOutputFile("pic.bin", OutputFileFlags.None, "key");

            bool ofActual = of.Exists(outputDirectory);
            bool cofActual = cof.Exists(outputDirectory);
            bool rofActual = rof.Exists(outputDirectory);

            Assert.False(ofActual);
            Assert.False(cofActual);
            Assert.False(rofActual);
        }

        [Fact]
        public void Exists_Invalid_False()
        {
            string outputDirectory = "INVALID";
            var of = new OutputFile("pic.bin", OutputFileFlags.None, "key");
            var cof = new CustomOutputFile("pic.bin", OutputFileFlags.None, "key", File.Exists);
            var rof = new RegexOutputFile("pic.bin", OutputFileFlags.None, "key");

            bool ofActual = of.Exists(outputDirectory);
            bool cofActual = cof.Exists(outputDirectory);
            bool rofActual = rof.Exists(outputDirectory);

            Assert.False(ofActual);
            Assert.False(cofActual);
            Assert.False(rofActual);
        }

        [Fact]
        public void Exists_Valid_True()
        {
            string outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "BaseProcessor");
            var of = new OutputFile("pic.bin", OutputFileFlags.None, "key");
            var cof = new CustomOutputFile("pic.bin", OutputFileFlags.None, "key", File.Exists);
            var rof = new RegexOutputFile("pic.bin", OutputFileFlags.None, "key");

            bool ofActual = of.Exists(outputDirectory);
            bool cofActual = cof.Exists(outputDirectory);
            bool rofActual = rof.Exists(outputDirectory);

            Assert.True(ofActual);
            Assert.True(cofActual);
            Assert.True(rofActual);
        }

        #endregion

        #region GetPaths

        [Fact]
        public void GetPaths_Empty_Empty()
        {
            string outputDirectory = string.Empty;
            var of = new OutputFile("pic.bin", OutputFileFlags.None, "key");
            var cof = new CustomOutputFile("pic.bin", OutputFileFlags.None, "key", File.Exists);
            var rof = new RegexOutputFile("pic.bin", OutputFileFlags.None, "key");

            var ofActual = of.GetPaths(outputDirectory);
            var cofActual = cof.GetPaths(outputDirectory);
            var rofActual = rof.GetPaths(outputDirectory);

            Assert.Empty(ofActual);
            Assert.Empty(cofActual);
            Assert.Empty(rofActual);
        }

        [Fact]
        public void GetPaths_Invalid_Empty()
        {
            string outputDirectory = "INVALID";
            var of = new OutputFile("pic.bin", OutputFileFlags.None, "key");
            var cof = new CustomOutputFile("pic.bin", OutputFileFlags.None, "key", File.Exists);
            var rof = new RegexOutputFile("pic.bin", OutputFileFlags.None, "key");

            var ofActual = of.GetPaths(outputDirectory);
            var cofActual = cof.GetPaths(outputDirectory);
            var rofActual = rof.GetPaths(outputDirectory);

            Assert.Empty(ofActual);
            Assert.Empty(cofActual);
            Assert.Empty(rofActual);
        }

        [Fact]
        public void GetPaths_Valid_Filled()
        {
            string outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "BaseProcessor");
            var of = new OutputFile("pic.bin", OutputFileFlags.None, "key");
            var cof = new CustomOutputFile("pic.bin", OutputFileFlags.None, "key", File.Exists);
            var rof = new RegexOutputFile("pic.bin", OutputFileFlags.None, "key");

            var ofActual = of.GetPaths(outputDirectory);
            var cofActual = cof.GetPaths(outputDirectory);
            var rofActual = rof.GetPaths(outputDirectory);

            Assert.Single(ofActual);
            Assert.Single(cofActual);
            Assert.Single(rofActual);
        }

        #endregion
    }
}