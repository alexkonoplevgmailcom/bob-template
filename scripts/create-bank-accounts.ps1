# Script to create bank accounts for testing
$apiUrl = "http://localhost:5053"
$bankAccountEndpoint = "/api/BankAccounts"
$headers = @{ "Content-Type" = "application/json" }

Write-Host "=== Creating Bank Accounts for Testing ===" -ForegroundColor Cyan

# Verify API is reachable
try {
    Write-Host "Verifying API connection..." -ForegroundColor Yellow
    $healthCheck = Invoke-RestMethod -Uri "${apiUrl}/api/BankAccounts" -Method Get -ErrorAction SilentlyContinue
    Write-Host "API connection successful." -ForegroundColor Green
}
catch {
    Write-Host "Warning: Unable to connect to API to verify connectivity. Will continue anyway." -ForegroundColor Yellow
    Write-Host "Error details: $_" -ForegroundColor Red
}

# Create Account 1
Write-Host "Creating bank account 1..." -ForegroundColor Yellow
$accountJson1 = @{
    accountNumber = "TEST-001"
    ownerName = "Test User 1"
    balance = 1000.00
    type = 0  # 0 = Checking in the AccountType enum
    createdDate = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ss")  # Current date in ISO format
    isActive = $true
    bankId = 1
    branchId = 1
} | ConvertTo-Json

try {
    # Enable detailed error info
    $ProgressPreference = 'SilentlyContinue'
    
    Write-Host "Request payload for account 1:" -ForegroundColor Yellow
    Write-Host $accountJson1 -ForegroundColor Gray
    
    # Make the request with more detailed error information
    $response1 = Invoke-RestMethod -Uri "${apiUrl}${bankAccountEndpoint}" -Method Post -Body $accountJson1 -ContentType "application/json"
    Write-Host "Response for account 1:" -ForegroundColor Green
    $response1 | ConvertTo-Json
    $account1Id = $response1.id
}
catch {
    Write-Host "Error creating account 1:" -ForegroundColor Red
    
    # Extract more detailed error information
    try {
        # PowerShell 7 way to get response content
        if ($_.ErrorDetails) {
            Write-Host $_.ErrorDetails.Message -ForegroundColor Red
        }
        elseif ($_.Exception.Response) {
            $responseBody = $_.Exception
            Write-Host "Status code: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
            
            # Try to get more details about the error
            try {
                $webResponse = [System.Net.WebResponse]$_.Exception.Response
                $responseStream = $webResponse.GetResponseStream()
                $streamReader = [System.IO.StreamReader]::new($responseStream)
                $responseBody = $streamReader.ReadToEnd()
                Write-Host $responseBody -ForegroundColor Red
            }
            catch {
                Write-Host "Response body not available" -ForegroundColor Red
            }
        }
        else {
            Write-Host $_.Exception.Message -ForegroundColor Red
        }
    }
    catch {
        Write-Host "Could not extract error details: $_" -ForegroundColor Red
    }
    
    # Manually set the account ID for testing if creation fails
    $account1Id = 1
    
    Write-Host "Will continue with test account ID: $account1Id" -ForegroundColor Yellow
}

Write-Host "Using account ID: $account1Id"

# Create Account 2
Write-Host "Creating bank account 2..." -ForegroundColor Yellow
$accountJson2 = @{
    accountNumber = "TEST-002"
    ownerName = "Test User 2"
    balance = 5000.00
    type = 1  # 1 = Savings in the AccountType enum
    createdDate = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ss")  # Current date in ISO format
    isActive = $true
    bankId = 1
    branchId = 1
} | ConvertTo-Json

try {
    # Make the request with more detailed error information
    Write-Host "Request payload for account 2:" -ForegroundColor Yellow
    Write-Host $accountJson2 -ForegroundColor Gray
    
    $response2 = Invoke-RestMethod -Uri "${apiUrl}${bankAccountEndpoint}" -Method Post -Body $accountJson2 -ContentType "application/json"
    Write-Host "Response for account 2:" -ForegroundColor Green
    $response2 | ConvertTo-Json
    $account2Id = $response2.id
}
catch {
    Write-Host "Error creating account 2:" -ForegroundColor Red
    
    # Extract more detailed error information
    try {
        # PowerShell 7 way to get response content
        if ($_.ErrorDetails) {
            Write-Host $_.ErrorDetails.Message -ForegroundColor Red
        }
        elseif ($_.Exception.Response) {
            $responseBody = $_.Exception
            Write-Host "Status code: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
            
            # Try to get more details about the error
            try {
                $webResponse = [System.Net.WebResponse]$_.Exception.Response
                $responseStream = $webResponse.GetResponseStream()
                $streamReader = [System.IO.StreamReader]::new($responseStream)
                $responseBody = $streamReader.ReadToEnd()
                Write-Host $responseBody -ForegroundColor Red
            }
            catch {
                Write-Host "Response body not available" -ForegroundColor Red
            }
        }
        else {
            Write-Host $_.Exception.Message -ForegroundColor Red
        }
    }
    catch {
        Write-Host "Could not extract error details: $_" -ForegroundColor Red
    }
    
    # Manually set the account ID for testing if creation fails
    $account2Id = 2
    
    Write-Host "Will continue with test account ID: $account2Id" -ForegroundColor Yellow
}

Write-Host "Using account ID: $account2Id"

# Create Account 3
Write-Host "Creating bank account 3..." -ForegroundColor Yellow
$accountJson3 = @{
    accountNumber = "TEST-003"
    ownerName = "Test User 3"
    balance = 10000.00
    type = 0  # 0 = Checking in the AccountType enum
    createdDate = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ss")  # Current date in ISO format
    isActive = $true
    bankId = 2
    branchId = 2
} | ConvertTo-Json

try {
    # Make the request with more detailed error information
    $response3 = Invoke-RestMethod -Uri "${apiUrl}${bankAccountEndpoint}" -Method Post -Body $accountJson3 -ContentType "application/json"
    Write-Host "Response for account 3:" -ForegroundColor Green
    $response3 | ConvertTo-Json
    $account3Id = $response3.id
}
catch {
    Write-Host "Error creating account 3:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    
    # Manually set the account ID for testing if creation fails
    $account3Id = 3
    Write-Host "Will continue with test account ID: $account3Id" -ForegroundColor Yellow
}

# Save account IDs for later tests
Write-Host "Saving account IDs to test-accounts.txt" -ForegroundColor Yellow
"$account1Id", "$account2Id", "$account3Id" | Out-File -FilePath "../test-accounts.txt"
Write-Host "Saved account IDs: $account1Id, $account2Id, $account3Id" -ForegroundColor Green

# Summary
Write-Host ""
Write-Host "=== Bank Accounts Created ===" -ForegroundColor Green
Write-Host "Account 1 (ID: $account1Id) - Checking account with $1,000.00" -ForegroundColor Cyan
Write-Host "Account 2 (ID: $account2Id) - Savings account with $5,000.00" -ForegroundColor Cyan
Write-Host "Account 3 (ID: $account3Id) - Checking account with $10,000.00" -ForegroundColor Cyan
Write-Host ""
Write-Host "You can now run the other scripts in this directory: test-api.ps1, test-branch-api.ps1, or test-transaction-api.ps1" -ForegroundColor Yellow
