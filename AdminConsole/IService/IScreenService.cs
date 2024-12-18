using ClassLibrary.DtoModels.Common;
using ClassLibrary.DtoModels.Screen;
using ClassLibrary.Models;

namespace AdminConsole.IService
{
    public interface IScreenService
    {
        Task<ApiResponse<List<ScreenDto>>> GetAllAsync();
        Task<ApiResponse<ScreenDto>> GetByIdAsync(int id);
        Task<ApiResponse<ScreenDto>> CreateAsync(CreateScreenDto createScreenDto);
        Task<ApiResponse<ScreenDto>> UpdateAsync(int id, UpdateScreenDto updateScreenDto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
} 