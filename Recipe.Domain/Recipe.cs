namespace Recipe.Domain;

public class Recipe
{
    public long RecipeId { get; set; }
    public string Title { get; set; } = null!;
    public ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();
    public string MethodText { get; set; } = null!;
    public long PosterId { get; set; }
    public string Author { get; set; } = null!;

    public Recipe()
    {

    }

    public Recipe(long recipeId, string title, ICollection<RecipeIngredient> recipeIngredients, string methodText, long posterId, string author)
    {
        RecipeId = recipeId;
        Title = title;
        RecipeIngredients = recipeIngredients;
        MethodText = methodText;
        PosterId = posterId;
        Author = author;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Recipe recipe)
        {
            return false;
        }
        else
        {
            return RecipeId == recipe.RecipeId;
        }
    }

    public override int GetHashCode()
    {
        return RecipeId.GetHashCode();
    }
}
