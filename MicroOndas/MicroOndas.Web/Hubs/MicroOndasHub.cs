using Microsoft.AspNetCore.SignalR;
using MicroOndas.Application.Services;
using MicroOndas.Domain.Enums;
using System.Threading.Tasks;

namespace MicroOndas.Web.Hubs
{
    /// <summary>
    /// Hub responsável por enviar ao front-end o estado atual do micro-ondas.
    /// O BackgroundService (MicroOndasTimerService) chama SendCurrentStatus() a cada segundo.
    /// </summary>
    public class MicroOndasHub : Hub
    {
        private readonly MicroOndasService _microOndasService;

        public MicroOndasHub(MicroOndasService microOndasService)
        {
            _microOndasService = microOndasService;
        }

        /// <summary>
        /// Envia o estado atual do micro-ondas ao cliente.
        /// Chamado pelo TimerService e também quando a página abre (via JS) para status inicial.
        /// </summary>
        public async Task SendCurrentStatus()
        {
            var program = _microOndasService.GetCurrentStatus();

            // Monta DTO para o SignalR
            var dto = new HeatingStatusDto
            {
                Status = program.Status.ToString(),
                // CRÍTICO: Usa a nova propriedade de string formatada (M:SS ou XXs)
                DisplayTimeFormatted = program.DisplayTimeFormatted,
                Power = program.Power,
                ProcessingString = program.ProcessingString
            };

            await Clients.All.SendAsync("ReceiveStatus", dto);
        }

        // Nota: O método SendStatusUpdate foi removido se ele apenas duplicava SendCurrentStatus. 
        // O TimerService agora chama diretamente SendCurrentStatus.
    }

    /// <summary>
    /// DTO enviado ao SignalR — padronizado e completo.
    /// OBS: O campo 'Time' (int) foi substituído por 'DisplayTimeFormatted' (string) para suportar a conversão M:SS.
    /// </summary>
    public class HeatingStatusDto
    {
        public string Status { get; set; } = string.Empty;

        // NOVO/ATUALIZADO: Campo para a visualização formatada (M:SS ou XXs).
        public string DisplayTimeFormatted { get; set; } = string.Empty;

        public int Power { get; set; }
        public string ProcessingString { get; set; } = string.Empty;
    }
}