using MicroOndas.Application.DTOs;
using MicroOndas.Application.Services;
using MicroOndas.Domain.Entities;
using MicroOndas.Domain.Enums;
using MicroOndas.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;

namespace MicroOndas.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly MicroOndasService _microondasService;
        // CORREÇÃO: Altera a dependência para o repositório unificado
        private readonly IHeatingProgramRepository _programRepository;

        public HeatingProgram CurrentHeatingStatus { get; set; } = new HeatingProgram(0, 10);

        public string Message { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string TimeConversionMessage { get; set; } = string.Empty;
        public string InstructionsMessage { get; set; } = string.Empty;

        [BindProperty]
        public int InputTime { get; set; } = 30;

        [BindProperty]
        public int InputPower { get; set; } = 10;

        [BindProperty]
        public string SelectedProgramName { get; set; } = string.Empty;

        // NOVO: BindProperty para o formulário de criação de programas
        [BindProperty]
        public ProgramCreationDto NewProgram { get; set; } = new ProgramCreationDto();

        // CORREÇÃO: Altera o tipo de retorno para a entidade unificada
        public IEnumerable<HeatingProgramDefinition> PredefinedPrograms { get; set; }
            = Enumerable.Empty<HeatingProgramDefinition>();

        // CORREÇÃO: Ajusta o construtor para receber a nova dependência
        public IndexModel(MicroOndasService microondasService, IHeatingProgramRepository programRepository)
        {
            _microondasService = microondasService;
            _programRepository = programRepository;
        }

        public void OnGet()
        {
            CurrentHeatingStatus = _microondasService.GetCurrentStatus();
            // CORREÇÃO: Chama o método do repositório unificado
            PredefinedPrograms = _programRepository.GetAll().OrderBy(p => p.Name);

            if (CurrentHeatingStatus.Status == HeatingStatus.Completed)
            {
                Message = "Aquecimento concluído!";
            }
        }

        // ===================== AÇÕES DE INÍCIO =====================

        public IActionResult OnPostStartHeating()
        {
            var dto = new ProgramInputDto
            {
                TimeInSeconds = InputTime,
                Power = InputPower
            };

            var (success, message, conversion) = _microondasService.StartHeating(dto);

            if (!success)
            {
                ErrorMessage = message;
                OnGet();
                return Page();
            }

            Message = message;
            TimeConversionMessage = conversion;
            InstructionsMessage = string.Empty;

            return RedirectToPage();
        }

        public IActionResult OnPostStartPredefinedHeating()
        {
            var (success, message, instructions) =
                _microondasService.StartPredefinedHeating(SelectedProgramName);

            if (!success)
            {
                ErrorMessage = message;
                OnGet();
                return Page();
            }

            Message = message;
            InstructionsMessage = instructions;
            TimeConversionMessage = string.Empty;

            return RedirectToPage();
        }

        public IActionResult OnPostQuickStart()
        {
            var (success, message, _) =
                _microondasService.StartHeating(new ProgramInputDto(), true);

            if (!success)
            {
                ErrorMessage = message;
                OnGet();
                return Page();
            }

            Message = message;
            TimeConversionMessage = string.Empty;
            InstructionsMessage = string.Empty;

            return RedirectToPage();
        }

        // ===================== AÇÃO DE CRIAÇÃO DE PROGRAMA (NOVO) =====================

        public IActionResult OnPostAddProgram()
        {
            // Chamamos o novo método do serviço que valida e persiste
            var (success, message) = _microondasService.AddNewProgram(NewProgram);

            if (!success)
            {
                ErrorMessage = message;
                // Recarrega o estado atual e a lista de programas para manter a UI consistente
                OnGet();
                return Page();
            }

            // Limpa o objeto DTO e define a mensagem de sucesso
            NewProgram = new ProgramCreationDto();
            Message = message;

            // Redireciona para um novo GET para limpar o estado POST e recarregar a lista atualizada de programas
            return RedirectToPage();
        }

        // ===================== CONTROLES DE AQUECIMENTO =====================

        public IActionResult OnPostPause()
        {
            _microondasService.PauseHeating();
            Message = "Aquecimento pausado.";
            return RedirectToPage();
        }

        public IActionResult OnPostContinue()
        {
            _microondasService.StartHeating(new ProgramInputDto());
            Message = "Aquecimento retomado.";
            return RedirectToPage();
        }

        public IActionResult OnPostCancel()
        {
            _microondasService.CancelHeating();
            Message = "Aquecimento cancelado.";
            return RedirectToPage();
        }

        public IActionResult OnPostClear()
        {
            return RedirectToPage();
        }
    }
}