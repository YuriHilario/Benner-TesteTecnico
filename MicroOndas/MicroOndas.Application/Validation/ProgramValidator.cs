using MicroOndas.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static (bool IsValid, string Message, int FinalTime, int FinalPower) ValidateAndProcess(ProgramInputDto input, bool isQuickStart = false)
        {
            int finalTime;
            int finalPower;
            string timeMessage = null;

            // --- 1. Quick Start Logic (Requirement J) ---
            if (isQuickStart)
            {
                finalTime = DefaultTime;
                finalPower = DefaultPower;
                return (true, "Quick Start (30s, Power 10) validated.", finalTime, finalPower);
            }

            // --- 2. Power Validation (Requirements F, I) ---
            if (input.Power.HasValue)
            {
                if (input.Power < MinPower || input.Power > MaxPower)
                {
                    // Requirement I: Invalid Power message
                    return (false, $"Invalid Power. Must be between {MinPower} and {MaxPower}.", 0, 0);
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
                return (false, "Time is mandatory for standard heating.", 0, 0);
            }

            int inputTime = input.TimeInSeconds.Value;

            if (inputTime < MinTime || inputTime > MaxTime)
            {
                // Requirement H: Time out of defined limits
                return (false, $"Invalid Time. Must be between {MinTime} and {MaxTime} seconds (1 second to 2 minutes).", 0, 0);
            }

            // Requirement G: Time conversion check (80 < Time < 100)
            if (inputTime > 80 && inputTime < 100)
            {
                int minutes = inputTime / 60;
                int seconds = inputTime % 60;
                // Note: The conversion is for display only, the time used is the input time.
                timeMessage = $"Conversion Note (Req G): {inputTime} seconds is displayed as {minutes} minute(s) and {seconds} second(s).";
            }

            finalTime = inputTime;

            return (true, timeMessage, finalTime, finalPower);
        }
    }
}
