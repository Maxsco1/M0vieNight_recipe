using Microsoft.EntityFrameworkCore;
using Recipe.Domain.Exceptions;
using Recipe.Domain;
using Recipe.Infrastructure;

namespace Recipe.Application;

public class RecipeService(RecipeDbContext context) : IRecipeService
{
    private readonly RecipeDbContext _context = context;
    public async Task<Domain.Recipe> CreateAsync(RecipeDto recipe, CancellationToken cancellationToken = default)
    {
        if (await ExistsByIdAsync(recipe.RecipeId, cancellationToken))
        {
            throw new EntityAlreadyExistsException("Recipe", "ID", recipe.RecipeId);
        }

        var namesFromRecipe = recipe.RecipeIngredients
            .Select(x => x.IngredientName.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var existingIngredients = await _context.Ingredients
            .Where(x => namesFromRecipe.Contains(x.Name))
            .ToDictionaryAsync(
                x => x.Name,
                StringComparer.OrdinalIgnoreCase,
                cancellationToken);

        foreach (var name in namesFromRecipe)
        {
            if (!existingIngredients.ContainsKey(name))
            {
                var newIngredient = new Ingredient(0, name);

                _context.Ingredients.Add(newIngredient);
                existingIngredients.Add(name, newIngredient);
            }
        }

        var newRecipe = new Domain.Recipe
        {
            Title = recipe.Title,
            Author = recipe.AuthorName,
            MethodText = recipe.Method,
            RecipeIngredients = new List<RecipeIngredient>()
        };

        foreach (var ingredientInput in recipe.RecipeIngredients)
        {
            var ingredient = existingIngredients[ingredientInput.IngredientName];

            newRecipe.RecipeIngredients.Add(new RecipeIngredient
            {
                Ingredient = ingredient,
                IngredientAmount = ingredientInput.IngredientAmount,
                AmountUnit = ingredientInput.AmountUnit
            });
        }

        _context.Recipes.Add(newRecipe);
        await _context.SaveChangesAsync(cancellationToken);
        return newRecipe;
    }

    public async Task DeleteByIdAsync(long recipeId, CancellationToken cancellationToken = default)
    {
        Domain.Recipe recipe = await GetByIdAsync(recipeId, cancellationToken);
        _context.Recipes.Remove(recipe);

        if (!(await _context.SaveChangesAsync(cancellationToken) > 0))
        {
            throw new ModifyFailedException("delete", "Recipe");
        }
    }

    public async Task<bool> ExistsByIdAsync(long recipeId, CancellationToken cancellationToken = default)
    {
        return await _context.Recipes.AnyAsync(r => r.RecipeId == recipeId, cancellationToken);
    }

    public async Task<IEnumerable<Domain.Recipe>> GetAll(CancellationToken cancellationToken)
    {
        return await _context.Recipes
            .Include(r => r.RecipeIngredients)
            .ThenInclude(ri => ri.Ingredient)
            .ToListAsync(cancellationToken);
    }

    public async Task<Domain.Recipe> GetByIdAsync(long recipeId, CancellationToken cancellationToken = default)
    {
        return await _context.Recipes
        .Include(r => r.RecipeIngredients)
        .ThenInclude(ri => ri.Ingredient)
        .FirstOrDefaultAsync(r => r.RecipeId == recipeId, cancellationToken)
            ?? throw new EntityDoesNotExistException("Recipe", "ID", recipeId);
    }

    public async Task UpdateAsync(RecipeDto updatedRecipe, CancellationToken cancellationToken = default)
    {
        Domain.Recipe recipeFromDb = await GetByIdAsync(updatedRecipe.RecipeId, cancellationToken);
        recipeFromDb.Author = updatedRecipe.AuthorName;
        recipeFromDb.MethodText = updatedRecipe.Method;
        recipeFromDb.Title = updatedRecipe.Title;

        var namesFromRecipeToUpdate = recipeFromDb.RecipeIngredients.Select(x => x.Ingredient.Name).ToHashSet();
        var namesFromUpdatedRecipe = updatedRecipe.RecipeIngredients.Select(x => x.IngredientName).ToHashSet();

        foreach (var name in namesFromRecipeToUpdate)
        {
            if (!namesFromUpdatedRecipe.Contains(name))
            {
                var toRemove = recipeFromDb.RecipeIngredients.First(x => x.Ingredient.Name == name);
                recipeFromDb.RecipeIngredients.Remove(toRemove);
            }
        }

        foreach (var recipeIngredient in updatedRecipe.RecipeIngredients)
        {
            if (namesFromRecipeToUpdate.Contains(recipeIngredient.IngredientName))
            {
                var toUpdate = recipeFromDb.RecipeIngredients.First(x => x.Ingredient.Name == recipeIngredient.IngredientName);
                toUpdate.AmountUnit = recipeIngredient.AmountUnit;
                toUpdate.IngredientAmount = recipeIngredient.IngredientAmount;
            }
            else
            {
                Ingredient newIngredient = new(0, recipeIngredient.IngredientName);
                _context.Ingredients.Add(newIngredient);
                recipeFromDb.RecipeIngredients.Add(new RecipeIngredient(0, newIngredient, recipeIngredient.IngredientAmount, recipeIngredient.AmountUnit));
            }
        }

        if (!(await _context.SaveChangesAsync(cancellationToken) > 0))
        {
            throw new ModifyFailedException("update", "Recipe");
        }
    }
}
