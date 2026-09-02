using Microsoft.AspNetCore.Mvc;
using Recipe.Application;
using Recipe.Domain;

namespace Recipe.Api.Controllers;

[ApiController]
[Route("api/recipes")]
public class RecipeController(IRecipeService recipeService) : ControllerBase
{
    private readonly IRecipeService recipeService = recipeService;

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllRecipes(CancellationToken cancellationToken = default)
    {
        return Ok(await recipeService.GetAll(cancellationToken));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRecipe(long id)
    {
        return Ok(await recipeService.GetByIdAsync(id));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateRecipe([FromBody] RecipeDto recipe)
    {
        var createdRecipe = await recipeService.CreateAsync(recipe);
        return Created($"api/recipes/{createdRecipe.RecipeId}", createdRecipe);
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateRecipe([FromBody] RecipeDto recipe)
    {
        await recipeService.UpdateAsync(recipe);
        return Created($"api/recipes/{recipe.RecipeId}", recipe);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteRecipe(long id)
    {
        await recipeService.DeleteByIdAsync(id);
        return NoContent();
    }
}
