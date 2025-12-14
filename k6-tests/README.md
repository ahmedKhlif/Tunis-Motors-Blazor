# Tests de Performance k6 - Tunisia Motors API

## Description

Scripts de test de charge et de performance pour l'API Tunisia Motors utilisant k6.

**Module**: Test et Qualité Logiciel 2025  
**Technique**: Test de charge avec k6  
**Type**: Test non-fonctionnel (Performance)

## Prérequis

1. Installer k6 : https://k6.io/docs/getting-started/installation/
   - Windows : `choco install k6` ou télécharger depuis https://k6.io/docs/getting-started/installation/
   - Linux : `sudo gpg -k && sudo gpg --no-default-keyring --keyring /usr/share/keyrings/k6-archive-keyring.gpg --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys C5AD17C747E3415A3642D57D77C6C491D6AC1D9B && echo "deb [signed-by=/usr/share/keyrings/k6-archive-keyring.gpg] https://dl.k6.io/deb stable main" | sudo tee /etc/apt/sources.list.d/k6.list && sudo apt-get update && sudo apt-get install k6`
   - macOS : `brew install k6`

2. Démarrer l'API Tunisia Motors :
   ```bash
   cd webappAPI/webappAPI
   dotnet run
   ```
   L'API sera accessible sur `http://localhost:5237` ou `https://localhost:7171`

## Exécution des Tests

### Test de base (performance-test.js)

```bash
# Depuis le dossier k6-tests
k6 run performance-test.js
```

### Avec URL personnalisée

```bash
k6 run --env BASE_URL=http://localhost:5237 performance-test.js
```

### Avec sortie JSON pour analyse

```bash
k6 run --out json=results.json performance-test.js
```

## Scénarios de Test

Le script `performance-test.js` teste les endpoints publics suivants :

1. **GET /api/carlistings** - Liste des annonces
2. **GET /api/categories** - Liste des catégories
3. **GET /api/carlistings/{id}** - Détails d'une annonce
4. **GET /api/carlistings** (avec filtres) - Recherche avec paramètres
5. **GET /api/carlistings/filters/brands** - Filtres de marques

## Profil de Charge

- **Montée progressive** : 0 → 10 utilisateurs en 30s
- **Stabilisation** : 20 utilisateurs pendant 1min
- **Pic de charge** : 30 utilisateurs en 30s
- **Charge soutenue** : 30 utilisateurs pendant 1min
- **Descente** : 30 → 0 utilisateurs en 30s

**Durée totale** : ~4 minutes

## Seuils de Performance (Thresholds)

- **95% des requêtes** < 500ms
- **99% des requêtes** < 1000ms
- **Taux d'erreur** < 1%

## Résultats Attendus

Les tests vérifient :
- Temps de réponse acceptable pour chaque endpoint
- Taux d'erreur minimal
- Disponibilité de l'API sous charge
- Performance des requêtes avec filtres

## Interprétation des Résultats

- ✅ **PASS** : Tous les seuils sont respectés
- ❌ **FAIL** : Un ou plusieurs seuils sont dépassés

Les métriques affichées incluent :
- `http_req_duration` : Temps de réponse des requêtes
- `http_req_failed` : Taux d'échec des requêtes
- `errors` : Erreurs personnalisées détectées
- `iterations` : Nombre d'itérations complétées
- `vus` : Nombre d'utilisateurs virtuels




