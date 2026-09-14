namespace SmartRecipeBook.Models;

/// <summary>
/// Рецепт: название, время приготовления, список нужных ингредиентов
/// (с граммовками/количеством) и пошаговая текстовая инструкция.
/// </summary>
public class Recipe
{
    public string Name { get; set; }
    public int CookingTimeMinutes { get; set; }
    public List<Ingredient> RequiredIngredients { get; set; }
    public List<RecipeStep> Steps { get; set; }

    public Recipe(string name, int cookingTimeMinutes)
    {
        Name = name;
        CookingTimeMinutes = cookingTimeMinutes;
        RequiredIngredients = new List<Ingredient>();
        Steps = new List<RecipeStep>();
    }

    public Recipe()
    {
        Name = string.Empty;
        RequiredIngredients = new List<Ingredient>();
        Steps = new List<RecipeStep>();
    }

    public void AddIngredient(string name, double amount, Unit unit)
    {
        RequiredIngredients.Add(new Ingredient(name, amount, unit));
    }

    public void AddStep(string description)
    {
        int order = Steps.Count + 1;
        Steps.Add(new RecipeStep(order, description));
    }

    public void Print()
    {
        Console.WriteLine($"\n=== {Name} ({CookingTimeMinutes} мин.) ===");
        Console.WriteLine("Ингредиенты:");
        foreach (var ingredient in RequiredIngredients)
        {
            Console.WriteLine($"  - {ingredient}");
        }

        if (Steps.Count > 0)
        {
            Console.WriteLine("Инструкция:");
            foreach (var step in Steps.OrderBy(s => s.Order))
            {
                Console.WriteLine($"  {step}");
            }
        }
    }
}
