using Microsoft.AspNetCore.Antiforgery;

namespace WhatsAppPlatform.Api.Identity;

internal sealed class AntiforgeryMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IAntiforgery antiforgery)
    {
        var request = context.Request;
        if (request.Path.StartsWithSegments("/api")) context.Response.Headers.CacheControl = "no-store";
        if (request.Path.StartsWithSegments("/api") &&
            !HttpMethods.IsGet(request.Method) && !HttpMethods.IsHead(request.Method) && !HttpMethods.IsOptions(request.Method))
        {
            try { await antiforgery.ValidateRequestAsync(context); }
            catch (AntiforgeryValidationException)
            {
                await Results.Problem(statusCode: 400, title: "Invalid antiforgery token.")
                    .ExecuteAsync(context);
                return;
            }
        }
        await next(context);
    }
}
