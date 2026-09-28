using Microsoft.Extensions.Configuration;

namespace CampusConnect.Tests
{
    /// <summary>
    /// Helpers para configuración de pruebas.
    /// </summary>
    public static class TestHelpers
    {
        public static IConfiguration GetFakeConfiguration()
        {
            var config = new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "CampusConnectTestSecretKey_32CharsMin!!",
                ["Jwt:Issuer"] = "CampusConnect",
                ["Jwt:Audience"] = "CampusConnectUsers"
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(config)
                .Build();
        }
    }
}
