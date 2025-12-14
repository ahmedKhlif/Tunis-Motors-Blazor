using System;
using System.Collections.Generic;

namespace webappAPI.Tests.TestHelpers;

/// <summary>
/// Fournisseur de données de test tunisiennes réelles
/// Module: Test et Qualité Logiciel 2025
/// Technique: Données réelles du marché tunisien
/// </summary>
public static class TunisianTestDataProvider
{
    #region Utilisateurs Tunisiens Réels

    public static class Users
    {
        public static readonly TestUser Admin = new()
        {
            Email = "admin@tunismotors.tn",
            Password = "AdminTunis2025!",
            FirstName = "Mohamed",
            LastName = "Ben Ali",
            PhoneNumber = "+216 71 234 567",
            Role = "Admin",
            Address = "Centre Urbain Nord, Tunis"
        };

        public static readonly TestUser Seller = new()
        {
            Email = "vendeur@tunismotors.tn",
            Password = "VendeurTN2025!",
            FirstName = "Ahmed",
            LastName = "Khlif",
            PhoneNumber = "+216 98 765 432",
            Role = "Seller",
            Address = "Lac 2, Tunis"
        };

        public static readonly TestUser Buyer = new()
        {
            Email = "acheteur@gmail.com",
            Password = "AcheteurTN2025!",
            FirstName = "Sami",
            LastName = "Trabelsi",
            PhoneNumber = "+216 55 123 456",
            Role = "Buyer",
            Address = "Sfax Centre"
        };

        public static readonly TestUser[] AllUsers = { Admin, Seller, Buyer };
    }

    #endregion

    #region Véhicules Populaires en Tunisie

    public static class Cars
    {
        public static readonly TestCarListing PeugeotPartner = new()
        {
            Name = "Peugeot Partner 2022",
            Brand = "Peugeot",
            Model = "Partner",
            Year = 2022,
            Price = 45000m,
            Mileage = 35000,
            FuelType = "Diesel",
            Transmission = "Manuelle",
            Color = "Blanc",
            Description = "Utilitaire en excellent état, première main, entretien concessionnaire"
        };

        public static readonly TestCarListing Clio5 = new()
        {
            Name = "Renault Clio 5 2021",
            Brand = "Renault",
            Model = "Clio",
            Year = 2021,
            Price = 38000m,
            Mileage = 42000,
            FuelType = "Essence",
            Transmission = "Automatique",
            Color = "Gris",
            Description = "Berline compacte, équipée, GPS intégré"
        };

        public static readonly TestCarListing Golf8 = new()
        {
            Name = "Volkswagen Golf 8 GTI 2023",
            Brand = "Volkswagen",
            Model = "Golf",
            Year = 2023,
            Price = 85000m,
            Mileage = 15000,
            FuelType = "Essence",
            Transmission = "Automatique DSG",
            Color = "Rouge",
            Description = "Performance GTI, full options, garantie constructeur"
        };

        public static readonly TestCarListing Dacia = new()
        {
            Name = "Dacia Duster 2020",
            Brand = "Dacia",
            Model = "Duster",
            Year = 2020,
            Price = 52000m,
            Mileage = 68000,
            FuelType = "Diesel",
            Transmission = "Manuelle",
            Color = "Noir",
            Description = "SUV familial, 4x4, idéal pour routes tunisiennes"
        };

        public static readonly TestCarListing Toyota = new()
        {
            Name = "Toyota Yaris Cross 2024",
            Brand = "Toyota",
            Model = "Yaris Cross",
            Year = 2024,
            Price = 75000m,
            Mileage = 5000,
            FuelType = "Hybride",
            Transmission = "CVT",
            Color = "Bleu",
            Description = "SUV compact hybride, économique, neuf"
        };

        public static readonly TestCarListing[] AllCars = { PeugeotPartner, Clio5, Golf8, Dacia, Toyota };
    }

    #endregion

    #region Catégories de Véhicules

    public static class Categories
    {
        public static readonly TestCategory Berline = new()
        {
            Id = 1,
            Name = "Berline",
            Description = "Voitures de tourisme classiques"
        };

        public static readonly TestCategory SUV = new()
        {
            Id = 2,
            Name = "SUV",
            Description = "Véhicules utilitaires sport"
        };

        public static readonly TestCategory Utilitaire = new()
        {
            Id = 3,
            Name = "Utilitaire",
            Description = "Véhicules commerciaux et fourgons"
        };

        public static readonly TestCategory Citadine = new()
        {
            Id = 4,
            Name = "Citadine",
            Description = "Petites voitures urbaines"
        };

        public static readonly TestCategory Luxe = new()
        {
            Id = 5,
            Name = "Luxe",
            Description = "Véhicules premium et haut de gamme"
        };

        public static readonly TestCategory[] AllCategories = { Berline, SUV, Utilitaire, Citadine, Luxe };
    }

    #endregion

    #region Plages de Prix Tunisiennes (TND)

    public static class PriceRanges
    {
        public static readonly (decimal Min, decimal Max) Budget = (15000m, 30000m);
        public static readonly (decimal Min, decimal Max) Moyenne = (30000m, 60000m);
        public static readonly (decimal Min, decimal Max) Premium = (60000m, 100000m);
        public static readonly (decimal Min, decimal Max) Luxe = (100000m, 500000m);
    }

    #endregion

    #region Données de Validation

    public static class ValidationData
    {
        // Emails valides
        public static readonly string[] ValidEmails =
        {
            "user@tunismotors.tn",
            "contact@gmail.com",
            "ahmed.khlif@yahoo.fr",
            "test123@outlook.com"
        };

        // Emails invalides
        public static readonly string[] InvalidEmails =
        {
            "invalid-email",
            "@missing-local.com",
            "missing@.com",
            "spaces in@email.com",
            ""
        };

        // Mots de passe valides (forts)
        public static readonly string[] ValidPasswords =
        {
            "TunisMotors2025!",
            "SecurePass123@",
            "MyStr0ng#Password",
            "Test!ng2025$"
        };

        // Mots de passe invalides (faibles)
        public static readonly string[] InvalidPasswords =
        {
            "123",           // Trop court
            "password",      // Pas de chiffres/majuscules
            "PASSWORD123",   // Pas de minuscules
            "Pass word1!",   // Contient espace
            ""               // Vide
        };

        // Numéros de téléphone tunisiens valides
        public static readonly string[] ValidTunisianPhones =
        {
            "+216 98 765 432",
            "+216 55 123 456",
            "+216 71 234 567",
            "+21620123456",
            "98765432"
        };
    }

    #endregion

    #region Scénarios de Test

    public static class Scenarios
    {
        public static TestScenario BuyerSearchCar => new()
        {
            Name = "Acheteur recherche véhicule",
            Description = "Un acheteur tunisien cherche une voiture entre 30k et 50k TND",
            MinPrice = 30000m,
            MaxPrice = 50000m,
            ExpectedBrands = new[] { "Peugeot", "Renault", "Dacia" }
        };

        public static TestScenario SellerCreateListing => new()
        {
            Name = "Vendeur crée annonce",
            Description = "Un vendeur publie une annonce pour sa Peugeot",
            Car = Cars.PeugeotPartner,
            User = Users.Seller
        };

        public static TestScenario AdminApproveListings => new()
        {
            Name = "Admin approuve annonces",
            Description = "L'administrateur valide les nouvelles annonces",
            User = Users.Admin,
            RequiredRole = "Admin"
        };
    }

    #endregion
}

#region Test Data Classes

public class TestUser
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}

public class TestCarListing
{
    public string Name { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal Price { get; set; }
    public int Mileage { get; set; }
    public string FuelType { get; set; } = string.Empty;
    public string Transmission { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class TestCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class TestScenario
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
    public string[]? ExpectedBrands { get; set; }
    public TestCarListing? Car { get; set; }
    public TestUser? User { get; set; }
    public string? RequiredRole { get; set; }
}

#endregion
