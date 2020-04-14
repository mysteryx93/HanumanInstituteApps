using System;
using System.IO;
using Moq;
using Xunit;
using EmergenceGuardian.CommonServices;
using EmergenceGuardian.NaturalGroundingPlayer.DataAccess;

namespace EmergenceGuardian.NaturalGroundingPlayer.Business.Test {
    public class DatabaseInitializerTest {
        private IEnvironmentService environment;
        private IAppPathService appPath;
        private IFileSystemService fileSystem;
        private IProcessService process;
        private IVersionAccess versionAccess;
        private readonly IDatabaseInitializer dbInit;

        public DatabaseInitializerTest() {

        }

    }
}