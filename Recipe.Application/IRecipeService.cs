using Recipe.Domain;

namespace Recipe.Application;

public interface IRecipeService
{
    Task<Domain.Recipe> CreateAsync(RecipeDto recipe, CancellationToken cancellationToken = default);
    Task<bool> ExistsByIdAsync(long recipeId, CancellationToken cancellationToken = default);
    Task<Domain.Recipe> GetByIdAsync(long recipeId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Domain.Recipe>> GetAll(CancellationToken cancellationToken);
    Task UpdateAsync(RecipeDto updatedRecipe, CancellationToken cancellationToken = default);
    Task DeleteByIdAsync(long recipeId, CancellationToken cancellationToken = default);
}
