using Recipe.Domain;

namespace Recipe.Application;

public interface IRecipeIngredientService
{
    Task<bool> ExistsByIdAsync(long recipeIngredientId, CancellationToken cancellationToken = default);
    Task<RecipeIngredient> GetByIdAsync(long recipeIngredientId, CancellationToken cancellationToken = default);
    Task UpdateAsync(RecipeIngredient updatedRecipeIngredient, CancellationToken cancellationToken = default);
    Task DeleteByIdAsync(long recipeIngredientId, CancellationToken cancellationToken = default);
    Task<List<RecipeIngredient>> GetAllByRecipeIdAsync(long recipeId, CancellationToken cancellationToken = default);
}
