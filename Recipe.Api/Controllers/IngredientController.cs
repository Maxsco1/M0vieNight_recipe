using Microsoft.AspNetCore.Mvc;
using Recipe.Application;
using Recipe.Domain;

namespace Recipe.Api.Controllers;

[ApiController]
[Route("api/ingredients")]
public class IngredientController(IIngredientService ingredientService) : ControllerBase
{
    private readonly IIngredientService ingredientService = ingredientService;

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllIngredients()
    {
        return Ok(await ingredientService.GetAllAsync());
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetIngredient(long id)
    {
        return Ok(await ingredientService.GetByIdAsync(id));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateIngredient([FromBody] Ingredient ingredient)
    {
        var createdIngredient = await ingredientService.CreateAsync(ingredient);
        return Created($"api/ingredients/{createdIngredient.IngredientId}", createdIngredient);
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateIngredient([FromBody] Ingredient ingredient)
    {
        await ingredientService.UpdateAsync(ingredient);
        return Created($"api/ingredients/{ingredient.IngredientId}", ingredient);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteIngredient(long id)
    {
        await ingredientService.DeleteByIdAsync(id);
        return NoContent();
    }
}