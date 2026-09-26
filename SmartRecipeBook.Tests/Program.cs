using SmartRecipeBook.Matcher;
using SmartRecipeBook.Models;
using SmartRecipeBook.Pantry;
using SmartRecipeBook.Recipes;
using SmartRecipeBook.Shopping;
using SmartRecipeBook.Storage;
using SmartRecipeBook.Tests;

Console.WriteLine("=== SmartRecipeBook — тестовый драйвер (TestRunner) ===\n");

// ---------------------------------------------------------------------
// Модуль 1: PantryManager — happy path
// ---------------------------------------------------------------------
var pantry = new PantryManager();

pantry.AddItem("Молоко", 500, Unit.Gram);
var milk = pantry.FindItem("Молоко");
TestReporter.Check(
    "PantryManager.AddItem — добавление нового продукта",
    milk != null && milk.Amount == 500,
    "Молоко, 500 г",
    milk == null ? "продукт не найден" : $"{milk.Name}, {milk.Amount} г");

pantry.AddItem("Молоко", 250, Unit.Gram);
milk = pantry.FindItem("Молоко");
TestReporter.Check(
    "PantryManager.AddItem — суммирование при повторном добавлении",
    milk != null && milk.Amount == 750,
    "750 г",
    $"{milk?.Amount} г");

pantry.AddItem("Сахар", 100, Unit.Gram);
bool removedFully = pantry.RemoveItem("Сахар", 100);
var sugar = pantry.FindItem("Сахар");
TestReporter.Check(
    "PantryManager.RemoveItem — полное списание удаляет продукт со склада",
    removedFully && sugar == null,
    "true, продукт отсутствует в списке",
    $"{removedFully}, {(sugar == null ? "отсутствует" : "присутствует")}");

// ---------------------------------------------------------------------
// Модуль 1: PantryManager — негативные тесты (баги)
// ---------------------------------------------------------------------
double milkBefore = pantry.FindItem("Молоко")!.Amount;
bool removeNegativeResult = pantry.RemoveItem("Молоко", -100);
double milkAfter = pantry.FindItem("Молоко")!.Amount;
TestReporter.Check(
    "PantryManager.RemoveItem — списание отрицательного количества должно быть отклонено (BUG-01)",
    removeNegativeResult == false && milkAfter == milkBefore,
    $"false, остаток не меняется ({milkBefore} г)",
    $"{removeNegativeResult}, остаток стал {milkAfter} г");

pantry.AddItem("Мука", -50, Unit.Gram);
var flour = pantry.FindItem("Мука");
TestReporter.Check(
    "PantryManager.AddItem — добавление отрицательного количества должно отклоняться (BUG-02)",
    flour == null,
    "продукт не создан / отрицательное количество отклонено",
    flour == null ? "продукт не создан" : $"создан продукт '{flour.Name}' с количеством {flour.Amount} г");

// ---------------------------------------------------------------------
// Модуль 2: RecipeCatalog — happy path
// ---------------------------------------------------------------------
var catalog = new RecipeCatalog();
var pancakes = new Recipe("Блины", 20);
pancakes.AddIngredient("Молоко", 500, Unit.Gram);
pancakes.AddIngredient("Мука", 300, Unit.Gram);
pancakes.AddIngredient("Яйцо", 2, Unit.Piece);
catalog.AddRecipe(pancakes);

var foundRecipe = catalog.FindByName("блины"); // проверка поиска без учёта регистра
TestReporter.Check(
    "RecipeCatalog.FindByName — поиск рецепта без учёта регистра",
    foundRecipe != null && foundRecipe.Name == "Блины",
    "Блины",
    foundRecipe?.Name ?? "не найдено");

// ---------------------------------------------------------------------
// Модуль 3: RecipeMatcherService — happy path
// ---------------------------------------------------------------------
var matcher = new RecipeMatcherService();

var fullPantry = new PantryManager();
fullPantry.AddItem("Молоко", 500, Unit.Gram);
fullPantry.AddItem("Мука", 300, Unit.Gram);
fullPantry.AddItem("Яйцо", 2, Unit.Piece);

var availableMatch = matcher.Match(pancakes, fullPantry);
TestReporter.Check(
    "RecipeMatcherService.Match — рецепт доступен, если хватает всех продуктов",
    availableMatch.IsAvailable && availableMatch.MissingIngredients.Count == 0,
    "IsAvailable = true, недостающих ингредиентов нет",
    $"IsAvailable = {availableMatch.IsAvailable}, недостающих: {availableMatch.MissingIngredients.Count}");

var shortPantry = new PantryManager();
shortPantry.AddItem("Молоко", 100, Unit.Gram); // меньше, чем нужно; Мука и Яйцо отсутствуют

var missingMatch = matcher.Match(pancakes, shortPantry);
TestReporter.Check(
    "RecipeMatcherService.Match — рецепт недоступен, недостающие продукты перечислены",
    !missingMatch.IsAvailable && missingMatch.MissingIngredients.Count == 3,
    "IsAvailable = false, 3 недостающих ингредиента",
    $"IsAvailable = {missingMatch.IsAvailable}, недостающих: {missingMatch.MissingIngredients.Count}");

// ---------------------------------------------------------------------
// Модуль 3: RecipeMatcherService — негативный тест (баг)
// ---------------------------------------------------------------------
var corruptRecipe = new Recipe("Повреждённый рецепт", 10);
corruptRecipe.AddIngredient("Соль", -10, Unit.Gram); // некорректные (отрицательные) входные данные

var pantryWithSalt = new PantryManager();
pantryWithSalt.AddItem("Соль", 5, Unit.Gram); // продукт есть на складе в любом количестве

var corruptMatch = matcher.Match(corruptRecipe, pantryWithSalt);
TestReporter.Check(
    "RecipeMatcherService.Match — рецепт с отрицательным количеством ингредиента не должен считаться доступным (BUG-03)",
    corruptMatch.MissingIngredients.Count > 0 || !corruptMatch.IsAvailable,
    "ингредиент с отрицательным количеством должен попасть в недостающие / вызвать ошибку валидации",
    $"IsAvailable = {corruptMatch.IsAvailable}, недостающих: {corruptMatch.MissingIngredients.Count}");

// ---------------------------------------------------------------------
// Модуль 4: ShoppingList — happy path
// ---------------------------------------------------------------------
var shoppingList = ShoppingList.FromMissingIngredients(missingMatch.MissingIngredients);
TestReporter.Check(
    "ShoppingList.FromMissingIngredients — список покупок формируется из недостающих продуктов",
    shoppingList.Items.Count == missingMatch.MissingIngredients.Count,
    $"{missingMatch.MissingIngredients.Count} позиций",
    $"{shoppingList.Items.Count} позиций");

// ---------------------------------------------------------------------
// Модуль 4: RecipeStorage — happy path (сохранение/загрузка без потерь)
// ---------------------------------------------------------------------
string tempFile = Path.Combine(Path.GetTempPath(), $"smart_recipe_book_test_{Guid.NewGuid():N}.json");
try
{
    var storage = new RecipeStorage(tempFile);
    var dataToSave = new AppData
    {
        Pantry = new List<Ingredient> { new("Соль", 100, Unit.Gram) },
        Recipes = new List<Recipe> { pancakes }
    };
    storage.Save(dataToSave);
    var loaded = storage.Load();

    TestReporter.Check(
        "RecipeStorage.Save/Load — данные сохраняются и загружаются без потерь",
        loaded.Pantry.Count == 1 && loaded.Recipes.Count == 1 && loaded.Pantry[0].Name == "Соль",
        "1 продукт на складе (Соль), 1 рецепт",
        $"{loaded.Pantry.Count} продукт(ов), {loaded.Recipes.Count} рецепт(ов)");
}
finally
{
    if (File.Exists(tempFile)) File.Delete(tempFile);
}

TestReporter.PrintSummary();
