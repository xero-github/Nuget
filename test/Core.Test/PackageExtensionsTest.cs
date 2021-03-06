﻿using System.Linq;
using Xunit;
using Xunit.Extensions;

namespace NuGet.Test
{
    public class PackageExtensionsTest
    {
        [Fact]
        public void FindPackagesOverloadLooksForSearchTermsInSpecificFields()
        {
            // Arrange
            var packages = new[] {
                PackageUtility.CreatePackage("Foo.Qux", description: "Some desc"),
                PackageUtility.CreatePackage("X-Package", tags: " lib qux "),
                PackageUtility.CreatePackage("Filtered"),
                PackageUtility.CreatePackage("B", description: "This is a package for qux and not one for baz"),
            };

            // Act
            var result1 = packages.AsQueryable().Find(new[] { "Description", "Tags" }, "Qux");
            var result2 = packages.AsQueryable().Find(new[] { "Id" }, "Filtered");

            // Assert
            Assert.Equal(new[] { packages[1], packages[3] }, result1.ToArray());
            Assert.Equal(new[] { packages[2], }, result2.ToArray());
        }

        [Fact]
        public void FindPackagesOverloadReturnsEmptySequenceIfTermIsNotFoundInProperties()
        {
            // Arrange
            var packages = new[] {
                PackageUtility.CreatePackage("Foo.Qux"),
                PackageUtility.CreatePackage("X-Package", tags: " lib qux "),
                PackageUtility.CreatePackage("Filtered"),
                PackageUtility.CreatePackage("B", description: "This is a package for qux and not one for baz"),
            };

            // Act
            var result1 = packages.AsQueryable().Find(new[] { "Summary" }, "Qux");

            // Assert
            Assert.Empty(result1);
        }


        /// <summary>
        /// Create a package with teh specified language and file, and expect that the file is treated
        /// as a satellite file.
        /// </summary>
        /// <param name="language">The language for the package.</param>
        /// <param name="file">The file expected to be matched as a satellite file.</param>
        [Theory]
        [InlineData(new object[] { "ja-jp", @"lib\ja-jp\assembly.dll" })]
        [InlineData(new object[] { "ja-jp", @"lib\foo\ja-jp\assembly.dll" })]
        [InlineData(new object[] { "ja-jp", @"lib\foo\ja-jp\bar\assembly.dll" })]
        [InlineData(new object[] { "ja-jp", @"lib\ja-jp\bar\assembly.dll" })]
        [InlineData(new object[] { "ja-JP", @"lib\ja-jp\assembly.dll" })] // case mismatch still works though
        public void GetSatelliteFilesReturnsFilesWithAnyCultureSubFolder(string language, string file)
        {
            // Arrange
            var package = PackageUtility.CreatePackage("Foo", "1.0.0", assemblyReferences: new[] { file }, language: language);

            // Act
            var satelliteFiles = package.GetSatelliteFiles();

            // Assert
            Assert.True(satelliteFiles.Select(f => f.Path).Contains(file));
        }

        /// <summary>
        /// Create a package with the specified language and file, and expect that the file is not treated
        /// as a satellite file.
        /// </summary>
        /// <param name="language">The language for the package.</param>
        /// <param name="file">The file expected not to be matched as a satellite file.</param>
        [Theory]
        [InlineData(new object[] { "ja-jp", @"lib\ja-jp" })] // a file with a name that matches the culture
        [InlineData(new object[] { "ja", @"lib\ja-jp\assembly.dll" })] // culture doesn't match
        [InlineData(new object[] { "fr-fr", @"lib\ja-jp\assembly.dll" })] // culture doesn't match
        [InlineData(new object[] { "ja-jp", @"ja-jp\assembly.dll" })] // not in the lib folder
        [InlineData(new object[] { "ja-jp", @"content\ja-jp\assembly.dll" })] // not in the lib folder
        public void GetSatelliteFilesDoesNotReturnFilesOutsideOfCultureSubfolder(string language, string file)
        {
            // Arrange
            var package = PackageUtility.CreatePackage("Foo", "1.0.0", assemblyReferences: new[] { file }, language: language);

            // Act
            var satelliteFiles = package.GetSatelliteFiles();

            // Assert
            Assert.False(satelliteFiles.Select(f => f.Path).Contains(file));
        }
    }
}
