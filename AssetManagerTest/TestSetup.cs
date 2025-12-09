using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AssetManagerTest;

[TestClass]
public class TestSetup
{
    [AssemblyInitialize]
    public static void AssemblyInitialize(TestContext context)
    {
        DatabaseConnection.Initialize(
            "Server=DITSERVERNAVN;Database=AssetManagement_Test;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;"
        );
    }
}

