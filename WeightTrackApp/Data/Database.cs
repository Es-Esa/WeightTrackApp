using SQLite;
using System.Linq.Expressions;

namespace WeightTrackApp.Data
{
    /// <summary>
    /// Provides an asynchronous context for managing SQLite database operations.
    /// </summary>
    public class DatabaseContext : IAsyncDisposable
    {
        /// <summary>
        /// The name of the database file.
        /// </summary>
        private const string DbName = "MyDatabase.db3";

        /// <summary>
        /// The full path to the database file in the application's data directory.
        /// </summary>
        private static string DbPath => Path.Combine(FileSystem.AppDataDirectory, DbName);

        private SQLiteAsyncConnection _connection;

        /// <summary>
        /// Initializes or retrieves the SQLite database connection.
        /// </summary>
        private SQLiteAsyncConnection Database =>
            (_connection ??= new SQLiteAsyncConnection(DbPath,
                SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.SharedCache));

        /// <summary>
        /// Ensures a table for the specified type exists in the database.
        /// </summary>
        /// <typeparam name="TTable">The type of the table to create.</typeparam>
        private async Task CreateTableIfNotExists<TTable>() where TTable : class, new()
        {
            await Database.CreateTableAsync<TTable>();
        }

        /// <summary>
        /// Retrieves a table query object for the specified type.
        /// </summary>
        /// <typeparam name="TTable">The type of the table to query.</typeparam>
        /// <returns>A query object for accessing the table.</returns>
        private async Task<AsyncTableQuery<TTable>> GetTableAsync<TTable>() where TTable : class, new()
        {
            await CreateTableIfNotExists<TTable>();
            return Database.Table<TTable>();
        }

        /// <summary>
        /// Retrieves all records from the specified table.
        /// </summary>
        /// <typeparam name="TTable">The type of the table to query.</typeparam>
        /// <returns>A collection of all records in the table.</returns>
        public async Task<IEnumerable<TTable>> GetAllAsync<TTable>() where TTable : class, new()
        {
            var table = await GetTableAsync<TTable>();
            return await table.ToListAsync();
        }

        /// <summary>
        /// Retrieves records from the specified table that match a given predicate.
        /// </summary>
        /// <typeparam name="TTable">The type of the table to query.</typeparam>
        /// <param name="predicate">The filter condition to apply.</param>
        /// <returns>A collection of records that match the filter condition.</returns>
        public async Task<IEnumerable<TTable>> GetFileteredAsync<TTable>(Expression<Func<TTable, bool>> predicate) where TTable : class, new()
        {
            var table = await GetTableAsync<TTable>();
            return await table.Where(predicate).ToListAsync();
        }

        /// <summary>
        /// Executes a given operation while ensuring the table exists.
        /// </summary>
        /// <typeparam name="TTable">The type of the table to operate on.</typeparam>
        /// <typeparam name="TResult">The type of the result of the operation.</typeparam>
        /// <param name="action">The operation to execute.</param>
        /// <returns>The result of the operation.</returns>
        private async Task<TResult> Execute<TTable, TResult>(Func<Task<TResult>> action) where TTable : class, new()
        {
            await CreateTableIfNotExists<TTable>();
            return await action();
        }

        /// <summary>
        /// Retrieves a record from the specified table by its primary key.
        /// </summary>
        /// <typeparam name="TTable">The type of the table to query.</typeparam>
        /// <param name="primaryKey">The primary key of the record to retrieve.</param>
        /// <returns>The record with the specified primary key.</returns>
        public async Task<TTable> GetItemByKeyAsync<TTable>(object primaryKey) where TTable : class, new()
        {
            return await Execute<TTable, TTable>(async () => await Database.GetAsync<TTable>(primaryKey));
        }

        /// <summary>
        /// Adds a new record to the specified table.
        /// </summary>
        /// <typeparam name="TTable">The type of the table to operate on.</typeparam>
        /// <param name="item">The record to add.</param>
        /// <returns>True if the operation was successful, otherwise false.</returns>
        public async Task<bool> AddItemAsync<TTable>(TTable item) where TTable : class, new()
        {
            return await Execute<TTable, bool>(async () => await Database.InsertAsync(item) > 0);
        }

        /// <summary>
        /// Updates an existing record in the specified table.
        /// </summary>
        /// <typeparam name="TTable">The type of the table to operate on.</typeparam>
        /// <param name="item">The record to update.</param>
        /// <returns>True if the operation was successful, otherwise false.</returns>
        public async Task<bool> UpdateItemAsync<TTable>(TTable item) where TTable : class, new()
        {
            await CreateTableIfNotExists<TTable>();
            return await Database.UpdateAsync(item) > 0;
        }

        /// <summary>
        /// Deletes a record from the specified table.
        /// </summary>
        /// <typeparam name="TTable">The type of the table to operate on.</typeparam>
        /// <param name="item">The record to delete.</param>
        /// <returns>True if the operation was successful, otherwise false.</returns>
        public async Task<bool> DeleteItemAsync<TTable>(TTable item) where TTable : class, new()
        {
            await CreateTableIfNotExists<TTable>();
            return await Database.DeleteAsync(item) > 0;
        }

        /// <summary>
        /// Deletes a record from the specified table by its primary key.
        /// </summary>
        /// <typeparam name="TTable">The type of the table to operate on.</typeparam>
        /// <param name="primaryKey">The primary key of the record to delete.</param>
        /// <returns>True if the operation was successful, otherwise false.</returns>
        public async Task<bool> DeleteItemByKeyAsync<TTable>(object primaryKey) where TTable : class, new()
        {
            await CreateTableIfNotExists<TTable>();
            return await Database.DeleteAsync<TTable>(primaryKey) > 0;
        }

        /// <summary>
        /// Disposes of the database connection asynchronously.
        /// </summary>
        public async ValueTask DisposeAsync() => await _connection?.CloseAsync();
    }
}
