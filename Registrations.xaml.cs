using System;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Curs.CursDataSetTableAdapters;

namespace Curs
{
    public partial class Registrations : Window
    {
        private accountsTableAdapter accounts = new accountsTableAdapter();
        private clientsTableAdapter clients = new clientsTableAdapter();
        private rolesTableAdapter roles = new rolesTableAdapter();
        private Validator validator = new Validator();

        public Registrations()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            DataTable rolesTable = roles.GetData();
            DataTable accountsTable = accounts.GetData();
            DataTable clientsTable = clients.GetData();
            validator = new Validator(rolesTable, accountsTable, clientsTable, null, null, null, null, null, null, null);
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string newUsername = UsernameTextBox.Text.Trim();
            string newPassword = PasswordBox.Password.Trim();
            int roleId = GetClientRoleId();

            if (!validator.ValidateUsername(newUsername))
            {
                CustomMessageBox.Show("Неверное имя пользователя.");
                Logger.Log($"Ошибка регистрации: неверное имя пользователя {newUsername}.");
                return;
            }

            if (!validator.ValidatePassword(newPassword))
            {
                CustomMessageBox.Show("Неверный пароль.");
                Logger.Log($"Ошибка регистрации: неверный пароль.");
                return;
            }

            if (!validator.ValidateName(FirstNameTextBox.Text))
            {
                CustomMessageBox.Show("Неверное имя.");
                Logger.Log($"Ошибка регистрации: неверное имя {FirstNameTextBox.Text}.");
                return;
            }

            if (!validator.ValidateName(LastNameTextBox.Text))
            {
                CustomMessageBox.Show("Неверная фамилия.");
                Logger.Log($"Ошибка регистрации: неверная фамилия {LastNameTextBox.Text}.");
                return;
            }

            if (!validator.ValidateDate(DateOfBirthPicker.SelectedDate.Value))
            {
                CustomMessageBox.Show("Неверная дата рождения.");
                Logger.Log($"Ошибка регистрации: неверная дата рождения {DateOfBirthPicker.SelectedDate.Value.ToString("yyyy-MM-dd")}.");
                return;
            }

            if (!validator.ValidatePhone(PhoneTextBox.Text))
            {
                CustomMessageBox.Show("Неверный телефон.");
                Logger.Log($"Ошибка регистрации: неверный телефон {PhoneTextBox.Text}.");
                return;
            }

            if (!validator.ValidateEmail(EmailTextBox.Text))
            {
                CustomMessageBox.Show("Неверный адрес электронной почты.");
                Logger.Log($"Ошибка регистрации: неверный адрес электронной почты {EmailTextBox.Text}.");
                return;
            }

            if (!validator.ValidateAddress(AddressTextBox.Text))
            {
                CustomMessageBox.Show("Неверный адрес.");
                Logger.Log($"Ошибка регистрации: неверный адрес {AddressTextBox.Text}.");
                return;
            }

            string hashedPassword = PasswordHelper.HashPassword(newPassword);

            accounts.InsertQuery(newUsername, hashedPassword, roleId);

            int accountId = (int)accounts.GetData().AsEnumerable().Last()["account_id"];

            try
            {
                Logger.Log($"Аккаунт добавлен. ID: {accountId}, Username: {newUsername}, RoleID: {roleId}");

                clients.InsertQuery(FirstNameTextBox.Text, LastNameTextBox.Text, DateOfBirthPicker.SelectedDate.Value.ToString("yyyy-MM-dd"), PhoneTextBox.Text,
                                    EmailTextBox.Text, AddressTextBox.Text, accountId);
                Logger.Log($"Клиент {FirstNameTextBox.Text} {LastNameTextBox.Text} успешно зарегистрирован. Аккаунт ID: {accountId}");
                MainWindow auth = new MainWindow();
                auth.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка при добавлении клиента: {ex.Message}");
                Logger.Log($"Ошибка при добавлении клиента: {ex.Message}");
            }
        }

        private int GetClientRoleId()
        {
            var allRoles = roles.GetData().Rows;

            for (int i = 0; i < allRoles.Count; i++)
            {
                if (allRoles[i]["role_name"].ToString() == "Client")
                {
                    return (int)allRoles[i]["role_id"];
                }
            }

            throw new Exception("Роль 'Client' не найдена.");
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowState = WindowState.Maximized;
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MoveArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow auth = new MainWindow();
            auth.Show();
            this.Close();
        }
    }
}
