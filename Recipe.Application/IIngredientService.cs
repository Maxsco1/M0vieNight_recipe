using Recipe.Domain;

namespace Recipe.Application;

public interface IIngredientService
{
    Task<Ingredient> CreateAsync(Ingredient ingredient, CancellationToken cancellationToken = default);
    Task<bool> ExistsByIdAsync(long ingredientId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string ingredientName, CancellationToken cancellationToken = default);
    Task<List<Ingredient>> GetAllAsync();
    Task<Ingredient> GetByIdAsync(long ingredientId, CancellationToken cancellationToken = default);
    Task UpdateAsync(Ingredient ingredient, CancellationToken cancellationToken = default);
    Task DeleteByIdAsync(long ingredientId, CancellationToken cancellationToken = default);
}
