using Autofac;

namespace Pulse.Search;

public class SearchModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        RegisterSearchEndpoints(builder);
    }

    private static void RegisterSearchEndpoints(ContainerBuilder builder) { }
}
