namespace Recipe.Domain;

public class RecipeIngredient
{
    public long RecipeIngredientId { get; set; }
    public long RecipeId { get; set; }
    public long IngredientId { get; set; }
    public Ingredient Ingredient { get; set; } = null!;
    public float IngredientAmount { get; set; }
    public IngredientUnit AmountUnit { get; set; }

    public RecipeIngredient()
    {

    }

    public RecipeIngredient(long recipeIngredientId, Ingredient ingredient, float ingredientAmount, IngredientUnit ingredientUnit)
    {
        RecipeIngredientId = recipeIngredientId;
        Ingredient = ingredient;
        IngredientAmount = ingredientAmount;
        AmountUnit = ingredientUnit;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not RecipeIngredient recipeIngredient)
        {
            return false;
        }
        else
        {
            return RecipeIngredientId == recipeIngredient.RecipeIngredientId;
        }
    }

    public override int GetHashCode()
    {
        return RecipeIngredientId.GetHashCode();
    }
}
