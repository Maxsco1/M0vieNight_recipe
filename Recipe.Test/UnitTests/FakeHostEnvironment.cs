using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace Recipe.Test.UnitTests;

public class FakeHostEnvironment(string environmentName) : IHostEnvironment
{

    public string EnvironmentName { get; set; } = environmentName;

    public string ApplicationName { get; set; } = "";
    public string ContentRootPath { get; set; } = "";
    IFileProvider IHostEnvironment.ContentRootFileProvider { get; set; } = default!;
    public bool IsDevelopment() => EnvironmentName == "Development";
}