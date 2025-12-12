using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroOndas.Domain.Entities
{
    /// <summary>
    /// Representa um programa de aquecimento fixo (Requisito Nível 2).
    /// </summary>
    public class PredefinedProgram
    {
        public string Name { get; }
        public string Alimento { get; }
        public int TimeInSeconds { get; }
        public int Power { get; }
        public char HeatingChar { get; }
        public string Instructions { get; }

        public PredefinedProgram(string name, string alimento, int timeInSeconds, int power, char heatingChar, string instructions)
        {
            Name = name;
            Alimento = alimento;
            TimeInSeconds = timeInSeconds;
            Power = power;
            HeatingChar = heatingChar;
            Instructions = instructions;
        }
    }
}
