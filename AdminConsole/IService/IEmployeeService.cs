using ClassLibrary.DtoModels.Common;
using ClassLibrary.DtoModels.Employee;

namespace AdminConsole.IService
{
    public interface IEmployeeService
    {
        Task<ApiResponse<IEnumerable<EmployeeDto>>> GetEmployeesAsync();
        Task<ApiResponse<EmployeeDto>> GetEmployeeAsync(int id);
        Task<ApiResponse<EmployeeDto>> CreateEmployeeAsync(CreateEmployeeDto employee);
        Task<ApiResponse<EmployeeDto>> UpdateEmployeeAsync(int id, UpdateEmployeeDto employee);
        Task<ApiResponse<object>> DeleteEmployeeAsync(int id);
    }
} 