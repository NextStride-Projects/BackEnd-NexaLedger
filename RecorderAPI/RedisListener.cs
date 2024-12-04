using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using RecorderAPI.Data;
using RecorderAPI.Models;

public class RedisListener : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RedisListener> _logger;
    private readonly string _redisConnection;
    private readonly string _redisChannel;

    public RedisListener(IServiceProvider serviceProvider, IConfiguration configuration, ILogger<RedisListener> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _redisConnection = configuration["Redis:Connection"];
        _redisChannel = configuration["Redis:Channel"];
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var redis = await ConnectionMultiplexer.ConnectAsync(_redisConnection);
        var subscriber = redis.GetSubscriber();

        _logger.LogInformation($"Subscribed to Redis channel: {_redisChannel}");

        await subscriber.SubscribeAsync(_redisChannel, async (channel, message) =>
        {
            var log = JsonSerializer.Deserialize<Log>(message);
            if (log != null)
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<LogContext>();
                await context.Logs.AddAsync(log, stoppingToken);
                await context.SaveChangesAsync(stoppingToken);
                _logger.LogInformation($"Processed log: {log.Action}");
            }
        });

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
