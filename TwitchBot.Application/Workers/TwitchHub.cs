using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

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

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("started");
            _client.OnMessageReceived += (sender, e) => {
                if (e.ChatMessage.Message.Contains("!test1")) {
                    _client.SendMessage("capsburg", "успешно тест");
                }
            };

            _client.Connect();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("stopped");
        }
    }
}