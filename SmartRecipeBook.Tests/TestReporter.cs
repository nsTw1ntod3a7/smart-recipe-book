namespace SmartRecipeBook.Tests;

/// <summary>
/// Простой тестовый драйвер без сторонних фреймворков.
/// Печатает [PASS]/[FAIL] по каждому сценарию и итоговую сводку.
/// </summary>
public static class TestReporter
{
    private static int _passed;
    private static int _failed;

    public static void Check(string testName, bool condition, string expected, string actual)
    {
        if (condition)
        {
            _passed++;
            Console.WriteLine($"[PASS] {testName}");
        }
        else
        {
            _failed++;
            Console.WriteLine($"[FAIL] {testName}");
            Console.WriteLine($"       Ожидалось: {expected}");
            Console.WriteLine($"       Получено:  {actual}");
        }
    }

    public static void PrintSummary()
    {
        Console.WriteLine();
        Console.WriteLine($"Итого: {_passed} PASS, {_failed} FAIL (всего {_passed + _failed})");
    }
}
