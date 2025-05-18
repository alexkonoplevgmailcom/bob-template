using System;

namespace Abstractions.Exceptions;

/// <summary>
/// Base exception for all banking application-specific exceptions
/// </summary>
public abstract class BankingException : Exception
{
    /// <summary>
    /// Error code for application-specific categorization
    /// </summary>
    public string ErrorCode { get; }

    protected BankingException(string message, string errorCode) : base(message)
    {
        ErrorCode = errorCode;
    }

    protected BankingException(string message, string errorCode, Exception innerException) 
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}
