using System;

namespace Abstractions.Exceptions;

/// <summary>
/// Exception thrown when there's an issue with external data access services
/// </summary>
public class DataAccessException : BankingException
{
    public DataAccessException(string message) 
        : base(message, "DATA_ACCESS_ERROR")
    {
    }

    public DataAccessException(string message, Exception innerException) 
        : base(message, "DATA_ACCESS_ERROR", innerException)
    {
    }
}
