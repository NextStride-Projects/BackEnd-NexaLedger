using System.Text.Json;
using StackExchange.Redis;
using Microsoft.Extensions.Configuration;

namespace AuthAPI.Utils
{
    public static class RedisPublisher
    {
        private static string? _redisConnection;

        public static void Configure(IConfiguration configuration)
        {
            _redisConnection = configuration["Redis:Connection"];
        }

        public static async Task PublishLogAsync(Models.Log log)
        {
            await PublishAsync("action_logs", log);
        }

        public static async Task PublishAsync(string channel, object message)
        {
            if (_redisConnection == null)
            {
                throw new InvalidOperationException("RedisPublisher is not configured properly.");
            }

            var redis = await ConnectionMultiplexer.ConnectAsync(_redisConnection);
            var subscriber = redis.GetSubscriber();
            var serializedMessage = JsonSerializer.Serialize(message);
            await subscriber.PublishAsync(channel, serializedMessage);
        }
    }
}
