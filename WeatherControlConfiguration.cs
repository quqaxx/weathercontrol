using AssettoServer.Server.Configuration;
using FluentValidation;
using JetBrains.Annotations;

namespace WeatherControlPlugin;

// API anahtari kaldirildi. Eski extra_cfg.yml'lerde "ApiKey" satiri kalmissa hata vermesin diye
// alan duruyor ama artik kullanilmiyor. Yeni kurulumda extra_cfg.yml'ye eklenti bolumu yazmaya gerek yok.
[UsedImplicitly(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.WithMembers)]
public class WeatherControlConfiguration : IValidateConfiguration<WeatherControlConfigurationValidator>
{
    public string ApiKey { get; init; } = "";
}

[UsedImplicitly]
public class WeatherControlConfigurationValidator : AbstractValidator<WeatherControlConfiguration>
{
}
