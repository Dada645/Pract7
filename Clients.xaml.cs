using Curs.CursDataSetTableAdapters;
using System;
using System.Data;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Curs
{
    public partial class Clients : Page
    {
        accountsTableAdapter accounts = new accountsTableAdapter();
        clientsTableAdapter clients = new clientsTableAdapter();
        employeesTableAdapter employees = new employeesTableAdapter();
        rolesTableAdapter roles = new rolesTableAdapter();
        Validator validator;

        public Clients()
        {
            InitializeComponent();
            AccountComboBox.ItemsSource = accounts.GetData();
            AccountComboBox.DisplayMemberPath = "username";
            AccountComboBox.SelectedValuePath = "account_id";
            LoadData();
        }

        private void LoadData()
        {
            DataTable clientsTable = clients.GetData();
            DataTable accountsTable = accounts.GetData();
            DataTable rolesTable = roles.GetData();
            DataTable employeesTable = employees.GetData();

            DataTable mergedTable = clientsTable.Clone();
            mergedTable.Columns.Add("username", typeof(string));

            foreach (DataRow clientRow in clientsTable.Rows)
            {
                DataRow newRow = mergedTable.NewRow();
                newRow.ItemArray = clientRow.ItemArray.Clone() as object[];

                DataRow accountRow = accountsTable.Rows.Find(clientRow["account_id"]);
                if (accountRow != null)
                {
                    newRow["username"] = accountRow["username"];
                }

                mergedTable.Rows.Add(newRow);
            }

            DataView view = mergedTable.DefaultView;
            view.Sort = "client_id ASC";
            ClientsDataGrid.ItemsSource = view;

            validator = new Validator(rolesTable, accountsTable, clientsTable, employeesTable);
        }

        private void EditClient_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            DataRowView rowView = button.DataContext as DataRowView;

            if (rowView != null)
            {
                int clientId = (int)rowView["client_id"];

                if (validator.ValidateName(FirstNameTextBox.Text) && validator.ValidateName(LastNameTextBox.Text) &&
                    validator.ValidateDate(DateOfBirthPicker.SelectedDate) && validator.ValidatePhone(PhoneTextBox.Text, clientId) &&
                    validator.ValidateEmail(EmailTextBox.Text, clientId) && validator.ValidateAddress(AddressTextBox.Text) &&
                    validator.ValidateAccountId(AccountComboBox.SelectedValue, clientId))
                {
                    clients.UpdateQuery(FirstNameTextBox.Text, LastNameTextBox.Text, DateOfBirthPicker.SelectedDate.Value.ToString("yyyy-MM-dd"), PhoneTextBox.Text,
                                        EmailTextBox.Text, AddressTextBox.Text, (int)AccountComboBox.SelectedValue, clientId);
                    LoadData();
                    Logger.Log($"Клиент с ID {clientId} успешно обновлен. Имя: {FirstNameTextBox.Text} {LastNameTextBox.Text}, Аккаунт ID: {(int)AccountComboBox.SelectedValue}");
                }
                else
                {
                    CustomMessageBox.Show("Неверные данные клиента.");
                    Logger.Log($"Ошибка обновления клиента с ID {clientId}. Неверные данные.");
                }
            }

            ClearFields();
        }

        private void DeleteClient_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            DataRowView rowView = button.DataContext as DataRowView;

            if (rowView != null)
            {
                int clientId = (int)rowView["client_id"];

                try
                {
                    clients.DeleteQuery(clientId);
                    LoadData();
                    Logger.Log($"Клиент с ID {clientId} успешно удален.");
                }
                catch (Exception ex)
                {
                    Logger.Log($"Ошибка при удалении клиента с ID {clientId}: {ex.Message}");
                }
            }

            ClearFields();
        }

        private void AddClient_Click(object sender, RoutedEventArgs e)
        {
            if (validator.ValidateName(FirstNameTextBox.Text) && validator.ValidateName(LastNameTextBox.Text) &&
                validator.ValidateDate(DateOfBirthPicker.SelectedDate) && validator.ValidatePhone(PhoneTextBox.Text) &&
                validator.ValidateEmail(EmailTextBox.Text) && validator.ValidateAddress(AddressTextBox.Text) &&
                validator.ValidateAccountId(AccountComboBox.SelectedValue))
            {
                clients.InsertQuery(FirstNameTextBox.Text, LastNameTextBox.Text, DateOfBirthPicker.SelectedDate.Value.ToString("yyyy-MM-dd"), PhoneTextBox.Text,
                                    EmailTextBox.Text, AddressTextBox.Text, (int)AccountComboBox.SelectedValue);
                LoadData();
                Logger.Log($"Клиент {FirstNameTextBox.Text} {LastNameTextBox.Text} успешно добавлен. Аккаунт ID: {(int)AccountComboBox.SelectedValue}");
            }
            else
            {
                CustomMessageBox.Show("Неверные данные клиента.");
                Logger.Log($"Ошибка добавления клиента {FirstNameTextBox.Text} {LastNameTextBox.Text}. Неверные данные.");
            }

            ClearFields();
        }

        private void ClientsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ClientsDataGrid.SelectedItem != null)
            {
                DataRowView rowView = ClientsDataGrid.SelectedItem as DataRowView;
                if (rowView != null)
                {
                    FirstNameTextBox.Text = rowView["first_name"].ToString();
                    LastNameTextBox.Text = rowView["last_name"].ToString();
                    DateOfBirthPicker.SelectedDate = (DateTime)rowView["date_of_birth"];
                    PhoneTextBox.Text = rowView["phone"].ToString();
                    EmailTextBox.Text = rowView["email"].ToString();
                    AddressTextBox.Text = rowView["address"].ToString();
                    AccountComboBox.SelectedValue = rowView["account_id"];
                }
            }
        }

        private void ClearFields()
        {
            FirstNameTextBox.Clear();
            LastNameTextBox.Clear();
            DateOfBirthPicker.SelectedDate = null;
            PhoneTextBox.Clear();
            EmailTextBox.Clear();
            AddressTextBox.Clear();
            AccountComboBox.SelectedIndex = -1;
            ClientsDataGrid.SelectedIndex = -1;
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
            string tableName = "clients";
            string csvFilePath = $@"D:\Шарага\Курс_4\Истинный_Курсач\Project2\{tableName}.csv";
            await Task.Run(() => DataExporter.ExportToCsv(connectionString, tableName, csvFilePath));
        }

        private async void ImportCsv_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = "Server=GG;Database=Curs;Integrated Security=True;";
            string tableName = "clients";
            string csvFilePath = $@"D:\Шарага\Курс_4\Истинный_Курсач\Project2\{tableName}.csv";
            await Task.Run(() => DataImporter.ImportFromCsv(connectionString, tableName, csvFilePath));

            LoadData();
        }
    }
}
