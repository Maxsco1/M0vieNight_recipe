namespace Recipe.Domain;

public sealed class RecipeIngredientInput(string ingredientName, long ingredientAmount, IngredientUnit amountUnit)
{
    public string IngredientName { get; set; } = ingredientName;
    public long IngredientAmount { get; set; } = ingredientAmount;
    public IngredientUnit AmountUnit { get; set; } = amountUnit;
}