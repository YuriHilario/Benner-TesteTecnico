using MicroOndas.Domain.Entities;
using MicroOndas.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroOndas.Application.Services
{
    /// <summary>
    /// Implementação de Repositório que armazena os 5 programas fixos em memória (Requisitos Nível 2).
    /// </summary>
    public class PredefinedProgramRepository : IPredefinedProgramRepository
    {
        private readonly List<PredefinedProgram> _programs;

        public PredefinedProgramRepository()
        {
            // Inicialização dos 5 Programas Pré-Definidos (Requisitos 1 a 5)
            _programs = new List<PredefinedProgram>
            {
                new PredefinedProgram(
                    name: "Pipoca",
                    alimento: "Pipoca (de micro-ondas)",
                    timeInSeconds: 3 * 60, // 3 minutos = 180s
                    power: 7,
                    heatingChar: 'P', // Caractere único (Req b)
                    instructions: "Observar o barulho de estouros do milho, caso houver um intervalo de mais de 10 segundos entre um estouro e outro, interrompa o aquecimento."
                ),
                new PredefinedProgram(
                    name: "Leite",
                    alimento: "Leite",
                    timeInSeconds: 5 * 60, // 5 minutos = 300s
                    power: 5,
                    heatingChar: 'L', // Caractere único (Req b)
                    instructions: "Cuidado com aquecimento de líquidos, o choque térmico aliado ao movimento do recipiente pode causar fervura imediata causando risco de queimaduras."
                ),
                new PredefinedProgram(
                    name: "Carnes de boi",
                    alimento: "Carne em pedaço ou fatias",
                    timeInSeconds: 14 * 60, // 14 minutos = 840s
                    power: 4,
                    heatingChar: 'C', // Caractere único (Req b)
                    instructions: "Interrompa o processo na metade e vire o conteúdo com a parte de baixo para cima para o descongelamento uniforme."
                ),
                new PredefinedProgram(
                    name: "Frango",
                    alimento: "Frango (qualquer corte)",
                    timeInSeconds: 8 * 60, // 8 minutos = 480s
                    power: 7,
                    heatingChar: 'F', // Caractere único (Req b)
                    instructions: "Interrompa o processo na metade e vire o conteúdo com a parte de baixo para cima para o descongelamento uniforme."
                ),
                new PredefinedProgram(
                    name: "Feijão",
                    alimento: "Feijão congelado",
                    timeInSeconds: 8 * 60, // 8 minutos = 480s
                    power: 9,
                    heatingChar: 'Z', // Caractere único (Req b)
                    instructions: "Deixe o recipiente destampado e em casos de plástico, cuidado ao retirar o recipiente pois o mesmo pode perder resistência em altas temperaturas."
                )
            };
        }

        public IEnumerable<PredefinedProgram> GetAllPrograms()
        {
            // Retorna uma cópia para garantir a imutabilidade externa (Req c)
            return _programs.AsReadOnly();
        }

        public PredefinedProgram GetProgramByName(string name)
        {
            var program = _programs.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (program == null)
            {
                // Lançar exceção ou retornar um programa default de erro
                throw new KeyNotFoundException($"Predefined program '{name}' not found.");
            }
            return program;
        }
    }
}
