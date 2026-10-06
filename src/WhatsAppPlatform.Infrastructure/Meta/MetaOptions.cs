using System.Text.RegularExpressions;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace WhatsAppPlatform.Infrastructure.Meta;

public sealed class MetaOptions
{
    public string AppId { get; set; } = "";
    [JsonIgnore] public string AppSecret { get; set; } = "";
    public string EmbeddedSignupConfigurationId { get; set; } = "";
    public string GraphApiVersion { get; set; } = "";
    public bool IsEnabled => new[] { AppId, AppSecret, EmbeddedSignupConfigurationId, GraphApiVersion }.Any(value => value is { Length: > 0 });
    public override string ToString() => "[Meta configuration]";
}

internal sealed class MetaOptionsValidator : IValidateOptions<MetaOptions>
{
    public ValidateOptionsResult Validate(string? name, MetaOptions options)
    {
        if (!options.IsEnabled) return ValidateOptionsResult.Success;
        return Regex.IsMatch(options.AppId ?? "", @"^[0-9]{1,100}$") &&
            Regex.IsMatch(options.EmbeddedSignupConfigurationId ?? "", @"^[0-9]{1,100}$") &&
            Regex.IsMatch(options.GraphApiVersion ?? "", @"^v[1-9][0-9]{0,2}\.0$") &&
            options.AppSecret is { Length: >= 16 and <= 512 } secret && secret.All(character => character is >= '!' and <= '~')
            ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail("Meta requires a valid AppId, AppSecret, EmbeddedSignupConfigurationId, and GraphApiVersion; supply all four through configuration.");
    }
}
