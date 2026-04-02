# BulkInsert
A simple way of doing bulk inserts for SQLServer

## Usage:

- First add the `Boxit.BulkInsert.SQLServer` package to your solution.
- Then create a collection of models to add.
- Finally call `BulkInsertAsync(entities)` on your DbContext

### Example

```csharp
var dbContext = serviceProvider.GetRequiredService<MyDbContext>();

var entities = sonSerializer.Deserialize<MyEntity>(someJsonString);

await dbContext.BulkInsertAsync(entities);
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
