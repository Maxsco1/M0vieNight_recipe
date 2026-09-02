namespace Recipe.Domain.Exceptions;

using System.Net;

public sealed class ValidationException(IDictionary<string, string[]> errors)
: BaseRecipeException("One or more validation errors occurred.", HttpStatusCode.BadRequest)
{
    public IDictionary<string, string[]> Errors { get; } = errors;
}