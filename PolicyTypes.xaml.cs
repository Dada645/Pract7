using Curs.CursDataSetTableAdapters;
using System;
using System.Data;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Curs
{
    public partial class PolicyTypes : Page
    {
        policy_typesTableAdapter policyTypes = new policy_typesTableAdapter();
        Validator validator;

        public PolicyTypes()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            DataTable policyTypeTable = policyTypes.GetData();
            DataView view = policyTypeTable.DefaultView;
            view.Sort = "type_id ASC";
            PolicyTypesDataGrid.ItemsSource = view;

            validator = new Validator(null, null, null, null, null, policyTypeTable);
        }

        private void EditPolicyType_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            DataRowView rowView = button.DataContext as DataRowView;

            if (rowView != null)
            {
                int typeId = (int)rowView["type_id"];

                if (validator.ValidateTypeName(TypeNameTextBox.Text, typeId) && validator.ValidateDescription(DescriptionTextBox.Text))
                {
                    policyTypes.UpdateQuery(TypeNameTextBox.Text, DescriptionTextBox.Text, typeId);
                    LoadData();
                    Logger.Log($"Тип полиса с ID {typeId} успешно обновлен. Название: {TypeNameTextBox.Text}, Описание: {DescriptionTextBox.Text}");
                }
                else
                {
                    CustomMessageBox.Show("Неверные данные типа полиса");
                    Logger.Log($"Ошибка обновления типа полиса с ID {typeId}. Неверные данные.");
                }
            }

            TypeNameTextBox.Clear();
            DescriptionTextBox.Clear();
            PolicyTypesDataGrid.SelectedIndex = -1;
        }

        private void DeletePolicyType_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            DataRowView rowView = button.DataContext as DataRowView;

            if (rowView != null)
            {
                int typeId = (int)rowView["type_id"];

                try
                {
                    policyTypes.DeleteQuery(typeId);
                    LoadData();
                    Logger.Log($"Тип полиса с ID {typeId} успешно удален.");
                }
                catch (Exception ex)
                {
                    Logger.Log($"Ошибка при удалении типа полиса с ID {typeId}: {ex.Message}");
                }
            }

            TypeNameTextBox.Clear();
            DescriptionTextBox.Clear();
            PolicyTypesDataGrid.SelectedIndex = -1;
        }

        private void AddPolicyType_Click(object sender, RoutedEventArgs e)
        {
            if (validator.ValidateTypeName(TypeNameTextBox.Text) && validator.ValidateDescription(DescriptionTextBox.Text))
            {
                policyTypes.InsertQuery(TypeNameTextBox.Text, DescriptionTextBox.Text);
                LoadData();
                Logger.Log($"Тип полиса {TypeNameTextBox.Text} успешно добавлен.");
                TypeNameTextBox.Clear();
            }
            else
            {
                CustomMessageBox.Show("Неверные данные типа полиса");
                Logger.Log($"Ошибка добавления типа полиса {TypeNameTextBox.Text}. Неверные данные.");
            }

            TypeNameTextBox.Clear();
            DescriptionTextBox.Clear();
            PolicyTypesDataGrid.SelectedIndex = -1;
        }

        private void RoleTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(e.Text, "^[a-zA-Zа-яА-Я]+$");
        }

        private void PolicyTypesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PolicyTypesDataGrid.SelectedItem != null)
            {
                DataRowView rowView = PolicyTypesDataGrid.SelectedItem as DataRowView;
                if (rowView != null)
                {
                    TypeNameTextBox.Text = rowView["type_name"].ToString();
                    DescriptionTextBox.Text = rowView["description"].ToString();
                }
            }
        }

        private async void ExportCsv_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = "Server=GG;Database=Curs;Integrated Security=True;";
            string tableName = "policy_types";
            string csvFilePath = $@"D:\Шарага\Курс_4\Истинный_Курсач\Project2\{tableName}.csv";
            await Task.Run(() => DataExporter.ExportToCsv(connectionString, tableName, csvFilePath));
        }

        private async void ImportCsv_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = "Server=GG;Database=Curs;Integrated Security=True;";
            string tableName = "policy_types";
            string csvFilePath = $@"D:\Шарага\Курс_4\Истинный_Курсач\Project2\{tableName}.csv";
            await Task.Run(() => DataImporter.ImportFromCsv(connectionString, tableName, csvFilePath));

            LoadData();
        }
    }
}
