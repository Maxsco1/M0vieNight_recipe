namespace Recipe.Test.UnitTests;

using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using Recipe.Api.Controllers;
using Recipe.Application;
using Recipe.Domain.Exceptions;
using Recipe.Domain;

public sealed class RecipeIngredientControllerTests
{
    private Mock<IRecipeIngredientService> recipeIngredientServiceMock = new();
    private RecipeIngredientController recipeIngredientController = null!;

    [SetUp]
    public void Setup()
    {
        recipeIngredientServiceMock = new();
        recipeIngredientController = new(recipeIngredientServiceMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        recipeIngredientServiceMock.Reset();
    }

    [Test]
    public async Task GetByIdReturnsOkWithRecipeIngredientInBodyWhenRecipeIngredientExists()
    {
        var recipeIngredient = new RecipeIngredient
        {
            RecipeIngredientId = 1,
            IngredientAmount = 250,
            AmountUnit = IngredientUnit.Grams
        };
        recipeIngredientServiceMock
            .Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(recipeIngredient);

        var result = await recipeIngredientController.GetRecipeIngredient(1);
        var okResult = result as OkObjectResult;

        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreSame(recipeIngredient, okResult.Value);
        recipeIngredientServiceMock.Verify(s => s.GetByIdAsync(1), Times.Once);
    }

    [Test]
    public void GetByIdThrowsWhenRecipeIngredientDoesNotExist()
    {
        const long id = 0;
        var expectedMessage = $"No RecipeIngredient with ID \"{id}\" exists.";
        recipeIngredientServiceMock
            .Setup(s => s.GetByIdAsync(id))
            .ThrowsAsync(new EntityDoesNotExistException("RecipeIngredient", "ID", id));

        var ex = Assert.ThrowsAsync<EntityDoesNotExistException>(
            async () => await recipeIngredientController.GetRecipeIngredient(id));

        Assert.AreEqual(expectedMessage, ex.Message);
        recipeIngredientServiceMock.Verify(s => s.GetByIdAsync(id), Times.Once);
    }

    [Test]
    public async Task UpdateRecipeIngredientReturnsCreatedWithRecipeIngredientInBody()
    {
        var recipeIngredient = new RecipeIngredient { RecipeIngredientId = 1 };

        var result = await recipeIngredientController.UpdateRecipeIngredient(1, 1, recipeIngredient);
        var createdResult = result as CreatedResult;

        Assert.IsNotNull(createdResult);
        Assert.AreEqual(201, createdResult.StatusCode);
        Assert.AreEqual("api/recipes/1/ingredients/1", createdResult.Location);
        Assert.AreSame(recipeIngredient, createdResult.Value);
        recipeIngredientServiceMock.Verify(s => s.UpdateAsync(recipeIngredient), Times.Once);
    }

    [Test]
    public void UpdateRecipeIngredientThrowsWhenRecipeIngredientDoesNotExist()
    {
        var recipeIngredient = new RecipeIngredient { RecipeIngredientId = 1 };
        var expectedMessage = "No RecipeIngredient with ID \"1\" exists.";
        recipeIngredientServiceMock
            .Setup(s => s.UpdateAsync(recipeIngredient))
            .ThrowsAsync(new EntityDoesNotExistException("RecipeIngredient", "ID", recipeIngredient.RecipeIngredientId));

        var ex = Assert.ThrowsAsync<EntityDoesNotExistException>(
            async () => await recipeIngredientController.UpdateRecipeIngredient(1, 1, recipeIngredient));

        Assert.AreEqual(expectedMessage, ex.Message);
        recipeIngredientServiceMock.Verify(s => s.UpdateAsync(recipeIngredient), Times.Once);
    }

    [Test]
    public void UpdateRecipeIngredientThrowsWhenUpdateFails()
    {
        var recipeIngredient = new RecipeIngredient { RecipeIngredientId = 1 };
        var expectedMessage = "Failed to update any RecipeIngredients.";
        recipeIngredientServiceMock
            .Setup(s => s.UpdateAsync(recipeIngredient))
            .ThrowsAsync(new ModifyFailedException("update", "RecipeIngredient"));

        var ex = Assert.ThrowsAsync<ModifyFailedException>(
            async () => await recipeIngredientController.UpdateRecipeIngredient(1, 1, recipeIngredient));

        Assert.AreEqual(expectedMessage, ex.Message);
        recipeIngredientServiceMock.Verify(s => s.UpdateAsync(recipeIngredient), Times.Once);
    }

    [Test]
    public async Task DeleteByIdReturnsNoContentWhenRecipeIngredientExists()
    {
        var result = await recipeIngredientController.DeleteRecipeIngredient(1);
        var noContentResult = result as NoContentResult;

        Assert.IsNotNull(noContentResult);
        Assert.AreEqual(204, noContentResult.StatusCode);
        recipeIngredientServiceMock.Verify(s => s.DeleteByIdAsync(1), Times.Once);
    }

    [Test]
    public void DeleteByIdThrowsWhenRecipeIngredientDoesNotExist()
    {
        const long id = 1;
        var expectedMessage = $"No RecipeIngredient with ID \"{id}\" exists.";
        recipeIngredientServiceMock
            .Setup(s => s.DeleteByIdAsync(id))
            .ThrowsAsync(new EntityDoesNotExistException("RecipeIngredient", "ID", id));

        var ex = Assert.ThrowsAsync<EntityDoesNotExistException>(
            async () => await recipeIngredientController.DeleteRecipeIngredient(id));

        Assert.AreEqual(expectedMessage, ex.Message);
        recipeIngredientServiceMock.Verify(s => s.DeleteByIdAsync(id), Times.Once);
    }

    [Test]
    public void DeleteByIdThrowsWhenDeleteFails()
    {
        const long id = 1;
        var expectedMessage = "Failed to delete any RecipeIngredients.";
        recipeIngredientServiceMock
            .Setup(s => s.DeleteByIdAsync(id))
            .ThrowsAsync(new ModifyFailedException("delete", "RecipeIngredient"));

        var ex = Assert.ThrowsAsync<ModifyFailedException>(
            async () => await recipeIngredientController.DeleteRecipeIngredient(id));

        Assert.AreEqual(expectedMessage, ex.Message);
        recipeIngredientServiceMock.Verify(s => s.DeleteByIdAsync(id), Times.Once);
    }
}
