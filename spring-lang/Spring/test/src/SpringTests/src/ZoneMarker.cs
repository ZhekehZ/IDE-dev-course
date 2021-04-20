using System;
using System.IO;
using System.Threading;
using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.Application.Environment;
using JetBrains.ReSharper.TestFramework;
using JetBrains.ReSharper.UnitTestRunner.Xunit;
using JetBrains.TestFramework;
using JetBrains.TestFramework.Application.Zones;
using JetBrains.TestFramework.Utils;
using NUnit.Framework;

[assembly: Apartment(ApartmentState.STA)]
namespace JetBrains.ReSharper.Plugins.SpringTests
{


    [ZoneDefinition]
    class ISpringTestsZone : ITestsEnvZone
    {

    }

    [ZoneActivator]
    class PsiFeatureTestZoneActivator : IActivate<PsiFeatureTestZone> 
    {
        public bool ActivatorEnabled()
        {
            return true;
        }
    }


    [SetUpFixture]
    class PsiFeaturesTestEnvironmentAssembly : ExtensionTestEnvironmentAssembly<ISpringTestsZone>
    {
        static PsiFeaturesTestEnvironmentAssembly()
        {
            //SetJetTestPackagesDir();
        }
        
        private static void SetJetTestPackagesDir()
        {
            if (Environment.GetEnvironmentVariable("JET_TEST_PACKAGES_DIR") == null)
            {
                var packages = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "JetTestPackages");
                if (!Directory.Exists(packages))
                {
                    TestUtil.SetHomeDir(typeof(TestEnvironment).Assembly);
                    var testData = TestUtil.GetTestDataPathBase(typeof(TestEnvironment).Assembly);
                    packages = testData.Parent.Combine("JetTestPackages").FullPath;
                }

                Environment.SetEnvironmentVariable("JET_TEST_PACKAGES_DIR", packages, EnvironmentVariableTarget.Process);
            }
        }
    }
    
    
}