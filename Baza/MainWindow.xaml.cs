using System.Collections.ObjectModel;
using System.Windows;
using System;
using System.Data.SQLite;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Windows.Data;
using ClosedXML.Excel;


namespace Baza
{
    public class Product
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Material { get; set; }
        public int Quantity { get; set; }
        public int MinQuantity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalPrice => Quantity * Price; // Вычисляемое свойство для расчета суммы
    }

    public partial class MainWindow : Window
    {

        private static string connectionString = "Data Source=./DataBase.db"; // Подключение к базе данных !!!ОСНОВНОЕ!!!

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        public ObservableCollection<Product> Products { get; set; } = new ObservableCollection<Product>();

        private void SaveToDatabase() // Метод для сохранения данных в базу данных
        {
            List<string> messages = new List<string>(); // Создаем список для хранения сообщений о продуктах

            using (var connection = new SQLiteConnection(connectionString)) // Используем подключение к SQLite базе данных
            {
                connection.Open(); // Открываем соединение

                foreach (var product in Products) // Для каждого продукта в списке
                {
                    // Проверяем, существует ли продукт с таким же ID
                    string checkQuery = $"SELECT COUNT(*) FROM Products WHERE ProductId = {product.ProductId}";
                    using (SQLiteCommand checkCommand = new SQLiteCommand(checkQuery, connection))
                    {
                        int count = Convert.ToInt32(checkCommand.ExecuteScalar());
                        if (count > 0)
                        {
                            // Если продукт с таким ID уже существует, добавляем предупреждение в список
                            messages.Add($"Товар с ID {product.ProductId}");
                            continue; // Пропускаем вставку этого продукта и переходим к следующему
                        }
                    }

                    // Если продукт с таким ID не существует, вставляем новые данные
                    string query = $"INSERT INTO Products (ProductId, Name, Material, Quantity, MinQuantity, Price) " +
                                   $"VALUES ({product.ProductId}, '{product.Name}', '{product.Material}', {product.Quantity}, {product.MinQuantity}, {product.Price})";
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.ExecuteNonQuery(); // Выполняем команду вставки
                        MessageBox.Show("Товар добавлен в таблицу", "Добавлено", MessageBoxButton.OK, MessageBoxImage.Information); // Показываем сообщение об успешной вставке
                    }
                }
            }

            // Выводим все предупреждения из списка в одном окне
            if (messages.Count > 0)
            {
                string allMessages = string.Join("\n", messages) + "\nУже существуют в базе данных!";
                MessageBox.Show(allMessages, "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning); // Показываем сообщение с предупреждениями
            }
        }


        private void LoadDataFromDatabase() // Загрузить в таблицу
        {
            string query = "SELECT * FROM Products";

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Product product = new Product
                            {
                                ProductId = Convert.ToInt32(reader["ProductId"]),
                                Name = reader["Name"].ToString(),
                                Material = reader["Material"].ToString(),
                                Quantity = Convert.ToInt32(reader["Quantity"]),
                                MinQuantity = Convert.ToInt32(reader["MinQuantity"]),
                                Price = Convert.ToDecimal(reader["Price"])
                            };
                            Products.Add(product);
                        }
                    }
                }
            }
        }

        public void LoadFilDatabase() // Загрузить в таблицу с фильтром
        {
            string query = "SELECT * FROM Products WHERE 1=1";

            // Проверяем заполнение поля "Имя"
            if (!string.IsNullOrWhiteSpace(Nfil.Text))
            {
                query += $" AND Name LIKE '%{Nfil.Text}%'";
            }

            // Проверяем заполнение поля "Материал"
            if (!string.IsNullOrWhiteSpace(Mfil.Text))
            {
                query += $" AND Material LIKE '%{Mfil.Text}%'";
            }

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (!reader.HasRows)
                        {
                            MessageBox.Show("Товар с указанными параметрами не найден.", "Поиск", MessageBoxButton.OK, MessageBoxImage.Information);
                            return; // Выходим из метода, так как результатов нет
                        }

                        while (reader.Read())
                        {
                            Product product = new Product
                            {
                                ProductId = Convert.ToInt32(reader["ProductId"]),
                                Name = reader["Name"].ToString(),
                                Material = reader["Material"].ToString(),
                                Quantity = Convert.ToInt32(reader["Quantity"]),
                                MinQuantity = Convert.ToInt32(reader["MinQuantity"]),
                                Price = Convert.ToDecimal(reader["Price"])
                            };
                            Products.Add(product);
                        }
                    }
                }
            }
        }


        private void PasteWithReplace() // Замена существующих данных из таблицы
        {

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                foreach (var product in Products)
                {
                    // Проверяем, существует ли продукт с таким же ID
                    string checkQuery = $"SELECT COUNT(*) FROM Products WHERE ProductId = {product.ProductId}";
                    using (SQLiteCommand checkCommand = new SQLiteCommand(checkQuery, connection))
                    {
                        int count = Convert.ToInt32(checkCommand.ExecuteScalar());
                        if (count > 0)
                        {
                            // Если продукт с таким ID уже существует, обновляем данные
                            string updateQuery = $"UPDATE Products SET Name = '{product.Name}', Material = '{product.Material}', Quantity = {product.Quantity}, MinQuantity = {product.MinQuantity}, Price = {product.Price} WHERE ProductId = {product.ProductId}";
                            using (SQLiteCommand updateCommand = new SQLiteCommand(updateQuery, connection))
                            {
                                updateCommand.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            // Если продукт с таким ID не существует, вставляем новые данные
                            string query = $"INSERT INTO Products (ProductId, Name, Material, Quantity, MinQuantity, Price) " +
                                           $"VALUES ({product.ProductId}, '{product.Name}', '{product.Material}', {product.Quantity}, {product.MinQuantity}, {product.Price})";
                            using (SQLiteCommand command = new SQLiteCommand(query, connection))
                            {
                                command.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
        }

        private void FillList(ComboBox comboBox, string columnName) // Список товара
        {
            comboBox.Items.Clear();

            // Используем HashSet для хранения уникальных значений
            HashSet<string> uniqueValues = new HashSet<string>();

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                Console.WriteLine($"Executing query: SELECT {columnName} FROM Products");
                var command = new SQLiteCommand($"SELECT {columnName} FROM Products", connection);
                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string value = reader[columnName].ToString();

                    // Проверяем, было ли значение уже добавлено
                    if (!uniqueValues.Contains(value))
                    {
                        comboBox.Items.Add(value);
                        uniqueValues.Add(value); // Добавляем значение в HashSet
                    }
                }

                reader.Close();
                connection.Close();
            }
        }

        private void DeleteSelectedProducts() // Удаление данных из базы данных
        {
            if (DataGrid.SelectedItems.Count > 0)
            {
                var selectedProducts = new List<Product>(DataGrid.SelectedItems.Cast<Product>());
                foreach (var product in selectedProducts)
                {
                    // Удаление продукта из базы данных
                    using (var connection = new SQLiteConnection(connectionString))
                    {
                        connection.Open();
                        var command = connection.CreateCommand();
                        command.CommandText = "DELETE FROM Products WHERE ProductId = @ProductId";
                        command.Parameters.AddWithValue("@ProductId", product.ProductId);
                        command.ExecuteNonQuery();
                    }

                    // Удаление продукта из коллекции
                    Products.Remove(product);
                }

                MessageBox.Show("Выбранные товары были успешно удалены!");
            }
            else
            {
                MessageBox.Show("Нет выбранных продуктов для удаления!");
            }
        }

        private void DeleteMarkerDatabase()
        {
            // Получаем выбранные элементы из таблицы
            var selectedProducts = DataGrid.SelectedItems.Cast<Product>().ToList();

            // Предполагаем, что у вас есть объект, представляющий вашу базу данных
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // Перебираем выбранные продукты
                foreach (var product in selectedProducts)
                {
                    string deleteQuery = "DELETE FROM Products WHERE ProductId = @ProductId";

                    using (var command = new SQLiteCommand(deleteQuery, connection))
                    {
                        // Добавляем параметр идентификатора продукта
                        command.Parameters.AddWithValue("@ProductId", product.ProductId);
                        command.ExecuteNonQuery();
                    }
                    try
                    {
                        MessageBox.Show("Данные удалены из базы данных.","Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        Products.Remove(product);    
                    }
                    catch { }
                }
            }
        }

        private void OnlyNumbers(object sender, TextCompositionEventArgs e) // Проверка ввода цифор там где их не должно быть!
        {
            AddProductWindow addProductWindow = new AddProductWindow();
            addProductWindow.OnlyNumber(e);
        } 

        private void Nedostacha()
        {
            // Очищаем коллекцию Products
            Products.Clear();

            // Выполняем SQL-запрос к базе данных, чтобы получить отфильтрованные товары
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string sql = "SELECT * FROM Products WHERE MinQuantity >= Quantity";
                var command = new SQLiteCommand(sql, connection);
                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    int id = Convert.ToInt32(reader["ProductId"]);
                    string name = reader["Name"].ToString();
                    string material = reader["Material"].ToString();
                    int quantity = Convert.ToInt32(reader["Quantity"]);
                    int minQuantity = Convert.ToInt32(reader["MinQuantity"]);
                    decimal price = Convert.ToDecimal(reader["Price"]);

                    Products.Add(new Product
                    {
                        ProductId = id,
                        Name = name,
                        Material = material,
                        Quantity = quantity,
                        MinQuantity = minQuantity,
                        Price = price
                    });
                }
                reader.Close();
                connection.Close();
            }
        }

        private void Othet()
        {
            Document document = new Document();
            try
            {
                Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
                saveFileDialog.Filter = "PDF файлы (*.pdf)|*.pdf";
                if (saveFileDialog.ShowDialog() == true)
                {
                    PdfWriter.GetInstance(document, new FileStream(saveFileDialog.FileName, FileMode.Create));
                    document.Open();

                    // Используем шрифт Arial для поддержки русского языка
                    string ttf = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "ARIAL.TTF");
                    var baseFont = BaseFont.CreateFont(ttf, BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
                    var font = new iTextSharp.text.Font(baseFont, iTextSharp.text.Font.DEFAULTSIZE, iTextSharp.text.Font.NORMAL);

                    // Предполагается, что 'document' - это экземпляр класса документа PDF (например, Document из iTextSharp)

                    // Добавляем строку с названием компании
                    Paragraph companyNameParagraph = new Paragraph("Название компании \n\n\n", font);
                    companyNameParagraph.Alignment = Element.ALIGN_CENTER; // Выравниваем по центру

                    document.Add(companyNameParagraph);


                    // Добавляем таблицу с данными
                    PdfPTable pdfTable = new PdfPTable(DataGrid.Columns.Count);
                    pdfTable.TotalWidth = 550f;
                    pdfTable.LockedWidth = true;

                    // Добавляем заголовки столбцов таблицы
                    foreach (DataGridColumn column in DataGrid.Columns)
                    {
                        PdfPCell cell = new PdfPCell(new Phrase(column.Header.ToString(), font)); // Предполагается, что 'font' инициализирован
                        pdfTable.AddCell(cell);
                    }

                    // Добавляем данные из источника данных DataGrid
                    foreach (var item in DataGrid.ItemsSource)
                    {
                        foreach (DataGridColumn column in DataGrid.Columns)
                        {
                            if (column is DataGridTextColumn)
                            {
                                // Обрабатываем столбцы типа DataGridTextColumn
                                Binding binding = (column as DataGridTextColumn)?.Binding as Binding;
                                if (binding != null)
                                {
                                    string propertyName = binding.Path.Path;
                                    var property = item.GetType().GetProperty(propertyName);
                                    if (property != null)
                                    {
                                        PdfPCell cell = new PdfPCell(new Phrase(property.GetValue(item)?.ToString(), font));
                                        pdfTable.AddCell(cell);
                                    }
                                }
                            }
                            else if (column is DataGridTemplateColumn)
                            {
                                // Обрабатываем столбцы типа DataGridTemplateColumn
                                var templateColumn = column as DataGridTemplateColumn;
                                var cellContent = templateColumn?.CellTemplate?.LoadContent() as FrameworkElement;
                                if (cellContent is TextBox)
                                {
                                    var binding = (cellContent as TextBox)?.GetBindingExpression(TextBox.TextProperty)?.ParentBinding;
                                    if (binding != null)
                                    {
                                        string propertyName = binding.Path.Path;
                                        var property = item.GetType().GetProperty(propertyName);
                                        if (property != null)
                                        {
                                            PdfPCell cell = new PdfPCell(new Phrase(property.GetValue(item)?.ToString(), font));
                                            pdfTable.AddCell(cell);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    document.Add(pdfTable);

                    // Добавляем поля для подписей кладовщика и поставщика
                    PdfPTable signatureTable = new PdfPTable(2);
                    signatureTable.SpacingBefore = 20f;
                    signatureTable.DefaultCell.Border = PdfPCell.NO_BORDER;
                    signatureTable.WidthPercentage = 100;

                    // Подпись кладовщика
                    PdfPCell storekeeperCell = new PdfPCell(new Phrase("Подпись кладовщика: \n\n\n\n____________________", font));
                    storekeeperCell.Border = PdfPCell.NO_BORDER;
                    signatureTable.AddCell(storekeeperCell);

                    // Подпись поставщика
                    PdfPCell supplierCell = new PdfPCell(new Phrase("Подпись поставщика: \n\n\n\n______________________", font));
                    supplierCell.Border = PdfPCell.NO_BORDER;
                    signatureTable.AddCell(supplierCell);

                    document.Add(signatureTable);

                    // Добавление поля для даты
                    PdfPTable dateTable = new PdfPTable(1);
                    dateTable.SpacingBefore = 20f;
                    dateTable.DefaultCell.Border = PdfPCell.NO_BORDER;
                    dateTable.WidthPercentage = 100;

                    // Форматирование даты
                    string currentDate = DateTime.Now.ToString("Дата dd.MM.yyyy г."); // Подстройте формат по необходимости
                    PdfPCell dateCell = new PdfPCell(new Phrase(currentDate, font));
                    dateCell.Border = PdfPCell.NO_BORDER;
                    dateTable.AddCell(dateCell);

                    document.Add(dateTable);
                    MessageBox.Show("Отчет успешно создан и сохранен по пути: " + saveFileDialog.FileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при создании отчета: " + ex.Message);
            }
            finally
            {
                document.Close();
            }
        }

        private void Excel_Create(string filePath)
        {
            // Создаем новый экземпляр рабочей книги Excel
            var workbook = new XLWorkbook();

            // Добавляем новый лист в рабочую книгу
            var worksheet = workbook.Worksheets.Add("Sheet1");

            // Добавляем заголовки столбцов
            worksheet.Cell(1, 1).Value = "ID";
            worksheet.Cell(1, 2).Value = "Имя";
            worksheet.Cell(1, 3).Value = "Материал";
            worksheet.Cell(1, 4).Value = "Количество";
            worksheet.Cell(1, 5).Value = "Мин Количество";
            worksheet.Cell(1, 6).Value = "Сумма";


            // Записываем данные в новый файл Excel и добавляем рамки вокруг ячеек
            for (int i = 0; i < Products.Count; i++)
            {
                worksheet.Cell(i + 2, 1).Value = Products[i].ProductId;
                worksheet.Cell(i + 2, 2).Value = Products[i].Name;
                worksheet.Cell(i + 2, 3).Value = Products[i].Material;
                worksheet.Cell(i + 2, 4).Value = Products[i].Quantity;
                worksheet.Cell(i + 2, 5).Value = Products[i].MinQuantity;
                worksheet.Cell(i + 2, 6).Value = Products[i].Price;

                // Добавляем рамки вокруг каждой ячейки
                for (int j = 1; j <= 6; j++)
                {
                    worksheet.Cell(i + 2, j).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(i + 2, j).Style.Border.OutsideBorderColor = XLColor.Black;
                }
            }

            // Автоматически подстраиваем ширину столбцов под содержимое
            worksheet.Columns().AdjustToContents();

            // Сохраняем рабочую книгу в новый файл Excel
            try
            {
                // Сохраняем рабочую книгу в новый файл Excel
                workbook.SaveAs(filePath);

                // Показываем сообщение об успешном сохранении
                MessageBox.Show("Таблица Excel успешно создана и сохранена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                // Показываем сообщение об ошибке
                MessageBox.Show($"Ошибка при сохранении таблицы Excel: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Import_Excel()
        {
            // Открывает диалог выбора файла
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Excel files (*.xlsx)|*.xlsx";
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (var workbook = new ClosedXML.Excel.XLWorkbook(openFileDialog.FileName))
                    {
                        var worksheet = workbook.Worksheet(1);
                        var range = worksheet.RangeUsed();

                        // Создаем список продуктов
                        var products = new List<Product>();

                        foreach (var row in range.RowsUsed().Skip(1)) // Пропускаем заголовок
                        {
                            var product = new Product
                            {
                                ProductId = row.Cell(1).GetValue<int>(),
                                Name = row.Cell(2).GetValue<string>(),
                                Material = row.Cell(3).GetValue<string>(),
                                Quantity = row.Cell(4).GetValue<int>(),
                                MinQuantity = row.Cell(5).GetValue<int>(),
                                Price = row.Cell(6).GetValue<decimal>(),
                            };
                            products.Add(product);
                        }

                        // Установка источника данных для DataGrid
                        DataGrid.ItemsSource = products;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при чтении файла: " + ex.Message);
                }
            }
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SaveToDatabase();
        }

        private void Button_Click_Load(object sender, RoutedEventArgs e)
        {
            LoadDataFromDatabase();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            Products.Clear();
        }

        private void UpdateDatabase_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите заменить данные?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                PasteWithReplace();
            }
            else
            {
            }
        }

        private void AddFil_Click(object sender, RoutedEventArgs e)
        {
            LoadFilDatabase();
        }

        private void Button_Clear(object sender, RoutedEventArgs e)
        {
            Mfil.Text = "";
            Nfil.Text = "";
        }

        private void Mfil_Click(object sender, EventArgs e)
        {
            FillList(Mfil, "Material");
        }

        private void Nfil_Click(object sender, EventArgs e)
        {
            FillList(Nfil, "Name");
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Вы уверены удалить данные из базы данных безвозвратно?", "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes) 
            {
                DeleteSelectedProducts();
            }
            else { }
        }

        private void AddNewWindow_Click(object sender, RoutedEventArgs e)
        {
            AddProductWindow addProductWindow = new AddProductWindow();
            addProductWindow.Show();
        }

        private void Nedostacha_Click(object sender, RoutedEventArgs e)
        {
            Nedostacha();
        }

        private void CreateOtchet_Click(object sender, RoutedEventArgs e)
        {
            Othet();
        }

        private void CreateExcel_Click(object sender, RoutedEventArgs e)
        {
            // Создаем диалоговое окно для выбора пути и имени нового файла Excel
            Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.Filter = "Excel Files (*.xlsx, *.xlsm, *.xls) | *.xlsx;*.xlsm;*.xls";

            // Показываем диалоговое окно и ожидаем выбора пользователем пути и имени нового файла
            if (dialog.ShowDialog() == true)
            {
                // Создаем новый файл Excel по выбранному пользователем пути и имени
                Excel_Create(dialog.FileName);
            }
        }

        private void DeleteMarker_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Вы уверены удалить выделенные данные из базы данных безвозратно?", "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes) 
            {
                DeleteMarkerDatabase();
            }
            else { }
        }

        private void ClearButtonMarker_Click(object sender, RoutedEventArgs e)
        {
            // Получаем выбранные элементы из DataGrid
            var selectedProducts = DataGrid.SelectedItems.Cast<Product>().ToList();

            // Удаляем выбранные элементы из коллекции, связанной с DataGrid
            foreach (var product in selectedProducts)
            {
                Products.Remove(product);
            }
        }

        private void ImportExcel_Click(object sender, RoutedEventArgs e)
        {
            Import_Excel();
        }
    }
}