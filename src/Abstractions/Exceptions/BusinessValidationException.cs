using System;

namespace Abstractions.Exceptions;

/// <summary>
/// Exception thrown when business validation rules are violated
/// </summary>
public class BusinessValidationException : BankingException
{
    public BusinessValidationException(string message) 
        : base(message, "BUSINESS_VALIDATION_ERROR")
    {
    }

    public BusinessValidationException(string message, Exception innerException) 
        : base(message, "BUSINESS_VALIDATION_ERROR", innerException)
    {
    }
}
