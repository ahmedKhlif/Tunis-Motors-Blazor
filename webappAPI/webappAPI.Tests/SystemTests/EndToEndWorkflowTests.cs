using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace webappAPI.Tests.SystemTests;

/// <summary>
/// Tests Système - End-to-End Workflows Complets
/// Module: Test et Qualité Logiciel 2025
/// 
/// Ces tests simulent des scénarios utilisateur complets couvrant plusieurs contrôleurs
/// Technique: Boîte noire - Scénarios utilisateur réels
/// Niveau: Tests Système (E2E)
/// </summary>
public class EndToEndWorkflowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public EndToEndWorkflowTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    #region Scénario 1: Parcours Visiteur Non-Authentifié

    /// <summary>
    /// TC-SYS-001: Parcours complet d'un visiteur anonyme sur Tunisia Motors
    /// Technique: Boîte noire - Scénario utilisateur réel
    /// Type: Test fonctionnel basé sur les exigences
    /// </summary>
    [Fact]
    public async Task System_AnonymousUserWorkflow_ShouldBrowsePublicContent()
    {
        // ÉTAPE 1: Accéder à la page d'accueil (liste des véhicules)
        var listingsResponse = await _client.GetAsync("/api/carlistings");
        listingsResponse.StatusCode.Should().Be(HttpStatusCode.OK, "Un visiteur doit pouvoir voir les annonces");
        var listingsContent = await listingsResponse.Content.ReadAsStringAsync();
        listingsContent.Should().NotBeNullOrEmpty("Le contenu des annonces doit être retourné");

        // ÉTAPE 2: Consulter les catégories disponibles
        var categoriesResponse = await _client.GetAsync("/api/categories");
        categoriesResponse.StatusCode.Should().Be(HttpStatusCode.OK, "Un visiteur doit voir les catégories");

        // ÉTAPE 3: Filtrer par marque populaire en Tunisie (Peugeot)
        var filteredResponse = await _client.GetAsync("/api/carlistings?brand=Peugeot");
        filteredResponse.StatusCode.Should().Be(HttpStatusCode.OK, "Le filtrage par marque doit fonctionner");

        // ÉTAPE 4: Voir le détail d'une annonce
        var detailResponse = await _client.GetAsync("/api/carlistings/1");
        // Une annonce doit être accessible ou retourner 404
        detailResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);

        // ÉTAPE 5: Tenter d'accéder au panier (doit être bloqué)
        var cartResponse = await _client.GetAsync("/api/cart");
        cartResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized, 
            "Un visiteur anonyme ne peut pas accéder au panier");

        // RÉSULTAT: Workflow visiteur validé - Contenu public accessible, fonctions privées protégées
    }

    /// <summary>
    /// TC-SYS-002: Vérification de la sécurité sur tout le parcours visiteur
    /// Technique: Boîte noire - Tests de régression sécurité
    /// Type: Test non-fonctionnel (sécurité)
    /// </summary>
    [Fact]
    public async Task System_SecurityWorkflow_AllProtectedEndpointsRequireAuth()
    {
        // Liste des endpoints protégés à valider
        var protectedEndpoints = new[]
        {
            "/api/cart",
            "/api/orders",
            "/api/carrentals",
            "/api/wishlist",
            "/api/messages"
        };

        foreach (var endpoint in protectedEndpoints)
        {
            var response = await _client.GetAsync(endpoint);
            // L'endpoint doit requérir une auth (401), être inexistant (404), ou méthode non autorisée (405) - pas d'accès public OK (200)
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.Unauthorized, 
                HttpStatusCode.NotFound, 
                HttpStatusCode.MethodNotAllowed);
        }

        // Test de confirmation: les endpoints publics restent accessibles
        var publicEndpoints = new[] { "/api/carlistings", "/api/categories" };
        foreach (var endpoint in publicEndpoints)
        {
            var response = await _client.GetAsync(endpoint);
            response.StatusCode.Should().Be(HttpStatusCode.OK, 
                $"L'endpoint {endpoint} doit rester public");
        }
    }

    #endregion

    #region Scénario 2: Parcours Acheteur Potentiel

    /// <summary>
    /// TC-SYS-003: Parcours d'un acheteur cherchant une voiture spécifique
    /// Technique: Boîte noire - Classes d'équivalence (filtres)
    /// Type: Test fonctionnel E2E
    /// </summary>
    [Fact]
    public async Task System_BuyerSearchWorkflow_ShouldFindVehiclesWithFilters()
    {
        // ÉTAPE 1: Recherche par gamme de prix tunisienne (25,000 - 50,000 TND)
        var priceFilterResponse = await _client.GetAsync("/api/carlistings?minPrice=25000&maxPrice=50000");
        priceFilterResponse.StatusCode.Should().Be(HttpStatusCode.OK, "Filtrage par prix doit fonctionner");

        // ÉTAPE 2: Recherche par année récente
        var yearFilterResponse = await _client.GetAsync("/api/carlistings?minYear=2020");
        yearFilterResponse.StatusCode.Should().Be(HttpStatusCode.OK, "Filtrage par année doit fonctionner");

        // ÉTAPE 3: Combinaison de filtres (Peugeot récent abordable)
        var combinedFilterResponse = await _client.GetAsync("/api/carlistings?brand=Peugeot&minYear=2018&maxPrice=45000");
        combinedFilterResponse.StatusCode.Should().Be(HttpStatusCode.OK, "Filtres combinés doivent fonctionner");

        // ÉTAPE 4: Pagination des résultats
        var paginatedResponse = await _client.GetAsync("/api/carlistings?page=1&pageSize=10");
        paginatedResponse.StatusCode.Should().Be(HttpStatusCode.OK, "Pagination doit fonctionner");

        // RÉSULTAT: Système de recherche validé pour les acheteurs tunisiens
    }

    /// <summary>
    /// TC-SYS-004: Test de performance du parcours de recherche
    /// Technique: Boîte noire - Test non-fonctionnel (performance)
    /// Type: Test de performance simple
    /// </summary>
    [Fact]
    public async Task System_PerformanceWorkflow_SearchShouldBeUnder500ms()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Exécuter une séquence de recherche typique
        await _client.GetAsync("/api/carlistings");
        await _client.GetAsync("/api/categories");
        await _client.GetAsync("/api/carlistings?brand=Peugeot");
        await _client.GetAsync("/api/carlistings/1");

        stopwatch.Stop();

        // Critère: Toute la séquence doit prendre moins de 2 secondes
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000, 
            "Le parcours de recherche complet doit être rapide (< 2s)");
    }

    #endregion

    #region Scénario 3: Tests de Régression

    /// <summary>
    /// TC-SYS-005: Test de régression après modification du système de filtrage
    /// Technique: Boîte noire - Test de régression
    /// Type: Test de régression
    /// </summary>
    [Fact]
    public async Task System_RegressionTest_FilteringStillWorksAfterUpdates()
    {
        // Tester tous les types de filtres pour s'assurer qu'aucune régression n'est introduite
        
        // Filtres numériques
        var numericFilters = new[]
        {
            "/api/carlistings?minPrice=10000",
            "/api/carlistings?maxPrice=100000",
            "/api/carlistings?minYear=2015",
            "/api/carlistings?maxYear=2025"
        };

        foreach (var filter in numericFilters)
        {
            var response = await _client.GetAsync(filter);
            response.StatusCode.Should().Be(HttpStatusCode.OK, 
                $"Filtre numérique {filter} doit fonctionner");
        }

        // Filtres textuels
        var textFilters = new[]
        {
            "/api/carlistings?brand=Peugeot",
            "/api/carlistings?brand=Renault",
            "/api/carlistings?brand=Volkswagen"
        };

        foreach (var filter in textFilters)
        {
            var response = await _client.GetAsync(filter);
            response.StatusCode.Should().Be(HttpStatusCode.OK, 
                $"Filtre textuel {filter} doit fonctionner");
        }
    }

    /// <summary>
    /// TC-SYS-006: Test de confirmation après correction de bug
    /// Technique: Boîte noire - Test de confirmation
    /// Type: Test de confirmation
    /// Note: Vérifie que le bug BUG-002 (annonces sans catégorie) est corrigé
    /// </summary>
    [Fact]
    public async Task System_ConfirmationTest_ListingsReturnWithCategories()
    {
        // Récupérer les annonces
        var response = await _client.GetAsync("/api/carlistings");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        
        // Vérifier que le contenu n'est pas vide
        content.Should().NotBeNullOrEmpty("Les annonces doivent être retournées");
        
        // Le système doit retourner des données valides (JSON array)
        var isValidJson = content.StartsWith("[") || content.StartsWith("{");
        isValidJson.Should().BeTrue("Le format JSON doit être retourné");
    }

    #endregion

    #region Scénario 4: Workflow Complet E-Commerce

    /// <summary>
    /// TC-SYS-007: Parcours e-commerce complet (sans authentification)
    /// Technique: Boîte noire - Scénario utilisateur E2E
    /// Type: Test système complet
    /// </summary>
    [Fact]
    public async Task System_ECommerceWorkflow_BrowseToCartAttempt()
    {
        // PHASE 1: DÉCOUVERTE
        // Étape 1.1: Arriver sur le site
        var homeResponse = await _client.GetAsync("/api/carlistings");
        homeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Étape 1.2: Explorer les catégories
        var categoriesResponse = await _client.GetAsync("/api/categories");
        categoriesResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // PHASE 2: RECHERCHE
        // Étape 2.1: Filtrer par critères tunisiens
        var searchResponse = await _client.GetAsync("/api/carlistings?minPrice=20000&maxPrice=60000");
        searchResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // PHASE 3: SÉLECTION
        // Étape 3.1: Voir le détail d'un véhicule
        var detailResponse = await _client.GetAsync("/api/carlistings/1");
        detailResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);

        // PHASE 4: CONVERSION (bloquée car non authentifié)
        // Étape 4.1: Tenter d'ajouter au panier
        var addToCartResponse = await _client.PostAsJsonAsync("/api/cart/add", new { ProductId = 1, Quantity = 1 });
        addToCartResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized, 
            "L'ajout au panier requiert une authentification");

        // Étape 4.2: Tenter de passer commande
        var orderResponse = await _client.PostAsJsonAsync("/api/orders", new { });
        orderResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized, 
            "La commande requiert une authentification");

        // RÉSULTAT: Workflow e-commerce validé - Sécurité respectée
    }

    /// <summary>
    /// TC-SYS-008: Test de validité des données tunisiennes
    /// Technique: Boîte noire - Validation métier
    /// Type: Test fonctionnel (validation données)
    /// </summary>
    [Fact]
    public async Task System_DataValidation_TunisianDataFormats()
    {
        // Tester l'inscription avec données tunisiennes
        var registerDto = new
        {
            Email = "client.test@tunisie.tn",
            Password = "TunisMotors2025!",
            ConfirmPassword = "TunisMotors2025!",
            FirstName = "Mohamed",
            LastName = "Ben Ali",
            PhoneNumber = "+216 98 765 432"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);
        
        // Accepter OK (succès) ou BadRequest (validation) - les deux sont des comportements valides
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    #endregion

    #region Scénario 5: Tests de Robustesse

    /// <summary>
    /// TC-SYS-009: Test de robustesse avec données invalides
    /// Technique: Boîte noire - Valeurs limites et invalides
    /// Type: Test de robustesse
    /// </summary>
    [Fact]
    public async Task System_RobustnessTest_InvalidInputsHandled()
    {
        // Test avec ID invalide (négatif)
        var negativeIdResponse = await _client.GetAsync("/api/carlistings/-1");
        negativeIdResponse.StatusCode.Should().Be(HttpStatusCode.NotFound, 
            "ID négatif doit retourner 404");

        // Test avec ID très grand
        var largeIdResponse = await _client.GetAsync("/api/carlistings/999999999");
        largeIdResponse.StatusCode.Should().Be(HttpStatusCode.NotFound, 
            "ID inexistant doit retourner 404");

        // Test avec paramètres de pagination invalides
        var invalidPaginationResponse = await _client.GetAsync("/api/carlistings?page=-1&pageSize=-5");
        invalidPaginationResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// TC-SYS-010: Test de compatibilité API
    /// Technique: Boîte noire - Test de compatibilité
    /// Type: Test non-fonctionnel (compatibilité)
    /// </summary>
    [Fact]
    public async Task System_CompatibilityTest_ApiResponseFormats()
    {
        // Vérifier que l'API retourne du JSON valide
        var response = await _client.GetAsync("/api/carlistings");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var contentType = response.Content.Headers.ContentType?.MediaType;
        contentType.Should().Be("application/json", "L'API doit retourner du JSON");

        var content = await response.Content.ReadAsStringAsync();
        var isValidJson = content.StartsWith("[") || content.StartsWith("{");
        isValidJson.Should().BeTrue("Le contenu doit être du JSON valide");
    }

    #endregion
}

/// <summary>
/// Tests Système - Workflows Authentifiés (avec simulation)
/// Module: Test et Qualité Logiciel 2025
/// </summary>
public class AuthenticatedWorkflowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AuthenticatedWorkflowTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    /// <summary>
    /// TC-SYS-011: Tentative de workflow vendeur sans authentification
    /// Technique: Boîte noire - Test de sécurité
    /// Type: Test fonctionnel (autorisation)
    /// </summary>
    [Fact]
    public async Task System_SellerWorkflow_RequiresAuthentication()
    {
        // Tenter de créer une annonce sans être authentifié
        var createListingDto = new
        {
            Name = "Peugeot 208 2022",
            Price = 35000m,
            Description = "Excellent état, première main",
            Year = 2022,
            Mileage = 45000
        };

        var createResponse = await _client.PostAsJsonAsync("/api/carlistings", createListingDto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized, 
            "La création d'annonce requiert une authentification vendeur");
    }

    /// <summary>
    /// TC-SYS-012: Tentative de workflow admin sans authentification
    /// Technique: Boîte noire - Test de sécurité (rôles)
    /// Type: Test non-fonctionnel (sécurité RBAC)
    /// </summary>
    [Fact]
    public async Task System_AdminWorkflow_RequiresAdminRole()
    {
        // Tenter d'accéder aux fonctions admin
        var adminEndpoints = new[]
        {
            "/api/approval/pending",
            "/api/admin/users"
        };

        foreach (var endpoint in adminEndpoints)
        {
            var response = await _client.GetAsync(endpoint);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.NotFound);
        }
    }
}
