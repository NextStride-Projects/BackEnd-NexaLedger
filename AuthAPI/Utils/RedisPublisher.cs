using System.Text.Json;
using System.Threading.Tasks;
using StackExchange.Redis;
using Microsoft.Extensions.Configuration;

namespace AuthAPI.Utils
{
    public static class RedisPublisher
    {
        private static string? _redisConnection;
        private static string? _redisChannel;

        public static void Configure(IConfiguration configuration)
        {
            _redisConnection = configuration["Redis:Connection"];
            _redisChannel = configuration["Redis:Channel"];
        }

        public static async Task PublishLogAsync(Models.Log log)
        {
            if (_redisConnection == null || _redisChannel == null)
            {
                throw new InvalidOperationException("RedisPublisher is not configured properly.");
            }

            var redis = await ConnectionMultiplexer.ConnectAsync(_redisConnection);
            var subscriber = redis.GetSubscriber();
            var message = JsonSerializer.Serialize(log);
            await subscriber.PublishAsync(_redisChannel, message);
        }
    }
}
