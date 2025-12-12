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

        // NOVO: Rastreia o caractere de aquecimento (Nível 2, Req b)
        public char HeatingChar { get; private set; } = '.';

        // NOVO: Rastreia se é um programa pré-definido (Nível 2, Req e)
        public bool IsPredefinedProgram { get; private set; }

        public HeatingProgram(int timeInSeconds, int power, char heatingChar = '.', bool isPredefined = false)
        {
            TimeInSeconds = timeInSeconds;
            Power = power;
            TimeRemaining = timeInSeconds;
            HeatingChar = heatingChar; // Define o caractere
            IsPredefinedProgram = isPredefined; // Define o tipo de programa

            // Chamada inicial para preencher o DisplayTimeFormatted corretamente
            UpdateDisplayTime();
        }

        /// <summary>
        /// Centraliza a lógica de formato de tempo (Requisito G).
        /// </summary>
        public void UpdateDisplayTime()
        {
            if (TimeRemaining >= 60)
            {
                int minutes = TimeRemaining / 60;
                int seconds = TimeRemaining % 60;
                // Formato M:SS para segundos
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

                // Requirement L e Nível 2 (Req b): Add Power dots/char per second
                for (int i = 0; i < Power; i++)
                {
                    // Usa HeatingChar dinamicamente ('.' ou 'P', 'L', 'C', 'F', 'Z' do Nível 2)
                    ProcessingString += HeatingChar;
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