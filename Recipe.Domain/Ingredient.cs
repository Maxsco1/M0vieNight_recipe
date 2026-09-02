namespace Recipe.Domain;

public class Ingredient(long ingredientId, string name)
{
    public long IngredientId { get; set; } = ingredientId;
    public string Name { get; set; } = name;

    public override bool Equals(object? obj)
    {
        if (obj is not Ingredient ingredient)
        {
            return false;
        }
        else
        {
            return IngredientId == ingredient.IngredientId && Name == ingredient.Name;
        }
    }

    public override int GetHashCode()
    {
        return IngredientId.GetHashCode();
    }
}
