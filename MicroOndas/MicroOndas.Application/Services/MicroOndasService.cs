using MicroOndas.Application.DTOs;
using MicroOndas.Application.Validation;
using MicroOndas.Domain.Entities;
using MicroOndas.Domain.Enums;
using MicroOndas.Domain.Interfaces;

namespace MicroOndas.Application.Services
{
    public class MicroOndasService
    {
        private readonly object _lock = new();
        private readonly IPredefinedProgramRepository _predefinedRepo;

        private HeatingProgram _currentProgram;

        public MicroOndasService(IPredefinedProgramRepository predefinedRepo)
        {
            _predefinedRepo = predefinedRepo;
            _currentProgram = new HeatingProgram(0, 10);
        }

        public HeatingProgram GetCurrentStatus()
        {
            lock (_lock)
            {
                return _currentProgram;
            }
        }

        // =======================
        // NÍVEL 2 — PRÉ-DEFINIDOS
        // =======================
        public (bool Success, string Message, string Instructions)
            StartPredefinedHeating(string programName)
        {
            lock (_lock)
            {
                if (_currentProgram.Status == HeatingStatus.InProgress)
                    return (false, "Aquecimento já está em andamento.", string.Empty);

                var program = _predefinedRepo.GetProgramByName(programName);

                _currentProgram = new HeatingProgram(
                    program.TimeInSeconds,
                    program.Power,
                    program.HeatingChar,
                    isPredefined: true
                );

                _currentProgram.Start();

                return (
                    true,
                    $"Aquecimento iniciado: {program.Name} | Potência {program.Power}.",
                    program.Instructions
                );
            }
        }

        // =======================
        // NÍVEL 1 — MANUAL / QUICK
        // =======================
        public (bool Success, string Message, string TimeConversionMessage)
            StartHeating(ProgramInputDto input, bool isQuickStart = false)
        {
            lock (_lock)
            {
                if (_currentProgram.Status == HeatingStatus.Paused)
                {
                    _currentProgram.Start();
                    return (true, "Aquecimento retomado.", null);
                }

                if (_currentProgram.Status == HeatingStatus.InProgress)
                {
                    if (_currentProgram.IsPredefinedProgram)
                        return (false, "Acréscimo de tempo não permitido para programas pré-definidos.", null);

                    _currentProgram.IncrementTime(30);
                    return (true, "+30 segundos adicionados.", null);
                }

                var (success, message, timeMsg, finalTime, finalPower) =
                    ProgramValidator.ValidateAndProcess(input, isQuickStart);

                if (!success)
                    return (false, message, null);

                _currentProgram = new HeatingProgram(
                    finalTime,
                    finalPower,
                    heatingChar: '*'
                );

                _currentProgram.Start();

                return (true, "Aquecimento iniciado.", timeMsg);
            }
        }

        // =======================
        // TIMER
        // =======================
        public HeatingProgram ProcessOneSecond()
        {
            lock (_lock)
            {
                _currentProgram.DecrementTime();
                return _currentProgram;
            }
        }

        // =======================
        // CONTROLES
        // =======================
        public void PauseHeating()
        {
            lock (_lock)
            {
                _currentProgram.Pause();
            }
        }

        public void CancelHeating()
        {
            lock (_lock)
            {
                _currentProgram.Cancel();
            }
        }
    }
}
