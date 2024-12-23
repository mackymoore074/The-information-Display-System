using ClassLibrary.DtoModels.Common;
using ClassLibrary.DtoModels.Department;
using ClassLibrary.Models;

namespace AdminConsole.IService
{
    public interface IDepartmentService
    {
        Task<ApiResponse<IEnumerable<DepartmentDto>>> GetDepartmentsAsync();
        Task<ApiResponse<DepartmentDto>> GetDepartmentByIdAsync(int id);
        Task<ApiResponse<DepartmentDto>> CreateDepartmentAsync(CreateDepartmentDto createDepartmentDto);
        Task<ApiResponse<DepartmentDto>> UpdateDepartmentAsync(int id, UpdateDepartmentDto updateDepartmentDto);
        Task<ApiResponse<object>> DeleteDepartmentAsync(int id);
    }
} 