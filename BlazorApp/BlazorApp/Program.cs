using BlazorApp.Services;
using BlazorApp.Models;
using Blazored.LocalStorage;
using Fluxor;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace BlazorApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            // Configure API base address (default: localhost:5000)
            var apiBaseUrl = "http://localhost:5237"; // Hardcoded for testing
            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });

            // Add local storage service for token persistence
            builder.Services.AddBlazoredLocalStorage();

            // Add API client and business services
            builder.Services.AddScoped<IApiClient, ApiClient>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<ICarListingService, CarListingService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<IWishlistService, WishlistService>();
            builder.Services.AddScoped<IMessageService, MessageService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IDashboardService, DashboardService>();
            builder.Services.AddScoped<IApprovalService, ApprovalService>();
            builder.Services.AddScoped<IAdminService, AdminService>();
            builder.Services.AddScoped<ICartService, CartService>();
            builder.Services.AddScoped<IPurchaseRequestService, PurchaseRequestService>();
            builder.Services.AddScoped<ICarRentalService, CarRentalService>();
            builder.Services.AddScoped<ICompareService, CompareService>();

            // Add Fluxor for state management
            builder.Services.AddFluxor(options =>
            {
                options.ScanAssemblies(typeof(Program).Assembly);
            });

            // Add authorization support
            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
            builder.Services.AddCascadingAuthenticationState();

            var host = builder.Build();
            
            // Preload authentication state before app starts to prevent race condition
            var authStateProvider = host.Services.GetRequiredService<AuthenticationStateProvider>();
            await authStateProvider.GetAuthenticationStateAsync();
            
            await host.RunAsync();
        }
    }
}
