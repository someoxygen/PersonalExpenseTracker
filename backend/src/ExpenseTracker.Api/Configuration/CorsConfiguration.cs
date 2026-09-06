namespace ExpenseTracker.Api.Configuration;

public static class CorsConfiguration
{
    public const string PolicyName = "Frontend";

    public static IServiceCollection AddConfiguredCors(
        this IServiceCollection services, IConfiguration configuration)
    {
        var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        if (origins.Length == 0 || origins.Any(origin =>
                !Uri.TryCreate(origin, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) ||
                uri.GetLeftPart(UriPartial.Authority) != origin))
        {
            throw new InvalidOperationException(
                "Cors:AllowedOrigins must contain explicit HTTP(S) origins without paths or trailing slashes.");
        }

        services.AddCors(options => options.AddPolicy(PolicyName, policy => policy
            .WithOrigins(origins)
            .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE")
            .WithHeaders("Content-Type", "Authorization")));
        return services;
    }
}
