using SmartRecipeBook.Models;

namespace SmartRecipeBook.Recipes;

/// <summary>
/// Модуль 2. База рецептов.
/// Хранит список рецептов, позволяет добавлять и просматривать их.
/// </summary>
public class RecipeCatalog
{
    private readonly List<Recipe> _recipes;

    public RecipeCatalog()
    {
        _recipes = new List<Recipe>();
    }

    public IReadOnlyList<Recipe> Recipes => _recipes;

    public void LoadRecipes(IEnumerable<Recipe> recipes)
    {
        _recipes.Clear();
        _recipes.AddRange(recipes);
    }

    public void AddRecipe(Recipe recipe)
    {
        _recipes.Add(recipe);
    }

    public Recipe? FindByName(string name)
    {
        return _recipes.FirstOrDefault(r =>
            string.Equals(r.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    public void PrintAll()
    {
        if (_recipes.Count == 0)
        {
            Console.WriteLine("В базе рецептов пока ничего нет.");
            return;
        }

        Console.WriteLine("\n--- Рецепты ---");
        foreach (var recipe in _recipes)
        {
            Console.WriteLine($"  - {recipe.Name} ({recipe.CookingTimeMinutes} мин.)");
        }
    }
}
