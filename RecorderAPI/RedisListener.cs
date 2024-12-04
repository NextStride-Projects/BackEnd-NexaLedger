
using StackExchange.Redis;
using System.Text.Json;
using System.Text.Json.Serialization;

using RecorderAPI.Data;
using RecorderAPI.Models;

public class RedisListener(IServiceProvider serviceProvider, IConfiguration configuration, ILogger<RedisListener> logger) : BackgroundService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ILogger<RedisListener> _logger = logger;
    private readonly string _redisConnection = configuration["Redis:Connection"];
    private readonly string _redisChannel = configuration["Redis:Channel"];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var redis = await ConnectionMultiplexer.ConnectAsync(_redisConnection);
        var subscriber = redis.GetSubscriber();

        _logger.LogInformation($"Subscribed to Redis channel: {_redisChannel}");

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true, // Ignore case for JSON property names
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull // Handle nulls gracefully
        };

        await subscriber.SubscribeAsync(_redisChannel, async (channel, message) =>
        {
            try
            {
                var log = JsonSerializer.Deserialize<Log>(message, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (log != null)
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<LogContext>();

                    // Save the log to the database
                    await context.Logs.AddAsync(log, stoppingToken);
                    await context.SaveChangesAsync(stoppingToken);

                    _logger.LogInformation($"Processed log: {log.Action}");
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError($"Failed to deserialize log message: {ex.Message}");
            }
        });


        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
