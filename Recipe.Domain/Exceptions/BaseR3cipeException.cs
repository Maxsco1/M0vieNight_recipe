using System.Net;

namespace Recipe.Domain.Exceptions;

public abstract class BaseRecipeException(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError) : Exception(message)
{
    public HttpStatusCode Status { get; set; } = statusCode;
}