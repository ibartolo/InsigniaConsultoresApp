using InsigniaConsultoresApp.Models;
using Microsoft.Maui.Storage;
using SQLite;

namespace InsigniaConsultoresApp.Services.Storage
{
    /// <summary>
    /// Fila de sesión persistida en SQLite (metadata + token solo si no está cifrado).
    /// </summary>
    [Table("Session")]
    public sealed class SessionRow
    {
        [PrimaryKey]
        public int Id { get; set; }

        [MaxLength(256)]
        public string? Username { get; set; }

        [MaxLength(64)]
        public string? TokenType { get; set; }

        [MaxLength(4096)]
        public string? AccessToken { get; set; }

        public DateTime ExpiresAtUtc { get; set; }
        public DateTime LastLoginUtc { get; set; }
        public bool IsEncrypted { get; set; }
    }

    /// <summary>
    /// Acceso a la base SQLite local (tabla <c>Session</c>) con inicialización lazy async.
    /// La conexión se crea en el primer uso y se llama <c>SQLitePCL.Batteries_V2.Init()</c> una sola vez.
    /// </summary>
    public sealed class SessionDatabase
    {
        private const int SessionRowId = 1;

        private static readonly object BatteriesLock = new();
        private static bool _batteriesInitialized;

        private readonly Lazy<Task<SQLiteAsyncConnection>> _connection;

        public SessionDatabase()
        {
            _connection = new Lazy<Task<SQLiteAsyncConnection>>(InitializeAsync);
        }

        private async Task<SQLiteAsyncConnection> InitializeAsync()
        {
            EnsureBatteriesInitialized();

            var path = Path.Combine(FileSystem.AppDataDirectory, "insignia.db3");
            var connection = new SQLiteAsyncConnection(path);
            await connection.CreateTableAsync<SessionRow>();
            return connection;
        }

        private static void EnsureBatteriesInitialized()
        {
            lock (BatteriesLock)
            {
                if (_batteriesInitialized)
                    return;
                SQLitePCL.Batteries_V2.Init(); // idempotente
                _batteriesInitialized = true;
            }
        }

        private Task<SQLiteAsyncConnection> GetConnectionAsync() => _connection.Value;

        /// <summary>
        /// Guarda la sesión (upsert de la única fila). El token solo se persiste
        /// en la fila cuando NO está cifrado (fallback por keystore no disponible).
        /// </summary>
        public async Task SaveAsync(SessionModel session)
        {
            var connection = await GetConnectionAsync();

            var row = new SessionRow
            {
                Id = SessionRowId,
                Username = session.Username,
                TokenType = session.TokenType,
                AccessToken = session.IsEncrypted ? null : session.AccessToken,
                ExpiresAtUtc = session.ExpiresAtUtc,
                LastLoginUtc = session.LastLoginUtc,
                IsEncrypted = session.IsEncrypted,
            };

            await connection.InsertOrReplaceAsync(row);
        }

        /// <summary>
        /// Lee la fila de sesión (null si no existe).
        /// </summary>
        public async Task<SessionModel?> GetAsync()
        {
            var connection = await GetConnectionAsync();
            var row = await connection.Table<SessionRow>().FirstOrDefaultAsync();
            if (row is null)
                return null;

            return new SessionModel
            {
                Username = row.Username,
                TokenType = row.TokenType,
                AccessToken = row.AccessToken,
                ExpiresAtUtc = row.ExpiresAtUtc,
                LastLoginUtc = row.LastLoginUtc,
                IsEncrypted = row.IsEncrypted,
            };
        }

        /// <summary>
        /// Elimina la fila de sesión.
        /// </summary>
        public async Task ClearAsync()
        {
            var connection = await GetConnectionAsync();
            await connection.DeleteAllAsync<SessionRow>();
        }
    }
}
