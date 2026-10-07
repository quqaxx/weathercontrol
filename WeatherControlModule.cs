using AssettoServer.Server.Plugin;
using Autofac;

namespace WeatherControlPlugin;

public class WeatherControlModule : AssettoServerModule<WeatherControlConfiguration>
{
    protected override void Load(ContainerBuilder builder)
    {
        // Servis kaydi gerekmiyor: WeatherControlController otomatik bulunur.
    }
}
