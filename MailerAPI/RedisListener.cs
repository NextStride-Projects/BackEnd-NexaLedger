using StackExchange.Redis;
using MimeKit;
using MailKit.Net.Smtp;
using System.Text.Json;
using MailerAPI.Utils;

namespace MailerAPI;

public class RedisListener : BackgroundService
{
    private readonly string _redisConnection;
    private readonly string _redisChannel;
    private readonly ILogger<RedisListener> _logger;
    private readonly IConfiguration _configuration;

    public RedisListener(IConfiguration configuration, ILogger<RedisListener> logger)
    {
        _redisConnection = configuration["Redis:Connection"];
        _redisChannel = configuration["Redis:Channel"];
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var redis = await ConnectionMultiplexer.ConnectAsync(_redisConnection);
        var subscriber = redis.GetSubscriber();

        _logger.LogInformation($"Subscribed to Redis channel: {_redisChannel}");

        await subscriber.SubscribeAsync(_redisChannel, async (channel, message) =>
        {
            try
            {
                var emailEvent = JsonSerializer.Deserialize<EmailEvent>(message);
                if (emailEvent != null)
                {
                    await SendEmail(emailEvent);
                    _logger.LogInformation($"Processed email event: {emailEvent.Template}");
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError($"Failed to deserialize email event: {ex.Message}");
            }
        });

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task SendEmail(EmailEvent emailEvent)
    {
        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse(_configuration["Email:Sender"]));
        email.To.Add(MailboxAddress.Parse(emailEvent.Recipient));
        email.Subject = emailEvent.Subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = EmailTemplateUtils.GetTemplate(emailEvent.Template, emailEvent.Data)
        };
        email.Body = bodyBuilder.ToMessageBody();

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(
            _configuration["Email:SmtpHost"],
            int.Parse(_configuration["Email:SmtpPort"]),
            MailKit.Security.SecureSocketOptions.StartTls
        );
        await smtp.AuthenticateAsync(_configuration["Email:Username"], _configuration["Email:Password"]);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}
