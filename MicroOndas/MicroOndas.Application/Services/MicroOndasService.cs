using MicroOndas.Application.DTOs;
using MicroOndas.Application.Validation;
using MicroOndas.Domain.Entities;
using MicroOndas.Domain.Enums;
using MicroOndas.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace MicroOndas.Application.Services
{
    public class MicroOndasService
    {
        private readonly object _lock = new();
        private readonly IServiceScopeFactory _scopeFactory;

        private HeatingProgram _currentProgram;

        public MicroOndasService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            _currentProgram = new HeatingProgram(0, 10);
        }

        // =======================
        // STATUS ATUAL
        // =======================
        public HeatingProgram GetCurrentStatus()
        {
            lock (_lock)
            {
                return _currentProgram;
            }
        }

        // =======================
        // PROGRAMAS PRÉ-DEFINIDOS (DB)
        // =======================
        public (bool Success, string Message, string Instructions)
            StartPredefinedHeating(string programName)
        {
            lock (_lock)
            {
                if (_currentProgram.Status == HeatingStatus.InProgress)
                    return (false, "Aquecimento já está em andamento.", string.Empty);

                using var scope = _scopeFactory.CreateScope();
                var repository = scope.ServiceProvider
                    .GetRequiredService<IHeatingProgramRepository>();

                var program = repository.GetByName(programName);

                if (program is null)
                    return (false, "Programa não encontrado.", string.Empty);

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
        // MANUAL / QUICK START
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
                if (_currentProgram.Status != HeatingStatus.InProgress)
                    return _currentProgram;

                _currentProgram.DecrementTime();

                if (_currentProgram.Status == HeatingStatus.Completed)
                {
                    _currentProgram = new HeatingProgram(0, 10);
                }

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
                _currentProgram = new HeatingProgram(0, 10);
            }
        }
    }
}
