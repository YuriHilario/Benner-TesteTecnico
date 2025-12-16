using MicroOndas.Domain.Entities;
using MicroOndas.Domain.Interfaces;
using Microsoft.Data.SqlClient;

namespace MicroOndas.Infrastructure.Repositories
{
    public class SqlPredefinedProgramRepository : IPredefinedProgramRepository
    {
        private readonly string _connectionString;

        public SqlPredefinedProgramRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IEnumerable<PredefinedProgram> GetAllPrograms()
        {
            var programs = new List<PredefinedProgram>();

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var command = new SqlCommand(@"
                SELECT 
                    Name,
                    Description,
                    TimeInSeconds,
                    Power,
                    HeatingChar,
                    Instructions
                FROM HeatingPrograms
                WHERE IsActive = 1
                ORDER BY Name
            ", connection);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                programs.Add(new PredefinedProgram(
                    name: reader.GetString(0),
                    description: reader.GetString(1),
                    timeInSeconds: reader.GetInt32(2),
                    power: reader.GetInt32(3),
                    heatingChar: reader.GetString(4)[0],
                    instructions: reader.GetString(5)
                ));
            }

            return programs;
        }

        public PredefinedProgram? GetProgramByName(string name)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var command = new SqlCommand(@"
                SELECT 
                    Name,
                    Description,
                    TimeInSeconds,
                    Power,
                    HeatingChar,
                    Instructions
                FROM HeatingPrograms
                WHERE IsActive = 1 AND Name = @Name
            ", connection);

            command.Parameters.AddWithValue("@Name", name);

            using var reader = command.ExecuteReader();

            if (!reader.Read())
                return null;

            return new PredefinedProgram(
                name: reader.GetString(0),
                description: reader.GetString(1),
                timeInSeconds: reader.GetInt32(2),
                power: reader.GetInt32(3),
                heatingChar: reader.GetString(4)[0],
                instructions: reader.GetString(5)
            );
        }
    }
}
