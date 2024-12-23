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
        Task<ApiResponse<MenuItem>> CreateMenuItemAsync(CreateMenuItemDto menuItem);
        Task<ApiResponse<MenuItem>> UpdateMenuItemAsync(int id, CreateMenuItemDto menuItem);
        Task<ApiResponse<object>> DeleteMenuItemAsync(int id);
    }
} 