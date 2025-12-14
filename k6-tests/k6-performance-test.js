import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate, Trend, Counter } from 'k6/metrics';

// Métriques personnalisées
const errorRate = new Rate('errors');
const responseTime = new Trend('response_time');
const requestCount = new Counter('requests');
const authSuccessRate = new Rate('auth_success');

// Configuration du test - Correspond à run-all-performance-tests.ps1
export const options = {
    stages: [
        { duration: '10s', target: 5 },    // Montée progressive : 5 utilisateurs en 10s
        { duration: '20s', target: 10 },   // Stabilisation : 10 utilisateurs pendant 20s
        { duration: '10s', target: 15 },   // Pic de charge : 15 utilisateurs en 10s
        { duration: '20s', target: 15 },   // Charge soutenue : 15 utilisateurs pendant 20s
        { duration: '10s', target: 0 },    // Descente progressive : retour à 0 en 10s
    ],
    thresholds: {
        http_req_duration: ['p(95)<500', 'p(99)<1000'], // 95% des requêtes < 500ms, 99% < 1s
        http_req_failed: ['rate<0.05'],                  // Taux d'erreur < 5%
        errors: ['rate<0.05'],                           // Erreurs personnalisées < 5%
        auth_success: ['rate>0.95'],                     // Taux de succès authentification > 95%
    },
};

// URL de base de l'API
const BASE_URL = __ENV.BASE_URL || 'http://localhost:5237';

// Credentials admin
const ADMIN_EMAIL = 'admin@tunis-motors.com';
const ADMIN_PASSWORD = 'Admin@123456';

// Headers par défaut
const defaultHeaders = {
    'Content-Type': 'application/json',
    'Accept': 'application/json',
};

/**
 * Fonction de setup - Authentification admin une fois
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
    
    // Authentification admin pour obtenir le token
    console.log(`🔐 Authentification avec compte admin...`);
    const loginResponse = http.post(`${BASE_URL}/api/auth/login`, 
        JSON.stringify({
            email: ADMIN_EMAIL,
            password: ADMIN_PASSWORD
        }),
        {
            headers: defaultHeaders,
        }
    );
    
    let token = null;
    if (loginResponse.status === 200) {
        try {
            const loginData = JSON.parse(loginResponse.body);
            // La réponse peut être directement AuthResponseDto ou encapsulée dans ApiResponse
            if (loginData.data && loginData.data.token) {
                // Format ApiResponse<AuthResponseDto>
                token = loginData.data.token;
                console.log(`✅ Authentification réussie - Token obtenu (format ApiResponse)`);
            } else if (loginData.data && loginData.data.Token) {
                // Format ApiResponse avec Token majuscule
                token = loginData.data.Token;
                console.log(`✅ Authentification réussie - Token obtenu (format ApiResponse Token)`);
            } else if (loginData.token) {
                // Format direct AuthResponseDto
                token = loginData.token;
                console.log(`✅ Authentification réussie - Token obtenu (format direct)`);
            } else if (loginData.Token) {
                // Format direct avec Token majuscule
                token = loginData.Token;
                console.log(`✅ Authentification réussie - Token obtenu (format direct Token)`);
            } else {
                console.warn(`⚠️  Token non trouvé dans la réponse`);
                console.warn(`   Structure réponse: ${JSON.stringify(loginData).substring(0, 200)}`);
            }
        } catch (e) {
            console.warn(`⚠️  Erreur parsing réponse login: ${e}`);
            console.warn(`   Réponse brute: ${loginResponse.body.substring(0, 200)}`);
        }
    } else {
        console.warn(`⚠️  Échec authentification - Status: ${loginResponse.status}`);
        console.warn(`   Réponse: ${loginResponse.body.substring(0, 200)}`);
    }
    
    return { 
        baseUrl: BASE_URL,
        token: token
    };
}

/**
 * Test de performance avec authentification
 */
export default function (data) {
    const token = data.token;
    
    if (!token) {
        console.error('❌ Token non disponible - Les tests authentifiés seront ignorés');
        errorRate.add(1);
        sleep(1);
        return;
    }
    
    // Headers avec authentification
    const authHeaders = {
        ...defaultHeaders,
        'Authorization': `Bearer ${token}`
    };
    
    // Liste des endpoints à tester (correspond à run-all-performance-tests.ps1)
    // Endpoints publics (5)
    const publicEndpoints = [
        { url: `${BASE_URL}/api/carlistings`, name: 'GetAllListings', useAuth: false },
        { url: `${BASE_URL}/api/categories`, name: 'GetCategories', useAuth: false },
        { url: `${BASE_URL}/api/carlistings/1`, name: 'GetListingDetail', useAuth: false },
        { url: `${BASE_URL}/api/carlistings?brand=Peugeot&page=1&pageSize=10`, name: 'SearchListings', useAuth: false },
        { url: `${BASE_URL}/api/carlistings/filters/brands`, name: 'GetBrands', useAuth: false },
    ];
    
    // Endpoints authentifiés (2) - seulement si token disponible
    const authEndpoints = [];
    if (token) {
        authEndpoints.push(
            { url: `${BASE_URL}/api/users/me`, name: 'GetUserProfile', useAuth: true },
            { url: `${BASE_URL}/api/carlistings/pending-approvals`, name: 'GetPendingApprovals', useAuth: true }
        );
    }
    
    // Combiner tous les endpoints
    const allEndpoints = [...publicEndpoints, ...authEndpoints];
    
    // Tester chaque endpoint de manière cyclique (comme dans run-all-performance-tests.ps1)
    const endpointIndex = __ITER % allEndpoints.length;
    const endpoint = allEndpoints[endpointIndex];
    
    const headers = endpoint.useAuth ? authHeaders : defaultHeaders;
    
    let response;
    if (endpoint.url.includes('?')) {
        // Endpoint avec paramètres de requête
        const urlParts = endpoint.url.split('?');
        const baseUrl = urlParts[0];
        const params = {};
        urlParts[1].split('&').forEach(param => {
            const [key, value] = param.split('=');
            params[key] = value;
        });
        response = http.get(baseUrl, {
            params: params,
            headers: headers,
            tags: { name: endpoint.name },
        });
    } else {
        response = http.get(endpoint.url, {
            headers: headers,
            tags: { name: endpoint.name },
        });
    }
    
    // Checks selon le type d'endpoint
    if (endpoint.name === 'GetListingDetail') {
        check(response, {
            [`${endpoint.name} - Status 200 or 404`]: (r) => r.status === 200 || r.status === 404,
            [`${endpoint.name} - Response time < 400ms`]: (r) => r.timings.duration < 400,
        }) || errorRate.add(1);
    } else if (endpoint.name === 'GetPendingApprovals') {
        check(response, {
            [`${endpoint.name} - Status 200 or 403`]: (r) => r.status === 200 || r.status === 403,
            [`${endpoint.name} - Response time < 500ms`]: (r) => r.timings.duration < 500,
        }) || errorRate.add(1);
    } else {
        check(response, {
            [`${endpoint.name} - Status 200`]: (r) => r.status === 200,
            [`${endpoint.name} - Response time < 500ms`]: (r) => r.timings.duration < 500,
            [`${endpoint.name} - Has content`]: (r) => r.body && r.body.length > 0,
        }) || errorRate.add(1);
    }
    
    requestCount.add(1);
    responseTime.add(response.timings.duration);
    sleep(0.05); // Correspond à 50ms dans PowerShell
}

/**
 * Fonction de teardown
 */
export function teardown(data) {
    console.log(`✅ Tests de performance terminés`);
    console.log(`📊 Consultez les métriques ci-dessus pour les résultats détaillés`);
}


