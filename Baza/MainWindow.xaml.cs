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
            Products = DatabaseHelper.GetProducts();
        }

    public ObservableCollection<Product> Products { get; set; }

        private void SaveToDatabase()
        {
            string connectionString = "Data Source=C:\\Users\\Vadim\\Desktop\\DiplomSoft\\Baza\\Baza\\DataBase.db";

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                foreach (var product in Products)
                {
                    string query = $"INSERT INTO Products (ProductId, Name, Quantity, MinQuantity, Price) " +
                                   $"VALUES ({product.ProductId}, '{product.Name}', {product.Quantity}, {product.MinQuantity}, {product.Price})";

                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.ExecuteNonQuery();
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
    }
}