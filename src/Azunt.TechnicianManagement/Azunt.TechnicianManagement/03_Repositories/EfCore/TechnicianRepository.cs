using Azunt.TechnicianManagement;
using Azunt.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azunt.Models.Common;

namespace Azunt.TechnicianManagement;

/// <summary>
/// Technician 테이블에 대한 Entity Framework Core 기반 리포지토리 구현체입니다.
/// Blazor Server 회로 유지 이슈를 피하고, 멀티테넌트 연결 문자열 지원을 위해 팩터리 사용.
/// </summary>
public class TechnicianRepository : ITechnicianRepository
{
    private readonly TechnicianAppDbContextFactory _factory;
    private readonly ILogger<TechnicianRepository> _logger;
    private readonly string? _connectionString;

    public TechnicianRepository(
        TechnicianAppDbContextFactory factory,
        ILoggerFactory loggerFactory)
    {
        _factory = factory;
        _logger = loggerFactory.CreateLogger<TechnicianRepository>();
    }

    public TechnicianRepository(
        TechnicianAppDbContextFactory factory,
        ILoggerFactory loggerFactory,
        string connectionString)
    {
        _factory = factory;
        _logger = loggerFactory.CreateLogger<TechnicianRepository>();
        _connectionString = connectionString;
    }

    private TechnicianAppDbContext CreateContext() =>
        string.IsNullOrWhiteSpace(_connectionString)
            ? _factory.CreateDbContext()
            : _factory.CreateDbContext(_connectionString);

    public async Task<Technician> AddAsyncDefault(Technician model)
    {
        await using var context = CreateContext();
        model.Created = DateTime.UtcNow;
        model.IsDeleted = false;
        context.Technicians.Add(model);
        await context.SaveChangesAsync();
        return model;
    }

    public async Task<Technician> AddAsync(Technician model)
    {
        await using var context = CreateContext();
        model.Created = DateTime.UtcNow;
        model.IsDeleted = false;

        // 현재 가장 높은 DisplayOrder 값 조회
        var maxDisplayOrder = await context.Technicians
            .Where(m => !m.IsDeleted)
            .MaxAsync(m => (int?)m.DisplayOrder) ?? 0;

        model.DisplayOrder = maxDisplayOrder + 1;

        context.Technicians.Add(model);
        await context.SaveChangesAsync();
        return model;
    }

    public async Task<IEnumerable<Technician>> GetAllAsync()
    {
        await using var context = CreateContext();
        return await context.Technicians
            .Where(m => !m.IsDeleted)
            //.OrderByDescending(m => m.Id)
            .OrderBy(m => m.DisplayOrder) // 정렬 순서 변경
            .ToListAsync();
    }

    public async Task<Technician> GetByIdAsync(long id)
    {
        await using var context = CreateContext();
        return await context.Technicians
            .Where(m => m.Id == id && !m.IsDeleted)
            .SingleOrDefaultAsync()
            ?? new Technician();
    }

    public async Task<bool> UpdateAsync(Technician model)
    {
        await using var context = CreateContext();
        context.Attach(model);
        context.Entry(model).State = EntityState.Modified;
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        await using var context = CreateContext();
        var entity = await context.Technicians.FindAsync(id);
        if (entity == null || entity.IsDeleted) return false;

        entity.IsDeleted = true;
        context.Technicians.Update(entity);
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<ArticleSet<Technician, int>> GetAllAsync<TParentIdentifier>(
        int pageIndex,
        int pageSize,
        string searchField,
        string searchQuery,
        string sortOrder,
        TParentIdentifier parentIdentifier)
    {
        await using var context = CreateContext();
        var query = context.Technicians
            .Where(m => !m.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrEmpty(searchQuery))
        {
            query = query.Where(m => m.Name != null && m.Name.Contains(searchQuery));
        }

        query = sortOrder switch
        {
            "Name" => query.OrderBy(m => m.Name),
            "NameDesc" => query.OrderByDescending(m => m.Name),
            "DisplayOrder" => query.OrderBy(m => m.DisplayOrder),
            _ => query.OrderBy(m => m.DisplayOrder) // 기본 정렬도 DisplayOrder
        };

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new ArticleSet<Technician, int>(items, totalCount);
    }

    public async Task<bool> MoveUpAsync(long id)
    {
        await using var context = CreateContext();
        var current = await context.Technicians.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (current == null) return false;

        var upper = await context.Technicians
            .Where(x => x.DisplayOrder < current.DisplayOrder && !x.IsDeleted)
            .OrderByDescending(x => x.DisplayOrder)
            .FirstOrDefaultAsync();

        if (upper == null) return false;

        // Swap
        int temp = current.DisplayOrder;
        current.DisplayOrder = upper.DisplayOrder;
        upper.DisplayOrder = temp;

        // 명시적 변경 추적
        context.Technicians.Update(current);
        context.Technicians.Update(upper);

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> MoveDownAsync(long id)
    {
        await using var context = CreateContext();
        var current = await context.Technicians.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (current == null) return false;

        var lower = await context.Technicians
            .Where(x => x.DisplayOrder > current.DisplayOrder && !x.IsDeleted)
            .OrderBy(x => x.DisplayOrder)
            .FirstOrDefaultAsync();

        if (lower == null) return false;

        // Swap
        int temp = current.DisplayOrder;
        current.DisplayOrder = lower.DisplayOrder;
        lower.DisplayOrder = temp;

        // 명시적 변경 추적
        context.Technicians.Update(current);
        context.Technicians.Update(lower);

        await context.SaveChangesAsync();
        return true;
    }
}