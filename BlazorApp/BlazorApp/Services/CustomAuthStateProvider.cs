using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Components.Authorization;
using Blazored.LocalStorage;
using System.Linq;

namespace BlazorApp.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private readonly HttpClient _httpClient;

        public CustomAuthStateProvider(ILocalStorageService localStorage, HttpClient httpClient)
        {
            _localStorage = localStorage;
            _httpClient = httpClient;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");

            if (string.IsNullOrWhiteSpace(token))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                if (jwtToken.ValidTo < DateTime.UtcNow)
                {
                    await _localStorage.RemoveItemAsync("authToken");
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }

                var claims = jwtToken.Claims.ToList();
                
                // Debug: Log claims
                Console.WriteLine($"[AuthStateProvider] Token claims count: {claims.Count}");
                foreach (var claim in claims)
                {
                    Console.WriteLine($"[AuthStateProvider] Claim: {claim.Type} = {claim.Value}");
                }
                
                // Map JWT claims to standard claim types
                var mappedClaims = new List<Claim>();
                foreach (var claim in claims)
                {
                    // Handle role claims - check for both "role" and full ClaimTypes.Role URI
                    if (claim.Type == "role" || claim.Type == ClaimTypes.Role || 
                        claim.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
                    {
                        // Map to standard role claim type
                        mappedClaims.Add(new Claim(ClaimTypes.Role, claim.Value));
                        Console.WriteLine($"[AuthStateProvider] Mapped role claim: {claim.Value}");
                    }
                    // Handle nameid/sub claims - check for both short and full forms
                    else if (claim.Type == "nameid" || claim.Type == "sub" || 
                             claim.Type == ClaimTypes.NameIdentifier ||
                             claim.Type == JwtRegisteredClaimNames.Sub)
                    {
                        // Map to standard NameIdentifier claim type
                        mappedClaims.Add(new Claim(ClaimTypes.NameIdentifier, claim.Value));
                        Console.WriteLine($"[AuthStateProvider] Mapped nameid/sub to NameIdentifier: {claim.Value}");
                    }
                    // Handle unique_name/name claims
                    else if (claim.Type == "unique_name" || claim.Type == ClaimTypes.Name ||
                             claim.Type == JwtRegisteredClaimNames.UniqueName)
                    {
                        // Map to standard Name claim type
                        mappedClaims.Add(new Claim(ClaimTypes.Name, claim.Value));
                        Console.WriteLine($"[AuthStateProvider] Mapped unique_name to Name: {claim.Value}");
                    }
                    // Handle email claims
                    else if (claim.Type == "email" || claim.Type == ClaimTypes.Email ||
                             claim.Type == JwtRegisteredClaimNames.Email)
                    {
                        mappedClaims.Add(new Claim(ClaimTypes.Email, claim.Value));
                        Console.WriteLine($"[AuthStateProvider] Mapped email claim: {claim.Value}");
                    }
                    else
                    {
                        // Keep all other claims as-is
                        mappedClaims.Add(claim);
                    }
                }
                
                // Create identity with authentication type and mark as authenticated
                var identity = new ClaimsIdentity(mappedClaims, "jwt", ClaimTypes.Name, ClaimTypes.Role);
                var user = new ClaimsPrincipal(identity);
                
                // Debug: Verify roles are present
                var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
                Console.WriteLine($"[AuthStateProvider] User roles found: {string.Join(", ", roles)}");
                Console.WriteLine($"[AuthStateProvider] IsAuthenticated: {user.Identity?.IsAuthenticated}");

                // Add token to HttpClient for subsequent requests
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                return new AuthenticationState(user);
            }
            catch
            {
                await _localStorage.RemoveItemAsync("authToken");
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }

        public async Task MarkUserAsAuthenticated(string token)
        {
            Console.WriteLine($"[AuthStateProvider] Storing token (length: {token?.Length ?? 0})");
            await _localStorage.SetItemAsync("authToken", token);
            
            // Add token to HttpClient immediately
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            
            Console.WriteLine("[AuthStateProvider] Token stored in localStorage");
            Console.WriteLine("[AuthStateProvider] Token added to HttpClient Authorization header");
            Console.WriteLine("[AuthStateProvider] Notifying authentication state changed...");
            
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            
            Console.WriteLine("[AuthStateProvider] Authentication state notification sent");
        }

        public async Task MarkUserAsLoggedOut()
        {
            await _localStorage.RemoveItemAsync("authToken");
            _httpClient.DefaultRequestHeaders.Authorization = null;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}