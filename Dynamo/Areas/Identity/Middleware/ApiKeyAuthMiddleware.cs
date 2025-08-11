namespace Areas.Identity.Middleware;

public class ApiKeyAuthMiddleware(RequestDelegate next, IConfiguration configuration)
{
    private readonly RequestDelegate _next = next;
    private readonly IConfiguration _configuration = configuration;

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value;
        if (path is not null && (
            //path.StartsWith("/scalar", StringComparison.OrdinalIgnoreCase) ||
            //path.StartsWith("/openapi", StringComparison.OrdinalIgnoreCase) ||
            //path.StartsWith("/favicon", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/swagger/index", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("/", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("", StringComparison.OrdinalIgnoreCase)
        ))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue("x-api-key", out var extractedApiKey))
        {
            await Results.Problem("API Key is missing", statusCode: StatusCodes.Status400BadRequest).ExecuteAsync(context);
            return;
        }

        var apiKey = _configuration["ApiKey"];
        if (apiKey == null || !apiKey.Equals(extractedApiKey))
        {
            await Results.Problem("Invalid API Key", statusCode: StatusCodes.Status401Unauthorized).ExecuteAsync(context);
            return;
        }

        await _next(context);
    }
}
