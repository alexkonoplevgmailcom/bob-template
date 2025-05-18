# BFB Template API Test Script
# This script runs a series of Invoke-RestMethod commands to test the BankAccount API
# and validate the caching strategy

$apiUrl = "http://localhost:5053"
$endpoint = "/api/BankAccounts"  # Updated to match the correct endpoint naming
$headers = @{ "Content-Type" = "application/json" }

Write-Host "=== Starting BankAccount API Tests ===" -ForegroundColor Cyan
Write-Host ""

Write-Host "=== Test 1: GET all bank accounts (first call - should be a cache miss) ===" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "${apiUrl}${endpoint}" -Method Get -Headers $headers
    $response | ConvertTo-Json
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
}
Start-Sleep -Seconds 1

Write-Host "=== Test 2: GET all bank accounts again (should be a cache hit) ===" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "${apiUrl}${endpoint}" -Method Get -Headers $headers
    $response | ConvertTo-Json
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
}
Start-Sleep -Seconds 1

Write-Host "=== Test 3: GET specific bank account with ID 1 (should be a cache miss) ===" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "${apiUrl}${endpoint}/1" -Method Get -Headers $headers
    $response | ConvertTo-Json
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
}
Start-Sleep -Seconds 1

Write-Host "=== Test 4: GET same bank account again (should be a cache hit) ===" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "${apiUrl}${endpoint}/1" -Method Get -Headers $headers
    $response | ConvertTo-Json
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
}
Start-Sleep -Seconds 1

Write-Host "=== Test 5: CREATE a new bank account (should update the cache) ===" -ForegroundColor Yellow
$body = @{
    accountNumber = "TEST-999"
    ownerName = "Test User 999"
    balance = 9999.00
    type = 0  # Checking
    createdDate = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ss")  # Current date in ISO format
    isActive = $true
    bankId = 1
    branchId = 1
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "${apiUrl}${endpoint}" -Method Post -Headers $headers -Body $body
    $newAccountId = $response.id
    Write-Host "Created bank account with ID: $newAccountId" -ForegroundColor Green
    $response | ConvertTo-Json
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
    $newAccountId = 99  # Fallback for testing
}
Start-Sleep -Seconds 1

Write-Host "=== Test 6: GET all bank accounts again (should be a cache miss because we created a new account) ===" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "${apiUrl}${endpoint}" -Method Get -Headers $headers
    $response | ConvertTo-Json
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
}
Start-Sleep -Seconds 1

Write-Host "=== Test 7: UPDATE the bank account we just created (should update the cache) ===" -ForegroundColor Yellow
$updateBody = @{
    id = $newAccountId
    accountNumber = "TEST-999"
    ownerName = "UPDATED Test User 999"  # Changed the name
    balance = 8888.00  # Changed the balance
    type = 0
    createdDate = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ss")
    isActive = $true
    bankId = 1
    branchId = 1
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "${apiUrl}${endpoint}/${newAccountId}" -Method Put -Headers $headers -Body $updateBody
    Write-Host "Updated bank account with ID: $newAccountId" -ForegroundColor Green
    $response | ConvertTo-Json
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
}
Start-Sleep -Seconds 1

Write-Host "=== Test 8: GET the updated bank account (should be a cache miss because we updated it) ===" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "${apiUrl}${endpoint}/${newAccountId}" -Method Get -Headers $headers
    $response | ConvertTo-Json
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
}
Start-Sleep -Seconds 1

Write-Host "=== Test 9: DELETE the bank account we created (should update the cache) ===" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "${apiUrl}${endpoint}/${newAccountId}" -Method Delete
    Write-Host "Deleted bank account with ID: $newAccountId" -ForegroundColor Green
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
}
Start-Sleep -Seconds 1

Write-Host "=== Test 10: GET all bank accounts after delete (should be a cache miss) ===" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "${apiUrl}${endpoint}" -Method Get -Headers $headers
    $response | ConvertTo-Json
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
}

Write-Host ""
Write-Host "=== BankAccount API Tests Complete ===" -ForegroundColor Cyan
