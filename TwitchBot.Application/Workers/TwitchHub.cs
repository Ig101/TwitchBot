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

namespace TwitchBot.Application.Workers
{
    public class TwitchHub : IHostedService
    {
        private readonly IServiceProvider _provider;
        private readonly ILogger<TwitchHub> _logger;
        private TwitchClient _client;

        public TwitchHub(IServiceProvider provider, ILogger<TwitchHub> logger)
        {
            _provider = provider;
            _logger = logger;
            var credentials = new ConnectionCredentials("capsburgbot", "oauth:90b1m2yyokiofbymk5ofkcnghnyzed");
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
            _client.OnMessageReceived += async (sender, e) => {
                var context = _provider.GetRequiredService<DbContext>();
                var hub = _provider.GetRequiredService<IHubContext<AdminHub>>();
                Expression<Func<Current, bool>> condition = (Current x) => x.UserName == e.ChatMessage.UserId;
                var current = await context.Currents.GetOneAsync(condition, cancellationToken) ?? new Current {
                        Id = Guid.NewGuid(),
                        Value = 0,
                        UserName = e.ChatMessage.UserId
                    };
                if (e.ChatMessage.Message.Contains("!поток")) {
                    await hub.Clients.All.SendAsync("BlueCurrent");
                  //  _client.SendMessage("capsburg", $"Поток болтовни {e.ChatMessage.Username} равен {current.Value}");
                } else {
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