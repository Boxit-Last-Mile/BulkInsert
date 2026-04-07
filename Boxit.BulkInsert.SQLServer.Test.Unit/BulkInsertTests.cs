using Boxit.BulkInsert.SQLServer.Test.Unit.EntityFramework;
using Boxit.BulkInsert.SQLServer.Test.Unit.EntityFramework.Models;
using EfLocalDbNunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Boxit.BulkInsert.SQLServer.Test.Unit;

/// <summary>
/// Tests the basic bulk insertion feature
/// </summary>
public class BulkInsertTests : LocalDbTestBase<DatabaseContext>
{
    [Test]
    public async Task BulkInsert_WithEmptyEnumerable_InsertsNothing()
    {
        await ActData.BulkInsert<ChildModel>([]).ExecuteAsync();

        var children = await AssertData.Children.ToArrayAsync();
        var parents = await AssertData.Parents.ToArrayAsync();

        children.Should().BeEmpty();
        parents.Should().BeEmpty();
    }

    [Test]
    public async Task BulkInsert_WithParentNotInDb_InsertsWithParentId()
    {
        var parent = new ParentModel { Id = Guid.NewGuid(), Name = "Parent" };
        var child = new ChildModel { Name = "Child", Parent = parent };

        await ActData.BulkInsert<ChildModel>([child]).ExecuteAsync();

        var children = await AssertData.Children.ToArrayAsync();
        var parents = await AssertData.Parents.ToArrayAsync();

        var savedChild = children.Should().ContainSingle().Subject;
        savedChild.Name.Should().Be(child.Name);
        savedChild.Id.Should().NotBeEmpty();
        (await Helpers.GetParentIdAsync(savedChild)).Should().Be(parent.Id);

        parents.Should().BeEmpty();
    }

    [Test]
    public async Task BulkInsert_WithParentNotTracked_InsertsWithParentId()
    {
        var parent = new ParentModel { Id = Guid.NewGuid(), Name = "Parent" };
        var child = new ChildModel { Name = "Child", Parent = parent };

        await ActData.BulkInsert<ParentModel>([parent]).ExecuteAsync();
        await ActData.BulkInsert<ChildModel>([child]).ExecuteAsync();

        var children = await AssertData.Children.ToArrayAsync();

        var savedChild = children.Should().ContainSingle().Subject;
        savedChild.Name.Should().Be(child.Name);
        savedChild.Id.Should().NotBeEmpty();
        (await Helpers.GetParentIdAsync(savedChild)).Should().Be(parent.Id);
    }

    [Test]
    public async Task BulkInsert_WithParentTracked_InsertsWithParentId()
    {
        var parent = new ParentModel { Id = Guid.NewGuid(), Name = "Parent" };

        parent = (await ArrangeData.Parents.AddAsync(parent)).Entity;
        await ArrangeData.SaveChangesAsync();

        var child = new ChildModel { Name = "Child", Parent = parent };
        await ActData.BulkInsert<ChildModel>([child]).ExecuteAsync();

        var children = await AssertData.Children.ToArrayAsync();

        var savedChild = children.Should().ContainSingle().Subject;
        savedChild.Name.Should().Be(child.Name);
        savedChild.Id.Should().NotBeEmpty();
        (await Helpers.GetParentIdAsync(savedChild)).Should().Be(parent.Id);
    }

    [Test]
    public async Task BulkInsert_WithMultipleEntities_InsertsAll()
    {
        var parent = new ParentModel { Id = Guid.NewGuid(), Name = "Parent" };

        parent = (await ArrangeData.Parents.AddAsync(parent)).Entity;
        await ArrangeData.SaveChangesAsync();

        var children = new ChildModel[]
        {
            new() { Name = "Child 1", Parent = parent },
            new() { Name = "Child 2", Parent = parent },
            new() { Name = "Child 3", Parent = parent },
            new() { Name = "Child 4", Parent = parent },
            new() { Name = "Child 5", Parent = parent },
        };

        await ActData.BulkInsert(children).ExecuteAsync();

        var savedChildren = await AssertData.Children.ToArrayAsync();

        savedChildren.Select(x => x.Name).Should().BeEquivalentTo(children.Select(x => x.Name));
        savedChildren.Select(x => x.Id).Should().AllSatisfy(id => id.Should().NotBeEmpty());
        savedChildren.Should().AllSatisfy(child => Helpers.GetParentIdAsync(child).Result.Should().Be(parent.Id));
    }

    [Test]
    public async Task BulkInsert_WithMultipleParents_SelectsParentIdCorrectly()
    {
        var parents = new ParentModel[]
        {
            new() { Id = Guid.NewGuid(), Name = "Parent 1" },
            new() { Id = Guid.NewGuid(), Name = "Parent 2" },
            new() { Id = Guid.NewGuid(), Name = "Parent 3" },
            new() { Id = Guid.NewGuid(), Name = "Parent 4" },
            new() { Id = Guid.NewGuid(), Name = "Parent 5" },
        };

        await ArrangeData.Parents.AddRangeAsync(parents);
        await ArrangeData.SaveChangesAsync();

        var children = new ChildModel[]
        {
            new() { Name = "Child 1", Parent = parents[0] },
            new() { Name = "Child 2", Parent = parents[1] },
            new() { Name = "Child 3", Parent = parents[2] },
            new() { Name = "Child 4", Parent = parents[3] },
            new() { Name = "Child 5", Parent = parents[4] },
        };

        await ActData.BulkInsert(children).ExecuteAsync();

        var savedChildren = await AssertData.Children.ToArrayAsync();
        savedChildren.Select(x => x.Name).Should().BeEquivalentTo(children.Select(x => x.Name));
        savedChildren.Select(x => x.Id).Should().AllSatisfy(id => id.Should().NotBeEmpty());
        savedChildren.Select(x => Helpers.GetParentIdAsync(x).Result).Should()
            .BeEquivalentTo(parents.Select(x => x.Id));
    }

    [Test]
    public async Task BulkInsert_ParentsAndChildren_IsRetrievable()
    {
        var parents = new ParentModel[]
        {
            new() { Name = "Parent 1" },
            new() { Name = "Parent 2" },
            new() { Name = "Parent 3" },
            new() { Name = "Parent 4" },
            new() { Name = "Parent 5" },
        };

        var children = new ChildModel[]
        {
            new() { Name = "Child 1", Parent = parents[0] },
            new() { Name = "Child 2", Parent = parents[1] },
            new() { Name = "Child 3", Parent = parents[2] },
            new() { Name = "Child 4", Parent = parents[3] },
            new() { Name = "Child 5", Parent = parents[4] },
        };

        await ActData.BulkInsert(parents).ExecuteAsync();
        await ActData.BulkInsert(children).ExecuteAsync();

        var savedChildren = await AssertData.Children.Include(x => x.Parent).ToArrayAsync();
        savedChildren.Select(x => x.Parent.Name).Should().BeEquivalentTo(parents.Select(x => x.Name));
    }
}
