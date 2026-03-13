using CommonLib.Entities;

namespace CommonLib.Interfaces
{
    public interface INewsRepository
    {
        Task<List<News>> GetAllAsync();
        Task<List<News>> GetPublishedAsync();
        Task<List<News>> GetPublishedForFreeAsync();
        Task<News?> GetByIdAsync(int id);
        Task<News?> GetByIdWithVocabulariesAsync(int id);
        Task<News> CreateAsync(News news);
        Task<News> UpdateAsync(News news);
        Task<bool> DeleteAsync(int id);
    }
}
