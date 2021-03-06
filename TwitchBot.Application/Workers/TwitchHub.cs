using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Newtonsoft.Json;
using TwitchBot.Domain.Db;
using TwitchBot.Domain.Db.Entities;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;
using System.Linq.Expressions;
using Microsoft.AspNetCore.SignalR;
using TwitchBot.Domain.AdminHub;
using TwitchBot.Domain.Enums;
using Microsoft.Extensions.Configuration;

namespace TwitchBot.Application.Workers
{
    public class TwitchHub : IHostedService
    {
        private readonly IServiceProvider _provider;
        private readonly ILogger<TwitchHub> _logger;
        private TwitchClient _client;
        private readonly IConfiguration _configuration;

        public TwitchHub(IServiceProvider provider, ILogger<TwitchHub> logger, IConfiguration configuration)
        {
            _configuration = configuration;
            _provider = provider;
            _logger = logger;
            var credentials = new ConnectionCredentials("capsburgbot", _configuration["BotToken"]);
            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };
            var customClient = new WebSocketClient(clientOptions);
            _client = new TwitchClient(customClient);
            _client.Initialize(credentials, "capsburg");
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("started");
            _client.OnMessageReceived += async (sender, e) =>
            {
                var context = _provider.GetRequiredService<DbContext>();
                var hub = _provider.GetRequiredService<IHubContext<AdminHub>>();
                Expression<Func<Current, bool>> condition = (Current x) => x.UserName == e.ChatMessage.Username && x.Type == CurrentType.Blue;
                var current = await context.Currents.GetOneAsync(condition, cancellationToken) ?? new Current
                {
                    Id = Guid.NewGuid(),
                    Value = 0,
                    UserName = e.ChatMessage.Username
                };
                if (e.ChatMessage.Message == "!??????????")
                {
                    _client.SendMessage("capsburg", $"?????????? ???????????????? {e.ChatMessage.Username} ?????????? {current.Value}");
                }
                else if (e.ChatMessage.Message == "!??????????????????????")
                {
                    if (current.Value >= 10.0)
                    {
                        await hub.Clients.All.SendAsync("BlueCurrent");
                        current.Value -= 10.0;
                        await context.Currents.ReplaceOneAtomicallyAsync(condition, current, true, cancellationToken);
                    }
                }
                else
                {
                    current.Value += 1.0;
                    await context.Currents.ReplaceOneAtomicallyAsync(condition, current, true, cancellationToken);
                }
            };

            _client.Connect();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("stopped");
            return Task.CompletedTask;
        }
    }
}