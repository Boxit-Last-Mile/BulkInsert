using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Boxit.BulkInsert.SQLServer;

public static class BulkInsertExtensions
{
    /// <summary>
    /// Adds a big number of models to your database
    /// </summary>
    /// <param name="dbContext">The <see cref="DbContext"/> to which to add the models</param>
    /// <param name="entities">The models to add</param>
    /// <typeparam name="TModel">The type of model</typeparam>
    /// <remarks>
    /// This function runs very fast, because it uses the bulk insert mechanism of SQL-Server
    ///
    /// Important to note:
    /// <ul>
    /// <li>
    /// Since this mechanism doesn't use EF-Core, the models won't be tracked. This is one of the speed improvements. <br/>
    /// </li>
    /// <li>
    /// Collections aren't (yet) supported as navigation properties. <br />
    /// If you have a collection as navigation property, you should leave it empty and bulk-insert the referenced models in a second step. 
    /// </li>
    /// </ul>
    /// </remarks>
    public static async Task BulkInsertAsync<TModel>(this DbContext dbContext, IEnumerable<TModel> entities)
        where TModel : class
    {
        var entityType = dbContext.Model.FindEntityType(typeof(TModel)) ??
                         throw new InvalidOperationException($"Entity type {typeof(TModel).Name} not in EF model");
        
        var connectionString = dbContext.Database.GetDbConnection().ConnectionString;
        var dataReader = new ModelDataReader<TModel>(entities)
            .WithFieldsFromDbContext(dbContext);

        var bulkCopy = new SqlBulkCopy(connectionString);
        bulkCopy.DestinationTableName = $"{entityType.GetSchema()}.{entityType.GetTableName()}";
        bulkCopy.EnableStreaming = true;

        foreach (var field in dataReader.Fields)
        {
            bulkCopy.ColumnMappings.Add(field.Name, field.Name);
        }

        await bulkCopy.WriteToServerAsync(dataReader);
    }
}