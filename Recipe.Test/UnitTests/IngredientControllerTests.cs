namespace Recipe.Test.UnitTests;

using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using Recipe.Api.Controllers;
using Recipe.Application;
using Recipe.Domain.Exceptions;
using Recipe.Domain;

public sealed class IngredientControllerTests
{
    private Mock<IIngredientService> ingredientServiceMock = new();
    private IngredientController ingredientController = null!;

    [SetUp]
    public void Setup()
    {
        ingredientServiceMock = new();
        ingredientController = new(ingredientServiceMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        ingredientServiceMock.Reset();
    }

    [Test]
    public async Task GetByIdReturnsOkWithIngredientInBodyWhenIngredientExists()
    {
        var ingredient = new Ingredient(1, "Flour");

        ingredientServiceMock
            .Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(ingredient);

        var result = await ingredientController.GetIngredient(1);
        var okResult = result as OkObjectResult;
        Ingredient? theIngredient = okResult.Value as Ingredient;

        Assert.IsNotNull(okResult);
        Assert.IsNotNull(theIngredient);
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreEqual(1, theIngredient.IngredientId);
        Assert.AreEqual("Flour", theIngredient.Name);
        ingredientServiceMock.Verify(s => s.GetByIdAsync(1), Times.Once);
    }

    [Test]
    public async Task GetByIdReturnsNotFoundWhenIngredientDoesNotExist()
    {
        var expectedMessage = "No Ingredient with ID \"0\" exists.";

        ingredientServiceMock
            .Setup(s => s.GetByIdAsync(0))
            .ThrowsAsync(new EntityDoesNotExistException("Ingredient", "ID", 0));

        var ex = Assert.ThrowsAsync<EntityDoesNotExistException>(
            async () => await ingredientController.GetIngredient(0)
        );

        Assert.AreEqual(expectedMessage, ex.Message);
        ingredientServiceMock.Verify(s => s.GetByIdAsync(0), Times.Once);
    }

    [Test]
    public async Task CreateIngredientReturnsCreatedWithIngredientInBodyIfIngredientDoesNotAlreadyExist()
    {
        var ingredient = new Ingredient(1, "Flour");

        ingredientServiceMock
            .Setup(s => s.CreateAsync(ingredient))
            .ReturnsAsync(ingredient);

        var result = await ingredientController.CreateIngredient(ingredient);
        var createdResult = result as CreatedResult;
        Ingredient? theIngredient = createdResult.Value as Ingredient;

        Assert.IsNotNull(createdResult);
        Assert.IsNotNull(theIngredient);
        Assert.AreEqual(201, createdResult.StatusCode);
        Assert.AreEqual(1, theIngredient.IngredientId);
        Assert.AreEqual("Flour", theIngredient.Name);
        ingredientServiceMock.Verify(s => s.CreateAsync(ingredient), Times.Once);
    }

    [Test]
    public async Task CreateIngredientThrowsIfIngredientWithIDAlreadyExists()
    {
        var expectedMessage = "A(n) Ingredient with ID \"1\" already exists.";
        var ingredient = new Ingredient(1, "Flour");

        ingredientServiceMock
            .Setup(s => s.CreateAsync(ingredient))
            .ThrowsAsync(new EntityAlreadyExistsException("Ingredient", "ID", 1));

        var ex = Assert.ThrowsAsync<EntityAlreadyExistsException>(
            async () => await ingredientController.CreateIngredient(ingredient)
        );

        Assert.AreEqual(expectedMessage, ex.Message);
        ingredientServiceMock.Verify(s => s.CreateAsync(ingredient), Times.Once);
    }

    [Test]
    public async Task CreateIngredientThrowsIfIngredientWithNameAlreadyExists()
    {
        var expectedMessage = "A(n) Ingredient with name \"Flour\" already exists.";
        var ingredient = new Ingredient(1, "Flour");

        ingredientServiceMock
            .Setup(s => s.CreateAsync(ingredient))
            .ThrowsAsync(new EntityAlreadyExistsException("Ingredient", "name", ingredient.Name));

        var ex = Assert.ThrowsAsync<EntityAlreadyExistsException>(
            async () => await ingredientController.CreateIngredient(ingredient)
        );

        Assert.AreEqual(expectedMessage, ex.Message);
        ingredientServiceMock.Verify(s => s.CreateAsync(ingredient), Times.Once);
    }

    [Test]
    public async Task UpdateIngredientReturns201IfNewNameUniqueAndNoIngredientWithIdExists()
    {
        var updatedIngredient = new Ingredient(1, "Fleur");

        var result = await ingredientController.UpdateIngredient(updatedIngredient);
        var createdResult = result as CreatedResult;
        Ingredient? theIngredient = createdResult.Value as Ingredient;

        Assert.IsNotNull(createdResult);
        Assert.IsNotNull(theIngredient);
        Assert.AreEqual(201, createdResult.StatusCode);
        Assert.AreEqual(1, theIngredient.IngredientId);
        Assert.AreEqual("Fleur", theIngredient.Name);
        ingredientServiceMock.Verify(s => s.UpdateAsync(updatedIngredient), Times.Once);
    }

    [Test]
    public async Task UpdateIngredientThrowsIfIngredientWithNameAlreadyExists()
    {
        var expectedMessage = "A(n) Ingredient with name \"Fleur\" already exists.";
        var ingredient = new Ingredient(1, "Fleur");

        ingredientServiceMock
            .Setup(s => s.UpdateAsync(ingredient))
            .ThrowsAsync(new EntityAlreadyExistsException("Ingredient", "name", ingredient.Name));

        var ex = Assert.ThrowsAsync<EntityAlreadyExistsException>(
            async () => await ingredientController.UpdateIngredient(ingredient)
        );

        Assert.AreEqual(expectedMessage, ex.Message);
        ingredientServiceMock.Verify(s => s.UpdateAsync(ingredient), Times.Once);
    }

    [Test]
    public async Task UpdateIngredientThrowsIfUpdateFailed()
    {
        var expectedMessage = "Failed to update any Ingredients.";
        var ingredient = new Ingredient(1, "Fleur");

        ingredientServiceMock
            .Setup(s => s.UpdateAsync(ingredient))
            .ThrowsAsync(new ModifyFailedException("update", "Ingredient"));

        var ex = Assert.ThrowsAsync<ModifyFailedException>(
            async () => await ingredientController.UpdateIngredient(ingredient)
        );

        Assert.AreEqual(expectedMessage, ex.Message);
        ingredientServiceMock.Verify(s => s.UpdateAsync(ingredient), Times.Once);
    }

    [Test]
    public async Task DeleteByIdReturnsNoContentIfIngredientWithIdExistsAndUpdateDoesNotFail()
    {
        var deletedIngredient = new Ingredient(1, "Fleur");

        var result = await ingredientController.DeleteIngredient(deletedIngredient.IngredientId);
        var deletedResult = result as NoContentResult;

        Assert.IsNotNull(deletedResult);
        Assert.AreEqual(204, deletedResult.StatusCode);
        ingredientServiceMock.Verify(s => s.DeleteByIdAsync(deletedIngredient.IngredientId), Times.Once);
    }

    [Test]
    public async Task DeleteByIdThrowsIfIngredientWithIdDoesNotExist()
    {
        var expectedMessage = "No Ingredient with ID \"1\" exists.";
        var deletedIngredient = new Ingredient(1, "Fleur");

        ingredientServiceMock
            .Setup(s => s.DeleteByIdAsync(deletedIngredient.IngredientId))
            .ThrowsAsync(new EntityDoesNotExistException("Ingredient", "ID", deletedIngredient.IngredientId));

        var ex = Assert.ThrowsAsync<EntityDoesNotExistException>(
            async () => await ingredientController.DeleteIngredient(deletedIngredient.IngredientId)
        );

        Assert.AreEqual(expectedMessage, ex.Message);
        ingredientServiceMock.Verify(s => s.DeleteByIdAsync(deletedIngredient.IngredientId), Times.Once);
    }

    [Test]
    public async Task DeleteByIdThrowsIfUpdateFails()
    {
        var expectedMessage = "Failed to delete any Ingredients.";
        var deletedIngredient = new Ingredient(1, "Fleur");

        ingredientServiceMock
            .Setup(s => s.DeleteByIdAsync(deletedIngredient.IngredientId))
            .ThrowsAsync(new ModifyFailedException("delete", "Ingredient"));

        var ex = Assert.ThrowsAsync<ModifyFailedException>(
            async () => await ingredientController.DeleteIngredient(deletedIngredient.IngredientId)
        );

        Assert.AreEqual(expectedMessage, ex.Message);
        ingredientServiceMock.Verify(s => s.DeleteByIdAsync(deletedIngredient.IngredientId), Times.Once);
    }
}
