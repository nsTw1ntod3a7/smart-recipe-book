using SmartRecipeBook.Matcher;
using SmartRecipeBook.Models;
using SmartRecipeBook.Pantry;
using SmartRecipeBook.Recipes;
using SmartRecipeBook.Shopping;
using SmartRecipeBook.Storage;

var pantry = new PantryManager();
var catalog = new RecipeCatalog();
var matcher = new RecipeMatcherService();
var storage = new RecipeStorage("data.json");

// Загружаем сохранённые данные при старте (если файла ещё нет — стартуем с пустого состояния).
var savedData = storage.Load();
pantry.LoadItems(savedData.Pantry);
catalog.LoadRecipes(savedData.Recipes);

bool running = true;
while (running)
{
    PrintMenu();
    string? choice = Console.ReadLine();

    switch (choice)
    {
        case "1":
            pantry.PrintAll();
            break;
        case "2":
            AddPantryItem();
            break;
        case "3":
            UpdatePantryItem();
            break;
        case "4":
            RemovePantryItem();
            break;
        case "5":
            AddRecipe();
            break;
        case "6":
            catalog.PrintAll();
            break;
        case "7":
            ShowAvailableRecipes();
            break;
        case "8":
            ShowMissingIngredients();
            break;
        case "9":
            BuildShoppingList();
            break;
        case "10":
            SaveData();
            break;
        case "0":
            SaveData();
            running = false;
            break;
        default:
            Console.WriteLine("Неизвестная команда, попробуйте ещё раз.");
            break;
    }
}

void PrintMenu()
{
    Console.WriteLine("\n========== Smart Recipe Book ==========");
    Console.WriteLine("1  - Показать склад");
    Console.WriteLine("2  - Добавить продукт на склад");
    Console.WriteLine("3  - Изменить количество продукта");
    Console.WriteLine("4  - Списать продукт со склада");
    Console.WriteLine("5  - Добавить рецепт");
    Console.WriteLine("6  - Показать все рецепты");
    Console.WriteLine("7  - Показать рецепты, доступные прямо сейчас");
    Console.WriteLine("8  - Показать недостающие ингредиенты для рецепта");
    Console.WriteLine("9  - Сформировать список покупок для рецепта");
    Console.WriteLine("10 - Сохранить данные");
    Console.WriteLine("0  - Сохранить и выйти");
    Console.Write("Выберите пункт: ");
}

void SaveData()
{
    var data = new AppData
    {
        Pantry = pantry.Items.ToList(),
        Recipes = catalog.Recipes.ToList()
    };
    storage.Save(data);
    Console.WriteLine("Данные сохранены.");
}

Unit? ReadUnit()
{
    Console.Write("Единица измерения (1 - граммы, 2 - штуки): ");
    string? input = Console.ReadLine();
    return input switch
    {
        "1" => Unit.Gram,
        "2" => Unit.Piece,
        _ => null
    };
}

void AddPantryItem()
{
    Console.Write("Название продукта: ");
    string name = Console.ReadLine() ?? "";

    Console.Write("Количество: ");
    if (!double.TryParse(Console.ReadLine(), out double amount))
    {
        Console.WriteLine("Некорректное количество.");
        return;
    }

    var unit = ReadUnit();
    if (unit == null)
    {
        Console.WriteLine("Некорректная единица измерения.");
        return;
    }

    pantry.AddItem(name, amount, unit.Value);
    Console.WriteLine($"Добавлено: {name} — {amount}");
}

void UpdatePantryItem()
{
    Console.Write("Название продукта: ");
    string name = Console.ReadLine() ?? "";

    Console.Write("Новое количество: ");
    if (!double.TryParse(Console.ReadLine(), out double amount))
    {
        Console.WriteLine("Некорректное количество.");
        return;
    }

    bool success = pantry.UpdateAmount(name, amount);
    Console.WriteLine(success
        ? "Количество обновлено."
        : "Продукт с таким названием не найден на складе.");
}

void RemovePantryItem()
{
    Console.Write("Название продукта: ");
    string name = Console.ReadLine() ?? "";

    Console.Write("Сколько списать: ");
    if (!double.TryParse(Console.ReadLine(), out double amount))
    {
        Console.WriteLine("Некорректное количество.");
        return;
    }

    bool success = pantry.RemoveItem(name, amount);
    Console.WriteLine(success
        ? "Продукт списан."
        : "Не удалось списать: продукта нет или его недостаточно.");
}

void AddRecipe()
{
    Console.Write("Название рецепта: ");
    string name = Console.ReadLine() ?? "";

    Console.Write("Время приготовления (мин.): ");
    int.TryParse(Console.ReadLine(), out int cookingTime);

    var recipe = new Recipe(name, cookingTime);

    Console.WriteLine("Добавление ингредиентов (пустое название — закончить):");
    while (true)
    {
        Console.Write("  Название ингредиента: ");
        string ingredientName = Console.ReadLine() ?? "";
        if (string.IsNullOrWhiteSpace(ingredientName)) break;

        Console.Write("  Количество: ");
        if (!double.TryParse(Console.ReadLine(), out double amount))
        {
            Console.WriteLine("  Некорректное количество, ингредиент пропущен.");
            continue;
        }

        var unit = ReadUnit();
        if (unit == null)
        {
            Console.WriteLine("  Некорректная единица измерения, ингредиент пропущен.");
            continue;
        }

        recipe.AddIngredient(ingredientName, amount, unit.Value);
    }

    Console.WriteLine("Добавление шагов инструкции (пустая строка — закончить):");
    while (true)
    {
        Console.Write("  Шаг: ");
        string step = Console.ReadLine() ?? "";
        if (string.IsNullOrWhiteSpace(step)) break;
        recipe.AddStep(step);
    }

    catalog.AddRecipe(recipe);
    Console.WriteLine($"Рецепт \"{name}\" добавлен.");
}

void ShowAvailableRecipes()
{
    var available = matcher.GetAvailableRecipes(catalog.Recipes, pantry);

    if (available.Count == 0)
    {
        Console.WriteLine("Сейчас нет рецептов, которые можно приготовить полностью из того, что есть на складе.");
        return;
    }

    Console.WriteLine("\n--- Можно приготовить прямо сейчас ---");
    foreach (var recipe in available)
    {
        recipe.Print();
    }
}

void ShowMissingIngredients()
{
    Console.Write("Название рецепта: ");
    string name = Console.ReadLine() ?? "";

    var recipe = catalog.FindByName(name);
    if (recipe == null)
    {
        Console.WriteLine("Рецепт не найден.");
        return;
    }

    var result = matcher.Match(recipe, pantry);
    if (result.IsAvailable)
    {
        Console.WriteLine("Все необходимые продукты есть на складе — недостающих нет.");
        return;
    }

    Console.WriteLine("\n--- Недостающие продукты ---");
    foreach (var missing in result.MissingIngredients)
    {
        Console.WriteLine($"  - {missing}");
    }
}

void BuildShoppingList()
{
    Console.Write("Название рецепта: ");
    string name = Console.ReadLine() ?? "";

    var recipe = catalog.FindByName(name);
    if (recipe == null)
    {
        Console.WriteLine("Рецепт не найден.");
        return;
    }

    var result = matcher.Match(recipe, pantry);
    var shoppingList = ShoppingList.FromMissingIngredients(result.MissingIngredients);
    shoppingList.Print();
}
