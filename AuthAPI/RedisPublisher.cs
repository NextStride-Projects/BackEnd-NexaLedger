using System.Text.Json;
using System.Threading.Tasks;
using StackExchange.Redis;

public static class RedisPublisher
{
    private static readonly string RedisConnection = "localhost:6379";
    private static readonly string RedisChannel = "action_logs";

    public static async Task PublishLogAsync(object log)
    {
        var redis = await ConnectionMultiplexer.ConnectAsync(RedisConnection);
        var subscriber = redis.GetSubscriber();
        var message = JsonSerializer.Serialize(log);
        await subscriber.PublishAsync(RedisChannel, message);
    }
}
