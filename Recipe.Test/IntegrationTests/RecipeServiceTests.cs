using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Recipe.Application;
using Recipe.Domain.Exceptions;
using Recipe.Domain;
using Recipe.Infrastructure;

namespace Recipe.Test.IntegrationTests;

public class RecipeServiceTests
{
    private readonly TestDatabaseContextFactory _factory = new();
    private RecipeDbContext recipeDbContext = null!;
    private RecipeService recipeService = null!;

    [SetUp]
    public async Task Setup()
    {
        recipeDbContext = _factory.CreateContext();
        recipeService = new(recipeDbContext);
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
        await recipeDbContext.SaveChangesAsync();
        await recipeDbContext.Recipes.AddAsync(recipe);
        await recipeDbContext.SaveChangesAsync();

    }

    [TearDown]
    public async Task Teardown()
    {
        recipeDbContext.Dispose();
        _factory.Dispose();
    }

    [Test]
    public async Task ExistsByIdAsyncReturnsTrueIfRecipeExists()
    {
        Assert.True(await recipeService.ExistsByIdAsync(1));
    }

    [Test]
    public async Task ExistsByIdAsyncReturnsFalseIfRecipeDoesNotExist()
    {
        Assert.False(await recipeService.ExistsByIdAsync(0));
    }

    [Test]
    public async Task GetByIdFindsRecipeIfRecipeExists()
    {
        var expectedRecipe = new Domain.Recipe
        {
            RecipeId = 1,
            Author = "Max",
            MethodText = "Take out of bag.",
            Title = "Flour",
            PosterId = 1
        };

        Assert.AreEqual(expectedRecipe, await recipeService.GetByIdAsync(1));
    }

    [Test]
    public async Task GetByIdThrowsIfRecipeDoesNotExist()
    {
        var expectedMessage = "No Recipe with ID \"0\" exists.";

        var ex = Assert.ThrowsAsync<EntityDoesNotExistException>(
            async () => await recipeService.GetByIdAsync(0)
        );

        Assert.AreEqual(expectedMessage, ex.Message);
    }

    [Test]
    public async Task CreateByIdCreatesNewRecipeIfRecipeDoesNotAlreadyExist()
    {
        var newRecipe = new Domain.Recipe
        {
            RecipeId = 2,
            Author = "Max",
            MethodText = "Combine flour and wat3r and mix with a spoon.",
            Title = "Fl0ur",
            PosterId = 1
        };
        var recipeDto = new RecipeDto(0, "Fl0ur", "Combine flour and wat3r and mix with a spoon.", new List<RecipeIngredientInput>(), "Max", 1);
        Assert.False(await recipeService.ExistsByIdAsync(2));
        await recipeService.CreateAsync(recipeDto);
        Assert.True(await recipeService.ExistsByIdAsync(2));
        Assert.AreEqual(newRecipe, await recipeService.GetByIdAsync(2));
    }

    [Test]
    public async Task CreatePersistsRecipeIngredientsAndUsesExistingIngredients()
    {
        var newRecipe = new Domain.Recipe
        {
            RecipeId = 2,
            Author = "Max",
            MethodText = "Mix everything.",
            Title = "Flour mix",
            PosterId = 1,
            RecipeIngredients = new List<RecipeIngredient>
            {
                new()
                {
                    Ingredient = new Ingredient(2, "Fl0ur"),
                    IngredientAmount = 250,
                    AmountUnit = IngredientUnit.Grams
                }
            }
        };

        var recipeDto = new RecipeDto(0, "Flour Mix", "Mix everything.", new List<RecipeIngredientInput>
            {
                new("Fl0ur", 250, IngredientUnit.Grams)
            }, "Max", 1);

        await recipeService.CreateAsync(recipeDto);

        var savedRecipe = await recipeService.GetByIdAsync(newRecipe.RecipeId);
        Assert.That(savedRecipe.RecipeIngredients.Single().Ingredient?.IngredientId, Is.EqualTo(2));
    }

    [Test]
    public async Task CreateByIdThrowsIfRecipeAlreadyExists()
    {
        var expectedMessage = "A(n) Recipe with ID \"1\" already exists.";

        var newRecipe = new Domain.Recipe
        {
            RecipeId = 1,
            Author = "Max",
            MethodText = "Combine flour and water and mix with a spoon.",
            Title = "Mix",
            PosterId = 1
        };

        var recipeDto = new RecipeDto(1, "Mix", "Combine flour and water and mix with a spoon.", new List<RecipeIngredientInput>(), "Max", 1);

        var ex = Assert.ThrowsAsync<EntityAlreadyExistsException>(
            async () => await recipeService.CreateAsync(recipeDto)
        );

        Assert.AreEqual(expectedMessage, ex.Message);
    }

    [Test]
    public async Task UpdateUpdatesFieldsIfRecipeExists()
    {
        var recipeDto = new RecipeDto(1, "Mix", "Combine flour and water and mix with a spoon.", new List<RecipeIngredientInput>(), "Jawn", 1);

        var recipeFromDb = await recipeService.GetByIdAsync(1);
        await recipeService.UpdateAsync(recipeDto);
        Assert.AreEqual(recipeFromDb.Title, recipeDto.Title);
        Assert.AreEqual(recipeFromDb.Author, recipeDto.AuthorName);
        Assert.AreEqual(recipeFromDb.MethodText, recipeDto.Method);
        Assert.AreEqual(recipeFromDb.RecipeIngredients, recipeDto.RecipeIngredients);
    }

    [Test]
    public async Task UpdateThrowsIfRecipeDoesNotExist()
    {
        var expectedMessage = "No Recipe with ID \"0\" exists.";

        var updatedRecipe = new Domain.Recipe
        {
            RecipeId = 0,
            Author = "Jawn",
            MethodText = "Combine flour and water and mix with a spoon.",
            Title = "Mix",
            PosterId = 1,
            RecipeIngredients = new List<RecipeIngredient>()
        };

        var recipeDto = new RecipeDto(0, "Mix", "Combine flour and water and mix with a spoon.", new List<RecipeIngredientInput>(), "Jawn", 1);

        var ex = Assert.ThrowsAsync<EntityDoesNotExistException>(
            async () => await recipeService.UpdateAsync(recipeDto)
        );

        Assert.AreEqual(expectedMessage, ex.Message);
    }

    [Test]
    public async Task DeleteDeletesIfRecipeExists()
    {
        Assert.True(await recipeService.ExistsByIdAsync(1));
        await recipeService.DeleteByIdAsync(1);
        Assert.False(await recipeService.ExistsByIdAsync(1));
    }

    [Test]
    public async Task DeleteThrowsIfRecipeDoesNotExist()
    {
        var expectedMessage = "No Recipe with ID \"0\" exists.";

        var ex = Assert.ThrowsAsync<EntityDoesNotExistException>(
            async () => await recipeService.DeleteByIdAsync(0)
        );

        Assert.AreEqual(expectedMessage, ex.Message);
    }
}
