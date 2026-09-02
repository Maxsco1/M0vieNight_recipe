using Microsoft.AspNetCore.Mvc;
using Recipe.Application;
using Recipe.Domain;

namespace Recipe.Api.Controllers;

[ApiController]
[Route("api/recipes/{recipeId}/ingredients")]
public class RecipeIngredientController(IRecipeIngredientService recipeIngredientService) : ControllerBase
{
    private readonly IRecipeIngredientService recipeIngredientService = recipeIngredientService;

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecipeIngredients(long recipeId)
    {
        return Ok(await recipeIngredientService.GetAllByRecipeIdAsync(recipeId));
    }

    [HttpGet("{recipeIngredientId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRecipeIngredient(long recipeIngredientId)
    {
        return Ok(await recipeIngredientService.GetByIdAsync(recipeIngredientId));
    }

    [HttpPut("{recipeIngredientId}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateRecipeIngredient(long recipeId, long recipeIngredientId, [FromBody] RecipeIngredient recipeIngredient)
    {
        await recipeIngredientService.UpdateAsync(recipeIngredient);
        return Created($"api/recipes/{recipeId}/ingredients/{recipeIngredientId}", recipeIngredient);
    }

    [HttpDelete("{recipeIngredientId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteRecipeIngredient(long recipeIngredientId)
    {
        await recipeIngredientService.DeleteByIdAsync(recipeIngredientId);
        return NoContent();
    }
}
