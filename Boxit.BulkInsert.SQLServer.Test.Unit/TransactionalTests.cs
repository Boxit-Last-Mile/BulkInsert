using Boxit.BulkInsert.SQLServer.Test.Unit.EntityFramework;
using Boxit.BulkInsert.SQLServer.Test.Unit.EntityFramework.Models;
using EfLocalDbNunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Boxit.BulkInsert.SQLServer.Test.Unit;

/// <summary>
/// These tests check the behavior when using a transaction
/// </summary>
public class TransactionalTests : LocalDbTestBase<DatabaseContext>
{
    [Test]
    public async Task BulkInsert_WhenTransactionIsCommitted_DoesChangeDatabase()
    {
        var parents = new ParentModel[]
        {
            new() { Name = "Parent 1" },
            new() { Name = "Parent 2" },
            new() { Name = "Parent 3" },
            new() { Name = "Parent 4" },
            new() { Name = "Parent 5" },
        };

        var transaction = await ActData.Database.BeginTransactionAsync();

        await ActData.BulkInsert(parents)
            .UseTransaction(transaction)
            .ExecuteAsync();

        await transaction.CommitAsync();
        
        var savedParents = await AssertData.Parents.ToArrayAsync();
        savedParents.Should().BeEquivalentTo(parents);
    }
    
    [Test]
    public async Task BulkInsert_WhenTransactionIsReverted_DoesNotChangeDatabase()
    {
        var parents = new ParentModel[]
        {
            new() { Name = "Parent 1" },
            new() { Name = "Parent 2" },
            new() { Name = "Parent 3" },
            new() { Name = "Parent 4" },
            new() { Name = "Parent 5" },
        };

        var transaction = await ActData.Database.BeginTransactionAsync();

        await ActData.BulkInsert(parents)
            .UseTransaction(transaction)
            .ExecuteAsync();

        await transaction.RollbackAsync();
        
        var savedParents = await AssertData.Parents.ToArrayAsync();
        savedParents.Should().BeEmpty();
    }
}