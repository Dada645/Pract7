using Curs.CursDataSetTableAdapters;
using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Curs
{
    public partial class Main : Page
    {
        public Main()
        {
            InitializeComponent();
            LoadClientEmployeeData();
        }

        private void Backup_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = "Server=GG;Database=Curs;Integrated Security=True;";
            string backupDirectory = @"D:\Шарага\Курс_4\Истинный_Курсач\Project2\Curs_backup.bak";

            BackupDatabase(connectionString, backupDirectory);
        }

        private void Restore_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = "Server=GG;Database=Curs;Integrated Security=True;";
            string backupFile = @"D:\Шарага\Курс_4\Истинный_Курсач\Project2\Curs_backup.bak";

            RestoreDatabase(connectionString, backupFile);
        }

        private void BackupDatabase(string connectionString, string backupDirectory)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string backupQuery = $"BACKUP DATABASE Curs TO DISK = '{backupDirectory}' WITH FORMAT, MEDIANAME = 'Curs_Backup', NAME = 'Full Backup of Curs';";
                SqlCommand command = new SqlCommand(backupQuery, connection);

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    Logger.Log("Резервное копирование базы данных выполнено успешно.");
                }
                catch (Exception ex)
                {
                    Logger.Log($"Ошибка при выполнении резервного копирования: {ex.Message}");
                    CustomMessageBox.Show($"Ошибка при выполнении резервного копирования: {ex.Message}");
                }
            }
        }

        private void RestoreDatabase(string connectionString, string backupFile)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string restoreQuery = @"
                    USE master;
                    ALTER DATABASE Curs SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                    RESTORE DATABASE Curs FROM DISK = '" + backupFile + @"' WITH REPLACE;
                    ALTER DATABASE Curs SET MULTI_USER;
                ";
                SqlCommand command = new SqlCommand(restoreQuery, connection);

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    Logger.Log("Восстановление базы данных выполнено успешно.");
                }
                catch (Exception ex)
                {
                    Logger.Log($"Ошибка при восстановлении базы данных: {ex.Message}");
                    CustomMessageBox.Show($"Ошибка при восстановлении базы данных: {ex.Message}");
                }
            }
        }

        private void LoadClientEmployeeData()
        {
            int clientCount = 0;
            int employeeCount = 0;

            try
            {
                clientsTableAdapter clients = new clientsTableAdapter();
                employeesTableAdapter employees = new employeesTableAdapter();

                var clientsDataTable = clients.GetData();
                var employeesDataTable = employees.GetData();

                clientCount = clientsDataTable.Count;
                employeeCount = employeesDataTable.Count;
            }
            catch (Exception ex)
            {
                Logger.Log($"Ошибка при загрузке данных клиентов и сотрудников: {ex.Message}");
            }

            DrawPieChart(clientCount, employeeCount);
        }

        private void DrawPieChart(int clientCount, int employeeCount)
        {
            ClientEmployeePieChart.Children.Clear();
            double total = clientCount + employeeCount;
            if (total == 0)
            {
                return;
            }

            double clientPercentage = clientCount / total;
            double employeePercentage = employeeCount / total;

            double canvasWidth = ClientEmployeePieChart.ActualWidth;
            double canvasHeight = ClientEmployeePieChart.ActualHeight;
            double radius = Math.Min(canvasWidth, canvasHeight) / 2;

            Point center = new Point(canvasWidth / 2, canvasHeight / 2);

            // Углы для секторов
            double startAngle = 0;
            double clientSweepAngle = clientPercentage * 360;
            double employeeSweepAngle = employeePercentage * 360;

            // Отрисовка секторов и добавление подписей
            DrawPieSlice(ClientEmployeePieChart, center, radius, startAngle, clientSweepAngle, Brushes.Peru);
            startAngle += clientSweepAngle;

            DrawPieSlice(ClientEmployeePieChart, center, radius, startAngle, employeeSweepAngle, Brushes.BlueViolet);

            // Добавление подписей после отрисовки секторов
            AddLabelToChart(ClientEmployeePieChart, center, radius, 0, clientSweepAngle, $"Клиенты: {clientPercentage:P0}", Brushes.White);
            AddLabelToChart(ClientEmployeePieChart, center, radius, startAngle, employeeSweepAngle, $"Сотрудники: {employeePercentage:P0}", Brushes.White);
        }

        private void AddLabelToChart(Canvas canvas, Point center, double radius, double startAngle, double sweepAngle, string text, Brush color)
        {
            double middleAngle = startAngle + sweepAngle / 2; // Середина сектора
            double labelRadius = radius * 0.6; // Радиус, где будет размещаться подпись (чуть меньше полного радиуса)

            // Координаты для подписи
            double x = center.X + labelRadius * Math.Cos(middleAngle * Math.PI / 180);
            double y = center.Y + labelRadius * Math.Sin(middleAngle * Math.PI / 180);

            TextBlock label = new TextBlock
            {
                Text = text,
                Foreground = color,
                FontSize = 14,
                FontWeight = FontWeights.Bold
            };

            // Важно учитывать размеры текста для точного центрирования
            label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double labelWidth = label.DesiredSize.Width;
            double labelHeight = label.DesiredSize.Height;

            // Установка координат с учетом размера текста
            Canvas.SetLeft(label, x - labelWidth / 2); // Центрируем по X
            Canvas.SetTop(label, y - labelHeight / 2);  // Центрируем по Y
            canvas.Children.Add(label);
        }

        private void DrawPieSlice(Canvas canvas, Point center, double radius, double startAngle, double sweepAngle, Brush fill)
        {
            PathFigure pieSliceFigure = new PathFigure { StartPoint = center };

            // Линия от центра до начала дуги
            LineSegment lineSegment = new LineSegment
            {
                Point = new Point(center.X + radius * Math.Cos(startAngle * Math.PI / 180),
                                  center.Y + radius * Math.Sin(startAngle * Math.PI / 180))
            };
            pieSliceFigure.Segments.Add(lineSegment);

            // Дуга
            ArcSegment arcSegment = new ArcSegment
            {
                Point = new Point(center.X + radius * Math.Cos((startAngle + sweepAngle) * Math.PI / 180),
                                  center.Y + radius * Math.Sin((startAngle + sweepAngle) * Math.PI / 180)),
                Size = new Size(radius, radius),
                SweepDirection = SweepDirection.Clockwise,
                IsLargeArc = sweepAngle > 180
            };
            pieSliceFigure.Segments.Add(arcSegment);

            // Линия от дуги обратно к центру
            pieSliceFigure.Segments.Add(new LineSegment { Point = center });

            PathGeometry geometry = new PathGeometry(new[] { pieSliceFigure });
            Path path = new Path
            {
                Fill = fill,
                Data = geometry
            };

            canvas.Children.Add(path);
        }

        private void ClientEmployeePieChart_Loaded(object sender, RoutedEventArgs e)
        {
            LoadClientEmployeeData();
        }
    }
}