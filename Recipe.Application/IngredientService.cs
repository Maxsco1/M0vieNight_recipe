using Microsoft.EntityFrameworkCore;
using Recipe.Domain.Exceptions;
using Recipe.Domain;
using Recipe.Infrastructure;

namespace Recipe.Application;

public class IngredientService(RecipeDbContext recipeDbContext) : IIngredientService
{
    private readonly RecipeDbContext _context = recipeDbContext;

    public async Task<bool> ExistsByIdAsync(long ingredientId, CancellationToken cancellationToken = default)
    {
        return await _context.Ingredients.AnyAsync(i => i.IngredientId == ingredientId, cancellationToken);
    }

    public async Task<Ingredient> GetByIdAsync(long ingredientId, CancellationToken cancellationToken = default)
    {
        return await _context.Ingredients
            .FirstOrDefaultAsync(i => i.IngredientId == ingredientId, cancellationToken) ??
            throw new EntityDoesNotExistException("Ingredient", "ID", ingredientId);
    }

    public async Task<bool> ExistsByNameAsync(string ingredientName, CancellationToken cancellationToken = default)
    {
        return await _context.Ingredients.AnyAsync(i => i.Name == ingredientName, cancellationToken);
    }

    public async Task<List<Ingredient>> GetAllAsync()
    {
        return await _context.Ingredients.ToListAsync();
    }

    public async Task<Ingredient> CreateAsync(Ingredient ingredient, CancellationToken cancellationToken = default)
    {
        if (await ExistsByIdAsync(ingredient.IngredientId, cancellationToken))
        {
            throw new EntityAlreadyExistsException("Ingredient", "ID", ingredient.IngredientId);
        }

        if (await ExistsByNameAsync(ingredient.Name, cancellationToken))
        {
            throw new EntityAlreadyExistsException("Ingredient", "name", ingredient.Name);
        }

        await _context.Ingredients.AddAsync(ingredient, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return ingredient;
    }

    public async Task UpdateAsync(Ingredient updatedIngredient, CancellationToken cancellationToken = default)
    {
        if (await ExistsByNameAsync(updatedIngredient.Name, cancellationToken))
        {
            throw new EntityAlreadyExistsException("Ingredient", "name", updatedIngredient.Name);
        }

        Ingredient ingredient = await GetByIdAsync(updatedIngredient.IngredientId, cancellationToken);
        ingredient.Name = updatedIngredient.Name;

        if (!(await _context.SaveChangesAsync(cancellationToken) > 0))
        {
            throw new ModifyFailedException("update", "Ingredient");
        }
    }

    public async Task DeleteByIdAsync(long ingredientId, CancellationToken cancellationToken = default)
    {
        Ingredient ingredient = await GetByIdAsync(ingredientId, cancellationToken);
        _context.Ingredients.Remove(ingredient);

        if (!(await _context.SaveChangesAsync(cancellationToken) > 0))
        {
            throw new ModifyFailedException("delete", "Ingredient");
        }
    }
}
