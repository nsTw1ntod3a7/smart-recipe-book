using System.Text.Json;
using SmartRecipeBook.Models;

namespace SmartRecipeBook.Storage;

/// <summary>
/// Снимок всех данных приложения, который сохраняется в файл.
/// </summary>
public class AppData
{
    public List<Ingredient> Pantry { get; set; } = new();
    public List<Recipe> Recipes { get; set; } = new();
}

/// <summary>
/// Модуль 4 (часть 2). Хранилище.
/// Сохраняет и загружает базу рецептов и склад в файл (JSON), чтобы данные
/// не терялись между запусками приложения.
/// </summary>
public class RecipeStorage
{
    private readonly string _filePath;
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    public RecipeStorage(string filePath = "data.json")
    {
        _filePath = filePath;
    }

    public void Save(AppData data)
    {
        string json = JsonSerializer.Serialize(data, SerializerOptions);
        File.WriteAllText(_filePath, json);
    }

    /// <summary>
    /// Загрузить данные из файла. Если файла ещё нет — возвращает пустые данные
    /// (это нормально для первого запуска приложения).
    /// </summary>
    public AppData Load()
    {
        if (!File.Exists(_filePath))
        {
            return new AppData();
        }

        string json = File.ReadAllText(_filePath);
        if (string.IsNullOrWhiteSpace(json))
        {
            return new AppData();
        }

        return JsonSerializer.Deserialize<AppData>(json) ?? new AppData();
    }
}
