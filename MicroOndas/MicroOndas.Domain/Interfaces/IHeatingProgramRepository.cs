using MicroOndas.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroOndas.Domain.Interfaces
{
    public interface IHeatingProgramRepository
    {
        IEnumerable<HeatingProgramDefinition> GetAll();
        HeatingProgramDefinition? GetByName(string name);
        bool HeatingCharExists(char heatingChar);
        void Add(HeatingProgramDefinition program);
    }
}
