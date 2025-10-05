namespace DataProcessorService.Models;

public class DeviceStateEntity
{
    public Guid Id { get; private set; }
    public string ModuleCategoryID { get; private set; } = null!;
    public string ModuleState { get; private set; } = null!;

    private DeviceStateEntity() {}

    private DeviceStateEntity(Guid id, string moduleCategoryId, string moduleState)
    {
        Id = id;
        ModuleCategoryID = moduleCategoryId;
        ModuleState = moduleState;
    }

    public static DeviceStateEntity Create(string moduleCategoryId, string moduleState)
    {
        return new DeviceStateEntity
        (
            Guid.NewGuid(),
            moduleCategoryId,
            moduleState
        );
    } 
}
