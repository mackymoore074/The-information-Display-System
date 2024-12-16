using ClassLibrary.DtoModels.Common;
using ClassLibrary.DtoModels.Location;
using ClassLibrary.Models;

namespace AdminConsole.IService
{
    public interface ILocationService
    {
        Task<ApiResponse<List<LocationDto>>> GetLocationsAsync(); // Changed from IEnumerable to List
        Task<ApiResponse<LocationDto>> GetLocationByIdAsync(int id);
        Task<ApiResponse<LocationDto>> CreateLocationAsync(CreateLocationDto createLocationDto);
        Task<ApiResponse<LocationDto>> UpdateLocationAsync(int id, UpdateLocationDto updateLocationDto);
        Task<ApiResponse<object>> DeleteLocationAsync(int id);
    }
}
