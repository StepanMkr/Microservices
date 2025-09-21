using CoreLib.Entities;
using CoreLib.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace TaskService.Dal.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly AppDbContext _context;

    public ProjectRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ProjectEntity> AddAsync(ProjectEntity project)
    {
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();
        return project;
    }

    public async Task<ProjectEntity?> GetByIdAsync(int id)
    {
        return await _context.Projects
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<ProjectEntity>> GetAllAsync(int? ownerId, int page, int pageSize)
    {
        var query = _context.Projects.AsQueryable();

        if (ownerId.HasValue)
            query = query.Where(p => p.OwnerId == ownerId.Value);

        return await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task UpdateAsync(ProjectEntity project)
    {
        _context.Projects.Update(project);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(ProjectEntity project)
    {
        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();
    }
}