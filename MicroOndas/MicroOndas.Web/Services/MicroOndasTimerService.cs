using MicroOndas.Application.Services;
using MicroOndas.Domain.Enums;
using MicroOndas.Web.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

public class MicroOndasTimerService : BackgroundService
{
    private readonly MicroOndasService _microondasService;
    private readonly IHubContext<MicroOndasHub> _hubContext;
    private Timer _timer;
    private readonly TimeSpan _period = TimeSpan.FromSeconds(1);

    public MicroOndasTimerService(
        MicroOndasService microondasService,
        IHubContext<MicroOndasHub> hubContext)
    {
        _microondasService = microondasService;
        _hubContext = hubContext;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _timer = new Timer(async _ => await DoWork(), null, _period, _period);
        return Task.CompletedTask;
    }

    private async Task DoWork()
    {
        var status = _microondasService.ProcessOneSecond();

        if (status == null)
            return;

        // Não envia nada se nunca iniciou
        if (status.Status == HeatingStatus.Stopped)
            return;

        var statusData = new
        {
            Status = status.Status.ToString(),
            DisplayTimeFormatted = status.DisplayTimeFormatted,
            Power = status.Power,
            ProcessingString = status.ProcessingString
        };

        // Envia sempre enquanto ativo ou pausado
        if (status.Status == HeatingStatus.InProgress ||
            status.Status == HeatingStatus.Paused)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveStatus", statusData);
            return;
        }

        // Envia uma última vez ao finalizar ou cancelar
        if (status.Status == HeatingStatus.Completed ||
            status.Status == HeatingStatus.Canceled)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveStatus", statusData);
        }
    }

    public override void Dispose()
    {
        _timer?.Dispose();
        base.Dispose();
    }
}
