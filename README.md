# Banking Facility Backend (BFB)

A robust banking facility backend system that provides RESTful APIs for managing bank accounts, branches, customers, and their accounts. The system is built using ASP.NET Core and supports multiple database systems including Microsoft SQL Server, MongoDB, IBM DB2, and a REST API for transactions.

## Features

- Multi-database architecture supporting:
  - Microsoft SQL Server for bank account management
  - MongoDB for bank branch management
  - IBM DB2 for customer data management
  - REST API for transaction management
- Comprehensive REST API endpoints
- Retry policies for improved reliability
- Swagger/OpenAPI documentation
- Global exception handling
- Model validation

## Prerequisites

- .NET 8.0 SDK or later
- Docker and Docker Compose
- One of the following database systems:
  - Microsoft SQL Server 2019 or later
  - MongoDB 4.4 or later
  - IBM DB2 11.5.7.0 or later

## Installation

1. Clone the repository:
```bash
git clone https://github.com/yourusername/bfb.git
cd bfb
```

2. Start the required databases using Docker Compose:
```bash
docker-compose up -d
```

3. Update the connection strings in `appsettings.json` if needed:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=BFBTemplateDB;Trusted_Connection=True;TrustServerCertificate=True;",
    "MongoConnection": "mongodb://localhost:27017/BankDatabase?authSource=admin"
  }
}
```

4. Build and run the application:
```bash
dotnet restore
dotnet build
dotnet run --project src/BFB.Template.Api
```

## Configuration

The application can be configured through the following files:

- `appsettings.json`: Main configuration file
- `appsettings.Development.json`: Development-specific settings
- `docker-compose.yml`: Database container configurations

Key configuration sections:

```json
{
  "RetryPolicy": {
    "MaxRetryAttempts": 3,
    "RetryTimeoutInSeconds": 30,
    "RetryDelayInMilliseconds": 500
  },
  "MongoDB": {
    "DatabaseName": "BankDatabase",
    "UseAuthentication": false
  }
}
```

## Testing Scripts

The project includes several PowerShell scripts in the `scripts` folder to test and interact with the API:

- `create-bank-accounts.ps1`: Creates test bank accounts
- `test-api.ps1`: Tests the BankAccount API functionality and cache behavior
- `test-branch-api.ps1`: Tests the BankBranch API functionality
- `test-transaction-api.ps1`: Tests the Transaction API functionality
- `PopulateData.ps1`: Populates the database with comprehensive sample data

To run the scripts (requires PowerShell):

```bash
cd scripts
pwsh ./create-bank-accounts.ps1
```

For more details, see the [Scripts README](scripts/README.md).

## Project Structure

The solution is organized into several projects:

- **Abstractions**: Contains interfaces, DTOs, and exceptions shared across the solution
- **BFB.BusinessServices**: Business logic implementation
- **BFB.DataAccess.MSSQL**: Data access for bank accounts using SQL Server
- **BFB.DataAccess.Mongo**: Data access for bank branches using MongoDB
- **BFB.DataAccess.DB2**: Data access for customer data using IBM DB2
- **BFB.DataAccess.RestApi**: Data access for transactions using REST API
- **BFB.Template.Api**: Main API project with controllers and middleware

## API Endpoints

### Bank Account Endpoints

| Method | Endpoint | Description | Request/Response Details |
|--------|----------|-------------|------------------------|
| GET | `/api/bankaccounts` | Retrieves all bank accounts. Returns 200 OK on success. | **Response:** Array of bank account objects:<br>```json<br>[<br>  {<br>    "id": 1,<br>    "accountNumber": "1234567890",<br>    "ownerName": "John Doe",<br>    "balance": 1000.00,<br>    "type": 0,<br>    "createdDate": "2023-01-01T00:00:00Z",<br>    "isActive": true,<br>    "bankId": 1,<br>    "branchId": 1<br>  }<br>]<br>``` |
| GET | `/api/bankaccounts/{id}` | Retrieves a specific bank account by ID. Returns 200 OK on success, 404 Not Found if account doesn't exist. | **Response:** Single bank account object |
| POST | `/api/bankaccounts` | Creates a new bank account. Returns 201 Created on success, 400 Bad Request if validation fails. | **Request:**<br>```json<br>{<br>  "accountNumber": "1234567890",<br>  "ownerName": "John Doe",<br>  "balance": 1000.00,<br>  "type": 0,<br>  "bankId": 1,<br>  "branchId": 1<br>}<br>``` |
| PUT | `/api/bankaccounts/{id}` | Updates an existing bank account. Returns 200 OK on success, 404 Not Found if account doesn't exist. | **Request:** Same as POST request format |
| DELETE | `/api/bankaccounts/{id}` | Deletes a bank account. Returns 200 OK on success, 404 Not Found if account doesn't exist. | No request body required |

### Bank Branch Endpoints

| Method | Endpoint | Description | Request/Response Details |
|--------|----------|-------------|------------------------|
| GET | `/api/bankbranches` | Retrieves all bank branches. Returns 200 OK on success. | **Response:** Array of bank branch objects |
| GET | `/api/bankbranches/{id}` | Retrieves a specific bank branch by ID. Returns 200 OK on success, 404 Not Found if branch doesn't exist. | **Response:** Single bank branch object |
| GET | `/api/bankbranches/bank/{bankId}` | Retrieves all branches for a specific bank. Returns 200 OK on success. | **Response:** Array of bank branch objects |
| POST | `/api/bankbranches` | Creates a new bank branch. Returns 201 Created on success, 400 Bad Request if validation fails. | **Request:**<br>```json<br>{<br>  "bankId": 1,<br>  "branchName": "Downtown Branch",<br>  "address": "123 Main St",<br>  "city": "New York",<br>  "state": "NY",<br>  "zipCode": "10001",<br>  "phoneNumber": "555-0123"<br>}<br>``` |
| PUT | `/api/bankbranches/{id}` | Updates an existing bank branch. Returns 200 OK on success, 404 Not Found if branch doesn't exist. | **Request:** Same as POST request format |
| DELETE | `/api/bankbranches/{id}` | Deletes a bank branch. Returns 200 OK on success, 404 Not Found if branch doesn't exist. | No request body required |

### Customer Endpoints

| Method | Endpoint | Description | Request/Response Details |
|--------|----------|-------------|------------------------|
| GET | `/api/customers` | Retrieves all customers. Returns 200 OK on success. | **Response:** Array of customer objects |
| GET | `/api/customers/{id}` | Retrieves a specific customer by ID. Returns 200 OK on success, 404 Not Found if customer doesn't exist. | **Response:** Single customer object |
| POST | `/api/customers` | Creates a new customer. Returns 201 Created on success, 400 Bad Request if validation fails. | **Request:**<br>```json<br>{<br>  "firstName": "Jane",<br>  "lastName": "Smith",<br>  "email": "jane.smith@example.com",<br>  "phoneNumber": "555-0123",<br>  "address": "456 Oak St",<br>  "city": "Los Angeles",<br>  "state": "CA",<br>  "zipCode": "90001"<br>}<br>``` |
| PUT | `/api/customers/{id}` | Updates an existing customer. Returns 200 OK on success, 404 Not Found if customer doesn't exist. | **Request:** Same as POST request format |
| DELETE | `/api/customers/{id}` | Deletes a customer. Returns 200 OK on success, 404 Not Found if customer doesn't exist. | No request body required |

### Customer Account Endpoints

| Method | Endpoint | Description | Request/Response Details |
|--------|----------|-------------|------------------------|
| GET | `/api/customeraccounts` | Retrieves all customer accounts. Returns 200 OK on success. | **Response:** Array of customer account objects |
| GET | `/api/customeraccounts/{id}` | Retrieves a specific customer account by ID. Returns 200 OK on success, 404 Not Found if account doesn't exist. | **Response:** Single customer account object |
| GET | `/api/customeraccounts/customer/{customerId}` | Retrieves all accounts for a specific customer. Returns 200 OK on success. | **Response:** Array of customer account objects |
| POST | `/api/customeraccounts` | Creates a new customer account. Returns 201 Created on success, 400 Bad Request if validation fails. | **Request:**<br>```json<br>{<br>  "customerId": 1,<br>  "accountNumber": "9876543210",<br>  "balance": 500.00,<br>  "type": 1<br>}<br>``` |
| PUT | `/api/customeraccounts/{id}` | Updates an existing customer account. Returns 200 OK on success, 404 Not Found if account doesn't exist. | **Request:** Same as POST request format |
| DELETE | `/api/customeraccounts/{id}` | Deletes a customer account. Returns 200 OK on success, 404 Not Found if account doesn't exist. | No request body required |

### Transaction Endpoints

| Method | Endpoint | Description | Request/Response Details |
|--------|----------|-------------|------------------------|
| GET | `/api/transactions` | Retrieves all transactions. Returns 200 OK on success. | **Response:** Array of transaction objects |
| GET | `/api/transactions/{id}` | Retrieves a specific transaction by ID. Returns 200 OK on success, 404 Not Found if transaction doesn't exist. | **Response:** Single transaction object |
| GET | `/api/transactions/account/{accountId}` | Retrieves all transactions for a specific account. Returns 200 OK on success. | **Response:** Array of transaction objects |
| POST | `/api/transactions` | Creates a new transaction. Returns 201 Created on success, 400 Bad Request if validation fails. | **Request:**<br>```json<br>{<br>  "accountId": 1,<br>  "amount": 100.00,<br>  "type": "Deposit",<br>  "description": "ATM Deposit"<br>}<br>``` |

### Common HTTP Status Codes

- 200 OK: Successful GET, PUT, DELETE operations
- 201 Created: Successful POST operations
- 400 Bad Request: Invalid input
- 404 Not Found: Resource not found
- 500 Internal Server Error: Server-side errors

For detailed request/response schemas, please refer to the Swagger documentation at `/swagger` when running the API locally.

## Development

### Running Tests

```bash
dotnet test
```

### Populating Sample Data

Use the provided PowerShell script to populate sample data:

```powershell
.\scripts\PopulateData.ps1
```

You can also use the provided shell scripts for testing the API:
```bash
./test-api.sh
./test-branch-api.sh
./test-transaction-api.sh
./create-bank-accounts.sh
```

## License

This project is licensed under the MIT License - see the LICENSE file for details.