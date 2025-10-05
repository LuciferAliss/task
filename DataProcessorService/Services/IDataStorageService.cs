using SharedKernel;

namespace DataProcessorService.Services;

public interface IDataStorageService
{
    Task SaveAsync(DeviceStatus deviceStatus);
}
