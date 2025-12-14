using System.Collections.Generic;
using Application.Models;
using Application.Models.DisplayModels;

namespace Application.Interfaces.Service
{
    public interface IDashboardService
    {
        IReadOnlyList<RequestDashboardDisplayModel> GetPendingRequests();
        IReadOnlyList<DeviceDashboardDisplayModel> GetDevicesByStatus(DeviceStatus status);
        IReadOnlyList<DeviceDashboardDisplayModel> GetDevicesWithTerminatedBorrowers();
    }
}
