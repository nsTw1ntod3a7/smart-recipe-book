using SmartRecipeBook.Models;

namespace SmartRecipeBook.Shopping;

/// <summary>
/// Модуль 4 (часть 1). Список покупок.
/// Формируется автоматически из недостающих ингредиентов выбранного рецепта.
/// </summary>
public class ShoppingList
{
    public List<Ingredient> Items { get; set; } = new();

    /// <summary>
    /// Построить список покупок из списка недостающих ингредиентов.
    /// </summary>
    public static ShoppingList FromMissingIngredients(List<Ingredient> missingIngredients)
    {
        return new ShoppingList
        {
            Items = missingIngredients
                .Select(i => new Ingredient(i.Name, i.Amount, i.Unit))
                .ToList()
        };
    }

    public void Print()
    {
        if (Items.Count == 0)
        {
            Console.WriteLine("Список покупок пуст — все продукты уже есть на складе.");
            return;
        }

        Console.WriteLine("\n--- Список покупок ---");
        foreach (var item in Items)
        {
            Console.WriteLine($"  - {item}");
        }
    }
}
