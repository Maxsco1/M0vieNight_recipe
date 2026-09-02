using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Recipe.Application;
using Recipe.Domain.Exceptions;
using Recipe.Domain;
using Recipe.Infrastructure;

namespace Recipe.Test.IntegrationTests;

public class RecipeIngredientServiceTests
{
    private readonly TestDatabaseContextFactory _factory = new();
    private RecipeDbContext recipeDbContext = null!;
    private RecipeIngredientService recipeIngredientService = null!;

    [SetUp]
    public async Task Setup()
    {
        recipeDbContext = _factory.CreateContext();
        recipeIngredientService = new(recipeDbContext);
        var ingredient = new Ingredient(0, "Flour");

        var recipe = new Domain.Recipe
        {
            RecipeId = 0,
            Author = "Max",
            MethodText = "Take out of bag.",
            Title = "Flour",
            PosterId = 1
        };

        await recipeDbContext.Ingredients.AddAsync(ingredient);
        await recipeDbContext.Recipes.AddAsync(recipe);
        await recipeDbContext.SaveChangesAsync();

        var recipeIngredient = new RecipeIngredient
        {
            RecipeIngredientId = 0,
            RecipeId = recipe.RecipeId,
            IngredientId = ingredient.IngredientId,
            Ingredient = ingredient,
            IngredientAmount = 10.0f,
            AmountUnit = IngredientUnit.Grams
        };

        recipe.RecipeIngredients = new List<RecipeIngredient> { recipeIngredient };
        await recipeDbContext.RecipeIngredients.AddAsync(recipeIngredient);
        await recipeDbContext.SaveChangesAsync();
    }

    [TearDown]
    public async Task Teardown()
    {
        recipeDbContext.Dispose();
        _factory.Dispose();
    }

    [Test]
    public async Task ExistByIdReturnsTrueIfEntityWithIdDoesExist()
    {
        Assert.True(await recipeIngredientService.ExistsByIdAsync(1));
    }

    [Test]
    public async Task ExistByIdReturnsFalseIfEntityWithIdDoesNotExist()
    {
        Assert.False(await recipeIngredientService.ExistsByIdAsync(0));
    }

    [Test]
    public async Task GetByIdReturnsExpectedRecipeIngredientIfExists()
    {
        var exampleIngredient = new Ingredient(1, "Flour");
        RecipeIngredient expectedRecipeIngredient = new(1, exampleIngredient, 10.0f, IngredientUnit.Grams);
        var ingredientFromDb = await recipeIngredientService.GetByIdAsync(1);
        Assert.AreEqual(expectedRecipeIngredient, ingredientFromDb);
    }

    [Test]
    public async Task GetByIdThrowsIfIngredientDoesNotExist()
    {
        string message = "No RecipeIngredient with ID \"0\" exists.";
        var ex = Assert.ThrowsAsync<EntityDoesNotExistException>(
            async () => await recipeIngredientService.GetByIdAsync(0)
        );

        Assert.AreEqual(ex.Message, message);
    }

    [Test]
    public async Task UpdateAltersUpdatedFields()
    {
        var previousRecipeIngredient = await recipeIngredientService.GetByIdAsync(1);
        Ingredient ingredient = new(0, "Sugar");
        RecipeIngredient newExpectedRecipeIngredient = new(1, ingredient, 1.0f, IngredientUnit.Kilograms);
        await recipeIngredientService.UpdateAsync(newExpectedRecipeIngredient);
        Assert.AreEqual(newExpectedRecipeIngredient, previousRecipeIngredient);
    }

    [Test]
    public async Task UpdateThrowsOnNonExistentId()
    {
        Ingredient ingredient = new(0, "Sugar");
        RecipeIngredient newExpectedRecipeIngredient = new(0, ingredient, 1.0f, IngredientUnit.Kilograms);
        var ex = Assert.ThrowsAsync<EntityDoesNotExistException>(
            async () => await recipeIngredientService.UpdateAsync(newExpectedRecipeIngredient)
        );

        Assert.AreEqual("No RecipeIngredient with ID \"0\" exists.", ex.Message);
    }

    [Test]
    public async Task DeleteDeletesIfRIWithIdExists()
    {
        Assert.True(await recipeIngredientService.ExistsByIdAsync(1));
        await recipeIngredientService.DeleteByIdAsync(1);
        Assert.False(await recipeIngredientService.ExistsByIdAsync(1));
    }

    [Test]
    public async Task DeleteThrowsOnNonExistentID()
    {
        Ingredient ingredient = new(0, "Sugar");
        RecipeIngredient newExpectedRecipeIngredient = new(0, ingredient, 1.0f, IngredientUnit.Kilograms);
        var ex = Assert.ThrowsAsync<EntityDoesNotExistException>(
            async () => await recipeIngredientService.DeleteByIdAsync(0)
        );

        Assert.AreEqual("No RecipeIngredient with ID \"0\" exists.", ex.Message);
    }
}
