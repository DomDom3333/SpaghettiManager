namespace SpaghettiManager.App.Services;

[Singleton]
public record BaseServices(
    IConfiguration Configuration,
    AppSettings Settings,
    ILoggerFactory LoggerFactory
);