using Curs.CursDataSetTableAdapters;
using System;
using System.Data;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Curs
{
    public partial class Payouts : Page
    {
        payoutsTableAdapter payouts = new payoutsTableAdapter();
        claimsTableAdapter claims = new claimsTableAdapter();

        Validator validator;

        public Payouts()
        {
            InitializeComponent();
            ClaimComboBox.ItemsSource = claims.GetData();
            ClaimComboBox.DisplayMemberPath = "claim_id";
            ClaimComboBox.SelectedValuePath = "claim_id";
            LoadData();
        }

        private void LoadData()
        {
            DataTable payoutsTable = payouts.GetData();
            DataTable claimsTable = claims.GetData();

            DataTable mergedTable = payoutsTable.Clone();

            if (!mergedTable.Columns.Contains("claim_id"))
            {
                mergedTable.Columns.Add("claim_id", typeof(int));
            }

            foreach (DataRow payoutRow in payoutsTable.Rows)
            {
                DataRow newRow = mergedTable.NewRow();
                newRow.ItemArray = payoutRow.ItemArray.Clone() as object[];

                DataRow claimRow = claimsTable.Rows.Find(payoutRow["claim_id"]);
                if (claimRow != null)
                {
                    newRow["claim_id"] = claimRow["claim_id"];
                }

                mergedTable.Rows.Add(newRow);
            }

            DataView view = mergedTable.DefaultView;
            view.Sort = "payout_id ASC";
            PayoutsDataGrid.ItemsSource = view;

            validator = new Validator(null, null, null, null, null, null, null, null, null, payoutsTable);
        }

        private void EditPayout_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            DataRowView rowView = button.DataContext as DataRowView;

            if (rowView != null)
            {
                int payoutId = (int)rowView["payout_id"];

                if (validator.ValidatePayoutUniqueness((int?)ClaimComboBox.SelectedValue, payoutId) &&
                    validator.ValidateDate(PayoutDatePicker.SelectedDate) &&
                    validator.ValidateAmount(Convert.ToDecimal(PayoutAmountTextBox.Text)))
                {
                    payouts.UpdateQuery(
                        (int)ClaimComboBox.SelectedValue,
                        PayoutDatePicker.SelectedDate.Value.ToString(),
                        Convert.ToDecimal(PayoutAmountTextBox.Text),
                        payoutId
                    );
                    LoadData();
                    Logger.Log($"Выплата с ID {payoutId} успешно обновлена. Претензия ID: {(int)ClaimComboBox.SelectedValue}, Сумма: {PayoutAmountTextBox.Text}");
                }
                else
                {
                    CustomMessageBox.Show("Неверные данные");
                    Logger.Log($"Ошибка обновления выплаты с ID {payoutId}. Неверные данные.");
                }
            }

            ClearFields();
        }

        private void DeletePayout_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            DataRowView rowView = button.DataContext as DataRowView;

            if (rowView != null)
            {
                int payoutId = (int)rowView["payout_id"];

                try
                {
                    payouts.DeleteQuery(payoutId);
                    LoadData();
                    Logger.Log($"Выплата с ID {payoutId} успешно удалена.");
                }
                catch (Exception ex)
                {
                    Logger.Log($"Ошибка при удалении выплаты с ID {payoutId}: {ex.Message}");
                }
            }

            ClearFields();
        }

        private void AddPayout_Click(object sender, RoutedEventArgs e)
        {
            if (validator.ValidatePayoutUniqueness((int?)ClaimComboBox.SelectedValue) &&
                validator.ValidateDate(PayoutDatePicker.SelectedDate) &&
                validator.ValidateAmount(Convert.ToDecimal(PayoutAmountTextBox.Text)))
            {
                payouts.InsertQuery(
                    (int)ClaimComboBox.SelectedValue,
                    PayoutDatePicker.SelectedDate.Value.ToString(),
                    Convert.ToDecimal(PayoutAmountTextBox.Text)
                );
                LoadData();
                Logger.Log($"Выплата успешно добавлена для претензии с ID {(int)ClaimComboBox.SelectedValue}. Сумма: {PayoutAmountTextBox.Text}");
            }
            else
            {
                CustomMessageBox.Show("Неверные данные");
                Logger.Log($"Ошибка добавления выплаты для претензии с ID {(int)ClaimComboBox.SelectedValue}. Неверные данные.");
            }

            ClearFields();
        }

        private void ClearFields()
        {
            ClaimComboBox.SelectedValue = -1;
            PayoutDatePicker.SelectedDate = null;
            PayoutAmountTextBox.Clear();
            PayoutsDataGrid.SelectedIndex = -1;
        }

        private void PayoutsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PayoutsDataGrid.SelectedItem != null)
            {
                DataRowView rowView = PayoutsDataGrid.SelectedItem as DataRowView;
                if (rowView != null)
                {
                    ClaimComboBox.SelectedValue = rowView["claim_id"];
                    PayoutDatePicker.SelectedDate = (DateTime?)rowView["payout_date"];
                    PayoutAmountTextBox.Text = rowView["payout_amount"].ToString();
                }
            }
        }

        private void PayoutAmountTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(e.Text, "^[0-9]+$");
        }
        private async void ExportCsv_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = "Server=GG;Database=Curs;Integrated Security=True;";
            string tableName = "payouts";
            string csvFilePath = $@"D:\Шарага\Курс_4\Истинный_Курсач\Project2\{tableName}.csv";
            await Task.Run(() => DataExporter.ExportToCsv(connectionString, tableName, csvFilePath));
        }

        private async void ImportCsv_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = "Server=GG;Database=Curs;Integrated Security=True;";
            string tableName = "payouts";
            string csvFilePath = $@"D:\Шарага\Курс_4\Истинный_Курсач\Project2\{tableName}.csv";
            await Task.Run(() => DataImporter.ImportFromCsv(connectionString, tableName, csvFilePath));

            LoadData();
        }
    }
}
