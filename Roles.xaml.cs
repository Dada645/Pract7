using Curs.CursDataSetTableAdapters;
using System;
using System.Data;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Curs
{
    public partial class Roles : Page
    {
        rolesTableAdapter roles = new rolesTableAdapter();
        Validator validator;

        public Roles()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            DataTable rolesTable = roles.GetData();
            DataTable accountsTable = roles.GetData(); // Инициализация rolesTable
            DataView view = rolesTable.DefaultView;
            view.Sort = "role_id ASC";
            RolesDataGrid.ItemsSource = view;

            validator = new Validator(rolesTable, accountsTable);
        }

        private void EditRole_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            DataRowView rowView = button.DataContext as DataRowView;

            if (rowView != null)
            {
                int roleId = (int)rowView["role_id"];

                if (validator.ValidateRoleName(RoleTextBox.Text))
                {
                    roles.UpdateQuery(RoleTextBox.Text, roleId);
                    LoadData();
                    Logger.Log($"Роль с ID {roleId} успешно обновлена на {RoleTextBox.Text}.");
                }
                else
                {
                    CustomMessageBox.Show("Неверное название роли или она уже существует.");
                    Logger.Log($"Ошибка обновления роли с ID {roleId}: неверное название роли или она уже существует.");
                }
            }

            RoleTextBox.Clear();
            RolesDataGrid.SelectedIndex = -1;
        }

        private void DeleteRole_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            DataRowView rowView = button.DataContext as DataRowView;

            if (rowView != null)
            {
                int roleId = (int)rowView["role_id"];

                try
                {
                    roles.DeleteQuery(roleId);
                    LoadData();
                    Logger.Log($"Роль с ID {roleId} успешно удалена.");
                }
                catch (Exception ex)
                {
                    Logger.Log($"Ошибка при удалении роли с ID {roleId}: {ex.Message}");
                }
            }

            RoleTextBox.Clear();
            RolesDataGrid.SelectedIndex = -1;
        }

        private void AddRole_Click(object sender, RoutedEventArgs e)
        {
            string newRole = RoleTextBox.Text.Trim();

            if (validator.ValidateRoleName(newRole))
            {
                roles.InsertQuery(newRole);
                LoadData();
                RoleTextBox.Clear();
                Logger.Log($"Роль {newRole} успешно добавлена.");
            }
            else
            {
                CustomMessageBox.Show("Неверное название роли или она уже существует.");
                Logger.Log($"Ошибка добавления роли {newRole}: неверное название роли или она уже существует.");
            }

            RoleTextBox.Clear();
            RolesDataGrid.SelectedIndex = -1;
        }

        private void RoleTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(e.Text, "^[a-zA-Zа-яА-Я]+$");
        }

        private void RolesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RolesDataGrid.SelectedItem != null)
            {
                DataRowView rowView = RolesDataGrid.SelectedItem as DataRowView;
                if (rowView != null)
                {
                    RoleTextBox.Text = rowView["role_name"].ToString();
                }
            }
        }

        private async void ExportCsv_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = "Server=GG;Database=Curs;Integrated Security=True;";
            string tableName = "roles";
            string csvFilePath = $@"D:\Шарага\Курс_4\Истинный_Курсач\Project2\{tableName}.csv";
            await Task.Run(() => DataExporter.ExportToCsv(connectionString, tableName, csvFilePath));
        }

        private async void ImportCsv_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = "Server=GG;Database=Curs;Integrated Security=True;";
            string tableName = "roles";
            string csvFilePath = $@"D:\Шарага\Курс_4\Истинный_Курсач\Project2\{tableName}.csv";
            await Task.Run(() => DataImporter.ImportFromCsv(connectionString, tableName, csvFilePath));

            LoadData();
        }
    }
}
