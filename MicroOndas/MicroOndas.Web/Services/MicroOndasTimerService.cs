using MicroOndas.Application.Services;
using MicroOndas.Domain.Enums;
using MicroOndas.Web.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting; // Necessário para BackgroundService
using System;
using System.Threading;
using System.Threading.Tasks;

public class MicroOndasTimerService : BackgroundService
{
    private readonly MicroOndasService _microondasService;
    private readonly IHubContext<MicroOndasHub> _hubContext;
    private Timer _timer;
    private readonly TimeSpan _period = TimeSpan.FromSeconds(1); // 1 segundo

    public MicroOndasTimerService(MicroOndasService microondasService, IHubContext<MicroOndasHub> hubContext)
    {
        _microondasService = microondasService;
        _hubContext = hubContext;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Inicia o timer para chamar DoWork a cada 1 segundo (Requisito L)
        _timer = new Timer(DoWork, null, _period, _period);
        return Task.CompletedTask;
    }

    private async void DoWork(object state) // Usamos async void pois contém uma chamada async (SendAsync)
    {
        // O TimerService chama o ProcessOneSecond que atualiza a entidade de domínio a cada tick.
        var status = _microondasService.ProcessOneSecond();

        if (status != null && status.Status != HeatingStatus.Stopped)
        {
            // Mapeamento do status para um objeto anônimo (DTO) a ser enviado via SignalR
            var statusData = new
            {
                // Status (InProgress, Paused, Completed)
                Status = status.Status.ToString(),

                // CORREÇÃO CRÍTICA: Envia a string de display formatada (M:SS ou XXs)
                DisplayTimeFormatted = status.DisplayTimeFormatted,

                // Power da HeatingProgram
                Power = status.Power,

                // ProcessingString (os pontos '....')
                ProcessingString = status.ProcessingString
            };

            // Envia o status para todos os clientes conectados ao hub
            await _hubContext.Clients.All.SendAsync("ReceiveStatus", statusData);
        }
    }

    public override void Dispose()
    {
        _timer?.Dispose();
        base.Dispose();
    }
}