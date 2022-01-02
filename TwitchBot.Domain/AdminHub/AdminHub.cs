using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace TwitchBot.Domain.AdminHub
{
  public class AdminHub : Hub
  {
    private readonly ILogger<AdminHub> _logger;

    public AdminHub(
        ILogger<AdminHub> logger)
    {
        _logger = logger;
    }

    public override Task OnConnectedAsync()
    {
        _logger.LogInformation($"connected to hub");

        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception exception)
    {
        _logger.LogInformation($"disconnected from hub");

        return base.OnDisconnectedAsync(exception);
    }
  }
}