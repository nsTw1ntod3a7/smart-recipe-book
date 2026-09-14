namespace SmartRecipeBook.Models;

/// <summary>
/// Единица измерения продукта.
/// </summary>
public enum Unit
{
    Gram,
    Piece
}

/// <summary>
/// Продукт: название, количество и единица измерения.
/// Используется и для склада (PantryManager), и для состава рецепта (Recipe).
/// </summary>
public class Ingredient
{
    public string Name { get; set; }
    public double Amount { get; set; }
    public Unit Unit { get; set; }

    public Ingredient(string name, double amount, Unit unit)
    {
        Name = name;
        Amount = amount;
        Unit = unit;
    }

    // Пустой конструктор нужен для десериализации из JSON.
    public Ingredient()
    {
        Name = string.Empty;
    }

    public override string ToString()
    {
        string unitLabel = Unit == Unit.Gram ? "г" : "шт";
        return $"{Name} — {Amount} {unitLabel}";
    }
}
