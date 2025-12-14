import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate, Trend, Counter } from 'k6/metrics';

// Métriques personnalisées
const errorRate = new Rate('errors');
const responseTime = new Trend('response_time');
const requestCount = new Counter('requests');

// Configuration du test
export const options = {
    stages: [
        { duration: '30s', target: 10 },   // Montée progressive : 10 utilisateurs en 30s
        { duration: '1m', target: 20 },    // Stabilisation : 20 utilisateurs pendant 1min
        { duration: '30s', target: 30 },    // Pic de charge : 30 utilisateurs en 30s
        { duration: '1m', target: 30 },   // Charge soutenue : 30 utilisateurs pendant 1min
        { duration: '30s', target: 0 },    // Descente progressive : retour à 0 en 30s
    ],
    thresholds: {
        http_req_duration: ['p(95)<500', 'p(99)<1000'], // 95% des requêtes < 500ms, 99% < 1s
        // Note: Les 404 pour IDs aléatoires sont attendus et validés par les checks (100% réussis)
        // On utilise seulement la métrique 'errors' personnalisée pour les vraies erreurs
        errors: ['rate<0.01'],                           // Erreurs personnalisées < 1% (vraies erreurs uniquement)
        // http_req_failed n'est pas utilisé car il compte les 404 comme échecs alors qu'ils sont attendus
    },
};

// URL de base de l'API (à adapter selon votre environnement)
const BASE_URL = __ENV.BASE_URL || 'http://localhost:5237';

// Headers par défaut
const defaultHeaders = {
    'Content-Type': 'application/json',
    'Accept': 'application/json',
};

/**
 * Test de performance - Endpoints publics (sans authentification)
 * Module: Test et Qualité Logiciel 2025
 * Technique: Test de charge avec k6
 * Type: Test non-fonctionnel (Performance)
 */
export default function () {
    // Test 1: GET /api/carlistings - Liste des annonces (endpoint public)
    const listingsResponse = http.get(`${BASE_URL}/api/carlistings`, {
        headers: defaultHeaders,
        tags: { name: 'GetAllListings' },
    });
    
    check(listingsResponse, {
        'Listings - Status 200': (r) => r.status === 200,
        'Listings - Response time < 500ms': (r) => r.timings.duration < 500,
        'Listings - Has content': (r) => r.body.length > 0,
    }) || errorRate.add(1);
    
    requestCount.add(1);
    responseTime.add(listingsResponse.timings.duration);
    
    sleep(1); // Pause entre les requêtes

    // Test 2: GET /api/categories - Liste des catégories (endpoint public)
    const categoriesResponse = http.get(`${BASE_URL}/api/categories`, {
        headers: defaultHeaders,
        tags: { name: 'GetCategories' },
    });
    
    check(categoriesResponse, {
        'Categories - Status 200': (r) => r.status === 200,
        'Categories - Response time < 300ms': (r) => r.timings.duration < 300,
        'Categories - Has content': (r) => r.body.length > 0,
    }) || errorRate.add(1);
    
    requestCount.add(1);
    responseTime.add(categoriesResponse.timings.duration);
    
    sleep(1);

    // Test 3: GET /api/carlistings/{id} - Détails d'une annonce (endpoint public)
    // Utiliser un ID dans une plage plus réaliste (1-20 pour avoir plus de chances d'exister)
    const listingId = Math.floor(Math.random() * 20) + 1; // ID aléatoire entre 1 et 20
    const listingDetailResponse = http.get(`${BASE_URL}/api/carlistings/${listingId}`, {
        headers: defaultHeaders,
        tags: { name: 'GetListingDetail' },
    });
    
    // 404 est acceptable pour des IDs aléatoires, ne pas compter comme erreur
    const isDetailValid = listingDetailResponse.status === 200 || listingDetailResponse.status === 404;
    check(listingDetailResponse, {
        'Listing Detail - Status 200 or 404': (r) => isDetailValid,
        'Listing Detail - Response time < 400ms': (r) => r.timings.duration < 400,
    });
    
    // Ne pas ajouter aux erreurs si c'est un 404 (comportement attendu)
    if (!isDetailValid || (listingDetailResponse.status !== 200 && listingDetailResponse.status !== 404)) {
        errorRate.add(1);
    }
    
    requestCount.add(1);
    responseTime.add(listingDetailResponse.timings.duration);
    
    sleep(1);

    // Test 4: GET /api/carlistings avec filtres - Recherche avec paramètres (endpoint public)
    const searchParams = {
        brand: 'Peugeot',
        minPrice: 10000,
        maxPrice: 50000,
        page: 1,
        pageSize: 10,
    };
    
    const searchResponse = http.get(`${BASE_URL}/api/carlistings`, {
        params: searchParams,
        headers: defaultHeaders,
        tags: { name: 'SearchListings' },
    });
    
    check(searchResponse, {
        'Search - Status 200': (r) => r.status === 200,
        'Search - Response time < 600ms': (r) => r.timings.duration < 600,
        'Search - Has content': (r) => r.body.length > 0,
    }) || errorRate.add(1);
    
    requestCount.add(1);
    responseTime.add(searchResponse.timings.duration);
    
    sleep(1);

    // Test 5: GET /api/carlistings/filters/brands - Filtres de marques (endpoint public)
    const brandsResponse = http.get(`${BASE_URL}/api/carlistings/filters/brands`, {
        headers: defaultHeaders,
        tags: { name: 'GetBrands' },
    });
    
    check(brandsResponse, {
        'Brands - Status 200': (r) => r.status === 200,
        'Brands - Response time < 300ms': (r) => r.timings.duration < 300,
    }) || errorRate.add(1);
    
    requestCount.add(1);
    responseTime.add(brandsResponse.timings.duration);
    
    sleep(1);
}

/**
 * Fonction de setup exécutée une fois avant le début des tests
 */
export function setup() {
    console.log(`🚀 Démarrage des tests de performance k6 pour Tunisia Motors API`);
    console.log(`📍 URL de base: ${BASE_URL}`);
    
    // Vérification de la disponibilité de l'API
    const healthCheck = http.get(`${BASE_URL}/api/carlistings`);
    if (healthCheck.status !== 200) {
        console.warn(`⚠️  Attention: L'API semble ne pas être disponible (Status: ${healthCheck.status})`);
    } else {
        console.log(`✅ API disponible et répond correctement`);
    }
    
    return { baseUrl: BASE_URL };
}

/**
 * Fonction de teardown exécutée une fois après la fin des tests
 */
export function teardown(data) {
    console.log(`✅ Tests de performance terminés`);
    console.log(`📊 Consultez les métriques ci-dessus pour les résultats détaillés`);
}
