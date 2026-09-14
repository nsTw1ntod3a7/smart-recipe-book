namespace SmartRecipeBook.Models;

/// <summary>
/// Один шаг текстовой инструкции приготовления.
/// </summary>
public class RecipeStep
{
    public int Order { get; set; }
    public string Description { get; set; }

    public RecipeStep(int order, string description)
    {
        Order = order;
        Description = description;
    }

    public RecipeStep()
    {
        Description = string.Empty;
    }

    public override string ToString() => $"{Order}. {Description}";
}
