using ClassLibrary.Models;
using ClassLibrary.DtoModels.Common;
using ClassLibrary.DtoModels.Screen;

namespace DsplayScreen.IService
{
    public interface IPublicDisplayService
    {
        Task<ApiResponse<ScreenDto>> GetScreenByIpAsync();
        Task<ApiResponse<List<MenuItem>>> GetMenuItemsForScreenAsync(int screenId);
        Task<ApiResponse<List<NewsItem>>> GetNewsItemsForScreenAsync(int screenId);
        Task<ApiResponse<bool>> TrackDisplaysAsync(int screenId, List<DisplayTracker> displays);
    } 
}