using Microsoft.EntityFrameworkCore;
using Recipe.Domain.Exceptions;
using Recipe.Domain;
using Recipe.Infrastructure;

namespace Recipe.Application;

public class RecipeIngredientService(RecipeDbContext context) : IRecipeIngredientService
{
    private readonly RecipeDbContext _context = context;

    public async Task DeleteByIdAsync(long recipeIngredientId, CancellationToken cancellationToken = default)
    {
        RecipeIngredient recipeIngredient = await GetByIdAsync(recipeIngredientId, cancellationToken);
        _context.RecipeIngredients.Remove(recipeIngredient);

        if (!(await _context.SaveChangesAsync(cancellationToken) > 0))
        {
            throw new ModifyFailedException("delete", "RecipeIngredient");
        }
    }

    public Task<bool> ExistsByIdAsync(long recipeIngredientId, CancellationToken cancellationToken = default)
    {
        return _context.RecipeIngredients.AnyAsync(ri => ri.RecipeIngredientId == recipeIngredientId, cancellationToken);
    }

    public Task<List<RecipeIngredient>> GetAllByRecipeIdAsync(long recipeId, CancellationToken cancellationToken = default)
    {
        return _context.RecipeIngredients
            .Where(ri => ri.RecipeId == recipeId)
            .Include(ri => ri.Ingredient)
            .ToListAsync(cancellationToken);
    }

    public async Task<RecipeIngredient> GetByIdAsync(long recipeIngredientId, CancellationToken cancellationToken = default)
    {
        return await _context.RecipeIngredients
            .Include(ri => ri.Ingredient)
            .FirstOrDefaultAsync(ri => ri.RecipeIngredientId == recipeIngredientId, cancellationToken)
            ?? throw new EntityDoesNotExistException("RecipeIngredient", "ID", recipeIngredientId);
    }

    public async Task UpdateAsync(RecipeIngredient updatedRecipeIngredient, CancellationToken cancellationToken = default)
    {
        RecipeIngredient recipeIngredient = await GetByIdAsync(updatedRecipeIngredient.RecipeIngredientId, cancellationToken);
        recipeIngredient.Ingredient = updatedRecipeIngredient.Ingredient;
        recipeIngredient.IngredientAmount = updatedRecipeIngredient.IngredientAmount;
        recipeIngredient.AmountUnit = updatedRecipeIngredient.AmountUnit;

        if (!(await _context.SaveChangesAsync(cancellationToken) > 0))
        {
            throw new ModifyFailedException("update", "RecipeIngredient");
        }
    }
}
