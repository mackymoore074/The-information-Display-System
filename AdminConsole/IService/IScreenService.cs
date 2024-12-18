using ClassLibrary.DtoModels.Common;
using ClassLibrary.DtoModels.Screen;
using ClassLibrary.Models;

namespace AdminConsole.IService
{
    public interface IScreenService
    {
        Task<ApiResponse<List<ScreenDto>>> GetScreensAsync();
        Task<ApiResponse<ScreenDto>> GetScreenByIdAsync(int id);
        Task<ApiResponse<ScreenDto>> CreateScreenAsync(CreateScreenDto createScreenDto);
        Task<ApiResponse<ScreenDto>> UpdateScreenAsync(int id, UpdateScreenDto updateScreenDto);
        Task<ApiResponse<object>> DeleteScreenAsync(int id);
    }
} 