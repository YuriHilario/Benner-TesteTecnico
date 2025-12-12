using MicroOndas.Application.Services;
using MicroOndas.Domain.Entities;
using MicroOndas.Domain.Enums;
using MicroOndas.Application.DTOs;
using MicroOndas.Web.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace MicroOndas.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly MicroOndasService _microondasService;
        private readonly IHubContext<MicroOndasHub> _hubContext; // Injeção do SignalR Hub

        // Estado do Micro-Ondas (acessível pelo Razor e pelos Handlers)
        public HeatingProgram CurrentHeatingStatus { get; set; } = new HeatingProgram(0, 10);
        public string Message { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;

        // Propriedade para a mensagem de conversão de tempo (Requisito G)
        public string TimeConversionMessage { get; set; } = string.Empty;

        // Propriedades para input manual
        [BindProperty]
        public int InputTime { get; set; } = 30; // Valor inicial
        [BindProperty]
        public int InputPower { get; set; } = 10; // Valor inicial

        public IndexModel(MicroOndasService microondasService, IHubContext<MicroOndasHub> hubContext)
        {
            _microondasService = microondasService;
            _hubContext = hubContext; // Inicialização do HubContext
        }

        // Chamado ao carregar a página
        public void OnGet()
        {
            // Busca o status atual para renderizar o estado inicial
            CurrentHeatingStatus = _microondasService.GetCurrentStatus();
        }

        // ====================================================================
        // 1. HANDLERS DE INÍCIO DE AQUECIMENTO
        // ====================================================================

        // Handler para Início Manual (Requisitos A, B, C, D)
        public Task<IActionResult> OnPostStartHeating()
        {
            var input = new ProgramInputDto
            {
                TimeInSeconds = InputTime,
                Power = InputPower
            };
            // Início manual: input preenchido, não é Quick Start.
            return HandleHeatingStart(input, isQuickStart: false);
        }

        // Handler para Quick Start (Requisito J)
        public Task<IActionResult> OnPostQuickStart()
        {
            // Quick Start: input vazio (o validador ignora), 'isQuickStart: true'.
            return HandleHeatingStart(new ProgramInputDto(), isQuickStart: true);
        }

        // Handler para Retomar o Aquecimento (Requisito M)
        public Task<IActionResult> OnPostContinue()
        {
            // Retomada: input vazio/nulo (o serviço identifica o status Paused), 'isQuickStart: false'.
            return HandleHeatingStart(new ProgramInputDto(), isQuickStart: false);
        }

        // Handler para Adicionar +30 segundos (Requisito K)
        public Task<IActionResult> OnPostAddThirtySeconds()
        {
            // +30s: input vazio/nulo (o serviço identifica o status InProgress/Paused), 'isQuickStart: false'.
            return HandleHeatingStart(new ProgramInputDto(), isQuickStart: false);
        }

        // ====================================================================
        // MÉTODO UNIFICADO PARA INICIAR/RETOMAR/INCREMENTAR
        // ====================================================================
        private async Task<IActionResult> HandleHeatingStart(ProgramInputDto input, bool isQuickStart)
        {
            // Chama o StartHeating no serviço, que lida com Quick Start, Retomada e +30s.
            var (success, message, timeConversionMessage) = _microondasService.StartHeating(input, isQuickStart);

            if (success)
            {
                CurrentHeatingStatus = _microondasService.GetCurrentStatus();
                Message = message;
                TimeConversionMessage = timeConversionMessage; // Exibe a mensagem de conversão (Req. G)

                // 1. Monta o DTO com o status atualizado
                var currentStatus = new
                {
                    Status = CurrentHeatingStatus.Status.ToString(),
                    // A nova propriedade DisplayTimeFormatted deve ser usada se implementada no HeatingProgram, 
                    // mas mantemos TimeRemaining aqui se o frontend espera 'Time'.
                    Time = CurrentHeatingStatus.TimeRemaining,
                    Power = CurrentHeatingStatus.Power,
                    ProcessingString = CurrentHeatingStatus.ProcessingString
                };

                // 2. Envia a mensagem SignalR Imediatamente para atualizar o display (CRÍTICO para Quick Start/Resume/+30s)
                await _hubContext.Clients.All.SendAsync("ReceiveStatus", currentStatus);

                return RedirectToPage();
            }
            else
            {
                ErrorMessage = message;
                return Page();
            }
        }

        // ====================================================================
        // 2. HANDLERS DE CONTROLE (PAUSE/CANCEL)
        // ====================================================================

        // Handler para Pausar Aquecimento (Requisito M)
        public async Task<IActionResult> OnPostPause()
        {
            _microondasService.PauseHeating();
            Message = "Aquecimento Pausado.";

            // Envia o status atualizado imediatamente via SignalR
            await SendCurrentStatusUpdate();

            return RedirectToPage();
        }

        // Handler para Cancelar Aquecimento (Requisito N)
        public async Task<IActionResult> OnPostCancel()
        {
            _microondasService.CancelHeating();
            Message = "Aquecimento Cancelado.";

            // Envia o status atualizado imediatamente via SignalR
            await SendCurrentStatusUpdate();

            return RedirectToPage();
        }

        // Handler para Limpar Mensagens (Geralmente usado após Completed/Stopped para resetar a tela)
        public IActionResult OnPostClear()
        {
            // Apenas recarrega a página, limpando mensagens (já que o Cancel é feito no OnPostCancel)
            return RedirectToPage();
        }

        // ====================================================================
        // MÉTODO AUXILIAR SIGNALR
        // ====================================================================
        private async Task SendCurrentStatusUpdate()
        {
            var status = _microondasService.GetCurrentStatus();

            // Monta o DTO para o SignalR
            var currentStatus = new
            {
                Status = status.Status.ToString(),
                Time = status.TimeRemaining,
                Power = status.Power,
                ProcessingString = status.ProcessingString
            };

            // Envia a mensagem SignalR imediatamente
            await _hubContext.Clients.All.SendAsync("ReceiveStatus", currentStatus);
        }
    }
}