using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;
using WhatsAppPlatform.Application.WhatsAppAccounts.MetaEmbeddedSignup;

namespace WhatsAppPlatform.Infrastructure.Meta;

public sealed class MetaGraphClient(HttpClient client, IOptions<MetaOptions> options)
{
    public async Task<MetaGatewayResult<JsonDocument>> GetAsync(string path, string? bearerToken, CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{options.Value.GraphApiVersion}/{path}");
            if (bearerToken is not null) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            using var response = await client.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return MetaGatewayResult<JsonDocument>.Failure(Classify(response.StatusCode));
            var document = JsonDocument.Parse(await response.Content.ReadAsByteArrayAsync(cancellationToken), new JsonDocumentOptions { MaxDepth = 16 });
            if (document.RootElement.ValueKind != JsonValueKind.Object || document.RootElement.TryGetProperty("error", out _))
            {
                document.Dispose();
                return MetaGatewayResult<JsonDocument>.Failure(MetaGatewayError.Rejected);
            }
            return MetaGatewayResult<JsonDocument>.Success(document);
        }
        catch (HttpRequestException) { return MetaGatewayResult<JsonDocument>.Failure(MetaGatewayError.Unavailable); }
        catch (IOException) { return MetaGatewayResult<JsonDocument>.Failure(MetaGatewayError.Unavailable); }
        catch (JsonException) { return MetaGatewayResult<JsonDocument>.Failure(MetaGatewayError.Rejected); }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        { return MetaGatewayResult<JsonDocument>.Failure(MetaGatewayError.Unavailable); }
    }

    public static MetaGatewayError Classify(HttpStatusCode status) =>
        (int)status >= 500 || status == HttpStatusCode.TooManyRequests
            ? MetaGatewayError.Unavailable : MetaGatewayError.Rejected;
}
