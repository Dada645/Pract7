using Curs.CursDataSetTableAdapters;
using System;
using System.Data;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Curs
{
    public partial class Policies : Page
    {
        accountsTableAdapter accounts = new accountsTableAdapter();
        employeesTableAdapter employees = new employeesTableAdapter();
        clientsTableAdapter clients = new clientsTableAdapter();
        rolesTableAdapter roles = new rolesTableAdapter();
        policiesTableAdapter policies = new policiesTableAdapter();
        Validator validator;

        public Policies()
        {
            InitializeComponent();
            ClientComboBox.ItemsSource = clients.GetData();
            ClientComboBox.DisplayMemberPath = "email";
            ClientComboBox.SelectedValuePath = "client_id";
            LoadData();
        }

        private void LoadData()
        {
            DataTable policiesTable = policies.GetData();
            DataTable clientsTable = clients.GetData();
            DataTable accountsTable = accounts.GetData();
            DataTable employeesTable = employees.GetData();
            DataTable rolesTable = roles.GetData();

            DataTable mergedTable = policiesTable.Clone();
            mergedTable.Columns.Add("email", typeof(string));

            foreach (DataRow policyRow in policiesTable.Rows)
            {
                DataRow newRow = mergedTable.NewRow();
                newRow.ItemArray = policyRow.ItemArray.Clone() as object[];

                DataRow clientRow = clientsTable.Rows.Find(policyRow["client_id"]);
                if (clientRow != null)
                {
                    newRow["email"] = clientRow["email"];
                }

                mergedTable.Rows.Add(newRow);
            }

            DataView view = mergedTable.DefaultView;
            view.Sort = "policy_id ASC";
            PoliciesDataGrid.ItemsSource = view;

            validator = new Validator(rolesTable, accountsTable, employeesTable, clientsTable, policiesTable);
        }

        private void EditPolicy_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            DataRowView rowView = button.DataContext as DataRowView;

            if (rowView != null)
            {
                int policyId = (int)rowView["policy_id"];
                string policyNumber = rowView["policy_number"].ToString(); // Сохранение текущего номера полиса

                if (validator.ValidateClientId(ClientComboBox.SelectedValue, policyId) &&
                     validator.ValidateDate(StartDate.SelectedDate) && validator.ValidateDate(EndDate.SelectedDate) &&
                     validator.ValidatePremiumAmount(Convert.ToDecimal(PremiumAmountTextBox.Text)) && validator.ValidateCoverageAmount(Convert.ToDecimal(CoverageAmountTextBox.Text)))
                {
                    policies.UpdateQuery(policyNumber, (int)ClientComboBox.SelectedValue, StartDate.SelectedDate.Value.ToString("yyyy-MM-dd"),
                                          EndDate.SelectedDate.Value.ToString("yyyy-MM-dd"), Convert.ToDecimal(PremiumAmountTextBox.Text), Convert.ToDecimal(CoverageAmountTextBox.Text), policyId);
                    LoadData();
                    Logger.Log($"Полис с ID {policyId} успешно обновлен. Номер полиса: {policyNumber}, Клиент ID: {(int)ClientComboBox.SelectedValue}");
                }
                else
                {
                    CustomMessageBox.Show("Неверные данные полиса.");
                    Logger.Log($"Ошибка обновления полиса с ID {policyId}. Неверные данные.");
                }
            }

            ClearFields();
        }

        private void DeletePolicy_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            DataRowView rowView = button.DataContext as DataRowView;

            if (rowView != null)
            {
                int policyId = (int)rowView["policy_id"];

                try
                {
                    policies.DeleteQuery(policyId);
                    LoadData();
                    Logger.Log($"Полис с ID {policyId} успешно удален.");
                }
                catch (Exception ex)
                {
                    Logger.Log($"Ошибка при удалении полиса с ID {policyId}: {ex.Message}");
                }
            }

            ClearFields();
        }

        private void AddPolicy_Click(object sender, RoutedEventArgs e)
        {
            if (validator.ValidateClientId(ClientComboBox.SelectedValue) &&
                     validator.ValidateDate(StartDate.SelectedDate.Value) && validator.ValidateDate(EndDate.SelectedDate.Value) &&
                     validator.ValidatePremiumAmount(Convert.ToDecimal(PremiumAmountTextBox.Text)) && validator.ValidateCoverageAmount(Convert.ToDecimal(CoverageAmountTextBox.Text)))
            {
                int newPolicyId = GetNewPolicyId();
                string policyNumber = "№ " + newPolicyId.ToString();
                policies.InsertQuery(policyNumber, (int)ClientComboBox.SelectedValue, StartDate.SelectedDate.Value.ToString("yyyy-MM-dd"),
                                      EndDate.SelectedDate.Value.ToString("yyyy-MM-dd"), Convert.ToDecimal(PremiumAmountTextBox.Text), Convert.ToDecimal(CoverageAmountTextBox.Text));
                LoadData();
                Logger.Log($"Полис {policyNumber} успешно добавлен для клиента с ID {(int)ClientComboBox.SelectedValue}");
            }
            else
            {
                CustomMessageBox.Show("Неверные данные полиса.");
                Logger.Log($"Ошибка добавления полиса. Неверные данные.");
            }

            ClearFields();
        }

        private int GetNewPolicyId()
        {
            DataTable policiesTable = policies.GetData();
            if (policiesTable.Rows.Count == 0)
            {
                return 1;
            }
            else
            {
                return (int)policiesTable.Compute("MAX(policy_id)", string.Empty) + 1;
            }
        }

        private void PoliciesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PoliciesDataGrid.SelectedItem != null)
            {
                DataRowView rowView = PoliciesDataGrid.SelectedItem as DataRowView;
                if (rowView != null)
                {
                    ClientComboBox.SelectedValue = rowView["client_id"];
                    StartDate.SelectedDate = (DateTime)rowView["start_date"];
                    EndDate.SelectedDate = (DateTime)rowView["end_date"];
                    PremiumAmountTextBox.Text = rowView["premium_amount"].ToString();
                    CoverageAmountTextBox.Text = rowView["coverage_amount"].ToString();
                }
            }
        }

        private void ClearFields()
        {
            ClientComboBox.SelectedIndex = -1;
            StartDate.SelectedDate = null;
            EndDate.SelectedDate = null;
            PremiumAmountTextBox.Clear();
            CoverageAmountTextBox.Clear();
            PoliciesDataGrid.SelectedIndex = -1;
        }

        private void TextBox_TextChanged(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(e.Text, "^[a-zA-Zа-яА-Я]+$");
        }

        private void TextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(e.Text, "^[0-9]+$");
        }
        private async void ExportCsv_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = "Server=GG;Database=Curs;Integrated Security=True;";
            string tableName = "policies";
            string csvFilePath = $@"D:\Шарага\Курс_4\Истинный_Курсач\Project2\{tableName}.csv";
            await Task.Run(() => DataExporter.ExportToCsv(connectionString, tableName, csvFilePath));
        }

        private async void ImportCsv_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = "Server=GG;Database=Curs;Integrated Security=True;";
            string tableName = "policies";
            string csvFilePath = $@"D:\Шарага\Курс_4\Истинный_Курсач\Project2\{tableName}.csv";
            await Task.Run(() => DataImporter.ImportFromCsv(connectionString, tableName, csvFilePath));

            LoadData();
        }
    }
}
