# Script pour executer plusieurs fois les tests de performance
# Module: Test et Qualite Logiciel 2025

param(
    [string]$BaseUrl = "http://localhost:5237",
    [int]$NumberOfRuns = 3,
    [int]$RequestsPerRun = 50
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Tests de Performance Multiples" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verifier que l'API est accessible
Write-Host "[Verification] Test de l'API..." -ForegroundColor Yellow
try {
    $healthCheck = Invoke-WebRequest -Uri "$BaseUrl/api/carlistings" -Method GET -TimeoutSec 5 -UseBasicParsing -ErrorAction Stop
    if ($healthCheck.StatusCode -eq 200) {
        Write-Host "API accessible sur $BaseUrl" -ForegroundColor Green
    }
} catch {
    Write-Host "L'API n'est pas accessible sur $BaseUrl" -ForegroundColor Red
    Write-Host "   Assurez-vous que l'API est demarree:" -ForegroundColor Yellow
    Write-Host "   cd webappAPI\webappAPI" -ForegroundColor White
    Write-Host "   dotnet run" -ForegroundColor White
    Write-Host ""
    exit 1
}

Write-Host ""
Write-Host "Configuration:" -ForegroundColor Yellow
Write-Host "  Nombre d'executions: $NumberOfRuns" -ForegroundColor White
Write-Host "  Requetes par execution: $RequestsPerRun" -ForegroundColor White
Write-Host ""

# Creer dossier pour les resultats
$resultsDir = Join-Path $PSScriptRoot "results"
if (-not (Test-Path $resultsDir)) {
    New-Item -ItemType Directory -Path $resultsDir | Out-Null
}

# ========================================
# TEST 1: Endpoints publics (performance-test.js equivalent)
# ========================================
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "TEST 1: Endpoints Publics" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$publicEndpoints = @(
    "/api/carlistings",
    "/api/categories",
    "/api/carlistings/1",
    "/api/carlistings?brand=Peugeot&page=1&pageSize=10",
    "/api/carlistings/filters/brands"
)

$allPublicResults = @()

for ($run = 1; $run -le $NumberOfRuns; $run++) {
    Write-Host "--- Execution $run/$NumberOfRuns (Endpoints publics) ---" -ForegroundColor Yellow
    
    $results = @{
        Total = 0
        Success = 0
        Failed = 0
        ResponseTimes = @()
    }
    
    for ($i = 0; $i -lt $RequestsPerRun; $i++) {
        $endpoint = $publicEndpoints[$i % $publicEndpoints.Count]
        $url = "$BaseUrl$endpoint"
        
        $startTime = Get-Date
        try {
            $response = Invoke-WebRequest -Uri $url -Method GET -TimeoutSec 5 -UseBasicParsing -ErrorAction Stop
            $endTime = Get-Date
            $duration = ($endTime - $startTime).TotalMilliseconds
            
            $results.Total++
            if ($response.StatusCode -eq 200 -or $response.StatusCode -eq 404) {
                $results.Success++
                $results.ResponseTimes += $duration
            } else {
                $results.Failed++
            }
            
            Write-Host "." -NoNewline -ForegroundColor Green
        } catch {
            $endTime = Get-Date
            $duration = ($endTime - $startTime).TotalMilliseconds
            $results.Total++
            $results.Failed++
            Write-Host "X" -NoNewline -ForegroundColor Red
        }
        
        Start-Sleep -Milliseconds 50
    }
    
    Write-Host ""
    
    if ($results.ResponseTimes.Count -gt 0) {
        $avg = ($results.ResponseTimes | Measure-Object -Average).Average
        $min = ($results.ResponseTimes | Measure-Object -Minimum).Minimum
        $max = ($results.ResponseTimes | Measure-Object -Maximum).Maximum
        $sorted = $results.ResponseTimes | Sort-Object
        $p95 = $sorted[[Math]::Floor($sorted.Count * 0.95)]
        $p99 = $sorted[[Math]::Floor($sorted.Count * 0.99)]
        
        $successRate = ($results.Success / $results.Total) * 100
        $errorRate = ($results.Failed / $results.Total) * 100
        
        $resultData = @{
            Run = $run
            TotalRequests = $results.Total
            SuccessfulRequests = $results.Success
            FailedRequests = $results.Failed
            SuccessRate = [Math]::Round($successRate, 2)
            ErrorRate = [Math]::Round($errorRate, 2)
            AverageResponseTime = [Math]::Round($avg, 2)
            MinResponseTime = [Math]::Round($min, 2)
            MaxResponseTime = [Math]::Round($max, 2)
            P95ResponseTime = [Math]::Round($p95, 2)
            P99ResponseTime = [Math]::Round($p99, 2)
            Timestamp = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss")
        }
        
        $allPublicResults += $resultData
        
        Write-Host "  Succes: $($results.Success) | Echecs: $($results.Failed) | Moyenne: $([Math]::Round($avg, 2))ms | P95: $([Math]::Round($p95, 2))ms" -ForegroundColor Cyan
    }
    
    Write-Host ""
    Start-Sleep -Seconds 2
}

# ========================================
# TEST 2: Endpoints avec authentification admin (k6-performance-test.js equivalent)
# ========================================
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "TEST 2: Endpoints avec Authentification Admin" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$adminEmail = "admin@tunis-motors.com"
$adminPassword = "Admin@123456"

$allAuthResults = @()

# Essayer de creer le compte admin s'il n'existe pas
Write-Host "Verification/Creation du compte admin..." -ForegroundColor Yellow
try {
    $registerBody = @{
        email = $adminEmail
        password = $adminPassword
        confirmPassword = $adminPassword
        firstName = "Admin"
        lastName = "Tunisia Motors"
        role = "Admin"
    } | ConvertTo-Json
    
    $registerResponse = Invoke-WebRequest -Uri "$BaseUrl/api/auth/register" -Method POST -Body $registerBody -ContentType "application/json" -UseBasicParsing -ErrorAction SilentlyContinue
    if ($registerResponse.StatusCode -eq 200) {
        Write-Host "  Compte admin cree (email doit etre confirme)" -ForegroundColor Yellow
    }
} catch {
    # Le compte existe peut-etre deja
    Write-Host "  Compte admin existe peut-etre deja" -ForegroundColor Gray
}

Write-Host ""

for ($run = 1; $run -le $NumberOfRuns; $run++) {
    Write-Host "--- Execution $run/$NumberOfRuns (Avec authentification) ---" -ForegroundColor Yellow
    
    # Authentification
    $token = $null
    try {
        $loginBody = @{
            email = $adminEmail
            password = $adminPassword
        } | ConvertTo-Json
        
        $loginResponse = Invoke-WebRequest -Uri "$BaseUrl/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json" -UseBasicParsing -ErrorAction Stop
        if ($loginResponse.StatusCode -eq 200) {
            $loginData = $loginResponse.Content | ConvertFrom-Json
            # La reponse est encapsulee: { success: true, data: { token: "...", ... } }
            if ($loginData.data -and $loginData.data.token) {
                $token = $loginData.data.token
                Write-Host "  Authentification reussie" -ForegroundColor Green
            } elseif ($loginData.token) {
                # Format alternatif
                $token = $loginData.token
                Write-Host "  Authentification reussie (format alternatif)" -ForegroundColor Green
            } else {
                Write-Host "  Token non trouve dans la reponse" -ForegroundColor Yellow
            }
        }
    } catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        if ($statusCode -eq 401) {
            Write-Host "  Echec authentification (401) - Compte inexistant ou email non confirme" -ForegroundColor Yellow
            Write-Host "  Continuation avec tests publics uniquement pour cette execution" -ForegroundColor Yellow
        } else {
            Write-Host "  Echec authentification: $_" -ForegroundColor Red
        }
    }
    
    if (-not $token) {
        Write-Host "  Execution avec endpoints publics uniquement (sans authentification)" -ForegroundColor Yellow
    }
    
    # Tests avec authentification
    $results = @{
        Total = 0
        Success = 0
        Failed = 0
        ResponseTimes = @()
    }
    
    $authHeaders = @{
        "Authorization" = "Bearer $token"
        "Content-Type" = "application/json"
        "Accept" = "application/json"
    }
    
    # Endpoints publics
    $publicEndpoints = @(
        "/api/carlistings",
        "/api/categories",
        "/api/carlistings/1",
        "/api/carlistings?brand=Peugeot&page=1&pageSize=10",
        "/api/carlistings/filters/brands"
    )
    
    # Endpoints authentifies (seulement si token disponible)
    $authEndpoints = @()
    if ($token) {
        $authEndpoints = @(
            "/api/users/me",
            "/api/carlistings/pending-approvals"
        )
    }
    
    $allEndpoints = $publicEndpoints + $authEndpoints
    
    for ($i = 0; $i -lt $RequestsPerRun; $i++) {
        $endpoint = $allEndpoints[$i % $allEndpoints.Count]
        $url = "$BaseUrl$endpoint"
        
        $useAuth = $authEndpoints -contains $endpoint
        
        $startTime = Get-Date
        try {
            if ($useAuth) {
                $response = Invoke-WebRequest -Uri $url -Method GET -Headers $authHeaders -TimeoutSec 5 -UseBasicParsing -ErrorAction Stop
            } else {
                $response = Invoke-WebRequest -Uri $url -Method GET -TimeoutSec 5 -UseBasicParsing -ErrorAction Stop
            }
            
            $endTime = Get-Date
            $duration = ($endTime - $startTime).TotalMilliseconds
            
            $results.Total++
            if ($response.StatusCode -eq 200 -or $response.StatusCode -eq 404 -or $response.StatusCode -eq 403) {
                $results.Success++
                $results.ResponseTimes += $duration
            } else {
                $results.Failed++
            }
            
            Write-Host "." -NoNewline -ForegroundColor Green
        } catch {
            $endTime = Get-Date
            $duration = ($endTime - $startTime).TotalMilliseconds
            $results.Total++
            $results.Failed++
            Write-Host "X" -NoNewline -ForegroundColor Red
        }
        
        Start-Sleep -Milliseconds 50
    }
    
    Write-Host ""
    
    if ($results.ResponseTimes.Count -gt 0) {
        $avg = ($results.ResponseTimes | Measure-Object -Average).Average
        $min = ($results.ResponseTimes | Measure-Object -Minimum).Minimum
        $max = ($results.ResponseTimes | Measure-Object -Maximum).Maximum
        $sorted = $results.ResponseTimes | Sort-Object
        $p95 = $sorted[[Math]::Floor($sorted.Count * 0.95)]
        $p99 = $sorted[[Math]::Floor($sorted.Count * 0.99)]
        
        $successRate = ($results.Success / $results.Total) * 100
        $errorRate = ($results.Failed / $results.Total) * 100
        
        $resultData = @{
            Run = $run
            TotalRequests = $results.Total
            SuccessfulRequests = $results.Success
            FailedRequests = $results.Failed
            SuccessRate = [Math]::Round($successRate, 2)
            ErrorRate = [Math]::Round($errorRate, 2)
            AverageResponseTime = [Math]::Round($avg, 2)
            MinResponseTime = [Math]::Round($min, 2)
            MaxResponseTime = [Math]::Round($max, 2)
            P95ResponseTime = [Math]::Round($p95, 2)
            P99ResponseTime = [Math]::Round($p99, 2)
            Timestamp = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss")
        }
        
        $allAuthResults += $resultData
        
        Write-Host "  Succes: $($results.Success) | Echecs: $($results.Failed) | Moyenne: $([Math]::Round($avg, 2))ms | P95: $([Math]::Round($p95, 2))ms" -ForegroundColor Cyan
    }
    
    Write-Host ""
    Start-Sleep -Seconds 2
}

# ========================================
# RESUME FINAL
# ========================================
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "RESUME FINAL" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Sauvegarder les resultats
$publicResultsFile = Join-Path $resultsDir "public-tests-results-$(Get-Date -Format 'yyyyMMdd-HHmmss').json"
$authResultsFile = Join-Path $resultsDir "auth-tests-results-$(Get-Date -Format 'yyyyMMdd-HHmmss').json"

$allPublicResults | ConvertTo-Json -Depth 10 | Out-File -FilePath $publicResultsFile -Encoding UTF8
$allAuthResults | ConvertTo-Json -Depth 10 | Out-File -FilePath $authResultsFile -Encoding UTF8

Write-Host "Tests publics:" -ForegroundColor Yellow
Write-Host "  Executions: $($allPublicResults.Count)" -ForegroundColor White
if ($allPublicResults.Count -gt 0) {
    $publicAvg = ($allPublicResults | ForEach-Object { $_.AverageResponseTime } | Measure-Object -Average).Average
    $publicP95 = ($allPublicResults | ForEach-Object { $_.P95ResponseTime } | Measure-Object -Average).Average
    Write-Host "  Temps reponse moyen: $([Math]::Round($publicAvg, 2))ms" -ForegroundColor White
    Write-Host "  P95 moyen: $([Math]::Round($publicP95, 2))ms" -ForegroundColor White
}
Write-Host "  Resultats: $publicResultsFile" -ForegroundColor Cyan
Write-Host ""

Write-Host "Tests avec authentification:" -ForegroundColor Yellow
Write-Host "  Executions: $($allAuthResults.Count)" -ForegroundColor White
if ($allAuthResults.Count -gt 0) {
    $authAvg = ($allAuthResults | ForEach-Object { $_.AverageResponseTime } | Measure-Object -Average).Average
    $authP95 = ($allAuthResults | ForEach-Object { $_.P95ResponseTime } | Measure-Object -Average).Average
    Write-Host "  Temps reponse moyen: $([Math]::Round($authAvg, 2))ms" -ForegroundColor White
    Write-Host "  P95 moyen: $([Math]::Round($authP95, 2))ms" -ForegroundColor White
}
Write-Host "  Resultats: $authResultsFile" -ForegroundColor Cyan
Write-Host ""

Write-Host "Tous les resultats sont sauvegardes dans: $resultsDir" -ForegroundColor Green
Write-Host ""
Write-Host "Tests termines!" -ForegroundColor Green
Write-Host ""

