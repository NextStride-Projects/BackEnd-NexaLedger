using MailerAPI;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddHostedService<RedisListener>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
