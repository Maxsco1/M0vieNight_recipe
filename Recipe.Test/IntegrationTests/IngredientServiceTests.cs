using NUnit.Framework;
using Recipe.Application;
using Recipe.Domain.Exceptions;
using Recipe.Domain;
using Recipe.Infrastructure;

namespace Recipe.Test.IntegrationTests;

public class IngredientServiceTests
{
    private readonly TestDatabaseContextFactory _factory = new();
    private IngredientService ingredientService = null!;

    [SetUp]
    public async Task Setup()
    {
        ingredientService = new(_factory.CreateContext());

        List<Ingredient> ingredients = [
            new Ingredient(1, "Flour"),
            new Ingredient(2, "Eggs"),
            new Ingredient(3, "Milk"),
            new Ingredient(4, "Sugar"),
            new Ingredient(5, "Chocolate chips")
        ];

        foreach (var ingredient in ingredients)
        {
            await ingredientService.CreateAsync(ingredient);
        }
    }

    [TearDown]
    public async Task Teardown()
    {
        await ingredientService.DeleteByIdAsync(1);
        await ingredientService.DeleteByIdAsync(2);
        await ingredientService.DeleteByIdAsync(3);
        await ingredientService.DeleteByIdAsync(4);
        await ingredientService.DeleteByIdAsync(5);
    }

    [Test]
    public async Task ExistByIdReturnsFalseIfEntityWithIdDoesExist()
    {
        Assert.True(await ingredientService.ExistsByIdAsync(1));
        Assert.True(await ingredientService.ExistsByIdAsync(2));
        Assert.True(await ingredientService.ExistsByIdAsync(3));
        Assert.True(await ingredientService.ExistsByIdAsync(4));
        Assert.True(await ingredientService.ExistsByIdAsync(5));
    }

    [Test]
    public async Task ExistByIdReturnsFalseIfEntityWithIdDoesNotExist()
    {
        Assert.False(await ingredientService.ExistsByIdAsync(0));
    }

    [Test]
    public async Task ExistsByNameReturnsTrueIfEntityWithNameExists()
    {
        Assert.True(await ingredientService.ExistsByNameAsync("Flour"));
    }

    [Test]
    public async Task ExistsByNameReturnsFalseIfEntityWithNameDoesNotExist()
    {
        Assert.False(await ingredientService.ExistsByNameAsync("Coca-Cola"));
    }

    [Test]
    public async Task GetByIdReturnsExpectedIngredientIfExists()
    {
        Ingredient expectedIngredient = new(1, "Flour");
        var ingredientFromDb = await ingredientService.GetByIdAsync(1);
        Assert.AreEqual(expectedIngredient, ingredientFromDb);
    }

    [Test]
    public async Task GetByIdThrowsIfIngredientDoesNotExist()
    {
        string message = "No Ingredient with ID \"0\" exists.";
        var ex = Assert.ThrowsAsync<EntityDoesNotExistException>(
            async () => await ingredientService.GetByIdAsync(0)
        );

        Assert.AreEqual(ex.Message, message);
    }

    [Test]
    public async Task GetAllReturnsAllIngredients()
    {
        List<Ingredient> expectedIngredients = [
            new Ingredient(1, "Flour"),
            new Ingredient(2, "Eggs"),
            new Ingredient(3, "Milk"),
            new Ingredient(4, "Sugar"),
            new Ingredient(5, "Chocolate chips")
        ];

        Assert.AreEqual(expectedIngredients, await ingredientService.GetAllAsync());
    }

    [Test]
    public async Task CreateCreatesIngredientIfItDoesNotAlreadyExist()
    {
        var nextIngredient = new Ingredient(6, "Caster sugar");
        var allIngredientsBefore = await ingredientService.GetAllAsync();
        Assert.That(allIngredientsBefore.Count, Is.EqualTo(5));

        var inserted = await ingredientService.CreateAsync(nextIngredient);

        Assert.AreEqual(nextIngredient, inserted);
        var allIngredientsAfter = await ingredientService.GetAllAsync();
        Assert.That(allIngredientsAfter.Count, Is.EqualTo(6));
        await ingredientService.DeleteByIdAsync(6);
    }

    [Test]
    public async Task CreateThrowsIfIngredientWithGivenIdAlreadyExists()
    {
        var ex = Assert.ThrowsAsync<EntityAlreadyExistsException>(
            async () => await ingredientService.CreateAsync(new Ingredient(1, "Flour"))
        );

        Assert.AreEqual("A(n) Ingredient with ID \"1\" already exists.", ex.Message);
    }

    [Test]
    public async Task CreateThrowsIfIngredientWithGivenNameAlreadyExists()
    {
        var ex = Assert.ThrowsAsync<EntityAlreadyExistsException>(
            async () => await ingredientService.CreateAsync(new Ingredient(7, "Flour"))
        );

        Assert.AreEqual("A(n) Ingredient with name \"Flour\" already exists.", ex.Message);
    }

    [Test]
    public async Task UpdateAltersNameIfNameChanged()
    {
        var flourBefore = await ingredientService.GetByIdAsync(1);
        var newFlour = new Ingredient(1, "Fl0ur");
        await ingredientService.UpdateAsync(newFlour);
        Assert.AreEqual(newFlour, flourBefore);
    }

    [Test]
    public async Task DeleteDeletes()
    {
        var allIngredientsBefore = await ingredientService.GetAllAsync();
        Assert.AreEqual(allIngredientsBefore.Count(), 5);

        Assert.True(await ingredientService.ExistsByIdAsync(1));
        await ingredientService.DeleteByIdAsync(1);
        var allIngredientsAfter = await ingredientService.GetAllAsync();
        Assert.AreEqual(allIngredientsAfter.Count, 4);
        Assert.False(await ingredientService.ExistsByIdAsync(1));
        await ingredientService.CreateAsync(new Ingredient(1, "Flour"));
    }
}
