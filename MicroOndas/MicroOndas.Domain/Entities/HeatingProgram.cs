using MicroOndas.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public HeatingProgram(int timeInSeconds, int power)
        {
            TimeInSeconds = timeInSeconds;
            Power = power;
            TimeRemaining = timeInSeconds;
        }

        // Called by the Application Layer's timer every second
        public void DecrementTime()
        {
            if (Status == HeatingStatus.InProgress && TimeRemaining > 0)
            {
                TimeRemaining--;

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
