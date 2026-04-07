# BulkInsert
A simple way of doing bulk inserts for SQLServer

## Usage:

- First add the `Boxit.BulkInsert.SQLServer` package to your solution.
- Then create a collection of models to add.
- Call `BulkInsert(entities)` on your DbContext
- _Optional:_ Call configuration methods on the returned `BulkInsertBuilder`
- Finally call `ExecuteAsync()` on the `BulkInsertBuilder` to run the insert operation

```csharp
var dbContext = serviceProvider.GetRequiredService<MyDbContext>();

var entities = sonSerializer.Deserialize<MyEntity>(someJsonString);

await dbContext.BulkInsert(entities).ExecuteAsync();
```


### Configuration

#### Transactions

To include a bulk insert into a transaction, call `UseTransaction(SqlTransaction)` on the `BulkInsertBuilder`.

```csharp
var dbContext = serviceProvider.GetRequiredService<MyDbContext>();

var entities = sonSerializer.Deserialize<MyEntity>(someJsonString);

var transaction = await dbContext.Database.BeginTransactionAsync();

try {
    await dbContext.BulkInsert(entities)
                   .UseTransaction(transaction)
                   .ExecuteAsync();
    
    // Do more stuff
    
    await transaction.CommitAsync();
} catch (...) {
    // Failure handling
    await transaction.RollbackAsync();
}
```

### Note

This library is pretty crude as of now and will grow with some love & care.

## License

The library is free and open source under the [MIT License](LICENSE).

## Contributing

Feel free to contribute 
- code or documentation updates by forking the repo and creating a pull request
- feature requests by adding an issue tagged as `feature request`
- bug reports by adding an issue tagged as `bug` 

  With bug reports it's easiest if you provide a minimal executable example _or_ create a PR which has the Issue-ID inside that contains an added, failing Test.
  However, this is not needed for bug reports, it just makes it easier for us to understand and reproduce the problem.
