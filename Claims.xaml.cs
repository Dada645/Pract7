using Curs.CursDataSetTableAdapters;
using System;
using System.Data;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Curs
{
    public partial class Claims : Page
    {
        claimsTableAdapter claims = new claimsTableAdapter();
        policiesTableAdapter policies = new policiesTableAdapter();

        Validator validator;

        public Claims()
        {
            InitializeComponent();
            PolicyComboBox.ItemsSource = policies.GetData();
            PolicyComboBox.DisplayMemberPath = "policy_number";
            PolicyComboBox.SelectedValuePath = "policy_id";
            LoadData();
        }

        private void LoadData()
        {
            DataTable claimsTable = claims.GetData();
            DataTable policiesTable = policies.GetData();

            DataTable mergedTable = claimsTable.Clone();
            mergedTable.Columns.Add("policy_number", typeof(string));

            foreach (DataRow claimRow in claimsTable.Rows)
            {
                DataRow newRow = mergedTable.NewRow();
                newRow.ItemArray = claimRow.ItemArray.Clone() as object[];

                DataRow policyRow = policiesTable.Rows.Find(claimRow["policy_id"]);
                if (policyRow != null)
                {
                    newRow["policy_number"] = policyRow["policy_number"];
                }

                mergedTable.Rows.Add(newRow);
            }

            DataView view = mergedTable.DefaultView;
            view.Sort = "claim_id ASC";
            ClaimsDataGrid.ItemsSource = view;

            validator = new Validator(policiesTable, claimsTable);
        }

        private void EditClaim_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            DataRowView rowView = button.DataContext as DataRowView;

            if (rowView != null)
            {
                int claimId = (int)rowView["claim_id"];

                if (validator.ValidateComboBoxSelection((int?)PolicyComboBox.SelectedValue) &&
                    validator.ValidateDate(IncidentDatePicker.SelectedDate) &&
                    validator.ValidateAmount(Convert.ToDecimal(ClaimAmountTextBox.Text)))
                {
                    claims.UpdateQuery((int)PolicyComboBox.SelectedValue, IncidentDatePicker.SelectedDate.Value.ToString(), Convert.ToDecimal(ClaimAmountTextBox.Text), DescriptionTextBox.Text, claimId);
                    LoadData();
                    Logger.Log($"Претензия с ID {claimId} успешно обновлена. Полис ID: {(int)PolicyComboBox.SelectedValue}, Сумма: {ClaimAmountTextBox.Text}");
                }
                else
                {
                    CustomMessageBox.Show("Неверные данные");
                    Logger.Log($"Ошибка обновления претензии с ID {claimId}. Неверные данные.");
                }
            }

            ClearFields();
        }

        private void DeleteClaim_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            DataRowView rowView = button.DataContext as DataRowView;

            if (rowView != null)
            {
                int claimId = (int)rowView["claim_id"];

                try
                {
                    claims.DeleteQuery(claimId);
                    LoadData();
                    Logger.Log($"Претензия с ID {claimId} успешно удалена.");
                }
                catch (Exception ex)
                {
                    Logger.Log($"Ошибка при удалении претензии с ID {claimId}: {ex.Message}");
                }
            }

            ClearFields();
        }

        private void AddClaim_Click(object sender, RoutedEventArgs e)
        {
            if (validator.ValidateComboBoxSelection((int?)PolicyComboBox.SelectedValue) &&
                validator.ValidateDate(IncidentDatePicker.SelectedDate) &&
                validator.ValidateAmount(Convert.ToDecimal(ClaimAmountTextBox.Text)))
            {
                claims.InsertQuery((int)PolicyComboBox.SelectedValue, IncidentDatePicker.SelectedDate.Value.ToString(), Convert.ToDecimal(ClaimAmountTextBox.Text), DescriptionTextBox.Text);
                LoadData();
                Logger.Log($"Претензия успешно добавлена для полиса с ID {(int)PolicyComboBox.SelectedValue}. Сумма: {ClaimAmountTextBox.Text}");
            }
            else
            {
                CustomMessageBox.Show("Неверные данные");
                Logger.Log($"Ошибка добавления претензии для полиса с ID {(int)PolicyComboBox.SelectedValue}. Неверные данные.");
            }

            ClearFields();
        }

        private void ClearFields()
        {
            PolicyComboBox.SelectedValue = -1;
            IncidentDatePicker.SelectedDate = null;
            ClaimAmountTextBox.Clear();
            DescriptionTextBox.Clear();
            StatusComboBox.SelectedIndex = -1;
            ClaimsDataGrid.SelectedIndex = -1;
        }

        private void ClaimsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ClaimsDataGrid.SelectedItem != null)
            {
                DataRowView rowView = ClaimsDataGrid.SelectedItem as DataRowView;
                if (rowView != null)
                {
                    PolicyComboBox.SelectedValue = rowView["policy_id"];
                    IncidentDatePicker.SelectedDate = (DateTime?)rowView["incident_date"];
                    ClaimAmountTextBox.Text = rowView["claim_amount"].ToString();
                    DescriptionTextBox.Text = rowView["description"].ToString();
                    StatusComboBox.SelectedValue = rowView["status"].ToString();
                }
            }
        }

        private void ClaimAmountTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(e.Text, "^[0-9]+$");
        }

        private async void ExportCsv_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = "Server=GG;Database=Curs;Integrated Security=True;";
            string tableName = "claims";
            string csvFilePath = $@"D:\Шарага\Курс_4\Истинный_Курсач\Project2\{tableName}.csv";
            await Task.Run(() => DataExporter.ExportToCsv(connectionString, tableName, csvFilePath));
        }

        private async void ImportCsv_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = "Server=GG;Database=Curs;Integrated Security=True;";
            string tableName = "claims";
            string csvFilePath = $@"D:\Шарага\Курс_4\Истинный_Курсач\Project2\{tableName}.csv";
            await Task.Run(() => DataImporter.ImportFromCsv(connectionString, tableName, csvFilePath));

            LoadData();
        }
    }
}
