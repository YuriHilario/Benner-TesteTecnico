using MicroOndas.Application.DTOs;
using MicroOndas.Application.Validation;
using MicroOndas.Domain.Entities;
using MicroOndas.Domain.Enums;
using MicroOndas.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;

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
        // CRIAÇÃO DE NOVOS PROGRAMAS (DB)
        // =======================
        public (bool Success, string Message) AddNewProgram(ProgramCreationDto dto)
        {
            lock (_lock)
            {
                // 1. Validação
                if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Food))
                {
                    return (false, "Nome e Alimento são campos obrigatórios.");
                }
                // CORREÇÃO APLICADA: Limitação de 120s removida para programas personalizados.
                if (dto.TimeInSeconds <= 0)
                {
                    return (false, "O tempo deve ser maior que 0");
                }
                if (dto.Power < 1 || dto.Power > 10)
                {
                    return (false, "A potência deve ser entre 1 e 10.");
                }
                if (dto.HeatingChar == '*' || char.IsWhiteSpace(dto.HeatingChar))
                {
                    return (false, "Caractere de aquecimento inválido. '*' é reservado.");
                }

                using var scope = _scopeFactory.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IHeatingProgramRepository>();

                // 2. Verifica unicidade
                if (repository.GetByName(dto.Name) != null)
                {
                    return (false, $"O nome '{dto.Name}' já está em uso.");
                }
                if (repository.HeatingCharExists(dto.HeatingChar))
                {
                    return (false, $"O caractere de aquecimento '{dto.HeatingChar}' já está em uso.");
                }

                // 3. Mapeamento do DTO para a Entidade (Garantindo que a Entidade é criada corretamente)
                var newProgram = new HeatingProgramDefinition(
                    name: dto.Name,
                    food: dto.Food,
                    timeInSeconds: dto.TimeInSeconds,
                    power: dto.Power,
                    heatingChar: dto.HeatingChar,
                    instructions: dto.Instructions,
                    isPredefined: false
                );

                // 4. Persiste no Repositório
                try
                {
                    repository.Add(newProgram);
                    return (true, $"Programa '{dto.Name}' adicionado com sucesso!");
                }
                catch (Exception ex)
                {
                    return (false, $"Erro ao salvar o programa: {ex.Message}");
                }
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