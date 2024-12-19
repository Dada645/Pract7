using Curs.CursDataSetTableAdapters;
using System;
using System.Data;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Curs
{
    public partial class ClientAgents : Page
    {
        client_agentsTableAdapter clientAgents = new client_agentsTableAdapter();
        employeesTableAdapter employees = new employeesTableAdapter();
        clientsTableAdapter clients = new clientsTableAdapter();

        Validator validator;

        public ClientAgents()
        {
            InitializeComponent();
            ClientComboBox.ItemsSource = clients.GetData();
            ClientComboBox.DisplayMemberPath = "email";
            ClientComboBox.SelectedValuePath = "client_id";
            EmployeeComboBox.ItemsSource = employees.GetData();
            EmployeeComboBox.DisplayMemberPath = "email";
            EmployeeComboBox.SelectedValuePath = "employee_id";
            LoadData();
        }

        private void LoadData()
        {
            DataTable clientAgentsTable = clientAgents.GetData();
            DataTable clientsTable = clients.GetData();
            DataTable employeesTable = employees.GetData();

            DataTable mergedTable = clientAgentsTable.Clone();
            mergedTable.Columns.Add("client_email", typeof(string));
            mergedTable.Columns.Add("employee_email", typeof(string));

            foreach (DataRow agentRow in clientAgentsTable.Rows)
            {
                DataRow newRow = mergedTable.NewRow();
                newRow.ItemArray = agentRow.ItemArray.Clone() as object[];

                DataRow clientRow = clientsTable.Rows.Find(agentRow["client_id"]);
                if (clientRow != null)
                {
                    newRow["client_email"] = clientRow["email"];
                }

                DataRow employeeRow = employeesTable.Rows.Find(agentRow["employee_id"]);
                if (employeeRow != null)
                {
                    newRow["employee_email"] = employeeRow["email"];
                }

                mergedTable.Rows.Add(newRow);
            }

            DataView view = mergedTable.DefaultView;
            view.Sort = "client_agent_id ASC";
            ClientAgentDataGrid.ItemsSource = view;

            validator = new Validator(null, null, clientsTable, employeesTable, null, null, null, clientAgentsTable);
        }

        private void EditClientAgent_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            DataRowView rowView = button.DataContext as DataRowView;

            if (rowView != null)
            {
                int clientAgentId = (int)rowView["client_agent_id"];

                if (validator.ValidateClientAgentMapping((int?)ClientComboBox.SelectedValue, (int?)EmployeeComboBox.SelectedValue) && validator.ValidateDate(AssignedDate.SelectedDate))
                {
                    clientAgents.UpdateQuery((int)ClientComboBox.SelectedValue, (int)EmployeeComboBox.SelectedValue, AssignedDate.SelectedDate.Value.ToString(), clientAgentId);
                    LoadData();
                    Logger.Log($"Связь клиент-агент с ID {clientAgentId} успешно обновлена. Клиент ID: {(int)ClientComboBox.SelectedValue}, Агент ID: {(int)EmployeeComboBox.SelectedValue}");
                }
                else
                {
                    CustomMessageBox.Show("Неверные данные");
                    Logger.Log($"Ошибка обновления связи клиент-агент с ID {clientAgentId}. Неверные данные.");
                }
            }

            ClearFields();
        }

        private void DeleteClientAgent_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            DataRowView rowView = button.DataContext as DataRowView;

            if (rowView != null)
            {
                int clientAgentId = (int)rowView["client_agent_id"];

                try
                {
                    clientAgents.DeleteQuery(clientAgentId);
                    LoadData();
                    Logger.Log($"Связь клиент-агент с ID {clientAgentId} успешно удалена.");
                }
                catch (Exception ex)
                {
                    Logger.Log($"Ошибка при удалении связи клиент-агент с ID {clientAgentId}: {ex.Message}");
                }
            }

            ClearFields();
        }

        private void AddClientAgent_Click(object sender, RoutedEventArgs e)
        {
            if (validator.ValidateClientAgentMapping((int?)ClientComboBox.SelectedValue, (int?)EmployeeComboBox.SelectedValue) && validator.ValidateDate(AssignedDate.SelectedDate))
            {
                clientAgents.InsertQuery((int)ClientComboBox.SelectedValue, (int)EmployeeComboBox.SelectedValue, AssignedDate.SelectedDate.Value.ToString());
                LoadData();
                Logger.Log($"Связь клиент-агент для клиента с ID {(int)ClientComboBox.SelectedValue} и агента с ID {(int)EmployeeComboBox.SelectedValue} успешно добавлена.");
            }
            else
            {
                CustomMessageBox.Show("Неверные данные");
                Logger.Log($"Ошибка добавления связи клиент-агент для клиента с ID {(int)ClientComboBox.SelectedValue} и агента с ID {(int)EmployeeComboBox.SelectedValue}. Неверные данные.");
            }

            ClearFields();
        }

        private void ClearFields()
        {
            ClientComboBox.SelectedValue = -1;
            EmployeeComboBox.SelectedIndex = -1;
            AssignedDate.SelectedDate = null;
            ClientAgentDataGrid.SelectedIndex = -1;
        }

        private void ClientAgentsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ClientAgentDataGrid.SelectedItem != null)
            {
                DataRowView rowView = ClientAgentDataGrid.SelectedItem as DataRowView;
                if (rowView != null)
                {
                    ClientComboBox.SelectedValue = rowView["client_id"];
                    EmployeeComboBox.SelectedValue = rowView["employee_id"];
                    AssignedDate.SelectedDate = (DateTime?)rowView["assigned_date"];
                }
            }
        }

        private async void ExportCsv_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = "Server=GG;Database=Curs;Integrated Security=True;";
            string tableName = "client_agents";
            string csvFilePath = $@"D:\Шарага\Курс_4\Истинный_Курсач\Project2\{tableName}.csv";
            await Task.Run(() => DataExporter.ExportToCsv(connectionString, tableName, csvFilePath));
        }

        private async void ImportCsv_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = "Server=GG;Database=Curs;Integrated Security=True;";
            string tableName = "client_agents";
            string csvFilePath = $@"D:\Шарага\Курс_4\Истинный_Курсач\Project2\{tableName}.csv";
            await Task.Run(() => DataImporter.ImportFromCsv(connectionString, tableName, csvFilePath));

            LoadData();
        }
    }
}
