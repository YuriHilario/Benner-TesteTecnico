using Microsoft.AspNetCore.SignalR;
using MicroOndas.Application.Services;
using MicroOndas.Application.DTOs;
using MicroOndas.Domain.Enums;

namespace MicroOndas.Web.Hubs
{
    /// <summary>
    /// Hub responsável por enviar ao front-end o estado atual do micro-ondas.
    /// O BackgroundService chama SendCurrentStatus() a cada segundo.
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
        /// Chamado pelo BackgroundService e também quando a página abre (via JS).
        /// </summary>
        public async Task SendCurrentStatus()
        {
            var program = _microOndasService.GetCurrentStatus();

            // Monta DTO para o SignalR
            var dto = new HeatingStatusDto
            {
                Status = program.Status.ToString(),
                Time = program.TimeRemaining,
                Power = program.Power,
                ProcessingString = program.ProcessingString
            };

            await Clients.All.SendAsync("ReceiveStatus", dto);
        }

        /// <summary>
        /// Método chamado pelo BackgroundService após cada segundo processado.
        /// </summary>
        public async Task SendStatusUpdate()
        {
            await SendCurrentStatus();
        }
    }

    /// <summary>
    /// DTO enviado ao SignalR — padronizado e completo.
    /// </summary>
    public class HeatingStatusDto
    {
        public string Status { get; set; } = string.Empty;
        public int Time { get; set; }
        public int Power { get; set; }
        public string ProcessingString { get; set; } = string.Empty;
    }
}
