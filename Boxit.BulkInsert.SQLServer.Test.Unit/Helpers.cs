using System.Data;
using Boxit.BulkInsert.SQLServer.Test.Unit.EntityFramework;
using Boxit.BulkInsert.SQLServer.Test.Unit.EntityFramework.Models;
using EfLocalDbNunit;

namespace Boxit.BulkInsert.SQLServer.Test.Unit;

public static class Helpers
{
    public static async Task<Guid?> GetParentIdAsync(ChildModel child)
    {
        await using var command = LocalDbTestBase<DatabaseContext>.Instance.Database.Connection.CreateCommand();
        command.CommandText = "SELECT ParentId FROM Test.Children WHERE Id = @id";

        var parameter = command.CreateParameter();
        parameter.ParameterName = "@id";
        parameter.Value = child.Id;
        command.Parameters.Add(parameter);

        if (command.Connection!.State != ConnectionState.Open)
            await command.Connection.OpenAsync();

        var result = await command.ExecuteScalarAsync();

        if (result == DBNull.Value || result == null)
            return null;

        return (Guid)result;
    }
}