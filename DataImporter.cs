using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

public static class DataImporter
{
    public static void ImportFromCsv(string connectionString, string tableName, string csvFilePath)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            DataTable dataTable = new DataTable();

            try
            {
                // Чтение данных из CSV
                string[] csvLines = File.ReadAllLines(csvFilePath);
                if (csvLines.Length == 0)
                {
                    throw new Exception("CSV файл пуст.");
                }

                // Определение столбцов
                string[] headers = csvLines[0].Split(';').Select(h => h.Trim().Trim('"')).ToArray();
                foreach (string header in headers)
                {
                    dataTable.Columns.Add(header);
                }

                // Чтение строк данных
                for (int i = 1; i < csvLines.Length; i++)
                {
                    string[] fields = csvLines[i].Split(';').Select(f => f.Trim().Trim('"')).ToArray();
                    dataTable.Rows.Add(fields);
                }

                connection.Open();

                // Определение столбцов с автоинкрементом (IDENTITY)
                var identityColumns = GetIdentityColumns(connection, tableName);

                foreach (DataRow row in dataTable.Rows)
                {
                    // Исключаем столбцы IDENTITY из вставки
                    DataRow filteredRow = FilterRow(row, identityColumns);

                    if (!RecordExists(connection, tableName, filteredRow))
                    {
                        InsertRecord(connection, tableName, filteredRow, identityColumns);
                    }
                }

                Logger.Log($"Импорт данных в таблицу {tableName} из CSV успешно выполнен.");
            }
            catch (Exception ex)
            {
                Logger.Log($"Ошибка импорта данных в таблицу {tableName} из CSV: {ex.Message}");
            }
        }
    }

    private static string[] GetIdentityColumns(SqlConnection connection, string tableName)
    {
        string query = $@"
            SELECT COLUMN_NAME
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_NAME = @TableName AND COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + TABLE_NAME), COLUMN_NAME, 'IsIdentity') = 1";

        using (SqlCommand command = new SqlCommand(query, connection))
        {
            command.Parameters.AddWithValue("@TableName", tableName);
            using (SqlDataReader reader = command.ExecuteReader())
            {
                var identityColumns = new System.Collections.Generic.List<string>();
                while (reader.Read())
                {
                    identityColumns.Add(reader.GetString(0));
                }
                return identityColumns.ToArray();
            }
        }
    }

    private static DataRow FilterRow(DataRow row, string[] identityColumns)
    {
        DataRow filteredRow = row.Table.NewRow();

        foreach (DataColumn column in row.Table.Columns)
        {
            if (!identityColumns.Contains(column.ColumnName))
            {
                filteredRow[column.ColumnName] = row[column];
            }
        }

        return filteredRow;
    }

    private static bool RecordExists(SqlConnection connection, string tableName, DataRow row)
    {
        // Генерация запроса проверки по всем столбцам, кроме IDENTITY
        string whereClause = string.Join(" AND ",
            row.Table.Columns.Cast<DataColumn>()
               .Where(c => row[c.ColumnName] != DBNull.Value)
               .Select(c => $"{c.ColumnName} = @{c.ColumnName}"));

        if (string.IsNullOrWhiteSpace(whereClause))
        {
            throw new Exception("Не удалось построить условие проверки существования записи.");
        }

        string query = $"SELECT COUNT(*) FROM {tableName} WHERE {whereClause}";
        using (SqlCommand command = new SqlCommand(query, connection))
        {
            foreach (DataColumn column in row.Table.Columns)
            {
                if (row[column.ColumnName] != DBNull.Value)
                {
                    command.Parameters.AddWithValue($"@{column.ColumnName}", row[column.ColumnName]);
                }
            }

            int count = (int)command.ExecuteScalar();
            return count > 0;
        }
    }

    private static void InsertRecord(SqlConnection connection, string tableName, DataRow row, string[] identityColumns)
    {
        // Исключаем столбцы IDENTITY из списка для вставки
        var insertColumns = row.Table.Columns.Cast<DataColumn>()
            .Where(c => !identityColumns.Contains(c.ColumnName))
            .ToArray();

        string columns = string.Join(", ", insertColumns.Select(c => c.ColumnName));
        string values = string.Join(", ", insertColumns.Select(c => "@" + c.ColumnName));
        string query = $"INSERT INTO {tableName} ({columns}) VALUES ({values})";

        using (SqlCommand command = new SqlCommand(query, connection))
        {
            foreach (var column in insertColumns)
            {
                command.Parameters.AddWithValue("@" + column.ColumnName, row[column.ColumnName]);
            }

            try
            {
                command.ExecuteNonQuery();
            }
            catch (SqlException ex) when (ex.Number == 2627) // Уникальное ограничение
            {
                Logger.Log($"Ошибка уникальности: Запись с ключевым значением уже существует.");
            }
            catch (Exception ex)
            {
                Logger.Log($"Ошибка вставки записи: {ex.Message}");
            }
        }
    }
}
