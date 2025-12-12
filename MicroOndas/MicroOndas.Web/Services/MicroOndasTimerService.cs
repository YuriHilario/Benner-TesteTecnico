using MicroOndas.Application.Services;
using MicroOndas.Domain.Enums;
using MicroOndas.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

// NOTE: A classe BackgroundService requer o using Microsoft.Extensions.Hosting.
// Assumindo que você tem essa dependência.

public class MicroOndasTimerService : BackgroundService
{
    private readonly MicroOndasService _microondasService;
    private readonly IHubContext<MicroOndasHub> _hubContext;
    private Timer _timer;
    private readonly TimeSpan _period = TimeSpan.FromSeconds(1);

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
        // O TimerService chama o ProcessOneSecond que atualiza a entidade de domínio.
        var status = _microondasService.ProcessOneSecond();

        if (status != null && status.Status != HeatingStatus.Stopped)
        {
            // Mapeamento do status para JSON a ser enviado via SignalR
            var statusData = new
            {
                Status = status.Status.ToString(),

                // CORREÇÃO: Mudar TimeRemaining para Time para alinhar com o JavaScript e Index.cshtml.cs
                Time = status.TimeRemaining,

                Power = status.Power,

                // REMOÇÃO: A propriedade Display foi removida, pois a string é montada no JavaScript
                // ProcessingString - OK
                ProcessingString = status.ProcessingString
            };

            // Envia o status para todos os clientes (operação fire-and-forget)
            // É seguro usar await em async void aqui para melhor tratamento de exceções,
            // mas o uso de SendAsync é um fire-and-forget, então a Task não atrapalha o Timer.
            await _hubContext.Clients.All.SendAsync("ReceiveStatus", statusData);
        }
    }

    public override void Dispose()
    {
        _timer?.Dispose();
        base.Dispose();
    }
}