using CommonLib.Entities;
using CommonLib.Interfaces;
using DataServiceLib.Data;
using Microsoft.EntityFrameworkCore;

namespace DataServiceLib.Repositories
{
    public class NewsRepository : INewsRepository
    {
        private readonly ApplicationDbContext _context;

        public NewsRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<News>> GetAllAsync()
        {
            return await _context.News
                .Include(n => n.Vocabularies)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<News>> GetPublishedAsync()
        {
            return await _context.News
                .Include(n => n.Vocabularies)
                .Where(n => n.IsPublished)
                .OrderByDescending(n => n.PublishedAt ?? n.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<News>> GetPublishedForFreeAsync()
        {
            return await _context.News
                .Include(n => n.Vocabularies)
                .Where(n => n.IsPublished && n.IsFreePreview)
                .OrderByDescending(n => n.PublishedAt ?? n.CreatedAt)
                .ToListAsync();
        }

        public async Task<News?> GetByIdAsync(int id)
        {
            return await _context.News
                .Include(n => n.Vocabularies)
                .FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task<News?> GetByIdWithVocabulariesAsync(int id)
        {
            return await _context.News
                .Include(n => n.Vocabularies.OrderBy(v => v.Order))
                .FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task<News> CreateAsync(News news)
        {
            _context.News.Add(news);
            await _context.SaveChangesAsync();
            return news;
        }

        public async Task<News> UpdateAsync(News news)
        {
            news.UpdatedAt = DateTime.UtcNow;
            _context.News.Update(news);
            await _context.SaveChangesAsync();
            return news;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var news = await _context.News
                .Include(n => n.Vocabularies)
                .FirstOrDefaultAsync(n => n.Id == id);
            
            if (news == null) return false;

            // Xóa vocabularies trước (nếu có)
            if (news.Vocabularies != null && news.Vocabularies.Any())
            {
                _context.NewsVocabularies.RemoveRange(news.Vocabularies);
            }

            _context.News.Remove(news);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
