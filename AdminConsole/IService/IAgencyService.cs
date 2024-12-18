using ClassLibrary.DtoModels.Common;
using ClassLibrary.DtoModels.Agency;
using ClassLibrary.Models;

namespace AdminConsole.IService
{
    public interface IAgencyService
    {
        Task<ApiResponse<List<AgencyDto>>> GetAgenciesAsync();
        Task<ApiResponse<AgencyDto>> GetAgencyByIdAsync(int id);
        Task<ApiResponse<AgencyDto>> CreateAgencyAsync(CreateAgencyDto createAgencyDto);
        Task<ApiResponse<AgencyDto>> UpdateAgencyAsync(int id, UpdateAgencyDto updateAgencyDto);
        Task<ApiResponse<object>> DeleteAgencyAsync(int id);
    }
} 