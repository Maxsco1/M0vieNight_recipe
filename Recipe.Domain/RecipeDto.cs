namespace Recipe.Domain;

public class RecipeDto(long recipeId, string title, string method, ICollection<RecipeIngredientInput> recipeIngredients, string authorName, long posterId)
{
    public long RecipeId { get; set; } = recipeId;
    public string Title { get; set; } = title;
    public string Method { get; set; } = method;
    public ICollection<RecipeIngredientInput> RecipeIngredients { get; set; } = recipeIngredients;
    public string AuthorName { get; set; } = authorName;
    public long PosterId { get; set; } = posterId;
}