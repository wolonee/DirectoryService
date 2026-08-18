using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace FileService.Infrastructure.Postgres.Initializers;

public class QuartzDbInitializer
{
    private readonly string _connectionString;
    private readonly ILogger<QuartzDbInitializer> _logger;

    public QuartzDbInitializer(
        IConfiguration configuration,
        ILogger<QuartzDbInitializer> logger)
    {
        _connectionString = configuration.GetConnectionString("FileServiceDb")
            ?? throw new InvalidOperationException("Connection string 'FileServiceDb' is not configured.");
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken stoppingToken = default)
    {
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(stoppingToken);

            // Скрипт tables_postgres.sql не идемпотентный (CREATE без IF NOT EXISTS).
            // Повторный запуск по той же БД падает на "already exists" — а старт может
            // выполниться дважды (например, WebApplicationFactory в интеграционных тестах).
            // Поэтому сначала проверяем, созданы ли уже Quartz-таблицы, и пропускаем init.
            if (await QuartzTablesExistAsync(connection, stoppingToken))
            {
                _logger.LogInformation("Quartz tables already exist, skipping initialization");
                return;
            }

            string sqlScript = await LoadSqlScriptAsync();

            await using var command = new NpgsqlCommand(sqlScript, connection);

            await command.ExecuteNonQueryAsync(stoppingToken);

            _logger.LogInformation("Quartz tables initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Quartz tables");
            throw;
        }
    }

    private static async Task<bool> QuartzTablesExistAsync(
        NpgsqlConnection connection,
        CancellationToken stoppingToken)
    {
        // to_regclass возвращает NULL, если таблицы нет (без исключения).
        await using var command = new NpgsqlCommand(
            "SELECT to_regclass('public.qrtz_locks') IS NOT NULL;",
            connection);

        object? result = await command.ExecuteScalarAsync(stoppingToken);

        return result is true;
    }

    private static async Task<string> LoadSqlScriptAsync()
    {
        Assembly assembly = typeof(QuartzDbInitializer).Assembly;
        string resourceName =
            "FileService.Infrastructure.Postgres.Scripts.tables_postgres.sql";

        await using Stream? stream =
            assembly.GetManifestResourceStream(resourceName);

        if (stream is null)
        {
            throw new FileNotFoundException(
                $"Embedded resource '{resourceName}' not found");
        }

        using StreamReader reader = new(stream);
        return await reader.ReadToEndAsync();
    }
}