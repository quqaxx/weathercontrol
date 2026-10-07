using AssettoServer.Server.Configuration;
using FluentValidation;
using JetBrains.Annotations;
using YamlDotNet.Serialization;

namespace WeatherControlPlugin;

[UsedImplicitly(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.WithMembers)]
public class WeatherControlConfiguration : IValidateConfiguration<WeatherControlConfigurationValidator>
{
    [YamlMember(Description = "Panelin kullanacagi gizli anahtar (en az 8 karakter). Panelde Ayarlar > Eklenti API Anahtari alanina da ayni degeri yazin.")]
    public string ApiKey { get; init; } = "";
}

[UsedImplicitly]
public class WeatherControlConfigurationValidator : AbstractValidator<WeatherControlConfiguration>
{
    public WeatherControlConfigurationValidator()
    {
        RuleFor(x => x.ApiKey).MinimumLength(8)
            .WithMessage("WeatherControlPlugin: ApiKey en az 8 karakter olmali (extra_cfg.yml).");
    }
}
