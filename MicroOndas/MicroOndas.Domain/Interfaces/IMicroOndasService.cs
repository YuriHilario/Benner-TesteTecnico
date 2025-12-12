using MicroOndas.Domain.Entities;

namespace MicroOndas.Domain.Interfaces
{
    public interface IMicroOndasService
    {
        /// <summary>
        /// Inicia o aquecimento usando o programa informado.
        /// </summary>
        void Start(HeatingProgram program);

        /// <summary>
        /// Pausa o aquecimento, mantendo o tempo restante.
        /// </summary>
        void Pause();

        /// <summary>
        /// Cancela o aquecimento e reseta o programa.
        /// </summary>
        void Stop();

        /// <summary>
        /// Retorna o programa atual sendo executado.
        /// </summary>
        HeatingProgram? GetCurrentProgram();

        /// <summary>
        /// Retorna um valor indicando se existe um aquecimento em andamento.
        /// </summary>
        bool IsRunning();

        /// <summary>
        /// Evento disparado a cada "tick" (geralmente 1 segundo).
        /// A UI usa este evento para atualizar a tela.
        /// </summary>
        event Action<HeatingProgram> OnTick;
    }
}
