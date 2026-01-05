using Microsoft.EntityFrameworkCore;
using CommonLib.Entities;
using CommonLib.Interfaces;
using DataServiceLib.Data;

namespace DataServiceLib.Repositories
{
    public class SkillRepository : ISkillRepository
    {
        private readonly ApplicationDbContext _context;

        public SkillRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Skill>> GetAllAsync()
        {
            return await _context.Skills
                .OrderBy(s => s.Id)
                .ToListAsync();
        }

        public async Task<Skill?> GetByIdAsync(int id)
        {
            return await _context.Skills.FindAsync(id);
        }

        public async Task<Skill?> GetByNameAsync(string name)
        {
            return await _context.Skills
                .FirstOrDefaultAsync(s => s.Name == name);
        }

        public async Task<Skill> CreateAsync(Skill skill)
        {
            skill.CreatedAt = DateTime.UtcNow;
            _context.Skills.Add(skill);
            await _context.SaveChangesAsync();
            return skill;
        }

        public async Task<Skill> UpdateAsync(Skill skill)
        {
            skill.UpdatedAt = DateTime.UtcNow;
            _context.Skills.Update(skill);
            await _context.SaveChangesAsync();
            return skill;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var skill = await _context.Skills.FindAsync(id);
            if (skill == null) return false;

            _context.Skills.Remove(skill);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

