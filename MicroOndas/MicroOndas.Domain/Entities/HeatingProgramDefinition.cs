using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroOndas.Domain.Entities
{
    public class HeatingProgramDefinition
    {
        public int Id { get; private set; }

        public string Name { get; private set; }
        public string Food { get; private set; }
        public int TimeInSeconds { get; private set; }
        public int Power { get; private set; }
        public char HeatingChar { get; private set; }
        public string? Instructions { get; private set; }
        public bool IsPredefined { get; private set; }

        protected HeatingProgramDefinition() { }

        public HeatingProgramDefinition(
            string name,
            string food,
            int timeInSeconds,
            int power,
            char heatingChar,
            string? instructions,
            bool isPredefined)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Nome do programa é obrigatório.");

            if (heatingChar == '*')
                throw new ArgumentException("O caractere '*' é reservado.");

            // ID é gerado pelo banco de dados (IDENTITY), não no construtor.

            Name = name;
            Food = food;
            TimeInSeconds = timeInSeconds;
            Power = power;
            HeatingChar = heatingChar;
            Instructions = instructions;
            IsPredefined = isPredefined;
        }
    }
}