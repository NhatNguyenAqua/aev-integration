using Autofac;

namespace Aev.Integration.BuildingBlocks.Infrastructure.Extensions;

public abstract class AutofacModuleBase : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        RegisterServices(builder);
    }

    protected abstract void RegisterServices(ContainerBuilder builder);
}
