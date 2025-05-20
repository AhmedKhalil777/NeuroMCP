using Microsoft.Data.SqlClient;

namespace NeuroMCP.SqlServer.Models
{
    public class DatabaseConfig
    {
        public string? ConnectionString { get; set; }

        // Individual connection properties
        public string? Server { get; set; }
        public string? Database { get; set; }
        public string? UserId { get; set; }
        public string? Password { get; set; }
        public bool TrustServerCertificate { get; set; } = true;
        public int? ConnectionTimeout { get; set; }
        public bool IntegratedSecurity { get; set; }

        // Connection pooling options
        public bool? Pooling { get; set; }
        public int? MinPoolSize { get; set; }
        public int? MaxPoolSize { get; set; }

        // Other options
        public string? ApplicationName { get; set; }

        // Get the effective connection string, building it from parts if needed
        public string GetEffectiveConnectionString()
        {
            // If a complete connection string is provided, use it
            if (!string.IsNullOrEmpty(ConnectionString))
            {
                return ConnectionString;
            }

            // Otherwise, build a connection string from the individual properties
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = Server ?? "localhost",
                InitialCatalog = Database ?? "",
                TrustServerCertificate = TrustServerCertificate
            };

            // Set authentication method
            if (IntegratedSecurity)
            {
                builder.IntegratedSecurity = true;
            }
            else
            {
                builder.UserID = UserId ?? "";
                builder.Password = Password ?? "";
            }

            // Set connection timeout if specified
            if (ConnectionTimeout.HasValue)
            {
                builder.ConnectTimeout = ConnectionTimeout.Value;
            }

            // Set pooling options if specified
            if (Pooling.HasValue)
            {
                builder.Pooling = Pooling.Value;
            }

            if (MinPoolSize.HasValue)
            {
                builder.MinPoolSize = MinPoolSize.Value;
            }

            if (MaxPoolSize.HasValue)
            {
                builder.MaxPoolSize = MaxPoolSize.Value;
            }

            // Set application name if specified
            if (!string.IsNullOrEmpty(ApplicationName))
            {
                builder.ApplicationName = ApplicationName;
            }

            return builder.ConnectionString;
        }
    }
}