using System.Net;

namespace Recipe.Domain.Exceptions;

public sealed class ModifyFailedException(string action, string entityType)
: BaseRecipeException($"Failed to {action} any {entityType}s.", HttpStatusCode.InternalServerError)
{
}