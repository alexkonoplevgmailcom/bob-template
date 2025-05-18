using System;

namespace Abstractions.Exceptions;

/// <summary>
/// Exception thrown when a resource (account, customer, transaction, etc.) is not found
/// </summary>
public class ResourceNotFoundException : BankingException
{
    public string ResourceType { get; }
    public string ResourceId { get; }

    public ResourceNotFoundException(string resourceType, string resourceId) 
        : base($"{resourceType} with ID {resourceId} not found", "RESOURCE_NOT_FOUND")
    {
        ResourceType = resourceType;
        ResourceId = resourceId;
    }

    public ResourceNotFoundException(string resourceType, int resourceId) 
        : this(resourceType, resourceId.ToString())
    {
    }
}
