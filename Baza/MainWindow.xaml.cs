using System.Collections.ObjectModel;
using System.Windows;
using System;
using System.Data.SQLite;
using System.Collections.Generic;


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

        private void SaveToDatabase() // Сохранить из таблицы
        {
            List<string> messages = new List<string>(); // Создаем список для сообщений

            using (var connection = new SQLiteConnection(connectionString))
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
                        command.ExecuteNonQuery();
                    }
                }
            }

            // Выводим все предупреждения из списка в одном окне
            if (messages.Count > 0)
            {
                string allMessages = string.Join("\n", messages) + "\nУже существуют в базе данных!";
                MessageBox.Show(allMessages, "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
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

                using (SQLiteCommand command = new SQLiteCommand(query, connection)) // Объявление переменной command
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        // Проверяем наличие результатов
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

        private void Nfil_spisoc()
        {
            Nfil.Items.Clear();

            // Используем HashSet для хранения уникальных значений
            HashSet<string> uniqueNames = new HashSet<string>();

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                Console.WriteLine("Executing query: SELECT Name FROM Products"); // Log the query
                var command = new SQLiteCommand("SELECT Name FROM Products", connection);
                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string productName = reader["Name"].ToString();

                    // Проверяем, было ли слово уже добавлено
                    if (!uniqueNames.Contains(productName))
                    {
                        Nfil.Items.Add(productName);
                        uniqueNames.Add(productName); // Добавляем слово в HashSet
                    }
                }

                reader.Close();
                connection.Close();
            }
        }

        private void Mfil_spisoc()
        {
            Mfil.Items.Clear();

            // Используем HashSet для хранения уникальных значений
            HashSet<string> uniqueMaterial = new HashSet<string>();

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                Console.WriteLine("Executing query: SELECT Material FROM Products"); // Log the query
                var command = new SQLiteCommand("SELECT Material FROM Products", connection);
                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string productMaterial = reader["Material"].ToString();

                    // Проверяем, было ли слово уже добавлено
                    if (!uniqueMaterial.Contains(productMaterial))
                    {
                        Mfil.Items.Add(productMaterial);
                        uniqueMaterial.Add(productMaterial); // Добавляем слово в HashSet
                    }
                }

                reader.Close();
                connection.Close();
            }
        }



        private ObservableCollection<Product> GetProducts()
        {
            ObservableCollection<Product> products = new ObservableCollection<Product>();

            return products;
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
            Mfil_spisoc();
        }

        private void Nfil_Click(object sender, EventArgs e)
        {
            Nfil_spisoc();
        }
    }
}