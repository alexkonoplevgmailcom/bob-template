# Transaction API Test Script
# This script runs a series of Invoke-RestMethod commands to test the Transaction API

$apiUrl = "http://localhost:5053"
$transactionEndpoint = "/api/Transactions"
$headers = @{ "Content-Type" = "application/json" }

# Read the account IDs from the file if it exists
if (Test-Path -Path "../test-accounts.txt") {
    $accountIds = Get-Content -Path "../test-accounts.txt"
    $account1Id = $accountIds[0]
    $account2Id = $accountIds[1]
    Write-Host "Using account IDs from file: $account1Id and $account2Id" -ForegroundColor Green
}
else {
    Write-Host "WARNING: test-accounts.txt not found. Please run create-bank-accounts.ps1 first." -ForegroundColor Yellow
    Write-Host "Using default account ID 1 which may not exist." -ForegroundColor Yellow
    $account1Id = 1
    $account2Id = 2
}

Write-Host "=== Starting Transaction API Tests ===" -ForegroundColor Cyan
Write-Host ""

Write-Host "=== Test 1: GET transactions for account ID $account1Id ===" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "${apiUrl}${transactionEndpoint}/account/${account1Id}" -Method Get -Headers $headers
    $response | ConvertTo-Json
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
}
Start-Sleep -Seconds 1

Write-Host "=== Test 2: CREATE a deposit transaction for account ID $account1Id ===" -ForegroundColor Yellow
$depositTransaction = @{
    accountId = $account1Id
    transactionType = 0  # 0 = Deposit in the TransactionType enum
    amount = 250.00
    description = "Test Deposit"
    transactionDate = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ss")  # Current date in ISO format
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "${apiUrl}${transactionEndpoint}" -Method Post -Headers $headers -Body $depositTransaction
    $depositTransactionId = $response.id
    Write-Host "Created deposit transaction with ID: $depositTransactionId" -ForegroundColor Green
    $response | ConvertTo-Json
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
    $depositTransactionId = 1  # Fallback for testing
}
Start-Sleep -Seconds 1

Write-Host "=== Test 3: CREATE a withdrawal transaction for account ID $account1Id ===" -ForegroundColor Yellow
$withdrawalTransaction = @{
    accountId = $account1Id
    transactionType = 1  # 1 = Withdrawal in the TransactionType enum
    amount = 100.00
    description = "Test Withdrawal"
    transactionDate = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ss")
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "${apiUrl}${transactionEndpoint}" -Method Post -Headers $headers -Body $withdrawalTransaction
    $withdrawalTransactionId = $response.id
    Write-Host "Created withdrawal transaction with ID: $withdrawalTransactionId" -ForegroundColor Green
    $response | ConvertTo-Json
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
    $withdrawalTransactionId = 2  # Fallback for testing
}
Start-Sleep -Seconds 1

Write-Host "=== Test 4: CREATE a transfer transaction from account ID $account1Id to account ID $account2Id ===" -ForegroundColor Yellow
$transferTransaction = @{
    accountId = $account1Id
    toAccountId = $account2Id
    transactionType = 2  # 2 = Transfer in the TransactionType enum
    amount = 75.00
    description = "Test Transfer"
    transactionDate = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ss")
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "${apiUrl}${transactionEndpoint}/transfer" -Method Post -Headers $headers -Body $transferTransaction
    $transferTransactionId = $response.id
    Write-Host "Created transfer transaction with ID: $transferTransactionId" -ForegroundColor Green
    $response | ConvertTo-Json
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
    $transferTransactionId = 3  # Fallback for testing
}
Start-Sleep -Seconds 1

Write-Host "=== Test 5: GET all transactions for account ID $account1Id after adding new transactions ===" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "${apiUrl}${transactionEndpoint}/account/${account1Id}" -Method Get -Headers $headers
    $response | ConvertTo-Json
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
}
Start-Sleep -Seconds 1

Write-Host "=== Test 6: GET all transactions for account ID $account2Id (should include the transfer) ===" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "${apiUrl}${transactionEndpoint}/account/${account2Id}" -Method Get -Headers $headers
    $response | ConvertTo-Json
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
}
Start-Sleep -Seconds 1

Write-Host "=== Test 7: GET transaction by ID ($depositTransactionId) ===" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "${apiUrl}${transactionEndpoint}/${depositTransactionId}" -Method Get -Headers $headers
    $response | ConvertTo-Json
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
}

Write-Host ""
Write-Host "=== Transaction API Tests Complete ===" -ForegroundColor Cyan
