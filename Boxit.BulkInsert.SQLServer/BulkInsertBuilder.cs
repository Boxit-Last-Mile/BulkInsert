using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;

// ReSharper disable MemberCanBePrivate.Global - This is a public library for consumption by other projects

namespace Boxit.BulkInsert.SQLServer;

public class BulkInsertBuilder<TModel>
{
    private SqlTransaction? _transaction;
    private readonly DbContext _dbContext;
    private readonly IEnumerable<TModel> _entities;
    private readonly IEntityType _entityType;

    internal BulkInsertBuilder(DbContext dbContext, IEnumerable<TModel> entities, IEntityType entityType)
    {
        _dbContext = dbContext;
        _entities = entities;
        _entityType = entityType;
    }

    /// <summary>
    /// Makes the bulk insert using an existing connection/transaction-pair
    /// </summary>
    /// <param name="transaction">The transaction to make the bulk-insert in</param>
    public BulkInsertBuilder<TModel> UseTransaction(SqlTransaction transaction)
    {
        _transaction = transaction;

        return this;
    }

    /// <summary>
    /// Executes the configured bulk insert
    /// </summary>
    public async Task ExecuteAsync()
    {
        var dataReader = new ModelDataReader<TModel>(_entities)
            .WithFieldsFromDbContext(_dbContext);

        var bulkCopy = _transaction is null 
            ? new SqlBulkCopy((SqlConnection)_dbContext.Database.GetDbConnection()) 
            : new SqlBulkCopy((SqlConnection)_dbContext.Database.GetDbConnection(), SqlBulkCopyOptions.Default,  _transaction);
        
        bulkCopy.DestinationTableName = $"{_entityType.GetSchema()}.{_entityType.GetTableName()}";
        bulkCopy.EnableStreaming = true;

        foreach (var field in dataReader.Fields)
        {
            bulkCopy.ColumnMappings.Add(field.Name, field.Name);
        }

        await bulkCopy.WriteToServerAsync(dataReader);
    }

    public BulkInsertBuilder<TModel> UseTransaction(IDbContextTransaction transaction)
    {
        return UseTransaction((SqlTransaction)transaction.GetDbTransaction());
    }
}