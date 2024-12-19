using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;

public static class DataExporter
{
    public static void ExportToCsv(string connectionString, string tableName, string csvFilePath)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string query = $"SELECT * FROM {tableName}";
            SqlCommand command = new SqlCommand(query, connection);
            DataTable dataTable = new DataTable();

            try
            {
                connection.Open();
                SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
                dataAdapter.Fill(dataTable);

                StringBuilder csvData = new StringBuilder();

                // Добавляем заголовки
                string[] columnNames = dataTable.Columns.Cast<DataColumn>()
                                       .Select(column => QuoteValue(column.ColumnName))
                                       .ToArray();
                csvData.AppendLine(string.Join(";", columnNames)); // Используем ';' вместо ','

                // Добавляем строки данных
                foreach (DataRow row in dataTable.Rows)
                {
                    string[] fields = row.ItemArray.Select(field => QuoteValue(field.ToString()))
                                      .ToArray();
                    csvData.AppendLine(string.Join(";", fields)); // Используем ';' вместо ','
                }

                File.WriteAllText(csvFilePath, csvData.ToString());
                Logger.Log($"Экспорт данных из таблицы {tableName} в CSV успешно выполнен.");
            }
            catch (Exception ex)
            {
                Logger.Log($"Ошибка экспорта данных из таблицы {tableName} в CSV: {ex.Message}");
            }
        }
    }

    private static string QuoteValue(string value)
    {
        return $"\"{value.Replace("\"", "\"\"")}\""; // Экранируем кавычки внутри значений
    }
}

