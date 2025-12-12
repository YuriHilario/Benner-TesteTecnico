using MicroOndas.Application.DTOs;
using MicroOndas.Application.Validation; // Importa a camada de validação
using MicroOndas.Domain.Entities;
using MicroOndas.Domain.Enums;

namespace MicroOndas.Application.Services
{
    /// <summary>
    /// Serviço que controla regras, estado e fluxo do micro-ondas.
    /// Mantém apenas 1 instância do programa atual.
    /// O timer externo chama ProcessOneSecond().
    /// </summary>
    public class MicroOndasService
    {
        private readonly object _lock = new object();
        private const int MaxTime = 120; // Limite máximo para o tempo restante (Requisito H)

        // Instância única do programa atual
        private HeatingProgram _currentProgram;

        public MicroOndasService()
        {
            // Estado inicial
            _currentProgram = new HeatingProgram(0, 10);
        }

        /// <summary>
        /// Usado pela UI e pelo timer para obter o estado atual.
        /// Nunca retorna null.
        /// </summary>
        public HeatingProgram GetCurrentStatus()
        {
            lock (_lock)
            {
                return _currentProgram;
            }
        }

        /// <summary>
        /// Inicia, retoma ou incrementa o tempo de um aquecimento.
        /// Implementa requisitos A, B, C, D, K e M.
        /// </summary>
        /// <returns>
        /// (Success, Message, TimeConversionMessage)
        /// </returns>
        public (bool Success, string Message, string TimeConversionMessage) StartHeating(ProgramInputDto input, bool isQuickStart = false)
        {
            lock (_lock)
            {
                // 1. Lógica de Continue (Requisito M)
                if (_currentProgram.Status == HeatingStatus.Paused && input.TimeInSeconds == null && input.Power == null)
                {
                    _currentProgram.Status = HeatingStatus.InProgress;
                    return (true, "Heating resumed.", null);
                }

                // 2. Lógica de Acréscimo de Tempo (+30s) - (Requisito K)
                if (_currentProgram.Status == HeatingStatus.InProgress && input.TimeInSeconds == null && input.Power == null)
                {
                    const int increment = 30;

                    // CORREÇÃO CRÍTICA: Aplica o limite de 120s no acréscimo
                    if (_currentProgram.TimeRemaining >= MaxTime)
                    {
                        return (false, $"Cannot add time. Current time is already at maximum ({MaxTime}s).", null);
                    }
                    else if (_currentProgram.TimeRemaining + increment > MaxTime)
                    {
                        // Se o incremento exceder, define o tempo para o máximo (120s)
                        _currentProgram.TimeRemaining = MaxTime;

                        // Garante que o display M:SS seja atualizado
                        _currentProgram.UpdateDisplayTime();

                        return (true, $"Time cannot exceed {MaxTime} seconds. Time set to maximum.", null);
                    }
                    else
                    {
                        _currentProgram.IncrementTime(increment);
                        return (true, "30 seconds added.", null);
                    }
                }

                // 3. Validação (Implementa Requisitos A, B, C, D, F, H, I, J, G)
                // Assumindo que ProgramValidator retorna (IsValid, Message, TimeConversionMessage, FinalTime, FinalPower)
                var validationResult = ProgramValidator.ValidateAndProcess(input, isQuickStart);

                if (!validationResult.IsValid)
                {
                    // RETORNA MENSAGEM DE ERRO (Requisitos C, H, I)
                    return (false, validationResult.Message, null);
                }

                // 4. Criação e Início do Programa
                _currentProgram = new HeatingProgram(validationResult.FinalTime, validationResult.FinalPower);
                _currentProgram.Status = HeatingStatus.InProgress;

                // RETORNA MENSAGEM DE SUCESSO e a mensagem de conversão
                return (true, "Heating initiated.", validationResult.TimeConversionMessage);
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
                    // O DecrementTime na Entidade cuida da contagem, dos pontos e da string DisplayTimeFormatted.
                    _currentProgram.DecrementTime();
                }

                return _currentProgram;
            }
        }

        /// <summary>
        /// REQUISITO M — Pausar aquecimento.
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
        /// REQUISITO N — Cancelar aquecimento.
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