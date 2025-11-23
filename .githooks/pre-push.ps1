# Pre-push hook for RazorConsole (PowerShell version)
# Runs tests before pushing to remote

$ErrorActionPreference = "Stop"

Write-Host "🔍 Running pre-push checks..." -ForegroundColor Cyan

# Change to repository root
$repoRoot = git rev-parse --show-toplevel
Set-Location $repoRoot

# Run tests
Write-Host "🧪 Running tests..." -ForegroundColor Cyan
$testResult = dotnet test RazorConsole.slnx --no-build --nologo --verbosity minimal 2>&1
$exitCode = $LASTEXITCODE

if ($exitCode -eq 0) {
    Write-Host "✅ All tests passed" -ForegroundColor Green
    exit 0
}
else {
    Write-Host "❌ Tests failed" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please fix failing tests before pushing." -ForegroundColor Yellow
    Write-Host ""
    exit 1
}
