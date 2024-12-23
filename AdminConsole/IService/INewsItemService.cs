using ClassLibrary.Models;
using ClassLibrary.DtoModels.NewsItem;
using ClassLibrary.DtoModels.Common;

namespace AdminConsole.IService
{
    public interface INewsItemService
    {
        Task<ApiResponse<List<NewsItem>>> GetAllNewsItemsAsync();
        Task<ApiResponse<NewsItem>> GetNewsItemByIdAsync(int id);
        Task<ApiResponse<NewsItem>> CreateNewsItemAsync(CreateNewsItemDto newsItem);
        Task<ApiResponse<NewsItem>> UpdateNewsItemAsync(int id, CreateNewsItemDto newsItem);
        Task<ApiResponse<object>> DeleteNewsItemAsync(int id);
    }
} 