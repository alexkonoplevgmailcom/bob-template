# BFB Template

A layered architecture template for building scalable .NET applications.

## Architecture

This project follows a clean layered architecture pattern:

- **Abstractions**: Contains interfaces, DTOs, and exceptions that are shared across layers
- **BFB.DataAccess.MSSQL**: Data access layer with Entity Framework Core and repositories
- **BFB.BusinessServices**: Business logic layer
- **BFB.Template.Api**: ASP.NET Core API with controllers

## Features

- **Layered Architecture**: Separation of concerns with clear layers
- **Dependency Injection**: All layers are loosely coupled
- **Caching**: Data access layer uses IMemoryCache for improved performance
- **Retry Policies**: Using Polly for resilient data access with configurable retry settings
- **Entity Framework Core**: For data access with in-memory database for development

## Getting Started

1. Clone the repository
2. Open the solution in Visual Studio or your preferred IDE
3. Run the API project

## API Endpoints

The API provides the following endpoints:

- `GET /api/BankAccount`: Get all bank accounts
- `GET /api/BankAccount/{id}`: Get a specific bank account
- `POST /api/BankAccount`: Create a new bank account
- `PUT /api/BankAccount/{id}`: Update an existing bank account
- `DELETE /api/BankAccount/{id}`: Delete a bank account