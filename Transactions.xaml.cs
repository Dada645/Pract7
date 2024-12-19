using Curs.CursDataSetTableAdapters;
using System;
using System.Data;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Curs
{
    public partial class Transactions : Page
    {
        transactionsTableAdapter transactions = new transactionsTableAdapter();
        clientsTableAdapter clients = new clientsTableAdapter();

        Validator validator;

        public Transactions()
        {
            InitializeComponent();
            ClientComboBox.ItemsSource = clients.GetData();
            ClientComboBox.DisplayMemberPath = "email";
            ClientComboBox.SelectedValuePath = "client_id";
            LoadData();
        }

        private void LoadData()
        {
            DataTable transactionsTable = transactions.GetData();
            DataTable clientsTable = clients.GetData();

            DataTable mergedTable = transactionsTable.Clone();
            mergedTable.Columns.Add("client_email", typeof(string));

            foreach (DataRow transactionRow in transactionsTable.Rows)
            {
                DataRow newRow = mergedTable.NewRow();
                newRow.ItemArray = transactionRow.ItemArray.Clone() as object[];

                DataRow clientRow = clientsTable.Rows.Find(transactionRow["client_id"]);
                if (clientRow != null)
                {
                    newRow["client_email"] = clientRow["email"];
                }

                mergedTable.Rows.Add(newRow);
            }

            DataView view = mergedTable.DefaultView;
            view.Sort = "transaction_id ASC";
            TransactionsDataGrid.ItemsSource = view;

            validator = new Validator(clientsTable, transactionsTable);
        }

        private void EditTransaction_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            DataRowView rowView = button.DataContext as DataRowView;

            if (rowView != null)
            {
                int transactionId = (int)rowView["transaction_id"];

                if (validator.ValidateComboBoxSelection((int?)ClientComboBox.SelectedValue) && validator.ValidateDate(TransactionDatePicker.SelectedDate) && validator.ValidateAmount(Convert.ToDecimal(AmountTextBox.Text)))
                {
                    transactions.UpdateQuery((int)ClientComboBox.SelectedValue, TransactionDatePicker.SelectedDate.Value.ToString(), Convert.ToDecimal(AmountTextBox.Text), DescriptionTextBox.Text, transactionId);
                    LoadData();
                    Logger.Log($"Транзакция с ID {transactionId} успешно обновлена. Клиент ID: {(int)ClientComboBox.SelectedValue}, Сумма: {AmountTextBox.Text}");
                }
                else
                {
                    CustomMessageBox.Show("Неверные данные");
                    Logger.Log($"Ошибка обновления транзакции с ID {transactionId}. Неверные данные.");
                }
            }

            ClearFields();
        }

        private void DeleteTransaction_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            DataRowView rowView = button.DataContext as DataRowView;

            if (rowView != null)
            {
                int transactionId = (int)rowView["transaction_id"];

                try
                {
                    transactions.DeleteQuery(transactionId);
                    LoadData();
                    Logger.Log($"Транзакция с ID {transactionId} успешно удалена.");
                }
                catch (Exception ex)
                {
                    Logger.Log($"Ошибка при удалении транзакции с ID {transactionId}: {ex.Message}");
                }
            }

            ClearFields();
        }

        private void AddTransaction_Click(object sender, RoutedEventArgs e)
        {
            if (validator.ValidateComboBoxSelection((int?)ClientComboBox.SelectedValue) && validator.ValidateDate(TransactionDatePicker.SelectedDate) && validator.ValidateAmount(Convert.ToDecimal(AmountTextBox.Text)))
            {
                transactions.InsertQuery((int)ClientComboBox.SelectedValue, TransactionDatePicker.SelectedDate.Value.ToString(), Convert.ToDecimal(AmountTextBox.Text), DescriptionTextBox.Text);
                LoadData();
                Logger.Log($"Транзакция успешно добавлена для клиента с ID {(int)ClientComboBox.SelectedValue}. Сумма: {AmountTextBox.Text}");
            }
            else
            {
                CustomMessageBox.Show("Неверные данные");
                Logger.Log($"Ошибка добавления транзакции для клиента с ID {(int)ClientComboBox.SelectedValue}. Неверные данные.");
            }

            ClearFields();
        }

        private void ClearFields()
        {
            ClientComboBox.SelectedValue = -1;
            TransactionDatePicker.SelectedDate = null;
            AmountTextBox.Clear();
            DescriptionTextBox.Clear();
            TransactionsDataGrid.SelectedIndex = -1;
        }

        private void TransactionsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TransactionsDataGrid.SelectedItem != null)
            {
                DataRowView rowView = TransactionsDataGrid.SelectedItem as DataRowView;
                if (rowView != null)
                {
                    ClientComboBox.SelectedValue = rowView["client_id"];
                    TransactionDatePicker.SelectedDate = (DateTime?)rowView["transaction_date"];
                    AmountTextBox.Text = rowView["amount"].ToString();
                    DescriptionTextBox.Text = rowView["description"].ToString();
                }
            }
        }

        private void AmountTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(e.Text, "^[0-9]+$");
        }

        private async void ExportCsv_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = "Server=GG;Database=Curs;Integrated Security=True;";
            string tableName = "transactions";
            string csvFilePath = $@"D:\Шарага\Курс_4\Истинный_Курсач\Project2\{tableName}.csv";
            await Task.Run(() => DataExporter.ExportToCsv(connectionString, tableName, csvFilePath));
        }

        private async void ImportCsv_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = "Server=GG;Database=Curs;Integrated Security=True;";
            string tableName = "transactions";
            string csvFilePath = $@"D:\Шарага\Курс_4\Истинный_Курсач\Project2\{tableName}.csv";
            await Task.Run(() => DataImporter.ImportFromCsv(connectionString, tableName, csvFilePath));

            LoadData();
        }
    }
}
