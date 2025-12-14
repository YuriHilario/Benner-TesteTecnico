using MicroOndas.Application.DTOs;
using MicroOndas.Application.Services;
using MicroOndas.Domain.Entities;
using MicroOndas.Domain.Enums;
using MicroOndas.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MicroOndas.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly MicroOndasService _microondasService;
        private readonly IPredefinedProgramRepository _programRepository;

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

        public IEnumerable<PredefinedProgram> PredefinedPrograms { get; set; }
            = Enumerable.Empty<PredefinedProgram>();

        public IndexModel(
            MicroOndasService microondasService,
            IPredefinedProgramRepository programRepository)
        {
            _microondasService = microondasService;
            _programRepository = programRepository;
        }

        public void OnGet()
        {
            PredefinedPrograms = _programRepository.GetAllPrograms();
            CurrentHeatingStatus = _microondasService.GetCurrentStatus();
        }

        // ===================== PROGRAMAS PRÉ-DEFINIDOS =====================
        public IActionResult OnPostStartPredefinedHeating()
        {
            if (string.IsNullOrEmpty(SelectedProgramName))
            {
                ErrorMessage = "Selecione um programa pré-definido.";
                OnGet();
                return Page();
            }

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

        // ===================== AQUECIMENTO MANUAL =====================
        public IActionResult OnPostStartHeating()
        {
            var input = new ProgramInputDto
            {
                TimeInSeconds = InputTime,
                Power = InputPower
            };

            var (success, message, conversion) =
                _microondasService.StartHeating(input);

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

        // ===================== CONTROLES =====================
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
