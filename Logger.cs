using System;
using System.IO;

public static class Logger
{
    private static readonly string logFilePath = @"D:\Шарага\Курс_4\Истинный_Курсач\Project2\user_actions.log";

    public static void Log(string message)
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine($"{DateTime.Now}: {message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка записи лога: {ex.Message}");
        }
    }
}
