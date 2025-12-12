using MicroOndas.Application.DTOs;
using MicroOndas.Application.Validation;
using MicroOndas.Domain.Entities;
using MicroOndas.Domain.Enums;
using MicroOndas.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace MicroOndas.Application.Services
{
    public class MicroOndasService
    {
        private readonly object _lock = new object();
        private readonly IPredefinedProgramRepository _predefinedRepo; // NOVO: Injeção do repositório
        private const int MaxStandardTime = 120; // Limite máximo para o tempo restante (Requisito Nível 1)

        // Instância única do programa atual
        private HeatingProgram _currentProgram;

        // ATUALIZAR CONSTRUTOR para injeção de dependência (Nível 2, Req a)
        public MicroOndasService(IPredefinedProgramRepository predefinedRepo)
        {
            _predefinedRepo = predefinedRepo;
            // Estado inicial usando o novo construtor
            _currentProgram = new HeatingProgram(0, 10);
        }

        // ... (Mantenha GetCurrentStatus) ...
        public HeatingProgram GetCurrentStatus()
        {
            lock (_lock)
            {
                return _currentProgram;
            }
        }

        /// <summary>
        /// NOVO MÉTODO: Inicia um programa pré-definido (Nível 2, Req a, d).
        /// </summary>
        public (bool Success, string Message, string Instructions) StartPredefinedHeating(string programName)
        {
            lock (_lock)
            {
                if (_currentProgram.Status == HeatingStatus.InProgress)
                {
                    return (false, "Heating is already in progress.", string.Empty);
                }

                // 1. Busca o programa no repositório
                // Note: O repositório deve lidar com erros se o programa não for encontrado.
                var program = _predefinedRepo.GetProgramByName(programName);

                // 2. Cria e inicia o novo HeatingProgram
                _currentProgram = new HeatingProgram(
                    timeInSeconds: program.TimeInSeconds,
                    power: program.Power,
                    heatingChar: program.HeatingChar,
                    isPredefined: true // CRÍTICO: Marca como pré-definido
                );
                _currentProgram.Status = HeatingStatus.InProgress;

                // 3. Retorna a mensagem e as instruções (Nível 2, Req a)
                return (true, $"Aquecimento iniciado: {program.Name} | Potência {program.Power}.", program.Instructions);
            }
        }

        /// <summary>
        /// Inicia, retoma ou incrementa o tempo de um aquecimento (Manual ou Quick Start).
        /// </summary>
        public (bool Success, string Message, string TimeConversionMessage) StartHeating(ProgramInputDto input, bool isQuickStart = false)
        {
            // Lógica de Retomada (Requisito M)
            if (_currentProgram.Status == HeatingStatus.Paused)
            {
                _currentProgram.Status = HeatingStatus.InProgress;
                return (true, "Aquecimento retomado.", null);
            }

            // Lógica de Acréscimo +30s (Requisito K)
            if (_currentProgram.Status == HeatingStatus.InProgress)
            {
                // CRÍTICO: Bloqueia +30s para programas pré-definidos (Nível 2, Req e)
                if (_currentProgram.IsPredefinedProgram)
                {
                    return (false, "Acréscimo de tempo (+30s) não é permitido para programas pré-definidos.", null);
                }

                _currentProgram.IncrementTime(30);

                // Limite: 2 minutos (120s) - (Garante a aderência ao Req H Nível 1, caso o incremento ultrapasse)
                if (_currentProgram.TimeRemaining > MaxStandardTime)
                {
                    _currentProgram.TimeRemaining = MaxStandardTime;
                    _currentProgram.UpdateDisplayTime();
                }

                return (true, "+30 segundos adicionados.", null);
            }

            // --- Lógica de Início Manual / Quick Start (Nível 1) ---

            var validationResult = ProgramValidator.ValidateAndProcess(input, isQuickStart);
            var (success, message, timeConversionMessage, finalTime, finalPower) = validationResult;

            if (success)
            {
                // 1. Inicia um novo programa (Não-pré-definido)
                _currentProgram = new HeatingProgram(
                    timeInSeconds: finalTime,
                    power: finalPower,
                    heatingChar: '.' // Caractere default para programas manuais
                                     // isPredefined: false (default no construtor)
                );
                _currentProgram.Status = HeatingStatus.InProgress;

                return (true, "Aquecimento iniciado.", timeConversionMessage);
            }
            else
            {
                return (false, message, null);
            }
        }

        /// <summary>
        /// Chamado pelo timer a cada segundo.
        /// Atualiza tempo, processamento e estado.
        /// </summary>
        public HeatingProgram ProcessOneSecond()
        {
            lock (_lock)
            {
                if (_currentProgram.Status == HeatingStatus.InProgress)
                {
                    _currentProgram.DecrementTime();
                }

                return _currentProgram;
            }
        }

        /// <summary>
        /// REQUISITO M e Nível 2 (Req f) — Pausar aquecimento.
        /// </summary>
        public void PauseHeating()
        {
            lock (_lock)
            {
                if (_currentProgram.Status == HeatingStatus.InProgress)
                    _currentProgram.Status = HeatingStatus.Paused;
            }
        }

        /// <summary>
        /// REQUISITO N e Nível 2 (Req f) — Cancelar aquecimento.
        /// Reseta completamente program e processamento.
        /// </summary>
        public void CancelHeating()
        {
            lock (_lock)
            {
                _currentProgram.Status = HeatingStatus.Stopped;
                _currentProgram.TimeRemaining = 0;
                _currentProgram.ProcessingString = string.Empty;

                // Garante que o display seja resetado
                _currentProgram.UpdateDisplayTime();
            }
        }
    }
}