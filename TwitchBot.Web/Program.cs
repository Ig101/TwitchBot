using System.Reflection;
using TwitchBot.Application.Workers;
using TwitchBot.Domain.Mongo;
using NLog.Web;
using TwitchBot.Domain.Db;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHostedService<TwitchHub>();
builder.WebHost.ConfigureLogging(options => {
    options.ClearProviders();
});
builder.WebHost.UseNLog();
builder.Services.Configure<MongoConnectionSettings>(builder.Configuration.GetSection("MongoConnection"));
builder.Services.Configure<MongoContextSettings<DbContext>>(builder.Configuration.GetSection("MongoConnection:Db"));
builder.Services.AddSingleton<IMongoConnection, MongoConnection>(
    provider => new MongoConnection(
        Assembly.GetAssembly(typeof(DbContext)),
        provider.GetRequiredService<IOptions<MongoConnectionSettings>>(),
        provider));
builder.Services.AddTransient<DbContext>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
