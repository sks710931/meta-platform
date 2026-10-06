namespace WhatsAppPlatform.Application.WhatsAppAccounts.MetaEmbeddedSignup;

public enum MetaGatewayError { Rejected, Unavailable, UnsupportedAssets }
public sealed class MetaGatewayResult<T>
{
    public T? Value { get; }
    public MetaGatewayError? Error { get; }
    private MetaGatewayResult(T? value, MetaGatewayError? error) { Value = value; Error = error; }
    public static MetaGatewayResult<T> Success(T value) => new(value, null);
    public static MetaGatewayResult<T> Failure(MetaGatewayError error) => new(default, error);
}
