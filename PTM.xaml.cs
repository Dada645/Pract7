using Curs.CursDataSetTableAdapters;
using System;
using System.Data;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Curs
{
    public partial class PTM : Page
    {
        policy_type_mappingTableAdapter policyAndType = new policy_type_mappingTableAdapter();
        policiesTableAdapter policies = new policiesTableAdapter();
        policy_typesTableAdapter policyTypes = new policy_typesTableAdapter();

        Validator validator;

        public PTM()
        {
            InitializeComponent();
            PolicyComboBox.ItemsSource = policies.GetData();
            PolicyComboBox.DisplayMemberPath = "policy_number";
            PolicyComboBox.SelectedValuePath = "policy_id";
            TypeComboBox.ItemsSource = policyTypes.GetData();
            TypeComboBox.DisplayMemberPath = "type_name";
            TypeComboBox.SelectedValuePath = "type_id";
            LoadData();
        }

        private void LoadData()
        {
            DataTable policyAndTypeTable = policyAndType.GetData();
            DataTable policiesTable = policies.GetData();
            DataTable policyTypesTable = policyTypes.GetData();

            DataTable mergedTable = policyAndTypeTable.Clone();
            mergedTable.Columns.Add("policy_number", typeof(string));
            mergedTable.Columns.Add("type_name", typeof(string));

            foreach (DataRow mappingRow in policyAndTypeTable.Rows)
            {
                DataRow newRow = mergedTable.NewRow();
                newRow.ItemArray = mappingRow.ItemArray.Clone() as object[];

                DataRow policyRow = policiesTable.Rows.Find(mappingRow["policy_id"]);
                if (policyRow != null)
                {
                    newRow["policy_number"] = policyRow["policy_number"];
                }

                DataRow typeRow = policyTypesTable.Rows.Find(mappingRow["type_id"]);
                if (typeRow != null)
                {
                    newRow["type_name"] = typeRow["type_name"];
                }

                mergedTable.Rows.Add(newRow);
            }

            DataView view = mergedTable.DefaultView;
            view.Sort = "policy_type_mapping_id ASC";
            PolicyAndTypeDataGrid.ItemsSource = view;

            validator = new Validator(null, null, null, null, null, null, policyAndTypeTable);
        }

        private void EditPolicyAndType_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            DataRowView rowView = button.DataContext as DataRowView;

            if (rowView != null)
            {
                int ptmId = (int)rowView["policy_type_mapping_id"];

                if (validator.ValidatePolicyTypeMapping((int?)PolicyComboBox.SelectedValue, (int?)TypeComboBox.SelectedValue))
                {
                    policyAndType.UpdateQuery((int)PolicyComboBox.SelectedValue, (int)TypeComboBox.SelectedValue, ptmId);
                    LoadData();
                    Logger.Log($"Полис и тип с ID {ptmId} успешно обновлены. Полис ID: {(int)PolicyComboBox.SelectedValue}, Тип ID: {(int)TypeComboBox.SelectedValue}");
                }
                else
                {
                    CustomMessageBox.Show("Неверные данные");
                    Logger.Log($"Ошибка обновления полиса и типа с ID {ptmId}. Неверные данные.");
                }
            }

            ClearFields();
        }

        private void DeletePolicyAndType_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            DataRowView rowView = button.DataContext as DataRowView;

            if (rowView != null)
            {
                int ptmId = (int)rowView["policy_type_mapping_id"];

                try
                {
                    policyAndType.DeleteQuery(ptmId);
                    LoadData();
                    Logger.Log($"Полис и тип с ID {ptmId} успешно удалены.");
                }
                catch (Exception ex)
                {
                    Logger.Log($"Ошибка при удалении полиса и типа с ID {ptmId}: {ex.Message}");
                }
            }

            ClearFields();
        }

        private void AddPolicyAndType_Click(object sender, RoutedEventArgs e)
        {
            if (validator.ValidatePolicyTypeMapping((int?)PolicyComboBox.SelectedValue, (int?)TypeComboBox.SelectedValue))
            {
                policyAndType.InsertQuery((int)PolicyComboBox.SelectedValue, (int)TypeComboBox.SelectedValue);
                LoadData();
                Logger.Log($"Полис и тип с Полисом ID {(int)PolicyComboBox.SelectedValue} и Типом ID {(int)TypeComboBox.SelectedValue} успешно добавлены.");
            }
            else
            {
                CustomMessageBox.Show("Неверные данные");
                Logger.Log($"Ошибка добавления полиса и типа с Полисом ID {(int)PolicyComboBox.SelectedValue} и Типом ID {(int)TypeComboBox.SelectedValue}. Неверные данные.");
            }

            ClearFields();
        }

        private void ClearFields()
        {
            PolicyComboBox.SelectedValue = -1;
            TypeComboBox.SelectedIndex = -1;
            PolicyAndTypeDataGrid.SelectedIndex = -1;
        }

        private void PolicyAndTypesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PolicyAndTypeDataGrid.SelectedItem != null)
            {
                DataRowView rowView = PolicyAndTypeDataGrid.SelectedItem as DataRowView;
                if (rowView != null)
                {
                    PolicyComboBox.SelectedValue = rowView["policy_id"];
                    TypeComboBox.SelectedValue = rowView["type_id"];
                }
            }
        }
        private async void ExportCsv_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = "Server=GG;Database=Curs;Integrated Security=True;";
            string tableName = "policy_type_mapping";
            string csvFilePath = $@"D:\Шарага\Курс_4\Истинный_Курсач\Project2\{tableName}.csv";
            await Task.Run(() => DataExporter.ExportToCsv(connectionString, tableName, csvFilePath));
        }

        private async void ImportCsv_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = "Server=GG;Database=Curs;Integrated Security=True;";
            string tableName = "policy_type_mapping";
            string csvFilePath = $@"D:\Шарага\Курс_4\Истинный_Курсач\Project2\{tableName}.csv";
            await Task.Run(() => DataImporter.ImportFromCsv(connectionString, tableName, csvFilePath));

            LoadData();
        }
    }
}
