using Curs.CursDataSetTableAdapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Curs
{
    public partial class MainWindow : Window
    {
        accountsTableAdapter accounts = new accountsTableAdapter();
        rolesTableAdapter roles = new rolesTableAdapter();

        public MainWindow()
        {
            InitializeComponent();
            password.PasswordChar = '*';
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var allAccounts = accounts.GetData().Rows;
            string inputUsername = username.Text;
            string inputPassword = PasswordHelper.HashPassword(password.Password); // Хэшируем введенный пароль

            for (int i = 0; i < allAccounts.Count; i++)
            {
                if (allAccounts[i]["username"].ToString() == inputUsername && allAccounts[i]["password"].ToString() == inputPassword)
                {
                    int role_id = (int)allAccounts[i]["role_id"];
                    var role = roles.GetData().FirstOrDefault(r => (int)r["role_id"] == role_id);

                    if (role != null)
                    {
                        string roleName = role["role_name"].ToString();

                        switch (roleName)
                        {
                            case "Admin":
                                Admin admin = new Admin();
                                admin.Show();
                                this.Close();
                                Logger.Log($"Выполнен вход в аккаунт '' {inputUsername} '' с паролем '' {inputPassword} ''");
                                return;
                            case "Agent":
                                Agent agent = new Agent(); // Страница для агента
                                agent.Show();
                                this.Close();
                                Logger.Log($"Выполнен вход в аккаунт '' {inputUsername} '' с паролем '' {inputPassword} ''");
                                return;
                            case "Client":
                                int clientId = (int)allAccounts[i]["account_id"];
                                AuthenticatedClients.AddClientId(clientId);

                                Client client = new Client(); // Страница для клиента
                                client.Show();
                                this.Close();
                                Logger.Log($"Выполнен вход в аккаунт '' {inputUsername} '' с паролем '' {inputPassword} ''");
                                return;
                            default:
                                CustomMessageBox.Show("Неизвестная роль пользователя");
                                Logger.Log($"Неизвестная роль пользователя: логин -  '' {inputUsername} '', пароль - '' {inputPassword} ''");
                                return;
                        }
                    }
                }
            }

            CustomMessageBox.Show("Такого профиля не существует");
            Logger.Log($"Несуществующий профиль: логин -  '' {inputUsername} '', пароль - '' {inputPassword} ''");
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

        private void firsttxt_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = char.IsDigit(e.Text, 0);
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {

            Registrations registration = new Registrations();
            registration.Show();
            this.Close();
        }
    }
}
