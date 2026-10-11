using System;
using BtrReturGudang.Infrastructure.Helpers;
using Xunit;

namespace BtrReturGudang.Infrastructure.Tests
{
    public class ConnStringHelperTests : IDisposable
    {
        public ConnStringHelperTests()
        {
            ConnStringHelper.ResetConnectionString();
        }

        public void Dispose()
        {
            ConnStringHelper.ResetConnectionString();
        }

        [Fact]
        public void GetConnectionString_WithoutOverride_ReturnsDefaultOrConfigured()
        {
            // Act
            var connStr = ConnStringHelper.GetConnectionString();

            // Assert
            Assert.NotNull(connStr);
            Assert.Contains("Database=btrgd", connStr);
            Assert.Contains("User Id=btrGudangLogin", connStr);
        }

        [Fact]
        public void SetConnectionString_OverridesResolvedString()
        {
            // Arrange
            const string customConn = "Server=test-server;Database=test-db;Trusted_Connection=True;";

            // Act
            ConnStringHelper.SetConnectionString(customConn);
            var resolved = ConnStringHelper.GetConnectionString();

            // Assert
            Assert.Equal(customConn, resolved);
        }

        [Fact]
        public void ResetConnectionString_RestoresDefaultFallback()
        {
            // Arrange
            ConnStringHelper.SetConnectionString("Server=custom;Database=custom;");

            // Act
            ConnStringHelper.ResetConnectionString();
            var resolved = ConnStringHelper.GetConnectionString();

            // Assert
            Assert.Contains("Database=btrgd", resolved);
        }

        [Fact]
        public void CreateConnection_ReturnsValidSqlConnectionObject()
        {
            // Act
            using (var conn = ConnStringHelper.CreateConnection())
            {
                // Assert
                Assert.NotNull(conn);
                Assert.Equal(System.Data.ConnectionState.Closed, conn.State);
                Assert.Contains("btrgd", conn.ConnectionString);
            }
        }
    }
}
