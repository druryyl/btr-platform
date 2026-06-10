using System;
using System.Data.SqlClient;
using btr.infrastructure.Helpers;

namespace btr.visitplan.worker
{
    internal static class DatabaseConnectionValidator
    {
        public sealed class ValidationResult
        {
            public string Server { get; set; }

            public string Database { get; set; }
        }

        public static ValidationResult Validate(ConnectionStringFactory factory)
        {
            if (factory is null)
                throw new ArgumentNullException(nameof(factory));

            var connectionString = factory.BuildConnectionString();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT 1";
                    command.ExecuteScalar();
                }
            }

            return new ValidationResult
            {
                Server = ConnStringHelper.Server,
                Database = ConnStringHelper.Database
            };
        }
    }
}
