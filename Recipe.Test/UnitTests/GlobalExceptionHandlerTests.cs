using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Recipe.Api;
using Recipe.Domain.Exceptions;

namespace Recipe.Test.UnitTests;

public sealed class GlobalExceptionHandlerTests()
{
    [Test]
    public async Task TryHandleAsyncArgumentNullExceptionMapsTo400DevWritesProblemDetails()
    {
        var loggerMock = new Mock<ILogger<GlobalExceptionHandler>>();

        ProblemDetailsContext? captured = null;
        var problemDetailsServiceMock = new Mock<IProblemDetailsService>();
        problemDetailsServiceMock
            .Setup(s => s.TryWriteAsync(It.IsAny<ProblemDetailsContext>()))
            .Callback<ProblemDetailsContext>(ctx => captured = ctx)
            .ReturnsAsync(true);

        var services = new ServiceCollection();
        services.AddSingleton<IHostEnvironment>(new FakeHostEnvironment(environmentName: "Development"));
        var serviceProvider = services.BuildServiceProvider();

        var httpContext = new DefaultHttpContext
        {
            Request =
            {
                Path = "/test/path"
            },
            TraceIdentifier = "trace-123",
            RequestServices = serviceProvider
        };

        var handler = new GlobalExceptionHandler(loggerMock.Object, problemDetailsServiceMock.Object);
        var ex = new ArgumentNullException("x");
        var result = await handler.TryHandleAsync(httpContext, ex, CancellationToken.None);

        Assert.True(result);
        Assert.AreEqual(StatusCodes.Status400BadRequest, httpContext.Response.StatusCode);

        problemDetailsServiceMock.Verify(
            s => s.TryWriteAsync(It.IsAny<ProblemDetailsContext>()),
            Times.Once);

        Assert.NotNull(captured);
        var pd = captured!.ProblemDetails;

        Assert.AreEqual(StatusCodes.Status400BadRequest, pd.Status);
        Assert.AreEqual("Invalid argument provided", pd.Title);
        Assert.AreEqual("https://tools.ietf.org/html/rfc9110#section-15.5.1", pd.Type);
        Assert.AreEqual("/test/path", pd.Instance);

        Assert.AreEqual(ex.Message, pd.Detail);

        Assert.True(pd.Extensions.ContainsKey("traceId"));
        Assert.AreEqual("trace-123", pd.Extensions["traceId"]);

        Assert.True(pd.Extensions.ContainsKey("timestamp"));
        Assert.NotNull(pd.Extensions["timestamp"]);
    }

    [Test]
    public async Task TryHandleAsyncArgumentExceptionProdUsesNullDetail()
    {
        var loggerMock = new Mock<ILogger<GlobalExceptionHandler>>();

        ProblemDetailsContext? captured = null;
        var problemDetailsServiceMock = new Mock<IProblemDetailsService>();
        problemDetailsServiceMock
            .Setup(s => s.TryWriteAsync(It.IsAny<ProblemDetailsContext>()))
            .Callback<ProblemDetailsContext>(ctx => captured = ctx)
            .ReturnsAsync(true);

        var services = new ServiceCollection();
        services.AddSingleton<IHostEnvironment>(new FakeHostEnvironment(environmentName: "Production"));
        var httpContext = new DefaultHttpContext
        {
            TraceIdentifier = "trace-xyz",
            Request = { Path = "/prod/test" },
            RequestServices = services.BuildServiceProvider()
        };

        var handler = new GlobalExceptionHandler(loggerMock.Object, problemDetailsServiceMock.Object);
        var ex = new ArgumentException("some arg error");

        await handler.TryHandleAsync(httpContext, ex);

        Assert.NotNull(captured);
        Assert.AreEqual("Invalid argument provided", captured!.ProblemDetails.Title);
        Assert.Null(captured!.ProblemDetails.Detail);
    }

    [Test]
    public async Task TryHandleAsyncUnauthorizedAccessExceptionProdMapsTo401AndNullDetail()
    {
        var loggerMock = new Mock<ILogger<GlobalExceptionHandler>>();
        ProblemDetailsContext? captured = null;

        var problemDetailsServiceMock = new Mock<IProblemDetailsService>();
        problemDetailsServiceMock
            .Setup(s => s.TryWriteAsync(It.IsAny<ProblemDetailsContext>()))
            .Callback<ProblemDetailsContext>(ctx => captured = ctx)
            .ReturnsAsync(true);

        var services = new ServiceCollection();
        services.AddSingleton<IHostEnvironment>(new FakeHostEnvironment(environmentName: "Production"));

        var httpContext = new DefaultHttpContext
        {
            TraceIdentifier = "t1",
            Request = { Path = "/unauthorized" },
            RequestServices = services.BuildServiceProvider()
        };

        var handler = new GlobalExceptionHandler(loggerMock.Object, problemDetailsServiceMock.Object);

        await handler.TryHandleAsync(httpContext, new UnauthorizedAccessException());

        Assert.AreEqual(StatusCodes.Status401Unauthorized, httpContext.Response.StatusCode);
        Assert.AreEqual("Unauthorized", captured!.ProblemDetails.Title);
        Assert.AreEqual("https://tools.ietf.org/html/rfc9110#section-15.5.2", captured!.ProblemDetails.Type);
        Assert.Null(captured!.ProblemDetails.Detail);
    }

    [Test]
    public async Task TryHandleAsyncBaseRecipeExceptionProdUsesExceptionStatusTitleAndDetail()
    {
        var loggerMock = new Mock<ILogger<GlobalExceptionHandler>>();
        ProblemDetailsContext? captured = null;

        var problemDetailsServiceMock = new Mock<IProblemDetailsService>();
        problemDetailsServiceMock
            .Setup(s => s.TryWriteAsync(It.IsAny<ProblemDetailsContext>()))
            .Callback<ProblemDetailsContext>(ctx => captured = ctx)
            .ReturnsAsync(true);

        var services = new ServiceCollection();
        services.AddSingleton<IHostEnvironment>(new FakeHostEnvironment(environmentName: "Production"));

        var httpContext = new DefaultHttpContext
        {
            TraceIdentifier = "t-recipe",
            Request = { Path = "/recipe" },
            RequestServices = services.BuildServiceProvider()
        };

        var handler = new GlobalExceptionHandler(loggerMock.Object, problemDetailsServiceMock.Object);
        var ex = new EntityAlreadyExistsException("Recipe", "ID", 1);

        await handler.TryHandleAsync(httpContext, ex);

        Assert.AreEqual(409, httpContext.Response.StatusCode);
        Assert.AreEqual("A(n) Recipe with ID \"1\" already exists.", captured!.ProblemDetails.Title);
        Assert.AreEqual(409, captured!.ProblemDetails.Status);
        Assert.AreEqual("https://tools.ietf.org/html/rfc9110#section-15.5.10", captured!.ProblemDetails.Type);
        Assert.AreEqual("A(n) Recipe with ID \"1\" already exists.", captured!.ProblemDetails.Detail);
    }

    [Test]
    public async Task TryHandleAsyncUnknownExceptionProdMapsTo500AndNullDetail()
    {
        var loggerMock = new Mock<ILogger<GlobalExceptionHandler>>();
        ProblemDetailsContext? captured = null;

        var problemDetailsServiceMock = new Mock<IProblemDetailsService>();
        problemDetailsServiceMock
            .Setup(s => s.TryWriteAsync(It.IsAny<ProblemDetailsContext>()))
            .Callback<ProblemDetailsContext>(ctx => captured = ctx)
            .ReturnsAsync(true);

        var services = new ServiceCollection();
        services.AddSingleton<IHostEnvironment>(new FakeHostEnvironment(environmentName: "Production"));

        var httpContext = new DefaultHttpContext
        {
            TraceIdentifier = "t500",
            Request = { Path = "/unknown" },
            RequestServices = services.BuildServiceProvider()
        };

        var handler = new GlobalExceptionHandler(loggerMock.Object, problemDetailsServiceMock.Object);
        await handler.TryHandleAsync(httpContext, new Exception("boom"));

        Assert.AreEqual(StatusCodes.Status500InternalServerError, httpContext.Response.StatusCode);
        Assert.AreEqual("An unexpected error occurred", captured!.ProblemDetails.Title);
        Assert.Null(captured!.ProblemDetails.Detail);
    }
}