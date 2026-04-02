namespace Boxit.BulkInsert.SQLServer.Test.Unit.EntityFramework.Models;

public class ParentModel
{
    public Guid Id { get; set; }
    public required string Name { get; set; }

    public List<ChildModel> Children { get; set; } = [];
}