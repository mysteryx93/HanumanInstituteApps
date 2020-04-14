using System;
using System.IO;
using Moq;
using Xunit;
using EmergenceGuardian.CommonServices;
using EmergenceGuardian.NaturalGroundingPlayer.DataAccess;

namespace EmergenceGuardian.NaturalGroundingPlayer.Business.Test {
    public class RegistryServiceTest {
        private readonly IRegistryService appPath;

        public RegistryServiceTest() { }

        [Theory]
        [InlineData(true, true, @"C:\Program Files (x86)\MPC-HC\mpc-hc_.exe")]
        [InlineData(true, false, @"C:\Program Files (x86)\MPC-HC\mpc-hc.exe")]
        [InlineData(false, false, @"C:\Program Files (x86)\MPC-HC\mpc-hc.exe")]
        public void GetMpcDefaultPath(bool registryExists, bool fileExists, string expected) {
            // Setup mocks.
            var environment = new Mock<IEnvironmentService>(MockBehavior.Strict);
            environment.Setup(x => x.ProgramFilesX86).Returns(@"C:\Program Files (x86)\");
            var fileSystem = new Mock<IFileSystemService>();
            fileSystem.Setup(x => x.Path.Combine(It.IsAny<string>(), It.IsAny<string>())).Returns((string a, string b) => Path.Combine(a, b));
            fileSystem.Setup(x => x.File.Exists(It.IsAny<string>())).Returns(fileExists);
            var registry = new RegistryService(environment.Object, fileSystem.Object);

            Assert.Equal(expected, registry.MpcExePath);
        }
    }
}