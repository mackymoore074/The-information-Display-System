using ClassLibrary.Models;
using ClassLibrary.DtoModels.Common;
using ClassLibrary.DtoModels.Screen;

namespace DsplayScreen.IService
{
    public interface IScreenService
    {
        Task<ApiResponse<string>> LoginAsync(LoginScreenDto loginDto);
        Task<ApiResponse<List<NewsItem>>> GetNewsItemsAsync();
        Task<ApiResponse<List<MenuItem>>> GetMenuItemsAsync();
    }
} 