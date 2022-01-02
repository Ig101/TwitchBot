using TwitchBot.Application.Workers;
using TwitchBot.Domain.TwitchProvider;
using NLog.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<ITwitchProvider, TwitchProvider>();
builder.Services.AddHostedService<TwitchHub>();
builder.WebHost.ConfigureLogging(options => {
    options.ClearProviders();
});
builder.WebHost.UseNLog();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
