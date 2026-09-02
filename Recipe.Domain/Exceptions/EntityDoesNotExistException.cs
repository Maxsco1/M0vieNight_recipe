using System.Net;

namespace Recipe.Domain.Exceptions;

public sealed class EntityDoesNotExistException(string entityType, string fieldName, object fieldValue) : BaseRecipeException($"No {entityType} with {fieldName} \"{fieldValue}\" exists.", HttpStatusCode.NotFound)
{
}