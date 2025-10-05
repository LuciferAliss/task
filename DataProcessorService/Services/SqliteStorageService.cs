using DataProcessorService.Database;
using DataProcessorService.Models;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace DataProcessorService.Services;

public class SqliteStorageService(AppDbContext dbContext, ILogger<SqliteStorageService> logger) : IDataStorageService
{
    private readonly ILogger<SqliteStorageService> _logger = logger;
    private readonly AppDbContext _dbContext = dbContext;

    public async Task SaveAsync(DeviceStatus deviceStatus)
    {
        if (deviceStatus.ParsedStatus == null)
        {
            _logger.LogWarning("Попытка сохранить статус для устройства {ModuleId} без распарсенных данных.", deviceStatus.ModuleCategoryID);
            return;
        }

        try
        {
            var newState = DeviceStateEntity.Create(
                deviceStatus.ModuleCategoryID,
                deviceStatus.ParsedStatus.ModuleState);

            await AddDeviceStateAsync(newState);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при сохранении статуса для модуля {ModuleId} в БД.", deviceStatus.ModuleCategoryID);
        }
    }

    private async Task AddDeviceStateAsync(DeviceStateEntity deviceStatus)
    {
        _logger.LogInformation("Сохранение статуса для модуля: {ModuleId}", deviceStatus.ModuleCategoryID);

        await _dbContext.DeviceStates.AddAsync(deviceStatus);
        
        await _dbContext.SaveChangesAsync();
    }
}