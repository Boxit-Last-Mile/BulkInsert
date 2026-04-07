namespace Boxit.BulkInsert.SQLServer.Test.Unit.EntityFramework.Models;

public class ChildModel
{
    public Guid Id { get; set; }
    public required string Name { get; set; }

    public required ParentModel Parent { get; set; }
}
