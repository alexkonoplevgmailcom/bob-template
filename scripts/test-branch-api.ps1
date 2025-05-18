# Test script for adding more bank branches using the BankBranchController
# This script will add new branches and verify they were added correctly

$apiUrl = "http://localhost:5053"
$endpoint = "/api/BankBranches"  # Updated to match the correct endpoint naming
$headers = @{ "Content-Type" = "application/json" }

Write-Host "=== Starting BankBranch API Tests ===" -ForegroundColor Cyan
Write-Host ""

Write-Host "=== Test 1: GET all bank branches (initial state) ===" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "${apiUrl}${endpoint}" -Method Get -Headers $headers
    $response | ConvertTo-Json
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
}
Start-Sleep -Seconds 1

Write-Host "=== Test 2: CREATE a new bank branch for Bank 1 ===" -ForegroundColor Yellow
$newBranch1 = @{
    bankId = 1
    branchName = "Financial District Branch"
    address = "555 Wall Street"
    city = "New York"
    state = "NY"
    zipCode = "10005"
    phoneNumber = "212-555-4321"
    isActive = $true
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "${apiUrl}${endpoint}" -Method Post -Headers $headers -Body $newBranch1
    $newBranchId1 = $response.id
    Write-Host "Created bank branch with ID: $newBranchId1" -ForegroundColor Green
    $response | ConvertTo-Json
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
    $newBranchId1 = 99  # Fallback for testing
}
Start-Sleep -Seconds 1

Write-Host "=== Test 3: CREATE a new bank branch for Bank 2 ===" -ForegroundColor Yellow
$newBranch2 = @{
    bankId = 2
    branchName = "Tech Center Branch"
    address = "123 Innovation Way"
    city = "San Francisco"
    state = "CA"
    zipCode = "94107"
    phoneNumber = "415-555-9876"
    isActive = $true
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "${apiUrl}${endpoint}" -Method Post -Headers $headers -Body $newBranch2
    $newBranchId2 = $response.id
    Write-Host "Created bank branch with ID: $newBranchId2" -ForegroundColor Green
    $response | ConvertTo-Json
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
    $newBranchId2 = 100  # Fallback for testing
}
Start-Sleep -Seconds 1

Write-Host "=== Test 4: GET all bank branches after adding new ones ===" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "${apiUrl}${endpoint}" -Method Get -Headers $headers
    $response | ConvertTo-Json
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
}
Start-Sleep -Seconds 1

Write-Host "=== Test 5: UPDATE the first branch we created ===" -ForegroundColor Yellow
$updateBranch1 = @{
    id = $newBranchId1
    bankId = 1
    branchName = "UPDATED Financial District Branch"  # Changed the name
    address = "555 Wall Street"
    city = "New York"
    state = "NY"
    zipCode = "10005"
    phoneNumber = "212-555-9999"  # Changed the phone number
    isActive = $true
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "${apiUrl}${endpoint}/${newBranchId1}" -Method Put -Headers $headers -Body $updateBranch1
    Write-Host "Updated bank branch with ID: $newBranchId1" -ForegroundColor Green
    $response | ConvertTo-Json
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
}
Start-Sleep -Seconds 1

Write-Host "=== Test 6: GET the updated branch ===" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "${apiUrl}${endpoint}/${newBranchId1}" -Method Get -Headers $headers
    $response | ConvertTo-Json
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
}
Start-Sleep -Seconds 1

Write-Host "=== Test 7: DELETE the second branch we created ===" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "${apiUrl}${endpoint}/${newBranchId2}" -Method Delete
    Write-Host "Deleted bank branch with ID: $newBranchId2" -ForegroundColor Green
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
}
Start-Sleep -Seconds 1

Write-Host "=== Test 8: GET all bank branches after deleting one ===" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "${apiUrl}${endpoint}" -Method Get -Headers $headers
    $response | ConvertTo-Json
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
}

Write-Host ""
Write-Host "=== BankBranch API Tests Complete ===" -ForegroundColor Cyan
