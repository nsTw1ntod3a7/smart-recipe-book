using SmartRecipeBook.Models;
using SmartRecipeBook.Pantry;

namespace SmartRecipeBook.Matcher;

/// <summary>
/// Результат сравнения одного рецепта со складом.
/// </summary>
public class MatchResult
{
    public Recipe Recipe { get; }
    public bool IsAvailable { get; }
    public List<Ingredient> MissingIngredients { get; }

    public MatchResult(Recipe recipe, bool isAvailable, List<Ingredient> missingIngredients)
    {
        Recipe = recipe;
        IsAvailable = isAvailable;
        MissingIngredients = missingIngredients;
    }
}

/// <summary>
/// Модуль 3. Движок подбора рецептов.
/// Сравнивает состав рецепта с продуктами на складе.
/// Рецепт считается доступным, если хватает 100% необходимых продуктов.
/// Если чего-то не хватает — возвращается список недостающих продуктов.
/// </summary>
public class RecipeMatcherService
{
    /// <summary>
    /// Сравнить один рецепт со складом.
    /// </summary>
    public MatchResult Match(Recipe recipe, PantryManager pantry)
    {
        var missing = new List<Ingredient>();

        foreach (var required in recipe.RequiredIngredients)
        {
            var inPantry = pantry.FindItem(required.Name);

            double availableAmount = inPantry?.Amount ?? 0;

            if (inPantry == null || inPantry.Unit != required.Unit || availableAmount < required.Amount)
            {
                double missingAmount = required.Amount - availableAmount;
                if (missingAmount < 0) missingAmount = required.Amount; // единицы измерения не совпали

                missing.Add(new Ingredient(required.Name, missingAmount, required.Unit));
            }
        }

        bool isAvailable = missing.Count == 0;
        return new MatchResult(recipe, isAvailable, missing);
    }

    /// <summary>
    /// Сравнить все рецепты из каталога со складом.
    /// </summary>
    public List<MatchResult> MatchAll(IEnumerable<Recipe> recipes, PantryManager pantry)
    {
        return recipes.Select(recipe => Match(recipe, pantry)).ToList();
    }

    /// <summary>
    /// Только те рецепты, которые можно приготовить прямо сейчас.
    /// </summary>
    public List<Recipe> GetAvailableRecipes(IEnumerable<Recipe> recipes, PantryManager pantry)
    {
        return MatchAll(recipes, pantry)
            .Where(result => result.IsAvailable)
            .Select(result => result.Recipe)
            .ToList();
    }
}
