#Requires -Version 5.1
<#
.SYNOPSIS
  HTTP end-to-end tests for the Mastercard card-upgrade API.

.PARAMETER AgainstRunningApi
  Hit a process already listening (default http://localhost:5088) instead of hosting in-process.

.PARAMETER LiveAcs
  Also attempt Product Graduation writes against Mastercard ACS when liveAcsReady is true.

.EXAMPLE
  .\run-e2e.ps1

.EXAMPLE
  .\run-e2e.ps1 -AgainstRunningApi

.EXAMPLE
  $env:CARD_UPGRADE_E2E_BASE_URL = "http://localhost:5088"
  .\run-e2e.ps1
#>
param(
    [switch] $AgainstRunningApi,
    [string] $BaseUrl = "http://localhost:5088",
    [switch] $LiveAcs
)

$ErrorActionPreference = "Stop"
$here = $PSScriptRoot
$testProject = Join-Path $here "MastercardCardUpgrade.E2ETests.csproj"

if ($AgainstRunningApi) {
    $env:CARD_UPGRADE_E2E_BASE_URL = $BaseUrl
    Write-Host "E2E against running API $BaseUrl"
} else {
    Remove-Item Env:CARD_UPGRADE_E2E_BASE_URL -ErrorAction SilentlyContinue
    Write-Host "E2E in-process (WebApplicationFactory, Sandbox environment)"
}

if ($LiveAcs) {
    $env:CARD_UPGRADE_E2E_LIVE_ACS = "1"
    Write-Host "Live ACS writes enabled (CARD_UPGRADE_E2E_LIVE_ACS=1)"
} else {
    Remove-Item Env:CARD_UPGRADE_E2E_LIVE_ACS -ErrorAction SilentlyContinue
}

dotnet test $testProject --nologo
exit $LASTEXITCODE
