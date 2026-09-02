using System.Net;

namespace Recipe.Domain.Exceptions;

public sealed class EntityAlreadyExistsException(string entityType, string fieldName, object fieldValue) :
    BaseRecipeException($"A(n) {entityType} with {fieldName} \"{fieldValue}\" already exists.", HttpStatusCode.Conflict)
{
}