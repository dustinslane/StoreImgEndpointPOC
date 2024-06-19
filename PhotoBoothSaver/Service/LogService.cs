using System.Text;
using Serilog;
using ILogger = Serilog.ILogger;

namespace PhotoBoothSaver.Service;

public static class LogService
{
    public static IServiceCollection AddFileLogging(this IServiceCollection collection)
    {
        var log = new LoggerConfiguration()
            .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day, encoding: Encoding.UTF8)
            .CreateLogger();

        collection.AddScoped<ILogger>(_ => log);

        return collection;
    }
    
    public static IServiceCollection AddConsoleLogging(this IServiceCollection collection)
        {
            var log = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();
    
            collection.AddScoped<ILogger>(_ => log);
    
            return collection;
        }
}