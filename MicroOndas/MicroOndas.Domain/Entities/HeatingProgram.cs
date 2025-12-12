using MicroOndas.Domain.Enums;
using System.Text;

namespace MicroOndas.Domain.Entities
{
    public class HeatingProgram
    {
        // Properties defined by the user input
        public int TimeInSeconds { get; private set; }
        public int Power { get; private set; }

        // State properties for heating control
        public int TimeRemaining { get; set; }
        public HeatingStatus Status { get; set; } = HeatingStatus.Stopped;

        // String to display the heating process (Requirement L)
        public string ProcessingString { get; set; } = string.Empty;

        // Propriedade para o display formatado (MM:SS ou XXs) - Requisito G
        public string DisplayTimeFormatted { get; private set; } = string.Empty;

        public HeatingProgram(int timeInSeconds, int power)
        {
            TimeInSeconds = timeInSeconds;
            Power = power;
            TimeRemaining = timeInSeconds;
            // Chamada inicial para preencher o DisplayTimeFormatted corretamente
            UpdateDisplayTime();
        }

        /// <summary>
        /// CRÍTICO: Agora PUBLIC para ser acessível pelo MicroOndasService.
        /// Centraliza a lógica de formato de tempo (Requisito G).
        /// O display deve mostrar M:SS sempre que o tempo restante for >= 60 segundos.
        /// </summary>
        public void UpdateDisplayTime()
        {
            // Se o tempo restante for maior ou igual a 60 segundos, mostramos M:SS.
            if (TimeRemaining >= 60)
            {
                int minutes = TimeRemaining / 60;
                int seconds = TimeRemaining % 60;

                // Formata como M:SS. O ":D2" garante dois dígitos para segundos (ex: 1:05)
                DisplayTimeFormatted = $"{minutes}:{seconds:D2}";
            }
            else
            {
                // Para 59 segundos ou menos (padrão), exibe apenas em segundos.
                DisplayTimeFormatted = $"{TimeRemaining}s";
            }
        }

        /// <summary>
        /// Implementa o incremento de tempo (Requisito K).
        /// </summary>
        public void IncrementTime(int seconds)
        {
            TimeRemaining += seconds;
            // AÇÃO OBRIGATÓRIA: Atualiza o display formatado após o incremento
            UpdateDisplayTime();
        }

        // Called by the Application Layer's timer every second
        public void DecrementTime()
        {
            if (Status == HeatingStatus.InProgress && TimeRemaining > 0)
            {
                TimeRemaining--;

                // AÇÃO OBRIGATÓRIA: Atualiza a string formatada a cada segundo
                UpdateDisplayTime();

                // Requirement L: Add Power dots ('.') per second
                for (int i = 0; i < Power; i++)
                {
                    ProcessingString += ".";
                }
            }

            // Requirement L: Completion
            if (TimeRemaining == 0 && Status == HeatingStatus.InProgress)
            {
                Status = HeatingStatus.Completed;
                ProcessingString += " Heating concluded";
            }
        }
    }
}