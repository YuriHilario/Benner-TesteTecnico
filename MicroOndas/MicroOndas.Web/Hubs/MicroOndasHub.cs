using Microsoft.AspNetCore.SignalR;

namespace MicroOndas.Web.Hubs
{
    /// <summary>
    /// Hub responsável exclusivamente por transmitir o estado do micro-ondas
    /// para os clientes conectados via SignalR.
    ///
    /// IMPORTANTE:
    /// - Este Hub NÃO contém lógica de negócio
    /// - Ele NÃO acessa Services ou Domínio
    /// - Ele atua apenas como canal de comunicação
    ///
    /// O envio das mensagens é feito exclusivamente pelo
    /// MicroOndasTimerService através do IHubContext.
    /// </summary>
    public class MicroOndasHub : Hub
    {
        // Hub propositalmente vazio.
        // Mantido apenas como endpoint SignalR.
    }

    /// <summary>
    /// DTO padronizado enviado ao front-end via SignalR.
    /// Representa o estado atual do micro-ondas.
    /// </summary>
    public class HeatingStatusDto
    {
        /// <summary>
        /// Estado atual do aquecimento:
        /// Stopped, InProgress, Paused, Completed, Canceled
        /// </summary>        
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Tempo formatado para exibição (M:SS ou XXs),
        /// conforme requisito do Nível 1.
        /// </summary>
        public string DisplayTimeFormatted { get; set; } = string.Empty;

        /// <summary>
        /// Potência atual do aquecimento.
        /// </summary>
        public int Power { get; set; }

        /// <summary>
        /// String de processamento do aquecimento
        /// (caracteres exibidos durante o processo).
        /// </summary>
        public string ProcessingString { get; set; } = string.Empty;
    }
}
