using MicroOndas.Application.DTOs;

namespace MicroOndas.Application.Validation
{
    public static class ProgramValidator
    {
        private const int MinTime = 1;
        private const int MaxTime = 120;
        private const int MinPower = 1;
        private const int MaxPower = 10;
        private const int DefaultPower = 10; // Requirement F, J
        private const int DefaultTime = 30; // Requirement J

        /// <summary>
        /// Valida e processa as entradas.
        /// </summary>
        /// <returns>
        /// (IsValid, Message, TimeConversionMessage, FinalTime, FinalPower)
        /// </returns>
        // NOVA ASSINATURA: Inclui TimeConversionMessage como o terceiro campo de retorno.
        public static (bool IsValid, string Message, string TimeConversionMessage, int FinalTime, int FinalPower) ValidateAndProcess(ProgramInputDto input, bool isQuickStart = false)
        {
            int finalTime;
            int finalPower;
            string timeConversionMessage = null; // Nova variável para Req G

            // --- 1. Quick Start Logic (Requirement J) ---
            if (isQuickStart)
            {
                finalTime = DefaultTime;
                finalPower = DefaultPower;
                // Retorno de Quick Start, com o novo campo como null
                return (true, "Quick Start (30s, Power 10) validated.", null, finalTime, finalPower);
            }

            // --- 2. Power Validation (Requirements F, I) ---
            if (input.Power.HasValue)
            {
                if (input.Power < MinPower || input.Power > MaxPower)
                {
                    // Retorno de erro, com o novo campo como null
                    return (false, $"Invalid Power. Must be between {MinPower} and {MaxPower}.", null, 0, 0);
                }
                finalPower = input.Power.Value;
            }
            else
            {
                // Requirement F: Default Power is 10
                finalPower = DefaultPower;
            }

            // --- 3. Time Validation (Requirements E, H, G) ---
            if (!input.TimeInSeconds.HasValue)
            {
                // Retorno de erro, com o novo campo como null
                return (false, "Time is mandatory for standard heating.", null, 0, 0);
            }

            int inputTime = input.TimeInSeconds.Value;

            if (inputTime < MinTime || inputTime > MaxTime)
            {
                // Retorno de erro, com o novo campo como null
                return (false, $"Invalid Time. Must be between {MinTime} and {MaxTime} seconds (1 second to 2 minutes).", null, 0, 0);
            }

            // Requirement G: Time conversion check (80 < Time < 100)
            if (inputTime > 80 && inputTime < 100)
            {
                int minutes = inputTime / 60;
                int seconds = inputTime % 60;
                // Atribui a mensagem de conversão à nova variável
                timeConversionMessage = $"Nota de Conversão (Req G): {inputTime} segundos são exibidos como {minutes} minuto(s) e {seconds} segundo(s).";
            }

            finalTime = inputTime;

            // Retorno final de sucesso, incluindo a mensagem de conversão (pode ser null)
            return (true, "Heating initiated.", timeConversionMessage, finalTime, finalPower);
        }
    }
}