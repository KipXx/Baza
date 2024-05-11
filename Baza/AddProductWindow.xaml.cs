using System.Windows;
using System.Data.SQLite;
using System.Windows.Input;

namespace Baza
{

    public partial class AddProductWindow : Window
    {
        public AddProductWindow()
        {
            InitializeComponent();

        }

        private static string connectionString = "Data Source=./DataBase.db";

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем, заполнены ли все текстовые поля
            if (string.IsNullOrWhiteSpace(IdTextBox.Text) ||
                string.IsNullOrWhiteSpace(NameTextBox.Text) ||
                string.IsNullOrWhiteSpace(MaterialTextBox.Text) ||
                string.IsNullOrWhiteSpace(QuantityTextBox.Text) ||
                string.IsNullOrWhiteSpace(MinQuantityTextBox.Text) ||
                string.IsNullOrWhiteSpace(PriceTextBox.Text))
            {
                // Если какое-то поле не заполнено, показываем предупреждение
                MessageBox.Show("Заполните все поля!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return; // Прекращаем выполнение метода, так как не все поля заполнены
            }

            // Получите данные из текстовых полей
            int id = int.Parse(IdTextBox.Text);
            string name = NameTextBox.Text;
            string material = MaterialTextBox.Text;
            int quantity = int.Parse(QuantityTextBox.Text);
            int minQuantity = int.Parse(MinQuantityTextBox.Text);
            decimal price = decimal.Parse(PriceTextBox.Text);

            // Создайте подключение к базе данных SQLite
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // Создайте SQL-запрос для вставки новой записи, который будет игнорировать вставку, если запись уже существует
                string sql = "INSERT OR IGNORE INTO Products (ProductId, Name, Material, Quantity, MinQuantity, Price) VALUES (@ProductId, @Name, @Material, @Quantity, @MinQuantity, @Price)";
                using (var command = new SQLiteCommand(sql, connection))
                {
                    // Параметризуйте запрос, чтобы избежать SQL-инъекций
                    command.Parameters.AddWithValue("@ProductId", id);
                    command.Parameters.AddWithValue("@Name", name);
                    command.Parameters.AddWithValue("@Material", material);
                    command.Parameters.AddWithValue("@Quantity", quantity);
                    command.Parameters.AddWithValue("@MinQuantity", minQuantity);
                    command.Parameters.AddWithValue("@Price", price);

                    // Выполните запрос
                    int rowsAffected = command.ExecuteNonQuery();

                    // Если ни одна запись не была добавлена (т.е. продукт уже существует), показать предупреждение
                    if (rowsAffected == 0)
                    {
                        MessageBox.Show("Продукт с таким ID уже существует.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        MessageBox.Show("Продукт успешно добавлен в базу данных.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        Close(); // Закрываем окно только если продукт был успешно добавлен
                    }
                }

                // Закройте подключение
                connection.Close();
            }
        }

        public void OnlyNumber(TextCompositionEventArgs e)
        {
            // Проверяем, является ли введенный символ цифрой или не числом
            if (!char.IsDigit(e.Text, 0))
            {
                e.Handled = true; // Если это не цифра, игнорируем ввод
            }
        }

        private void OnlyNumbers(object sender, TextCompositionEventArgs e)
        {
            OnlyNumber(e);
        }

        

    }
}
