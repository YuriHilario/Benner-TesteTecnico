using Dapper;
using MicroOndas.Domain.Entities;
using MicroOndas.Domain.Interfaces;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;

namespace MicroOndas.Infrastructure.Repositories
{
    public class SqlHeatingProgramRepository : IHeatingProgramRepository
    {
        private readonly string _connectionString;

        private const string SelectColumns =
            "Id, Name, Food, TimeInSeconds, Power, HeatingChar, Instructions, IsPredefined, IsActive";

        public SqlHeatingProgramRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IEnumerable<HeatingProgramDefinition> GetAll()
        {
            using var connection = new SqlConnection(_connectionString);
            return connection.Query<HeatingProgramDefinition>(
                $"SELECT {SelectColumns} FROM HeatingPrograms WHERE IsActive = 1 ORDER BY Name");
        }

        public HeatingProgramDefinition? GetByName(string name)
        {
            using var connection = new SqlConnection(_connectionString);
            return connection.QueryFirstOrDefault<HeatingProgramDefinition>(
                $"SELECT {SelectColumns} FROM HeatingPrograms WHERE Name = @Name AND IsActive = 1",
                new { Name = name });
        }

        public bool HeatingCharExists(char heatingChar)
        {
            using var connection = new SqlConnection(_connectionString);
            return connection.ExecuteScalar<bool>(
                "SELECT COUNT(1) FROM HeatingPrograms WHERE HeatingChar = @Char",
                new { Char = heatingChar });
        }

        public void Add(HeatingProgramDefinition program)
        {
            using var connection = new SqlConnection(_connectionString);

            connection.Execute(@$"
                INSERT INTO HeatingPrograms
                (Name, Food, TimeInSeconds, Power, HeatingChar, Instructions, IsPredefined, IsActive)
                VALUES
                (@Name, @Food, @TimeInSeconds, @Power, @HeatingChar, @Instructions, @IsPredefined, 1)",
                program);
        }
    }
}