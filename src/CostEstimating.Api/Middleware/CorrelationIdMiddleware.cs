namespace CostEstimating.Api.Middleware;

public sealed class CorrelationIdMiddleware
{
    private const string Header = "X-Correlation-ID";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[Header].FirstOrDefault()
            ?? Guid.NewGuid().ToString("N");

        context.Items[Header] = correlationId;
        context.Response.Headers[Header] = correlationId;

        await _next(context);
    }
}
