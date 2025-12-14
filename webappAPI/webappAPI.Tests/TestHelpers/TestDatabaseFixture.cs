using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using webappAPI.Data;
using webappAPI.Repositories;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace webappAPI.Tests.TestHelpers
{
    /// <summary>
    /// Fixture de test pour gérer la base de données de test
    /// Utilisé par les tests unitaires et de service
    /// </summary>
    public class TestDatabaseFixture : IDisposable
    {
        public AppDbContext DbContext { get; private set; }
        public ICarListingRepository CarListingRepository { get; private set; }
        public ICategoryRepository CategoryRepository { get; private set; }
        public IOrderRepository OrderRepository { get; private set; }
        public UserManager<IdentityUser> UserManager { get; private set; }

        public TestDatabaseFixture()
        {
            var services = new ServiceCollection();

            // Configuration de la base de données en mémoire
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}"));

            // Configuration d'Identity
            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            // Enregistrement des repositories
            services.AddScoped<ICarListingRepository, CarListingRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();

            var serviceProvider = services.BuildServiceProvider();

            // Récupération des instances
            DbContext = serviceProvider.GetRequiredService<AppDbContext>();
            CarListingRepository = serviceProvider.GetRequiredService<ICarListingRepository>();
            CategoryRepository = serviceProvider.GetRequiredService<ICategoryRepository>();
            OrderRepository = serviceProvider.GetRequiredService<IOrderRepository>();
            UserManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            // Initialisation de la base de données
            DbContext.Database.EnsureCreated();
            SeedTestData();
        }

        private void SeedTestData()
        {
            // Ajouter des données de test si nécessaire
            // Cette méthode peut être étendue selon les besoins
        }

        public void Dispose()
        {
            DbContext?.Database.EnsureDeleted();
            DbContext?.Dispose();
        }
    }
}
