using System.Collections.ObjectModel;
using System.Windows;
using System;
using System.Data.SQLite;


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
        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;
            
        }

        public ObservableCollection<Product> Products { get; set; } = new ObservableCollection<Product>();

        private void SaveToDatabase()
        {
            string connectionString = "Data Source=C:\\Users\\Vadim\\Desktop\\DiplomSoft\\Baza\\Baza\\DataBase.db";

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
                            // Если продукт с таким ID уже существует, выведем предупреждение
                            MessageBox.Show($"Продукт с ID {product.ProductId} уже существует в базе данных!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
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
        }

        private void LoadDataFromDatabase()
        {
            string connectionString = "Data Source=C:\\Users\\Vadim\\Desktop\\DiplomSoft\\Baza\\Baza\\DataBase.db";
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

        public void LoadFilDatabase()
        {
            string connectionString = "Data Source=C:\\Users\\Vadim\\Desktop\\DiplomSoft\\Baza\\Baza\\DataBase.db";
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

        private void PasteWithReplace()
        {
            string connectionString = "Data Source=C:\\Users\\Vadim\\Desktop\\DiplomSoft\\Baza\\Baza\\DataBase.db";

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
    }
}