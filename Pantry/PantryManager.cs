using SmartRecipeBook.Models;

namespace SmartRecipeBook.Pantry;

/// <summary>
/// Модуль 1. Виртуальный склад / Холодильник.
/// Учёт остатков продуктов: добавление, списание, изменение остатка, просмотр.
/// </summary>
public class PantryManager
{
    private readonly List<Ingredient> _items;

    public PantryManager()
    {
        _items = new List<Ingredient>();
    }

    public IReadOnlyList<Ingredient> Items => _items;

    /// <summary>
    /// Загрузить состав склада (используется при чтении сохранённых данных).
    /// </summary>
    public void LoadItems(IEnumerable<Ingredient> items)
    {
        _items.Clear();
        _items.AddRange(items);
    }

    /// <summary>
    /// Найти продукт на складе по названию (без учёта регистра).
    /// </summary>
    public Ingredient? FindItem(string name)
    {
        return _items.FirstOrDefault(i =>
            string.Equals(i.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Добавить продукт. Если продукт с таким названием уже есть — количество суммируется.
    /// </summary>
    public void AddItem(string name, double amount, Unit unit)
    {
        var existing = FindItem(name);
        if (existing != null)
        {
            existing.Amount += amount;
        }
        else
        {
            _items.Add(new Ingredient(name, amount, unit));
        }
    }

    /// <summary>
    /// Изменить (перезаписать) остаток продукта на складе.
    /// </summary>
    public bool UpdateAmount(string name, double newAmount)
    {
        var existing = FindItem(name);
        if (existing == null) return false;

        existing.Amount = newAmount;
        return true;
    }

    /// <summary>
    /// Списать (использовать) продукт со склада.
    /// Возвращает false, если продукта нет или его не хватает.
    /// </summary>
    public bool RemoveItem(string name, double amount)
    {
        var existing = FindItem(name);
        if (existing == null || existing.Amount < amount) return false;

        existing.Amount -= amount;

        // Если продукт закончился полностью — убираем его со склада.
        if (existing.Amount <= 0)
        {
            _items.Remove(existing);
        }

        return true;
    }

    public void PrintAll()
    {
        if (_items.Count == 0)
        {
            Console.WriteLine("Склад пуст.");
            return;
        }

        Console.WriteLine("\n--- Склад ---");
        foreach (var item in _items)
        {
            Console.WriteLine($"  - {item}");
        }
    }
}
