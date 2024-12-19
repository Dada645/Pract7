using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Curs
{
    /// <summary>
    /// Логика взаимодействия для Agent.xaml
    /// </summary>
    public partial class Agent : Window
    {
        public Agent()
        {
            InitializeComponent();
        }

        private void Navigate(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button; if (button != null)
            {
                switch (button.Content.ToString())
                {
                    case "Клиенты":
                        MainFrame.Navigate(new Clients());
                        Logger.Log($"Переход на страницу '' Клиенты ''");
                        break;
                    case "Полисы":
                        MainFrame.Navigate(new Policies());
                        Logger.Log($"Переход на страницу '' Полисы ''");
                        break;
                    case "Типы полисов":
                        MainFrame.Navigate(new PolicyTypes());
                        Logger.Log($"Переход на страницу '' Типы полисов ''");
                        break;
                    case "Маппинг полисов":
                        MainFrame.Navigate(new PTM());
                        Logger.Log($"Переход на страницу '' Маппинг полисов ''");
                        break;
                    case "Агенты клиентов":
                        MainFrame.Navigate(new ClientAgents());
                        Logger.Log($"Переход на страницу '' Агенты клиентов ''");
                        break;
                    case "Транзакции":
                        MainFrame.Navigate(new Transactions());
                        Logger.Log($"Переход на страницу '' Транзакции ''");
                        break;
                    case "Претензии":
                        MainFrame.Navigate(new Claims());
                        Logger.Log($"Переход на страницу '' Претензии ''");
                        break;
                    case "История полисов":
                        MainFrame.Navigate(new ClaimStatusHistory());
                        Logger.Log($"Переход на страницу '' История полисов ''");
                        break;
                    case "Выплаты":
                        MainFrame.Navigate(new Payouts());
                        Logger.Log($"Переход на страницу '' Выплаты ''");
                        break;
                }
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            MainWindow auf = new MainWindow();
            auf.Show();
            this.Close();
            Logger.Log($"Переход на страницу '' Авторизация ''");

        }

        private void MainFrame_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {

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
    }
}
