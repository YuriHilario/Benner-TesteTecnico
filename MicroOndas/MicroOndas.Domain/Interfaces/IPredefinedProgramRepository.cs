using MicroOndas.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroOndas.Domain.Interfaces
{
    public interface IPredefinedProgramRepository
    {
        IEnumerable<PredefinedProgram> GetAllPrograms();
        PredefinedProgram GetProgramByName(string name);
    }
}
