# Banking Facility Backend Scripts

This directory contains PowerShell scripts for testing and working with the Banking Facility Backend (BFB) application.

## Available Scripts

### create-bank-accounts.ps1
Creates test bank accounts for use with the other test scripts. This script should be run first to populate the database with test accounts.
- Creates 3 bank accounts with different properties
- Saves account IDs to `../test-accounts.txt` for use by other scripts

### test-api.ps1
Tests the BankAccount API functionality and cache behavior:
- GET, POST, PUT, DELETE operations
- Validates cache hits and misses
- Creates, updates, and deletes test accounts

### test-branch-api.ps1
Tests the BankBranch API functionality:
- GET, POST, PUT, DELETE operations
- Creates new branches
- Updates and deletes branches

### test-transaction-api.ps1
Tests the Transaction API functionality:
- Creates deposit transactions
- Creates withdrawal transactions
- Creates transfer transactions
- Retrieves transaction histories

### PopulateData.ps1
Populates the database with comprehensive sample data for testing:
- Creates bank branches
- Creates accounts
- Creates customer records
- Links customers to accounts
- Creates sample transactions

## Usage

1. Start the BFB application
2. Run scripts from this directory using PowerShell:

```powershell
pwsh ./create-bank-accounts.ps1
pwsh ./test-api.ps1
pwsh ./test-branch-api.ps1
pwsh ./test-transaction-api.ps1
pwsh ./PopulateData.ps1
```

**Note**: These scripts expect the API to be running at http://localhost:5053. If your API is running on a different port, you'll need to update the `$apiUrl` variable in each script.
