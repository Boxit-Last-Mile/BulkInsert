using System.Runtime.CompilerServices;
using Boxit.BulkInsert.SQLServer.Test.Unit.EntityFramework;
using EfLocalDbNunit;

namespace Boxit.BulkInsert.SQLServer.Test.Unit;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        VerifierSettings.InitializePlugins();
        LocalDbLogging.EnableVerbose();
        LocalDbSettings.ConnectionBuilder(_ => _.ConnectTimeout = 300);
        LocalDbTestBase<DatabaseContext>.Initialize();
    }
}