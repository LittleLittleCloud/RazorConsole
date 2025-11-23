# Pre-commit hook for RazorConsole (PowerShell version)
# Runs dotnet format to verify code formatting before commit

$ErrorActionPreference = "Stop"

Write-Host "🔍 Running pre-commit checks..." -ForegroundColor Cyan

# Change to repository root
$repoRoot = git rev-parse --show-toplevel
Set-Location $repoRoot

# Run dotnet format with verification
Write-Host "📝 Checking code formatting..." -ForegroundColor Cyan
$formatResult = dotnet format RazorConsole.slnx --verify-no-changes --verbosity quiet 2>&1
$exitCode = $LASTEXITCODE

if ($exitCode -eq 0) {
    Write-Host "✅ Code formatting check passed" -ForegroundColor Green
    exit 0
}
else {
    Write-Host "❌ Code formatting check failed" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please run the following command to fix formatting issues:" -ForegroundColor Yellow
    Write-Host "  dotnet format RazorConsole.slnx" -ForegroundColor Yellow
    Write-Host ""
    exit 1
}
