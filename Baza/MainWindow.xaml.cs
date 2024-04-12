using System.Collections.ObjectModel;
using System.Windows;
using System;


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
            Products = GetProducts();
        }

        public ObservableCollection<Product> Products { get; set; }

        private ObservableCollection<Product> GetProducts()
        {
            ObservableCollection<Product> products = new ObservableCollection<Product>();

            // Добавляем несколько тестовых продуктов
            products.Add(new Product { ProductId = 1, Name = "Product 1", Quantity = 10, Price = 10.99m });
            products.Add(new Product { ProductId = 2, Name = "Product 2", Quantity = 15, Price = 20.49m });
            products.Add(new Product { ProductId = 3, Name = "Product 3", Quantity = 6, Price = 15.29m });

            return products;
        }

    }
}
