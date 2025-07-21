using Azunt.Models.Common;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Azunt.TechnicianManagement;

public class TechnicianRepositoryDapper : ITechnicianRepository
{
    private readonly string _connectionString;
    private readonly ILogger<TechnicianRepositoryDapper> _logger;

    public TechnicianRepositoryDapper(string connectionString, ILoggerFactory loggerFactory)
    {
        _connectionString = connectionString;
        _logger = loggerFactory.CreateLogger<TechnicianRepositoryDapper>();
    }

    private SqlConnection GetConnection() => new(_connectionString);

    public async Task<Technician> AddAsync(Technician model)
    {
        const string sql = @"
            INSERT INTO Technicians (Active, Created, CreatedBy, Name, IsDeleted)
            OUTPUT INSERTED.Id
            VALUES (@Active, @Created, @CreatedBy, @Name, 0)";

        model.Created = DateTimeOffset.UtcNow;

        using var conn = GetConnection();
        model.Id = await conn.ExecuteScalarAsync<long>(sql, model);
        return model;
    }

    public async Task<IEnumerable<Technician>> GetAllAsync()
    {
        const string sql = @"
            SELECT Id, Active, Created, CreatedBy, Name 
            FROM Technicians 
            WHERE IsDeleted = 0 
            ORDER BY Id DESC";

        using var conn = GetConnection();
        return await conn.QueryAsync<Technician>(sql);
    }

    public async Task<Technician> GetByIdAsync(long id)
    {
        const string sql = @"
            SELECT Id, Active, Created, CreatedBy, Name 
            FROM Technicians 
            WHERE Id = @Id AND IsDeleted = 0";

        using var conn = GetConnection();
        return await conn.QuerySingleOrDefaultAsync<Technician>(sql, new { Id = id }) ?? new Technician();
    }

    public async Task<bool> UpdateAsync(Technician model)
    {
        const string sql = @"
            UPDATE Technicians SET
                Active = @Active,
                Name = @Name
            WHERE Id = @Id AND IsDeleted = 0";

        using var conn = GetConnection();
        var affected = await conn.ExecuteAsync(sql, model);
        return affected > 0;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        const string sql = @"
            UPDATE Technicians SET IsDeleted = 1 
            WHERE Id = @Id AND IsDeleted = 0";

        using var conn = GetConnection();
        var affected = await conn.ExecuteAsync(sql, new { Id = id });
        return affected > 0;
    }

    public async Task<ArticleSet<Technician, int>> GetAllAsync<TParentIdentifier>(
        int pageIndex, int pageSize, string searchField, string searchQuery, string sortOrder, TParentIdentifier parentIdentifier)
    {
        var all = await GetAllAsync();
        var filtered = string.IsNullOrWhiteSpace(searchQuery)
            ? all
            : all.Where(m => m.Name != null && m.Name.Contains(searchQuery)).ToList();

        var paged = filtered
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToList();

        return new ArticleSet<Technician, int>(paged, filtered.Count());
    }

    public Task<bool> MoveUpAsync(long id)
    {
        throw new NotImplementedException();
    }

    public Task<bool> MoveDownAsync(long id)
    {
        throw new NotImplementedException();
    }
}