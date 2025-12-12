using MicroOndas.Application.DTOs;
using MicroOndas.Application.Services;
using MicroOndas.Domain.Entities;
using MicroOndas.Domain.Enums;
using MicroOndas.Domain.Interfaces;
using MicroOndas.Web.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MicroOndas.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly MicroOndasService _microondasService;
        private readonly IHubContext<MicroOndasHub> _hubContext;
        // NOVO: Repositório injetado para carregar a lista de programas (Nível 2)
        private readonly IPredefinedProgramRepository _programRepository;

        // Estado do Micro-Ondas (acessível pelo Razor e pelos Handlers)
        public HeatingProgram CurrentHeatingStatus { get; set; } = new HeatingProgram(0, 10);
        public string Message { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string TimeConversionMessage { get; set; } = string.Empty; // Requisito G

        // NOVO: Propriedade para as instruções complementares (Nível 2, Req a)
        public string InstructionsMessage { get; set; } = string.Empty;

        // Propriedades para input manual (Nível 1)
        [BindProperty]
        public int InputTime { get; set; } = 30;
        [BindProperty]
        public int InputPower { get; set; } = 10;

        // NOVO: Propriedade para seleção do programa pré-definido (Nível 2)
        [BindProperty]
        public string SelectedProgramName { get; set; } = string.Empty;

        // NOVO: Lista de programas para o dropdown no Razor (Nível 2, Req a)
        public IEnumerable<PredefinedProgram> PredefinedPrograms { get; set; } = Enumerable.Empty<PredefinedProgram>();

        // ATUALIZAR CONSTRUTOR para injeção do repositório (Nível 2)
        public IndexModel(MicroOndasService microondasService, IHubContext<MicroOndasHub> hubContext, IPredefinedProgramRepository programRepository)
        {
            _microondasService = microondasService;
            _hubContext = hubContext;
            _programRepository = programRepository;
        }

        public void OnGet()
        {
            // Carrega os programas pré-definidos para o dropdown (Nível 2, Req a)
            PredefinedPrograms = _programRepository.GetAllPrograms();
            CurrentHeatingStatus = _microondasService.GetCurrentStatus();
        }

        // --- NOVO HANDLER: Iniciar Programa Pré-Definido (Nível 2) ---
        public async Task<IActionResult> OnPostStartPredefinedHeating()
        {
            if (string.IsNullOrEmpty(SelectedProgramName))
            {
                ErrorMessage = "Selecione um programa de aquecimento pré-definido.";
                OnGet();
                return Page();
            }

            var (success, message, instructions) = _microondasService.StartPredefinedHeating(SelectedProgramName);

            if (success)
            {
                Message = message;
                InstructionsMessage = instructions; // Exibe as instruções (Nível 2, Req a)
                TimeConversionMessage = string.Empty; // Limpa a mensagem de conversão (que é só para manual)

                await SendCurrentStatusUpdate();
                return RedirectToPage();
            }
            else
            {
                ErrorMessage = message;
                OnGet();
                return Page();
            }
        }

        // Handler para Iniciar Aquecimento Manual ou +30s (Nível 1, Requisitos C, K)
        public async Task<IActionResult> OnPostStartHeating()
        {
            var inputDto = new ProgramInputDto
            {
                TimeInSeconds = InputTime,
                Power = InputPower
            };

            var (success, message, timeConversionMessage) = _microondasService.StartHeating(inputDto, isQuickStart: false);

            if (success)
            {
                Message = message;
                TimeConversionMessage = timeConversionMessage; // Requisito G
                InstructionsMessage = string.Empty; // Limpar instruções se for manual

                await SendCurrentStatusUpdate();
                return RedirectToPage();
            }
            else
            {
                ErrorMessage = message;
                OnGet();
                return Page();
            }
        }

        // Handler para Quick Start (Nível 1, Requisito J)
        public async Task<IActionResult> OnPostQuickStart()
        {
            var inputDto = new ProgramInputDto
            {
                // Os valores são ignorados pelo Service, que usa os defaults (30s, P10)
                TimeInSeconds = null,
                Power = null
            };

            var (success, message, _) = _microondasService.StartHeating(inputDto, isQuickStart: true);

            if (success)
            {
                Message = message;
                TimeConversionMessage = string.Empty;
                InstructionsMessage = string.Empty;

                await SendCurrentStatusUpdate();
                return RedirectToPage();
            }
            else
            {
                ErrorMessage = message;
                OnGet();
                return Page();
            }
        }

        // Handler para Pausar Aquecimento (Requisito M, Nível 2 Req f)
        public async Task<IActionResult> OnPostPause()
        {
            _microondasService.PauseHeating();
            Message = "Aquecimento Pausado.";

            await SendCurrentStatusUpdate();
            return RedirectToPage();
        }

        // Handler para Continuar Aquecimento (Requisito M, Nível 2 Req f)
        public async Task<IActionResult> OnPostContinue()
        {
            // O StartHeating já contém a lógica de 'resume' se o status for Paused.
            var (success, message, _) = _microondasService.StartHeating(new ProgramInputDto(), isQuickStart: false);

            if (success)
            {
                Message = message;
                await SendCurrentStatusUpdate();
                return RedirectToPage();
            }
            else
            {
                ErrorMessage = "Erro ao tentar continuar o aquecimento.";
                OnGet();
                return Page();
            }
        }

        // Handler para Cancelar Aquecimento (Requisito N, Nível 2 Req f)
        public async Task<IActionResult> OnPostCancel()
        {
            _microondasService.CancelHeating();
            Message = "Aquecimento Cancelado.";

            await SendCurrentStatusUpdate();

            return RedirectToPage();
        }

        // Handler para Limpar Mensagens
        public IActionResult OnPostClear()
        {
            // Apenas recarrega a página, limpando mensagens
            return RedirectToPage();
        }

        // ====================================================================\r\n
        // MÉTODO AUXILIAR SIGNALR (Modificado para incluir DisplayTimeFormatted e IsPredefinedProgram)
        // ====================================================================\r\n
        private async Task SendCurrentStatusUpdate()
        {
            var status = _microondasService.GetCurrentStatus();

            // Monta o DTO para o SignalR
            var currentStatus = new
            {
                Status = status.Status.ToString(),
                // Usa DisplayTimeFormatted (M:SS ou XXs)
                DisplayTimeFormatted = status.DisplayTimeFormatted,
                Power = status.Power,
                ProcessingString = status.ProcessingString,
                // NOVO: Informação para o front-end (Req e)
                IsPredefinedProgram = status.IsPredefinedProgram
            };

            // Envia a mensagem SignalR imediatamente
            await _hubContext.Clients.All.SendAsync("ReceiveStatus", currentStatus);
        }
    }
}