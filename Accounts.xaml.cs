using Curs.CursDataSetTableAdapters;
using System;
using System.Data;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Curs
{
    public partial class Accounts : Page
    {
        accountsTableAdapter accounts = new accountsTableAdapter();
        rolesTableAdapter roles = new rolesTableAdapter();
        Validator validator;

        public Accounts()
        {
            InitializeComponent();
            RoleComboBox.ItemsSource = roles.GetData();
            RoleComboBox.DisplayMemberPath = "role_name";
            RoleComboBox.SelectedValuePath = "role_id";
            LoadData();
        }

        private void LoadData()
        {
            DataTable accountsTable = accounts.GetData();
            DataTable rolesTable = roles.GetData();

            DataTable mergedTable = accountsTable.Clone();
            mergedTable.Columns.Add("role_name", typeof(string));

            foreach (DataRow accountRow in accountsTable.Rows)
            {
                DataRow newRow = mergedTable.NewRow();
                newRow.ItemArray = accountRow.ItemArray.Clone() as object[];

                DataRow roleRow = rolesTable.Rows.Find(accountRow["role_id"]);
                if (roleRow != null)
                {
                    newRow["role_name"] = roleRow["role_name"];
                }

                mergedTable.Rows.Add(newRow);
            }

            DataView view = mergedTable.DefaultView;
            view.Sort = "account_id ASC";
            AccountsDataGrid.ItemsSource = view;

            validator = new Validator(rolesTable, accountsTable);
        }

        private void EditAccount_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            DataRowView rowView = button.DataContext as DataRowView;

            if (rowView != null)
            {
                int accountId = (int)rowView["account_id"];
                string newUsername = UsernameTextBox.Text.Trim();
                string newPassword = PasswordTextBox.Text.Trim();
                int? roleId = RoleComboBox.SelectedValue as int?;

                if (validator.ValidateUsername(newUsername, accountId) &&
                    validator.ValidatePassword(newPassword) &&
                    validator.ValidateRoleId(roleId))
                {
                    string hashedPassword = PasswordHelper.HashPassword(newPassword);
                    accounts.UpdateQuery(newUsername, hashedPassword, roleId.Value, accountId);
                    LoadData();
                    Logger.Log($"Аккаунт с ID {accountId} успешно обновлен. Имя пользователя: {newUsername}, роль: {roleId}");
                }
                else
                {
                    CustomMessageBox.Show("Неверное имя пользователя, пароль или роль.");
                    Logger.Log($"Ошибка обновления аккаунта с ID {accountId}. Неверное имя пользователя, пароль или роль.");
                }
            }

            ClearFields();
        }

        private void DeleteAccount_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            DataRowView rowView = button.DataContext as DataRowView;

            if (rowView != null)
            {
                int accountId = (int)rowView["account_id"];

                try
                {
                    accounts.DeleteQuery(accountId);
                    LoadData();
                    Logger.Log($"Аккаунт с ID {accountId} успешно удален.");
                }
                catch (Exception ex)
                {
                    Logger.Log($"Ошибка при удалении аккаунта с ID {accountId}: {ex.Message}");
                }
            }

            ClearFields();
        }

        private void AddAccount_Click(object sender, RoutedEventArgs e)
        {
            string newUsername = UsernameTextBox.Text.Trim();
            string newPassword = UsernameTextBox.Text.Trim();
            int? roleId = RoleComboBox.SelectedValue as int?;

            if (validator.ValidateUsername(newUsername) && validator.ValidatePassword(newPassword) && validator.ValidateRoleId(roleId))
            {
                string hashedPassword = PasswordHelper.HashPassword(newPassword);
                accounts.InsertQuery(newUsername, hashedPassword, roleId.Value);
                LoadData();
                Logger.Log($"Аккаунт для пользователя {newUsername} успешно добавлен. Роль: {roleId}");
            }
            else
            {
                CustomMessageBox.Show("Неверное имя пользователя, пароль или роль.");
                Logger.Log($"Ошибка добавления аккаунта для пользователя {newUsername}. Неверное имя пользователя, пароль или роль.");
            }

            ClearFields();
        }

        private void AccountsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AccountsDataGrid.SelectedItem != null)
            {
                DataRowView rowView = AccountsDataGrid.SelectedItem as DataRowView;
                if (rowView != null)
                {
                    UsernameTextBox.Text = rowView["username"].ToString();
                    PasswordTextBox.Text = rowView["password"].ToString();
                    RoleComboBox.SelectedValue = rowView["role_id"];
                }
            }
        }

        private void ClearFields()
        {
            UsernameTextBox.Clear();
            PasswordTextBox.Clear();
            RoleComboBox.SelectedIndex = -1;
            AccountsDataGrid.SelectedIndex = -1;
        }

        private async void ExportCsv_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = "Server=GG;Database=Curs;Integrated Security=True;";
            string tableName = "accounts";
            string csvFilePath = $@"D:\Шарага\Курс_4\Истинный_Курсач\Project2\{tableName}.csv";
            await Task.Run(() => DataExporter.ExportToCsv(connectionString, tableName, csvFilePath));
        }

        private async void ImportCsv_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = "Server=GG;Database=Curs;Integrated Security=True;";
            string tableName = "accounts";
            string csvFilePath = $@"D:\Шарага\Курс_4\Истинный_Курсач\Project2\{tableName}.csv";
            await Task.Run(() => DataImporter.ImportFromCsv(connectionString, tableName, csvFilePath));

            LoadData();
        }
    }
}
