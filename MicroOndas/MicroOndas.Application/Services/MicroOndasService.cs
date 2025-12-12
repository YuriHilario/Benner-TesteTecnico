using MicroOndas.Application.DTOs;
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

        // Instância única do programa atual
        private HeatingProgram _currentProgram;

        public MicroOndasService()
        {
            // Estado inicial para evitar null (conforme sua lógica)
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
        public (bool Success, string Message) StartHeating(ProgramInputDto input, bool isQuickStart = false)
        {
            lock (_lock)
            {
                // ------------------------------
                // VALID AÇÕES DE ENTRADA
                // ------------------------------
                if (input.TimeInSeconds < 1 || input.TimeInSeconds > 120)
                    return (false, "Time must be between 1 and 120 seconds.");

                if (input.Power < 1 || input.Power > 10)
                    return (false, "Power must be between 1 and 10.");

                // ------------------------------
                // REQUISITO K — Incrementar tempo
                // ------------------------------
                if (_currentProgram.Status == HeatingStatus.InProgress && !isQuickStart)
                {
                    int novoTotal = _currentProgram.TimeRemaining + input.TimeInSeconds.Value;

                    if (novoTotal > 120)
                        return (false, "Cannot add time. Total time remaining exceeds 120 seconds.");

                    _currentProgram.TimeRemaining = novoTotal;

                    return (true, $"Added {input.TimeInSeconds} seconds to current heating.");
                }

                // ------------------------------
                // REQUISITO M — Retomar se pausado
                // ------------------------------
                if (_currentProgram.Status == HeatingStatus.Paused)
                {
                    _currentProgram.Status = HeatingStatus.InProgress;
                    return (true, "Heating resumed.");
                }

                // ------------------------------
                // NOVO AQUECIMENTO
                // ------------------------------
                _currentProgram = new HeatingProgram(input.TimeInSeconds.Value, input.Power.Value);
                _currentProgram.Status = HeatingStatus.InProgress;

                return (true, "Heating initiated.");
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

                    // Aqui você pode implementar triggers adicionais se quiser
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
            }
        }
    }
}
