# PowerShell script to create a GitHub release
# Requires: GitHub personal access token with repo permissions
# Usage: .\scripts\create-release.ps1 -Version "v1.7.0" -Token "your_token_here"

param(
    [Parameter(Mandatory=$true)]
    [string]$Version,
    
    [Parameter(Mandatory=$false)]
    [string]$Token = $env:GITHUB_TOKEN,
    
    [Parameter(Mandatory=$false)]
    [string]$Repo = "CrudePixels/CastleStory-Modding-Tool"
)

if (-not $Token) {
    Write-Host "Error: GitHub token required. Set GITHUB_TOKEN environment variable or pass -Token parameter" -ForegroundColor Red
    exit 1
}

$releaseNotes = Get-Content "RELEASE_NOTES.md" -Raw
$releaseNotes = $releaseNotes -replace '"', '\"'
$releaseNotes = $releaseNotes -replace "`n", "\n"
$releaseNotes = $releaseNotes -replace "`r", ""

$body = @{
    tag_name = $Version
    name = "$Version - Jason's Enhancements Mod"
    body = $releaseNotes
    draft = $false
    prerelease = $false
} | ConvertTo-Json

$headers = @{
    "Authorization" = "token $Token"
    "Accept" = "application/vnd.github.v3+json"
}

$uri = "https://api.github.com/repos/$Repo/releases"

Write-Host "Creating release $Version..." -ForegroundColor Green
try {
    $response = Invoke-RestMethod -Uri $uri -Method Post -Headers $headers -Body $body -ContentType "application/json"
    Write-Host "Release created successfully!" -ForegroundColor Green
    Write-Host "Release URL: $($response.html_url)" -ForegroundColor Cyan
    return $response
} catch {
    Write-Host "Error creating release: $_" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response: $responseBody" -ForegroundColor Yellow
    }
    exit 1
}
