using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Boxit.BulkInsert.SQLServer;

/// <summary>
/// This class enables the <see cref="SqlBulkCopy"/> class to operate on a <see cref="IEnumerable{T}"/> 
/// </summary>
/// <typeparam name="TModel">The type of model</typeparam>
internal class ModelDataReader<TModel> : IDataReader {
    public record Field(string Name, Func<TModel, object?> Accessor);

    private readonly List<Field> _fields = [];
    private IEnumerator<TModel>? _enumerator;

    internal IReadOnlyCollection<Field> Fields => _fields.AsReadOnly();

    private static object? GetForeignKeyValue(TModel entity, PropertyInfo navigationProperty, PropertyInfo principalKeyProperty) {
        var principalEntity = navigationProperty.GetValue(entity);
        return principalEntity == null
            ? null
            : principalKeyProperty.GetValue(principalEntity);
    }

    public ModelDataReader(IEnumerable<TModel> entities) {
        var data = entities.ToArray();
        RecordsAffected = data.Length;
        _enumerator = (data as IEnumerable<TModel>).GetEnumerator();
    }

    /// <summary>
    /// Reads information that EF has about the model from the <see cref="DbContext"/>
    /// </summary>
    /// <param name="dbContext">The <see cref="DbContext"/> to read the information from</param>
    /// <returns>Itself to support a fluent notation</returns>
    /// <exception cref="InvalidOperationException">If the given <see cref="DbContext"/> does not know <see cref="TModel"/></exception>
    /// <exception cref="NotSupportedException">
    /// If the model uses one of these features:
    /// <ul>
    /// <li>Composite foreign keys</li>
    /// <li>Foreign keys as shadow-property without a corresponding navigation property</li>
    /// </ul>
    /// </exception>
    public ModelDataReader<TModel> WithFieldsFromDbContext(DbContext dbContext) {
        var entityType = dbContext.Model.FindEntityType(typeof(TModel)) ??
                         throw new InvalidOperationException($"Entity type {typeof(TModel).Name} not in EF model");
        var tableId = StoreObjectIdentifier.Create(entityType, StoreObjectType.Table);
        var selector = dbContext.GetService<IValueGeneratorSelector>();

        _fields.Clear();
        foreach (var prop in entityType.GetProperties()) {
            var columnName = tableId.HasValue
                ? prop.GetColumnName(tableId.Value) ?? prop.Name
                : prop.Name;

            Func<TModel, object?> accessor;

            var generatedValue = prop.ValueGenerated is ValueGenerated.OnAdd or ValueGenerated.OnAddOrUpdate || prop.GetValueGeneratorFactory() != null;

            if (generatedValue && selector.TrySelect(prop, entityType, out var generator)) {
                accessor = entity => generator!.Next(dbContext.Entry(entity!));
            } else if (!prop.IsShadowProperty()) {
                // Regular CLR property
                accessor = entity => prop.PropertyInfo!.GetValue(entity);
            } else {
                // Shadow property → check if this is a foreign key field

                // Find the foreign key that uses this shadow property
                var fk = entityType.GetForeignKeys()
                                   .FirstOrDefault(f => f.Properties.Contains(prop));

                // Get navigation to referenced entity
                var navigation = fk?.DependentToPrincipal;

                if (navigation is null) {
                    // Shadow property is not a foreign key -> Just get the value 
                    accessor = entity => prop.PropertyInfo!.GetValue(entity);
                } else {
                    // Get primary key property of referenced entity
                    var principalKeyProperty = fk?.PrincipalKey.Properties.SingleOrDefault()?.PropertyInfo;
                    if (principalKeyProperty == null) {
                        throw new NotSupportedException("Can only work with non-composite foreign keys");
                    }

                    // Create method to create foreign key from referenced object
                    var navigationProperty = navigation.PropertyInfo;
                    if (navigationProperty is null) {
                        throw new NotSupportedException("Cannot work with foreign keys that don't have a navigation property.");
                    }

                    accessor = entity => GetForeignKeyValue(entity, navigationProperty, principalKeyProperty);
                }
            }

            WithField(columnName, accessor);
        }

        return this;
    }

    /// <summary>
    /// Beschreibt ein Datenbankfeld für den Modeltypen, welches in der Datenbank gleich heißt, wie die Property am Model
    /// </summary>
    /// <param name="fieldExpression">Eine Funktion, die das Feld aus dem Model liest</param>
    /// <typeparam name="TField">Der Typ der Feld-Property</typeparam>
    /// <returns>Den <see cref="ModelDataReader{TModel}"/>, um eine Fluent-Schreibweise zu ermöglichen</returns>
    public ModelDataReader<TModel> WithField<TField>(Expression<Func<TModel, TField?>> fieldExpression) {
        var accessor = fieldExpression.Compile();
        var name = ExtractPropertyNameFromLambda(fieldExpression);
        return WithField(name, accessor);
    }

    /// <summary>
    /// Beschreibt ein Datenbankfeld für den Typ des Models, welches in der Datenbank anders heißt, wie die Property am Model
    /// </summary>
    /// <param name="name">Der Feldname in der Datenbank</param>
    /// <param name="accessor">Eine Funktion, die das Feld aus dem Model liest</param>
    /// <typeparam name="TField">Der Typ der Feld-Property</typeparam>
    /// <returns>Den <see cref="ModelDataReader{TModel}"/>, um eine Fluent-Schreibweise zu ermöglichen</returns>
    public ModelDataReader<TModel> WithField<TField>(string name, Func<TModel, TField?> accessor) {
        _fields.Add(new Field(name, x => accessor(x)));
        return this;
    }

    internal static string ExtractPropertyNameFromLambda(LambdaExpression expression) {
        ArgumentNullException.ThrowIfNull(expression);

        var memberExpression = expression.Body as MemberExpression;
        return memberExpression == null
            ? throw new ArgumentException("Expression must access a member", nameof(expression))
            : memberExpression.Member.Name;
    }

    public bool GetBoolean(int i) {
        throw new NotSupportedException();
    }

    public byte GetByte(int i) {
        throw new NotSupportedException();
    }

    public long GetBytes(int i, long fieldOffset, byte[]? buffer, int bufferoffset, int length) {
        throw new NotSupportedException();
    }

    public char GetChar(int i) {
        throw new NotSupportedException();
    }

    public long GetChars(int i, long fieldoffset, char[]? buffer, int bufferoffset, int length) {
        throw new NotSupportedException();
    }

    public IDataReader GetData(int i) {
        throw new NotSupportedException();
    }

    public string GetDataTypeName(int i) {
        throw new NotSupportedException();
    }

    public DateTime GetDateTime(int i) {
        throw new NotSupportedException();
    }

    public decimal GetDecimal(int i) {
        throw new NotSupportedException();
    }

    public double GetDouble(int i) {
        throw new NotSupportedException();
    }

    public Type GetFieldType(int i) {
        throw new NotSupportedException();
    }

    public float GetFloat(int i) {
        throw new NotSupportedException();
    }

    public Guid GetGuid(int i) {
        throw new NotSupportedException();
    }

    public short GetInt16(int i) {
        throw new NotSupportedException();
    }

    public int GetInt32(int i) {
        throw new NotSupportedException();
    }

    public long GetInt64(int i) {
        throw new NotSupportedException();
    }

    public string GetName(int i) {
        throw new NotSupportedException();
    }

    public int GetOrdinal(string name) {
        return _fields.FindIndex(x => x.Name == name);
    }

    public string GetString(int i) {
        throw new NotSupportedException();
    }

    public object GetValue(int i) {
        return _fields[i].Accessor(_enumerator!.Current) ?? DBNull.Value;
    }

    public int GetValues(object[] values) {
        throw new NotSupportedException();
    }

    public bool IsDBNull(int i) {
        return _fields[i].Accessor(_enumerator!.Current) == null;
    }

    public int FieldCount => _fields.Count;

    public object this[int i] => throw new NotSupportedException();

    public object this[string name] => throw new NotSupportedException();

    public void Close() {
        _enumerator = null;
    }

    public DataTable GetSchemaTable() {
        throw new NotSupportedException();
    }

    public bool NextResult() {
        throw new NotSupportedException();
    }

    public bool Read() {
        return _enumerator!.MoveNext();
    }

    public int Depth => throw new NotSupportedException();

    public bool IsClosed => _enumerator == null;
    public int RecordsAffected { get; }

    public void Dispose() {
        _enumerator?.Dispose();
    }
}