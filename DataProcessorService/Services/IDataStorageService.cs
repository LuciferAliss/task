using SharedKernel;

namespace DataProcessorService.Services;

public interface IDataStorageService
{
    Task SaveDeviceStatusAsync(DeviceStatus deviceStatus);
}
