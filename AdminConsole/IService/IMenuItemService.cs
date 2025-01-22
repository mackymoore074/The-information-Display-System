using ClassLibrary.Models;
using ClassLibrary.DtoModels.Common;
using ClassLibrary.DtoModels.MenuItem;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AdminConsole.IService
{
    public interface IMenuItemService
    {
        Task<ApiResponse<List<MenuItem>>> GetAllMenuItemsAsync();
        Task<ApiResponse<MenuItem>> GetMenuItemAsync(int id);
        Task<ApiResponse<MenuItem>> CreateMenuItemAsync(CreateMenuItemDto model);
        Task<ApiResponse<MenuItem>> UpdateMenuItemAsync(int id, CreateMenuItemDto model);
        Task<ApiResponse<bool>> DeleteMenuItemAsync(int id);
    }
} 