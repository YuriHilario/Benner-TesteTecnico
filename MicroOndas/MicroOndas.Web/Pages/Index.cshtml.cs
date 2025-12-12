using MicroOndas.Application.Services;
using MicroOndas.Domain.Entities;
using MicroOndas.Domain.Enums;
using MicroOndas.Application.DTOs;
using MicroOndas.Web.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;

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
            CurrentHeatingStatus = _microondasService.GetCurrentStatus();
        }

        // Tratador para o botão START
        public async Task<IActionResult> OnPostStart()
        {
            var inputDto = new ProgramInputDto
            {
                TimeInSeconds = InputTime,
                Power = InputPower
            };

            var (success, message) = _microondasService.StartHeating(inputDto);

            if (success)
            {
                if (!string.IsNullOrEmpty(message)) Message = message;

                // 1. Atualiza o estado para enviar o status correto
                CurrentHeatingStatus = _microondasService.GetCurrentStatus();

                // 2. Cria o DTO anônimo para o SignalR
                var currentStatus = new
                {
                    Status = CurrentHeatingStatus.Status.ToString(),
                    Time = CurrentHeatingStatus.TimeRemaining,
                    Power = CurrentHeatingStatus.Power, // Incluir Power
                    ProcessingString = CurrentHeatingStatus.ProcessingString
                };

                // 3. Envia a mensagem SignalR imediatamente
                await _hubContext.Clients.All.SendAsync("ReceiveStatus", currentStatus);

                return RedirectToPage();
            }
            else
            {
                ErrorMessage = message;
                return Page();
            }
        }

        // Tratador para o botão QUICK START
        public async Task<IActionResult> OnPostQuickStart()
        {
            var inputDto = new ProgramInputDto
            {
                TimeInSeconds = 30,
                Power = 10
            };

            // O QuickStart usa TimeInSeconds=30 e Power=10
            var (success, message) = _microondasService.StartHeating(inputDto, isQuickStart: true);

            if (success)
            {
                Message = "Quick Start (30s, Power 10) initiated.";

                // 1. Atualiza o estado para enviar o status correto
                CurrentHeatingStatus = _microondasService.GetCurrentStatus();

                // 2. Cria o DTO anônimo para o SignalR
                var currentStatus = new
                {
                    Status = CurrentHeatingStatus.Status.ToString(),
                    Time = CurrentHeatingStatus.TimeRemaining,
                    Power = CurrentHeatingStatus.Power, // Incluir Power
                    ProcessingString = CurrentHeatingStatus.ProcessingString
                };

                // 3. Envia a mensagem SignalR imediatamente
                await _hubContext.Clients.All.SendAsync("ReceiveStatus", currentStatus);

                return RedirectToPage();
            }
            else
            {
                // Deve falhar apenas se o estado estiver inválido, o que é improvável aqui
                ErrorMessage = message;
                return Page();
            }
        }

        // Tratador para os botões Pause, Continue e Cancel (Requisitos M, N)
        public IActionResult OnPostPauseOrCancel()
        {
            var currentStatus = _microondasService.GetCurrentStatus();

            if (currentStatus.Status == HeatingStatus.InProgress)
            {
                _microondasService.PauseHeating();
                Message = "Heating Paused.";
            }
            else if (currentStatus.Status == HeatingStatus.Paused)
            {
                _microondasService.CancelHeating();
                Message = "Heating Canceled.";
            }
            // Para Completed/Stopped, o botão é "Clear", que apenas recarrega/limpa.

            return RedirectToPage();
        }
    }
}