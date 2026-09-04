#Requires -Version 5.1
<#
.SYNOPSIS
  Starts the Card Upgrade API (Sandbox profile) if needed and calls Mastercard sandbox BIN Lookup.
#>
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$apiProject = Join-Path $root "src\MastercardCardUpgrade.Api"
$baseUrl = "http://localhost:5088"

function Test-ApiUp {
    try {
        $null = Invoke-WebRequest -Uri "$baseUrl/api/mastercard/sandbox/status" -UseBasicParsing -TimeoutSec 3
        return $true
    } catch {
        return $false
    }
}

$startedHere = $false
$apiProcess = $null

if (-not (Test-ApiUp)) {
    Write-Host "Starting Mastercard Card Upgrade API (Sandbox) on $baseUrl ..."
    $apiProcess = Start-Process -FilePath "dotnet" `
        -ArgumentList @("run", "--project", $apiProject, "--launch-profile", "Sandbox") `
        -WorkingDirectory $root `
        -PassThru `
        -WindowStyle Hidden
    $startedHere = $true

    $deadline = (Get-Date).AddSeconds(60)
    while (-not (Test-ApiUp)) {
        if ($apiProcess.HasExited) {
            throw "API process exited before becoming ready. Run: dotnet run --project src/MastercardCardUpgrade.Api --launch-profile Sandbox"
        }
        if ((Get-Date) -gt $deadline) {
            throw "API did not become ready at $baseUrl within 60s. Check the Sandbox profile and credentials."
        }
        Start-Sleep -Seconds 1
    }
}

try {
    Write-Host "GET /api/mastercard/sandbox/status"
    $status = Invoke-RestMethod -Uri "$baseUrl/api/mastercard/sandbox/status" -Method GET
    $status | ConvertTo-Json -Depth 6 | Write-Host

    if (-not $status.credentialsConfigured -or -not $status.signingKeyFileFound) {
        Write-Host ""
        Write-Host "Sandbox keys are not configured yet."
        Write-Host "Copy src/MastercardCardUpgrade.Api/appsettings.Local.json.example to appsettings.Local.json and fill ConsumerKey + .p12 path."
        exit 2
    }

    Write-Host ""
    Write-Host "POST /api/mastercard/sandbox/bin-lookup  (Mastercard sandbox)"
    $result = Invoke-RestMethod -Uri "$baseUrl/api/mastercard/sandbox/bin-lookup" -Method POST `
        -ContentType "application/json" `
        -Body '{"panOrAccountRange":"585240844"}'
    $result | ConvertTo-Json -Depth 8 | Write-Host
    Write-Host ""
    Write-Host "Sandbox BIN Lookup succeeded."
}
finally {
    if ($startedHere -and $null -ne $apiProcess -and -not $apiProcess.HasExited) {
        Stop-Process -Id $apiProcess.Id -Force -ErrorAction SilentlyContinue
    }
}
