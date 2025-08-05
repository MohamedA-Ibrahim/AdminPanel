using Duende.IdentityServer.Models;

namespace AdminPanel.Identity
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
                { };

        public static IEnumerable<Client> Clients =>
            new Client[]
                {
                    new Client
                    {
                        ClientId = "pat.client",
                        AllowedGrantTypes = GrantTypes.Code,
                        ClientSecrets =
                        {
                            new Secret("secret".Sha256())
                        },
                        AllowedScopes = { "api" }
                    }
                };
    }
}
