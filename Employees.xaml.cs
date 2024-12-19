using Curs.CursDataSetTableAdapters;
using System;
using System.Data;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Curs
{
    public partial class Employees : Page
    {
        accountsTableAdapter accounts = new accountsTableAdapter();
        employeesTableAdapter employees = new employeesTableAdapter();
        clientsTableAdapter clients = new clientsTableAdapter();
        rolesTableAdapter roles = new rolesTableAdapter();
        Validator validator;

        public Employees()
        {
            InitializeComponent();
            AccountComboBox.ItemsSource = accounts.GetData();
            AccountComboBox.DisplayMemberPath = "username";
            AccountComboBox.SelectedValuePath = "account_id";
            LoadData();
        }

        private void LoadData()
        {
            DataTable employeesTable = employees.GetData();
            DataTable accountsTable = accounts.GetData();
            DataTable rolesTable = roles.GetData();
            DataTable clientsTable = clients.GetData();

            DataTable mergedTable = employeesTable.Clone();
            mergedTable.Columns.Add("username", typeof(string));

            foreach (DataRow employeeRow in employeesTable.Rows)
            {
                DataRow newRow = mergedTable.NewRow();
                newRow.ItemArray = employeeRow.ItemArray.Clone() as object[];

                DataRow accountRow = accountsTable.Rows.Find(employeeRow["account_id"]);
                if (accountRow != null)
                {
                    newRow["username"] = accountRow["username"];
                }

                mergedTable.Rows.Add(newRow);
            }

            DataView view = mergedTable.DefaultView;
            view.Sort = "employee_id ASC";
            EmployeesDataGrid.ItemsSource = view;

            validator = new Validator(rolesTable, accountsTable, clientsTable, employeesTable);
        }


        private void EditEmployee_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            DataRowView rowView = button.DataContext as DataRowView;

            if (rowView != null)
            {
                int employeeId = (int)rowView["employee_id"];

                if (validator.ValidateName(FirstNameTextBox.Text) && validator.ValidateName(LastNameTextBox.Text) &&
                    validator.ValidatePhone(PhoneTextBox.Text, employeeId) && validator.ValidateEmail(EmailTextBox.Text, employeeId) &&
                    validator.ValidateDate(HireDate.SelectedDate) && validator.ValidateAddress(JobTitle.Text) &&
                    validator.ValidateAccountId(AccountComboBox.SelectedValue, employeeId))
                {
                    employees.UpdateQuery(FirstNameTextBox.Text, LastNameTextBox.Text, PhoneTextBox.Text, EmailTextBox.Text,
                                          HireDate.SelectedDate.Value.ToString("yyyy-MM-dd"), JobTitle.Text, (int)AccountComboBox.SelectedValue, employeeId);
                    LoadData();
                    Logger.Log($"Сотрудник с ID {employeeId} успешно обновлен. Имя: {FirstNameTextBox.Text} {LastNameTextBox.Text}, Аккаунт ID: {(int)AccountComboBox.SelectedValue}");
                }
                else
                {
                    CustomMessageBox.Show("Неверные данные сотрудника.");
                    Logger.Log($"Ошибка обновления сотрудника с ID {employeeId}. Неверные данные.");
                }
            }

            ClearFields();
        }

        private void DeleteEmployee_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            DataRowView rowView = button.DataContext as DataRowView;

            if (rowView != null)
            {
                int employeeId = (int)rowView["employee_id"];

                try
                {
                    employees.DeleteQuery(employeeId);
                    LoadData();
                    Logger.Log($"Сотрудник с ID {employeeId} успешно удален.");
                }
                catch (Exception ex)
                {
                    Logger.Log($"Ошибка при удалении сотрудника с ID {employeeId}: {ex.Message}");
                }
            }

            ClearFields();
        }

        private void AddEmployee_Click(object sender, RoutedEventArgs e)
        {
            if (validator.ValidateName(FirstNameTextBox.Text) && validator.ValidateName(LastNameTextBox.Text) &&
                validator.ValidatePhone(PhoneTextBox.Text) && validator.ValidateEmail(EmailTextBox.Text) &&
                validator.ValidateDate(HireDate.SelectedDate) && validator.ValidateAddress(JobTitle.Text) &&
                validator.ValidateAccountId(AccountComboBox.SelectedValue))
            {
                employees.InsertQuery(FirstNameTextBox.Text, LastNameTextBox.Text, PhoneTextBox.Text, EmailTextBox.Text,
                                      HireDate.SelectedDate.Value.ToString("yyyy-MM-dd"), JobTitle.Text, (int)AccountComboBox.SelectedValue);
                LoadData();
                Logger.Log($"Сотрудник {FirstNameTextBox.Text} {LastNameTextBox.Text} успешно добавлен. Аккаунт ID: {(int)AccountComboBox.SelectedValue}");
            }
            else
            {
                CustomMessageBox.Show("Неверные данные сотрудника.");
                Logger.Log($"Ошибка добавления сотрудника {FirstNameTextBox.Text} {LastNameTextBox.Text}. Неверные данные.");
            }

            ClearFields();
        }

        private void EmployeesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EmployeesDataGrid.SelectedItem != null)
            {
                DataRowView rowView = EmployeesDataGrid.SelectedItem as DataRowView;
                if (rowView != null)
                {
                    FirstNameTextBox.Text = rowView["first_name"].ToString();
                    LastNameTextBox.Text = rowView["last_name"].ToString();
                    PhoneTextBox.Text = rowView["phone"].ToString();
                    EmailTextBox.Text = rowView["email"].ToString();
                    HireDate.SelectedDate = (DateTime)rowView["hire_date"];
                    JobTitle.Text = rowView["job_title"].ToString();
                    AccountComboBox.SelectedValue = rowView["account_id"];
                }
            }
        }

        private void ClearFields()
        {
            FirstNameTextBox.Clear();
            LastNameTextBox.Clear();
            PhoneTextBox.Clear();
            EmailTextBox.Clear();
            HireDate.SelectedDate = null;
            JobTitle.Clear();
            AccountComboBox.SelectedIndex = -1;
            EmployeesDataGrid.SelectedIndex = -1;
        }

        private void TextBox_TextChanged(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(e.Text, "^[a-zA-Zа-яА-Я]+$");
        }

        private void PhoneTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(e.Text, "^[0-9]+$");
        }
        private async void ExportCsv_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = "Server=GG;Database=Curs;Integrated Security=True;";
            string tableName = "employees";
            string csvFilePath = $@"D:\Шарага\Курс_4\Истинный_Курсач\Project2\{tableName}.csv";
            await Task.Run(() => DataExporter.ExportToCsv(connectionString, tableName, csvFilePath));
        }

        private async void ImportCsv_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = "Server=GG;Database=Curs;Integrated Security=True;";
            string tableName = "employees";
            string csvFilePath = $@"D:\Шарага\Курс_4\Истинный_Курсач\Project2\{tableName}.csv";
            await Task.Run(() => DataImporter.ImportFromCsv(connectionString, tableName, csvFilePath));

            LoadData();
        }
    }
}
