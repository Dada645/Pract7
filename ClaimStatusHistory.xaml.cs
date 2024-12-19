using Curs.CursDataSetTableAdapters;
using System;
using System.Data;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Curs
{
    public partial class ClaimStatusHistory : Page
    {
        claim_status_historyTableAdapter claimStatusHistory = new claim_status_historyTableAdapter();
        claimsTableAdapter claims = new claimsTableAdapter();

        Validator validator;

        public ClaimStatusHistory()
        {
            InitializeComponent();
            ClaimComboBox.ItemsSource = claims.GetData();
            ClaimComboBox.DisplayMemberPath = "incident_date";
            ClaimComboBox.SelectedValuePath = "claim_id";
            LoadData();
        }

        private void LoadData()
        {
            DataTable claimStatusHistoryTable = claimStatusHistory.GetData();
            DataTable claimsTable = claims.GetData();

            DataTable mergedTable = claimStatusHistoryTable.Clone();
            mergedTable.Columns.Add("incident_date", typeof(DateTime));

            foreach (DataRow historyRow in claimStatusHistoryTable.Rows)
            {
                DataRow newRow = mergedTable.NewRow();
                newRow.ItemArray = historyRow.ItemArray.Clone() as object[];

                DataRow claimRow = claimsTable.Rows.Find(historyRow["claim_id"]);
                if (claimRow != null)
                {
                    newRow["incident_date"] = claimRow["incident_date"];
                }

                mergedTable.Rows.Add(newRow);
            }

            DataView view = mergedTable.DefaultView;
            view.Sort = "history_id ASC";
            ClaimStatusHistoryDataGrid.ItemsSource = view;

            validator = new Validator(claimsTable, claimStatusHistoryTable);
        }

        private void EditClaimStatusHistory_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            DataRowView rowView = button.DataContext as DataRowView;

            if (rowView != null)
            {
                int historyId = (int)rowView["history_id"];

                if (validator.ValidateComboBoxSelection((int?)ClaimComboBox.SelectedValue) &&
                    validator.ValidateDate(ChangeDatePicker.SelectedDate) &&
                    validator.ValidateComboBoxSelection(StatusComboBox.SelectedValue.ToString()))
                {
                    claimStatusHistory.UpdateQuery(
                        (int)ClaimComboBox.SelectedValue,
                        StatusComboBox.SelectedValue.ToString(),
                        ChangeDatePicker.SelectedDate.Value,
                        historyId
                    );
                    LoadData();
                    Logger.Log($"История статуса претензии с ID {historyId} успешно обновлена. Претензия ID: {(int)ClaimComboBox.SelectedValue}, Статус: {StatusComboBox.SelectedValue}");
                }
                else
                {
                    CustomMessageBox.Show("Неверные данные");
                    Logger.Log($"Ошибка обновления истории статуса претензии с ID {historyId}. Неверные данные.");
                }
            }

            ClearFields();
        }

        private void DeleteClaimStatusHistory_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            DataRowView rowView = button.DataContext as DataRowView;

            if (rowView != null)
            {
                int historyId = (int)rowView["history_id"];

                try
                {
                    claimStatusHistory.DeleteQuery(historyId);
                    LoadData();
                    Logger.Log($"История статуса претензии с ID {historyId} успешно удалена.");
                }
                catch (Exception ex)
                {
                    Logger.Log($"Ошибка при удалении истории статуса претензии с ID {historyId}: {ex.Message}");
                }
            }

            ClearFields();
        }

        private void AddClaimStatusHistory_Click(object sender, RoutedEventArgs e)
        {
            if (validator.ValidateComboBoxSelection((int?)ClaimComboBox.SelectedValue) &&
                validator.ValidateDate(ChangeDatePicker.SelectedDate) &&
                validator.ValidateComboBoxSelection(StatusComboBox.SelectedValue.ToString()))
            {
                claimStatusHistory.InsertQuery(
                    (int)ClaimComboBox.SelectedValue,
                    StatusComboBox.SelectedValue.ToString(),
                    ChangeDatePicker.SelectedDate.Value
                );
                LoadData();
                Logger.Log($"История статуса претензии успешно добавлена для претензии с ID {(int)ClaimComboBox.SelectedValue}. Статус: {StatusComboBox.SelectedValue}");
            }
            else
            {
                CustomMessageBox.Show("Неверные данные");
                Logger.Log($"Ошибка добавления истории статуса претензии для претензии с ID {(int)ClaimComboBox.SelectedValue}. Неверные данные.");
            }

            ClearFields();
        }

        private void ClearFields()
        {
            ClaimComboBox.SelectedValue = -1;
            ChangeDatePicker.SelectedDate = null;
            StatusComboBox.SelectedIndex = -1;
            ClaimStatusHistoryDataGrid.SelectedIndex = -1;
        }

        private void ClaimStatusHistoryDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ClaimStatusHistoryDataGrid.SelectedItem != null)
            {
                DataRowView rowView = ClaimStatusHistoryDataGrid.SelectedItem as DataRowView;
                if (rowView != null)
                {
                    ClaimComboBox.SelectedValue = rowView["claim_id"];
                    ChangeDatePicker.SelectedDate = (DateTime?)rowView["change_date"];
                    StatusComboBox.SelectedValue = rowView["status"].ToString();
                }
            }
        }
        private async void ExportCsv_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = "Server=GG;Database=Curs;Integrated Security=True;";
            string tableName = "claim_status_history";
            string csvFilePath = $@"D:\Шарага\Курс_4\Истинный_Курсач\Project2\{tableName}.csv";
            await Task.Run(() => DataExporter.ExportToCsv(connectionString, tableName, csvFilePath));
        }

        private async void ImportCsv_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = "Server=GG;Database=Curs;Integrated Security=True;";
            string tableName = "claim_status_history";
            string csvFilePath = $@"D:\Шарага\Курс_4\Истинный_Курсач\Project2\{tableName}.csv";
            await Task.Run(() => DataImporter.ImportFromCsv(connectionString, tableName, csvFilePath));

            LoadData();
        }
    }
}
