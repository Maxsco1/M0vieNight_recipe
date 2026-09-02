namespace Recipe.Test.UnitTests;

using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using Recipe.Api.Controllers;
using Recipe.Application;
using Recipe.Domain.Exceptions;
using Recipe.Domain;

public sealed class RecipeControllerTests
{
    private Mock<IRecipeService> recipeServiceMock = new();
    private RecipeController recipeController = null!;

    [SetUp]
    public void Setup()
    {
        recipeServiceMock = new();
        recipeController = new(recipeServiceMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        recipeServiceMock.Reset();
    }

    [Test]
    public async Task GetAllRecipesReturnsOkWithRecipesInBody()
    {
        var recipes = new[] { new Recipe { RecipeId = 1, Title = "Pasta" } };
        recipeServiceMock.Setup(s => s.GetAll(CancellationToken.None)).ReturnsAsync(recipes);

        var result = await recipeController.GetAllRecipes();
        var okResult = result as OkObjectResult;

        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreSame(recipes, okResult.Value);
        recipeServiceMock.Verify(s => s.GetAll(CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task GetByIdReturnsOkWithRecipeInBodyWhenRecipeExists()
    {
        var recipe = new Recipe { RecipeId = 1, Title = "Pasta" };
        recipeServiceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(recipe);

        var result = await recipeController.GetRecipe(1);
        var okResult = result as OkObjectResult;

        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreSame(recipe, okResult.Value);
        recipeServiceMock.Verify(s => s.GetByIdAsync(1), Times.Once);
    }

    [Test]
    public void GetByIdThrowsWhenRecipeDoesNotExist()
    {
        var expectedMessage = "No Recipe with ID \"0\" exists.";
        recipeServiceMock
            .Setup(s => s.GetByIdAsync(0))
            .ThrowsAsync(new EntityDoesNotExistException("Recipe", "ID", 0));

        var ex = Assert.ThrowsAsync<EntityDoesNotExistException>(
            async () => await recipeController.GetRecipe(0));

        Assert.AreEqual(expectedMessage, ex.Message);
        recipeServiceMock.Verify(s => s.GetByIdAsync(0), Times.Once);
    }

    [Test]
    public async Task CreateRecipeReturnsCreatedWithRecipeInBody()
    {
        var recipe = new Recipe { RecipeId = 1, Title = "Pasta" };
        var recipeDto = new RecipeDto(0, "Pasta", "Boil", new List<RecipeIngredientInput>(), "Max", 1);
        recipeServiceMock.Setup(s => s.CreateAsync(recipeDto)).ReturnsAsync(recipe);

        var result = await recipeController.CreateRecipe(recipeDto);
        var createdResult = result as CreatedResult;

        Assert.IsNotNull(createdResult);
        Assert.AreEqual(201, createdResult.StatusCode);
        Assert.AreEqual("api/recipes/1", createdResult.Location);
        Assert.AreSame(recipe, createdResult.Value);
        recipeServiceMock.Verify(s => s.CreateAsync(recipeDto), Times.Once);
    }

    [Test]
    public void CreateRecipeThrowsWhenRecipeAlreadyExists()
    {
        var expectedMessage = "A(n) Recipe with ID \"1\" already exists.";
        var recipe = new Recipe { RecipeId = 1, Title = "Pasta" };
        var recipeDto = new RecipeDto(1, "Pasta", "Boil", new List<RecipeIngredientInput>(), "Max", 1);

        recipeServiceMock
            .Setup(s => s.CreateAsync(recipeDto))
            .ThrowsAsync(new EntityAlreadyExistsException("Recipe", "ID", 1));

        var ex = Assert.ThrowsAsync<EntityAlreadyExistsException>(
            async () => await recipeController.CreateRecipe(recipeDto));

        Assert.AreEqual(expectedMessage, ex.Message);
        recipeServiceMock.Verify(s => s.CreateAsync(recipeDto), Times.Once);
    }

    [Test]
    public async Task UpdateRecipeReturnsCreatedWithRecipeInBody()
    {
        var recipeDto = new RecipeDto(1, "Pasta", "B0il", new List<RecipeIngredientInput>(), "Max", 1);

        var result = await recipeController.UpdateRecipe(recipeDto);
        var createdResult = result as CreatedResult;

        Assert.IsNotNull(createdResult);
        Assert.AreEqual(201, createdResult.StatusCode);
        Assert.AreEqual("api/recipes/1", createdResult.Location);
        Assert.AreEqual(recipeDto, createdResult.Value);
        recipeServiceMock.Verify(s => s.UpdateAsync(recipeDto), Times.Once);
    }

    [Test]
    public void UpdateRecipeThrowsWhenRecipeDoesNotExist()
    {
        var expectedMessage = "No Recipe with ID \"1\" exists.";
        var recipe = new Recipe { RecipeId = 1, Title = "Pasta" };
        var recipeDto = new RecipeDto(0, "Pasta", "Boil", new List<RecipeIngredientInput>(), "Max", 1);
        recipeServiceMock
            .Setup(s => s.UpdateAsync(recipeDto))
            .ThrowsAsync(new EntityDoesNotExistException("Recipe", "ID", 1));

        var ex = Assert.ThrowsAsync<EntityDoesNotExistException>(
            async () => await recipeController.UpdateRecipe(recipeDto));

        Assert.AreEqual(expectedMessage, ex.Message);
        recipeServiceMock.Verify(s => s.UpdateAsync(recipeDto), Times.Once);
    }

    [Test]
    public void UpdateRecipeThrowsWhenUpdateFails()
    {
        var expectedMessage = "Failed to update any Recipes.";
        var recipe = new Recipe { RecipeId = 1, Title = "Pasta" };
        var recipeDto = new RecipeDto(0, "Pasta", "Boil", new List<RecipeIngredientInput>(), "Max", 1);
        recipeServiceMock
            .Setup(s => s.UpdateAsync(recipeDto))
            .ThrowsAsync(new ModifyFailedException("update", "Recipe"));

        var ex = Assert.ThrowsAsync<ModifyFailedException>(
            async () => await recipeController.UpdateRecipe(recipeDto));

        Assert.AreEqual(expectedMessage, ex.Message);
        recipeServiceMock.Verify(s => s.UpdateAsync(recipeDto), Times.Once);
    }

    [Test]
    public async Task DeleteByIdReturnsNoContentWhenRecipeExists()
    {
        var result = await recipeController.DeleteRecipe(1);
        var noContentResult = result as NoContentResult;

        Assert.IsNotNull(noContentResult);
        Assert.AreEqual(204, noContentResult.StatusCode);
        recipeServiceMock.Verify(s => s.DeleteByIdAsync(1), Times.Once);
    }

    [Test]
    public void DeleteByIdThrowsWhenRecipeDoesNotExist()
    {
        var expectedMessage = "No Recipe with ID \"1\" exists.";
        recipeServiceMock
            .Setup(s => s.DeleteByIdAsync(1))
            .ThrowsAsync(new EntityDoesNotExistException("Recipe", "ID", 1));

        var ex = Assert.ThrowsAsync<EntityDoesNotExistException>(
            async () => await recipeController.DeleteRecipe(1));

        Assert.AreEqual(expectedMessage, ex.Message);
        recipeServiceMock.Verify(s => s.DeleteByIdAsync(1), Times.Once);
    }

    [Test]
    public void DeleteByIdThrowsWhenDeleteFails()
    {
        var expectedMessage = "Failed to delete any Recipes.";
        recipeServiceMock
            .Setup(s => s.DeleteByIdAsync(1))
            .ThrowsAsync(new ModifyFailedException("delete", "Recipe"));

        var ex = Assert.ThrowsAsync<ModifyFailedException>(
            async () => await recipeController.DeleteRecipe(1));

        Assert.AreEqual(expectedMessage, ex.Message);
        recipeServiceMock.Verify(s => s.DeleteByIdAsync(1), Times.Once);
    }
}
