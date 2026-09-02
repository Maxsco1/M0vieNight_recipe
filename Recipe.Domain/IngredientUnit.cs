namespace Recipe.Domain;

public enum IngredientUnit
{
    Grams,
    Kilograms,
    Milliliters,
    Liters,
    Teaspoons,
    Tablespoons,
    Whole,
    Cloves,
    KnifesEdges
}

static class IngredientUnitMethods
{
    public static string IngredientUnitToString(IngredientUnit ingredientUnit)
    {
        return nameof(ingredientUnit);
    }

    public static string? IngredientUnitAbbreviation(IngredientUnit ingredientUnit)
    {
        return ingredientUnit switch
        {
            IngredientUnit.Grams => "g",
            IngredientUnit.Kilograms => "kg",
            IngredientUnit.Milliliters => "mL",
            IngredientUnit.Liters => "L",
            IngredientUnit.Teaspoons => "tsp.",
            IngredientUnit.Tablespoons => "tbsp.",
            _ => null,
        };
    }
}