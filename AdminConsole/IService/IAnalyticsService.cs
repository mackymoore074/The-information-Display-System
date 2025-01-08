using ClassLibrary.DtoModels.Admin;
using ClassLibrary.DtoModels.Common;

namespace AdminConsole.IService
{
    public interface IAnalyticsService
    {
        Task<ApiResponse<DashboardAnalyticsDto>> GetDashboardAnalyticsAsync();
    }
} 