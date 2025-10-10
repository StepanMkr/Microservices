using Microsoft.EntityFrameworkCore;
using TaskService.Domain.Entities;
using TaskService.Domain.Interfaces;

namespace TaskService.Infrastructure.Persistence;

public class ProjectRepository : IProjectRepository
{
    private readonly ProjectDbContext _context;

    public ProjectRepository(ProjectDbContext context)
    {
        _context = context;
    }

    public async Task<Project> AddAsync(Project project)
    {
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();
        return project;
    }

    public async Task<Project?> GetByIdAsync(int id)
    {
        return await _context.Projects
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Project>> GetAllAsync(int? ownerId, int page, int pageSize)
    {
        var query = _context.Projects.AsQueryable();

        if (ownerId.HasValue)
            query = query.Where(p => p.OwnerId == ownerId.Value);

        return await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task UpdateAsync(Project project)
    {
        _context.Projects.Update(project);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Project project)
    {
        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();
    }
}