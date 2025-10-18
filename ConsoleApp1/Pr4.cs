using System.Runtime.InteropServices;

class Pr4
{
    public enum DishCategory
    {
        Breakfasts,
        Appetizers,
        Salads,
        Soups,
        MainCourses,
        Sides,
        Desserts,
        Drinks
    }

    public static string GetCategoryRussianName(DishCategory category)
    {
        return category switch
        {
            DishCategory.Breakfasts => "Завтраки",
            DishCategory.Appetizers => "Закуски",
            DishCategory.Salads => "Салаты",
            DishCategory.Soups => "Супы",
            DishCategory.MainCourses => "Основные блюда",
            DishCategory.Sides => "Гарниры",
            DishCategory.Desserts => "Десерты",
            DishCategory.Drinks => "Напитки",
            _ => category.ToString()
        };
    }

    public static string Input(string prompt = "")
    {
        string? input = null;
        while (string.IsNullOrEmpty(input))
        {
            Console.Write(prompt);
            input = Console.ReadLine();
        }
        return input;
    }

    public static int InputInt(string prompt)
    {
        while (true)
        {
            try
            {
                return Convert.ToInt32(Input(prompt));
            }
            catch (FormatException)
            {
                Console.WriteLine("Ошибка: введите целое число");
            }
        }
    }

    public static double InputDouble(string prompt)
    {
        while (true)
        {
            try
            {
                return Convert.ToDouble(Input(prompt));
            }
            catch (FormatException)
            {
                Console.WriteLine("Ошибка: введите число");
            }
        }
    }

    class Dish
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public string Composition { get; private set; }
        public string Weight { get; private set; }
        public double Price { get; private set; }
        public DishCategory Category { get; private set; }
        public int CookingTime { get; private set; }
        public List<string> Type { get; private set; }

        private static int _lastId = 0;

        public Dish(
            string name,
            string composition,
            string weight,
            double price,
            DishCategory category,
            int cookingTime,
            List<string> type)
        {
            _lastId++;

            Name = name;
            Composition = composition;
            Weight = weight;
            Price = price;
            Category = category;
            CookingTime = cookingTime;
            Type = type;
            Id = _lastId;
        }

        public static Dish Create()
        {
            Console.WriteLine("Создание нового блюда:");

            string? name = Input("Введите название блюда: ");
            string? composition = Input("Введите состав блюда: ");
            string? weight = Input("Введите вес: ");
            double price = InputDouble("Введите цену: ");
            Console.WriteLine("Выберите категорию блюда:");
            Console.WriteLine("0 - Завтраки");
            Console.WriteLine("1 - Закуски");
            Console.WriteLine("2 - Салаты");
            Console.WriteLine("3 - Супы");
            Console.WriteLine("4 - Основные блюда");
            Console.WriteLine("5 - Гарниры");
            Console.WriteLine("6 - Десерты");
            Console.WriteLine("7 - Напитки");
            DishCategory category = (DishCategory)Convert.ToInt32(Input("Введите номер категории: "));
            int cookingTime = InputInt("Введите время приготовления (в минутах): ");
            string? typesInput = Input("Введите типы блюда (через запятую: Вегетарианское,Острое...): ");
            List<string>? type = typesInput?.Split(',').Select(t => t.Trim()).ToList();
            type ??= [];

            return new Dish(name, composition, weight, price, category, cookingTime, type);
        }

        public void Edit(
            string name = "",
            string composition = "",
            string weight = "",
            double price = -1,
            DishCategory? category = null,
            int cookingTime = -1,
            List<string>? type = null)
        {
            if (!string.IsNullOrEmpty(name))
                Name = name;
            if (!string.IsNullOrEmpty(composition))
                Composition = composition;
            if (!string.IsNullOrEmpty(weight))
                Weight = weight;
            if (!double.IsNegative(price))
                Price = price;
            if (category != null)
                Category = category.Value;
            if (!int.IsNegative(cookingTime))
                CookingTime = cookingTime;
            if (type != null)
                Type = type;

        }

        public static Dish? FindById(List<Dish> dishList, int id)
        {
            return dishList.Find(t => t.Id == id);
        }

        public void DisplayInfo()
        {
            Console.WriteLine(
                "-------------------------------------------------\n" +
                $"БЛЮДО: {Name}\n" +
                $"ID: {Id}\n" +
                $"Категория: {GetCategoryRussianName(Category)}\n" +
                $"Цена: {Price:F2}р\n" +
                $"Вес: {Weight}г\n" +
                $"Время приготовления: {CookingTime} мин\n" +
                $"Состав: {Composition}\n"
            );

            if (Type != null && Type.Count > 0)
            {
                Console.WriteLine("Типы:");
                foreach (var type in Type)
                    Console.WriteLine($"  - {type}");
            }
            else
                Console.WriteLine("Типы: не указаны");
        }

        public static void DisplayMenu(List<Dish> dish)
        {
            Console.WriteLine("\n");
            foreach (var category in Enum.GetValues<DishCategory>())
            {
                Console.WriteLine($"\n{GetCategoryRussianName(category).ToUpper()}:");
                foreach (var d in dish.Where(t => t.Category == category).ToList())
                {
                    Console.WriteLine(
                        $"\"{d.Id}.{d.Name}\" - {d.Weight}г. | {d.Price}р.     ---     {d.Type.First()}!\n" +
                        $"  Состав: {d.Composition.ToLower()}"
                        );
                }
            }
        }

    }

    class Order
    {
        public int Id { get; private set; }
        public int TableId { get; private set; }
        public Dictionary<Dish, int> DishDictionary;
        public string Comment { get; private set; }
        public DateTime OrderAcceptanceTime { get; private set; }
        public int Waiter { get; private set; }
        public DateTime? OrderClosingTime { get; private set; }
        public double TotalCost { get; private set; }
        public bool IsClosed => OrderClosingTime != null;

        private static int _lastId = 0;

        public Order(int tableId, Dictionary<Dish, int> dishDictionary, int waiter, string comment = "")
        {
            _lastId++;
            Id = _lastId;
            TableId = tableId;
            DishDictionary = dishDictionary;
            Waiter = waiter;
            Comment = comment;
            OrderAcceptanceTime = DateTime.Now;
            OrderClosingTime = null;
            TotalCost = 0;
            UpdateTotalCost();
        }

        private void UpdateTotalCost() => TotalCost = DishDictionary.Sum(t => t.Key.Price * t.Value);

        public static Order? Create(List<Dish> menu)
        {
            Console.WriteLine("\nСОЗДАНИЕ НОВОГО ЗАКАЗА");

            int tableId = InputInt("Введите номер стола: ");
            int waiter = InputInt("Введите номер официанта: ");
            string comment = Input("Введите комментарий: ");

            Dictionary<Dish, int> dishDictionary = new Dictionary<Dish, int>();

            while (true)
            {
                Console.WriteLine("\nДоступные блюда:");
                Dish.DisplayMenu(menu);

                int dishId = InputInt("\nВведите ID блюда для добавления (0 - завершить заказ): ");

                if (dishId == 0)
                    break;

                var selectedDish = Dish.FindById(menu, dishId);
                if (selectedDish == null)
                {
                    Console.WriteLine("Блюдо с таким ID не найдено!");
                    continue;
                }

                int quantity = InputInt($"Введите количество для \"{selectedDish.Name}\": ");

                if (dishDictionary.ContainsKey(selectedDish))
                    dishDictionary[selectedDish] += quantity;
                else
                    dishDictionary.Add(selectedDish, quantity);

                Console.WriteLine("\nТекущий заказ:");
                DisplayOrderItems(dishDictionary);
            }

            if (dishDictionary.Count == 0)
            {
                Console.WriteLine("Заказ не создан - нет блюд!");
                return null;
            }

            return new Order(tableId, dishDictionary, waiter, comment);
        }

        public void DisplayInfo(bool detailed = true)
        {
            Console.WriteLine($"\nИНФОРМАЦИЯ О ЗАКАЗЕ #{Id}");
            Console.WriteLine($"Стол: {TableId}");
            Console.WriteLine($"Официант: {Waiter}");
            Console.WriteLine($"Время принятия: {OrderAcceptanceTime:dd.MM.yyyy HH:mm}");
            Console.WriteLine($"Статус: {(IsClosed ? "ЗАКРЫТ" : "ОТКРЫТ")}");

            if (IsClosed)
                Console.WriteLine($"Время закрытия: {OrderClosingTime:dd.MM.yyyy HH:mm}");

            if (detailed)
            {
                Console.WriteLine("\nСостав заказа:");
                DisplayOrderItems(DishDictionary);
            }

            Console.WriteLine($"\nОбщая стоимость: {TotalCost:F2}р");

            if (!string.IsNullOrEmpty(Comment))
                Console.WriteLine($"Комментарий: {Comment}");
        }

        public void Close()
        {
            if (IsClosed)
            {
                Console.WriteLine("Заказ уже закрыт!");
                return;
            }

            OrderClosingTime = DateTime.Now;
            UpdateTotalCost();
            Console.WriteLine($"Заказ #{Id} закрыт. Общая стоимость: {TotalCost:F2}р");
        }

        public void PrintCheck()
        {
            if (!IsClosed)
            {
                Console.WriteLine("Невозможно распечатать чек для открытого заказа!");
                return;
            }

            Console.WriteLine("\n" + new string('=', 40));
            Console.WriteLine("             ЧЕК");
            Console.WriteLine(new string('=', 40));
            Console.WriteLine($"Заказ #: {Id}");
            Console.WriteLine($"Стол: {TableId}");
            Console.WriteLine($"Официант: {Waiter}");
            Console.WriteLine($"Время принятия: {OrderAcceptanceTime:dd.MM.yyyy HH:mm}");
            Console.WriteLine($"Время закрытия: {OrderClosingTime:dd.MM.yyyy HH:mm}");
            Console.WriteLine(new string('-', 40));

            foreach (var item in DishDictionary)
            {
                double itemTotal = item.Key.Price * item.Value;
                Console.WriteLine($"{item.Key.Name,-20} x{item.Value,2} {itemTotal,8:F2}р");
                Console.WriteLine($"  ({item.Key.Weight}g)");
            }

            Console.WriteLine(new string('-', 40));
            Console.WriteLine($"ИТОГО: {TotalCost,26:F2}р");

            if (!string.IsNullOrEmpty(Comment))
            {
                Console.WriteLine(new string('-', 40));
                Console.WriteLine($"Комментарий: {Comment}");
            }

            Console.WriteLine(new string('=', 40));
            Console.WriteLine("       СПАСИБО ЗА ВИЗИТ!");
            Console.WriteLine(new string('=', 40));
        }

        public static Order? FindById(List<Order> orderList, int id)
        {
            return orderList.Find(o => o.Id == id);
        }

        public void Edit(List<Dish> menu)
        {
            if (IsClosed)
            {
                Console.WriteLine("Невозможно изменить закрытый заказ!");
                return;
            }

            Console.WriteLine("ИЗМЕНЕНИЕ ЗАКАЗА");

            while (true)
            {
                Console.WriteLine("\nТекущий состав заказа:");
                DisplayOrderItems(DishDictionary);

                Console.WriteLine("\nОпции изменения:");
                Console.WriteLine("1 - Добавить блюдо");
                Console.WriteLine("2 - Изменить количество блюда");
                Console.WriteLine("3 - Удалить блюдо");
                Console.WriteLine("4 - Изменить комментарий");
                Console.WriteLine("5 - Завершить изменение");

                int choice = InputInt("Выберите опцию: ");

                switch (choice)
                {
                    case 1:
                        AddDishToOrder(menu);
                        break;
                    case 2:
                        ChangeDishQuantity();
                        break;
                    case 3:
                        RemoveDishFromOrder();
                        break;
                    case 4:
                        Comment = Input("Введите новый комментарий: ");
                        Console.WriteLine("Комментарий обновлен!");
                        break;
                    case 5:
                        UpdateTotalCost();
                        Console.WriteLine("Изменения сохранены!");
                        return;
                    default:
                        Console.WriteLine("Неверный выбор!");
                        break;
                }
            }
        }

        private void AddDishToOrder(List<Dish> menu)
        {
            Console.WriteLine("\nДоступные блюда:");
            Dish.DisplayMenu(menu);

            int dishId = InputInt("Введите ID блюда для добавления: ");
            var selectedDish = Dish.FindById(menu, dishId);

            if (selectedDish == null)
            {
                Console.WriteLine("Блюдо с таким ID не найдено!");
                return;
            }

            int quantity = InputInt($"Введите количество для \"{selectedDish.Name}\": ");

            if (DishDictionary.ContainsKey(selectedDish))
                DishDictionary[selectedDish] += quantity;
            else
                DishDictionary.Add(selectedDish, quantity);

            Console.WriteLine("Блюдо добавлено в заказ!");
        }

        private void ChangeDishQuantity()
        {
            if (DishDictionary.Count == 0)
            {
                Console.WriteLine("В заказе нет блюд!");
                return;
            }

            int dishId = InputInt("Введите ID блюда для изменения количества: ");
            var dish = DishDictionary.Keys.FirstOrDefault(d => d.Id == dishId);

            if (dish == null)
            {
                Console.WriteLine("Блюдо с таким ID не найдено в заказе!");
                return;
            }

            int newQuantity = InputInt($"Введите новое количество для \"{dish.Name}\": ");

            if (newQuantity <= 0)
            {
                DishDictionary.Remove(dish);
                Console.WriteLine("Блюдо удалено из заказа!");
            }
            else
            {
                DishDictionary[dish] = newQuantity;
                Console.WriteLine("Количество обновлено!");
            }
        }

        private void RemoveDishFromOrder()
        {
            if (DishDictionary.Count == 0)
            {
                Console.WriteLine("В заказе нет блюд!");
                return;
            }

            int dishId = InputInt("Введите ID блюда для удаления: ");
            var dish = DishDictionary.Keys.FirstOrDefault(d => d.Id == dishId);

            if (dish == null)
            {
                Console.WriteLine("Блюдо с таким ID не найдено в заказе!");
                return;
            }

            DishDictionary.Remove(dish);
            Console.WriteLine("Блюдо удалено из заказа!");
        }

        private static void DisplayOrderItems(Dictionary<Dish, int> dishDictionary)
        {
            if (dishDictionary.Count == 0)
            {
                Console.WriteLine("  Заказ пуст");
                return;
            }

            double total = 0;
            foreach (var item in dishDictionary)
            {
                double itemTotal = item.Key.Price * item.Value;
                total += itemTotal;
                Console.WriteLine($"(id:{item.Key.Id}) {item.Key.Name} x{item.Value} = {itemTotal:F2}р");
            }
            Console.WriteLine($"Итого: {total:F2}р");

        }
    }

    class ShowOrderManagementMenu
    {
        private List<Dish> menu;
        private List<Order> _orders;

        public ShowOrderManagementMenu(List<Dish> initialMenu)
        {
            menu = initialMenu;
            _orders = new List<Order>();
        }

        public void ShowMainMenu(out List<Order> orders)
        {
            while (true)
            {
                Console.WriteLine("\n СИСТЕМА УПРАВЛЕНИЯ ЗАКАЗАМИ");
                Console.WriteLine("1. Создание заказа");
                Console.WriteLine("2. Изменение заказа");
                Console.WriteLine("3. Просмотр информации о заказе");
                Console.WriteLine("4. Закрытие заказа");
                Console.WriteLine("5. Печать чека");
                Console.WriteLine("6. Просмотр меню");
                Console.WriteLine("7. Просмотр всех заказов");
                Console.WriteLine("0. Выход");

                int choice = InputInt("Выберите действие: ");

                switch (choice)
                {
                    case 1:
                        CreateOrder();
                        break;
                    case 2:
                        EditOrder();
                        break;
                    case 3:
                        DisplayOrderInfo();
                        break;
                    case 4:
                        CloseOrder();
                        break;
                    case 5:
                        PrintCheck();
                        break;
                    case 6:
                        Dish.DisplayMenu(menu);
                        break;
                    case 7:
                        DisplayAllOrders();
                        break;
                    case 0:
                        orders = _orders;
                        return;
                    default:
                        Console.WriteLine("Неверный выбор!");
                        break;
                }
            }
        }

        public List<Order> GetOrders()
        {
            return _orders;
        }

        private void CreateOrder()
        {
            var order = Order.Create(menu);
            if (order != null)
            {
                _orders.Add(order);
                Console.WriteLine($"\nЗаказ #{order.Id} успешно создан!");
                order.DisplayInfo();
            }
        }

        private void EditOrder()
        {
            if (_orders.Count == 0)
            {
                Console.WriteLine("Нет доступных заказов!");
                return;
            }

            DisplayAllOrders();
            int orderId = InputInt("Введите ID заказа для изменения: ");
            var order = Order.FindById(_orders, orderId);

            if (order == null)
            {
                Console.WriteLine("Заказ с таким ID не найден!");
                return;
            }

            order.Edit(menu);
        }

        private void DisplayOrderInfo()
        {
            if (_orders.Count == 0)
            {
                Console.WriteLine("Нет доступных заказов!");
                return;
            }

            DisplayAllOrders();
            int orderId = InputInt("Введите ID заказа для просмотра: ");
            var order = Order.FindById(_orders, orderId);

            if (order == null)
            {
                Console.WriteLine("Заказ с таким ID не найден!");
                return;
            }

            order.DisplayInfo();
        }

        private void CloseOrder()
        {
            if (_orders.Count == 0)
            {
                Console.WriteLine("Нет доступных заказов!");
                return;
            }

            var openOrders = _orders.Where(o => !o.IsClosed).ToList();
            if (openOrders.Count == 0)
            {
                Console.WriteLine("Нет открытых заказов!");
                return;
            }

            Console.WriteLine("Открытые заказы:");
            foreach (var o in openOrders)
            {
                o.DisplayInfo(false);
            }

            int orderId = InputInt("Введите ID заказа для закрытия: ");
            var order = Order.FindById(_orders, orderId);

            if (order == null)
            {
                Console.WriteLine("Заказ с таким ID не найден!");
                return;
            }

            if (order.IsClosed)
            {
                Console.WriteLine("Заказ уже закрыт!");
                return;
            }

            order.Close();
        }

        private void PrintCheck()
        {
            if (_orders.Count == 0)
            {
                Console.WriteLine("Нет доступных заказов!");
                return;
            }

            var closedOrders = _orders.Where(o => o.IsClosed).ToList();
            if (closedOrders.Count == 0)
            {
                Console.WriteLine("Нет закрытых заказов!");
                return;
            }

            Console.WriteLine("Закрытые заказы:");
            foreach (var o in closedOrders)
            {
                o.DisplayInfo(false);
            }

            int orderId = InputInt("Введите ID заказа для печати чека: ");
            var order = Order.FindById(_orders, orderId);

            if (order == null)
            {
                Console.WriteLine("Заказ с таким ID не найден!");
                return;
            }

            order.PrintCheck();
        }

        private void DisplayAllOrders()
        {
            if (_orders.Count == 0)
            {
                Console.WriteLine("Нет заказов!");
                return;
            }

            Console.WriteLine("\nВСЕ ЗАКАЗЫ:");
            foreach (var order in _orders)
            {
                order.DisplayInfo(false);
                Console.WriteLine();
            }
        }
    }

    class DishManager
    {
        private List<Dish> _menu;

        public DishManager(List<Dish> initialMenu)
        {
            _menu = initialMenu;
        }

        public void ShowDishManagementMenu(out List<Dish> menu)
        {
            while (true)
            {
                Console.WriteLine("\n  УПРАВЛЕНИЕ БЛЮДАМИ  ");
                Console.WriteLine("1. Создать новое блюдо");
                Console.WriteLine("2. Изменить существующее блюдо");
                Console.WriteLine("3. Удалить блюдо");
                Console.WriteLine("4. Просмотреть все блюда");
                Console.WriteLine("5. Поиск блюда по ID");
                Console.WriteLine("6. Поиск блюд по категории");
                Console.WriteLine("0. Назад в главное меню");

                int choice = InputInt("Выберите действие: ");

                switch (choice)
                {
                    case 1:
                        CreateDish();
                        break;
                    case 2:
                        EditDish();
                        break;
                    case 3:
                        DeleteDish();
                        break;
                    case 4:
                        DisplayAllDishes();
                        break;
                    case 5:
                        FindDishById();
                        break;
                    case 6:
                        FindDishesByCategory();
                        break;
                    case 0:
                        menu = _menu;
                        return;
                    default:
                        Console.WriteLine("Неверный выбор!");
                        break;
                }
            }
        }

        private void CreateDish()
        {
            Console.WriteLine("\n  СОЗДАНИЕ НОВОГО БЛЮДА  ");
            var newDish = Dish.Create();
            _menu.Add(newDish);
            Console.WriteLine($"\nБлюдо \"{newDish.Name}\" успешно создано с ID: {newDish.Id}");
            newDish.DisplayInfo();
        }

        private void EditDish()
        {
            if (_menu.Count == 0)
            {
                Console.WriteLine("Меню пусто! Нет блюд для изменения.");
                return;
            }

            DisplayAllDishes();
            int dishId = InputInt("\nВведите ID блюда для изменения: ");
            var dish = Dish.FindById(_menu, dishId);

            if (dish == null)
            {
                Console.WriteLine("Блюдо с таким ID не найдено!");
                return;
            }

            Console.WriteLine($"\nРедактирование блюда: {dish.Name} (ID: {dish.Id})");
            Console.WriteLine("Текущая информация:");
            dish.DisplayInfo();

            Console.WriteLine("\nВведите новые данные (оставьте пустым для сохранения текущего значения):");

            string newName = Input($"Название [{dish.Name}]: ");
            string newComposition = Input($"Состав [{dish.Composition}]: ");
            string newWeight = Input($"Вес [{dish.Weight}]: ");

            double newPrice = -1;
            string priceInput = Input($"Цена [{dish.Price}]: ");
            if (!string.IsNullOrEmpty(priceInput))
                double.TryParse(priceInput, out newPrice);

            DishCategory? newCategory = null;
            Console.WriteLine("Категории:");
            foreach (var category in Enum.GetValues<DishCategory>())
            {
                Console.WriteLine($"{(int)category} - {GetCategoryRussianName(category)}");
            }
            string categoryInput = Input($"Категория [{(int)dish.Category}]: ");
            if (!string.IsNullOrEmpty(categoryInput) && Enum.IsDefined(typeof(DishCategory), int.Parse(categoryInput)))
                newCategory = (DishCategory)int.Parse(categoryInput);

            int newCookingTime = -1;
            string cookingTimeInput = Input($"Время приготовления [{dish.CookingTime}]: ");
            if (!string.IsNullOrEmpty(cookingTimeInput))
                int.TryParse(cookingTimeInput, out newCookingTime);

            List<string>? newTypes = null;
            string typesInput = Input($"Типы [{string.Join(", ", dish.Type)}]: ");
            if (!string.IsNullOrEmpty(typesInput))
                newTypes = typesInput.Split(',').Select(t => t.Trim()).Where(t => !string.IsNullOrEmpty(t)).ToList();

            dish.Edit(
                name: string.IsNullOrEmpty(newName) ? "" : newName,
                composition: string.IsNullOrEmpty(newComposition) ? "" : newComposition,
                weight: string.IsNullOrEmpty(newWeight) ? "" : newWeight,
                price: newPrice,
                category: newCategory,
                cookingTime: newCookingTime,
                type: newTypes
            );

            Console.WriteLine($"\nБлюдо \"{dish.Name}\" успешно обновлено!");
            dish.DisplayInfo();
        }

        private void DeleteDish()
        {
            if (_menu.Count == 0)
            {
                Console.WriteLine("Меню пусто! Нет блюд для удаления.");
                return;
            }

            DisplayAllDishes();
            int dishId = InputInt("\nВведите ID блюда для удаления: ");
            var dish = Dish.FindById(_menu, dishId);

            if (dish == null)
            {
                Console.WriteLine("Блюдо с таким ID не найдено!");
                return;
            }

            Console.WriteLine($"\nВы уверены, что хотите удалить блюдо \"{dish.Name}\"?");
            Console.WriteLine("1 - Да, удалить");
            Console.WriteLine("2 - Нет, отменить");

            int confirmation = InputInt("Ваш выбор: ");

            if (confirmation == 1)
            {
                _menu.Remove(dish);
                Console.WriteLine($"Блюдо \"{dish.Name}\" успешно удалено!");
            }
            else
            {
                Console.WriteLine("Удаление отменено.");
            }
        }

        private void DisplayAllDishes()
        {
            if (_menu.Count == 0)
            {
                Console.WriteLine("Меню пусто!");
                return;
            }

            Console.WriteLine("\n  ВСЕ БЛЮДА В МЕНЮ  ");
            foreach (var dish in _menu)
            {
                dish.DisplayInfo();
            }
        }

        private void FindDishById()
        {
            if (_menu.Count == 0)
            {
                Console.WriteLine("Меню пусто!");
                return;
            }

            int dishId = InputInt("Введите ID блюда для поиска: ");
            var dish = Dish.FindById(_menu, dishId);

            if (dish == null)
            {
                Console.WriteLine("Блюдо с таким ID не найдено!");
                return;
            }

            Console.WriteLine("\nНайденное блюдо:");
            dish.DisplayInfo();
        }

        private void FindDishesByCategory()
        {
            if (_menu.Count == 0)
            {
                Console.WriteLine("Меню пусто!");
                return;
            }

            Console.WriteLine("Выберите категорию для поиска:");
            foreach (var category in Enum.GetValues<DishCategory>())
            {
                Console.WriteLine($"{(int)category} - {GetCategoryRussianName(category)}");
            }

            int categoryId = InputInt("Введите номер категории: ");
            if (!Enum.IsDefined(typeof(DishCategory), categoryId))
            {
                Console.WriteLine("Неверный номер категории!");
                return;
            }

            var selectedCategory = (DishCategory)categoryId;
            var dishesInCategory = _menu.Where(d => d.Category == selectedCategory).ToList();

            if (dishesInCategory.Count == 0)
            {
                Console.WriteLine($"В категории \"{GetCategoryRussianName(selectedCategory)}\" нет блюд.");
                return;
            }

            Console.WriteLine($"\n  БЛЮДА В КАТЕГОРИИ: {GetCategoryRussianName(selectedCategory).ToUpper()}  ");
            foreach (var dish in dishesInCategory)
            {
                dish.DisplayInfo();
            }
        }

        public List<Dish> GetMenu()
        {
            return _menu;
        }
    }

    class OrderManager
    {
        private List<Dish> menu;
        private List<Order> orders;
        private DishManager dishManager;
        private ShowOrderManagementMenu showOrderManagementMenu;

        public OrderManager(List<Dish> initialMenu)
        {
            menu = initialMenu;
            orders = new List<Order>();
            dishManager = new DishManager(menu);
            showOrderManagementMenu = new ShowOrderManagementMenu(menu);
        }

        public double CalculateTotalClosedOrdersRevenue()
        {
            double totalRevenue = orders
                .Where(order => order.IsClosed)
                .Sum(order => order.TotalCost);

            Console.WriteLine($"\n  ОБЩАЯ ВЫРУЧКА ПО ЗАКРЫТЫМ ЗАКАЗАМ  ");
            Console.WriteLine($"Количество закрытых заказов: {orders.Count(o => o.IsClosed)}");
            Console.WriteLine($"Общая выручка: {totalRevenue:F2}р");

            return totalRevenue;
        }

        public void CalculateWaiterClosedOrders(int waiterId)
        {
            var waiterOrders = orders
                .Where(order => order.IsClosed && order.Waiter == waiterId)
                .ToList();

            int orderCount = waiterOrders.Count;
            double totalRevenue = waiterOrders.Sum(order => order.TotalCost);

            Console.WriteLine($"\n  СТАТИСТИКА ОФИЦИАНТА #{waiterId}  ");
            Console.WriteLine($"Количество закрытых заказов: {orderCount}");
            Console.WriteLine($"Общая выручка: {totalRevenue:F2}р");

            if (orderCount > 0)
            {
                Console.WriteLine("\nДетализация заказов:");
                foreach (var order in waiterOrders)
                {
                    Console.WriteLine($"Заказ #{order.Id} - {order.TotalCost:F2}р ({order.OrderAcceptanceTime:dd.MM.yyyy})");
                }

                double averageOrder = totalRevenue / orderCount;
                Console.WriteLine($"Средний чек: {averageOrder:F2}р");
            }
        }

        public void CalculateDishStatistics()
        {
            var dishStats = new Dictionary<string, (int Quantity, double Revenue)>();

            foreach (var order in orders.Where(o => o.IsClosed))
            {
                foreach (var (dish, quantity) in order.DishDictionary)
                {
                    string dishName = dish.Name;
                    double dishRevenue = dish.Price * quantity;

                    if (dishStats.ContainsKey(dishName))
                    {
                        dishStats[dishName] = (
                            dishStats[dishName].Quantity + quantity,
                            dishStats[dishName].Revenue + dishRevenue
                        );
                    }
                    else
                    {
                        dishStats[dishName] = (quantity, dishRevenue);
                    }
                }
            }

            Console.WriteLine($"\n  СТАТИСТИКА ПО БЛЮДАМ  ");
            Console.WriteLine($"Всего закрытых заказов: {orders.Count(o => o.IsClosed)}");

            if (dishStats.Count == 0)
            {
                Console.WriteLine("Нет данных о заказанных блюдах.");
                return;
            }

            var sortedStats = dishStats.OrderByDescending(x => x.Value.Quantity).ToList();

            Console.WriteLine("\nТоп блюд по количеству заказов:");
            Console.WriteLine("#        Блюдо               Количество             Выручка");

            for (int i = 0; i < Math.Min(10, sortedStats.Count); i++)
            {
                var (dishName, (quantity, revenue)) = sortedStats[i];
                string truncatedName = dishName.Length > 20 ? dishName.Substring(0, 17) + "..." : dishName.PadRight(20);
                Console.WriteLine($"{i + 1,-3}      {truncatedName}     {quantity,-8}       {revenue,9:F2}р");
            }

            Console.WriteLine($"\nОбщее количество проданных блюд: {sortedStats.Sum(x => x.Value.Quantity)}");
            Console.WriteLine($"Общая выручка от блюд: {sortedStats.Sum(x => x.Value.Revenue):F2}р");

            var mostPopular = sortedStats.First();
            Console.WriteLine($"Самое популярное блюдо: \"{mostPopular.Key}\" ({mostPopular.Value.Quantity} заказов)");
        }

        public void CalculatePeriodStatistics(DateTime startDate, DateTime endDate)
        {
            var periodOrders = orders
                .Where(order => order.IsClosed &&
                               order.OrderClosingTime >= startDate &&
                               order.OrderClosingTime <= endDate)
                .ToList();

            Console.WriteLine($"\n  СТАТИСТИКА ЗА ПЕРИОД  ");
            Console.WriteLine($"С {startDate:dd.MM.yyyy} по {endDate:dd.MM.yyyy}");
            Console.WriteLine($"Количество заказов: {periodOrders.Count}");
            Console.WriteLine($"Общая выручка: {periodOrders.Sum(o => o.TotalCost):F2}р");

            if (periodOrders.Count > 0)
            {
                Console.WriteLine($"Средний чек: {periodOrders.Average(o => o.TotalCost):F2}р");
            }
        }

        public void ShowMainMenu()
        {
            while (true)
            {
                Console.WriteLine("\n  СИСТЕМА УПРАВЛЕНИЯ РЕСТОРАНОМ  ");
                Console.WriteLine("1. Управление заказами");
                Console.WriteLine("2. Управление блюдами");
                Console.WriteLine("3. Просмотр меню");
                Console.WriteLine("4. Статистика и отчеты");
                Console.WriteLine("0. Выход");

                int choice = InputInt("Выберите действие: ");

                switch (choice)
                {
                    case 1:
                        showOrderManagementMenu.ShowMainMenu(out orders);
                        break;
                    case 2:
                        dishManager.ShowDishManagementMenu(out menu);
                        break;
                    case 3:
                        Dish.DisplayMenu(menu);
                        break;
                    case 4:
                        ShowStatisticsMenu();
                        break;
                    case 0:
                        return;
                    default:
                        Console.WriteLine("Неверный выбор!");
                        break;
                }
            }
        }

        private void ShowStatisticsMenu()
        {
            while (true)
            {
                Console.WriteLine("\n  СТАТИСТИКА И ОТЧЕТЫ  ");
                Console.WriteLine("1. Общая выручка по закрытым заказам");
                Console.WriteLine("2. Статистика официанта");
                Console.WriteLine("3. Статистика по блюдам");
                Console.WriteLine("4. Общая сводка");
                Console.WriteLine("0. Назад");

                int choice = InputInt("Выберите отчет: ");

                switch (choice)
                {
                    case 1:
                        CalculateTotalClosedOrdersRevenue();
                        break;
                    case 2:
                        int waiterId = InputInt("Введите ID официанта: ");
                        CalculateWaiterClosedOrders(waiterId);
                        break;
                    case 3:
                        CalculateDishStatistics();
                        break;
                    case 4:
                        ShowGeneralSummary();
                        break;
                    case 0:
                        return;
                    default:
                        Console.WriteLine("Неверный выбор!");
                        break;
                }
            }
        }

        private void ShowGeneralSummary()
        {
            Console.WriteLine($"\nОБЩАЯ СВОДКА");
            Console.WriteLine($"Всего заказов: {orders.Count}");
            Console.WriteLine($"Открытых заказов: {orders.Count(o => !o.IsClosed)}");
            Console.WriteLine($"Закрытых заказов: {orders.Count(o => o.IsClosed)}");

            if (orders.Count(o => o.IsClosed) > 0)
            {
                CalculateTotalClosedOrdersRevenue();
                Console.WriteLine();
                CalculateDishStatistics();
            }

            var waiterStats = orders
                .Where(o => o.IsClosed)
                .GroupBy(o => o.Waiter)
                .Select(g => new { WaiterId = g.Key, OrderCount = g.Count(), Revenue = g.Sum(o => o.TotalCost) })
                .OrderByDescending(w => w.Revenue)
                .ToList();

            if (waiterStats.Count > 0)
            {
                Console.WriteLine($"\nТОП ОФИЦИАНТОВ");
                foreach (var waiter in waiterStats)
                {
                    Console.WriteLine($"Официант #{waiter.WaiterId}: {waiter.OrderCount} заказов, {waiter.Revenue:F2}р");
                }
            }
        }
    }

    public Pr4()
    {
        Dish d1 = new("Овсяная каша", "Овсяные хлопья, Молоко, Мед, Фрукты", "300", 180.50, DishCategory.Breakfasts, 15, ["Вегетарианское", "Полезное"]);
        Dish d2 = new("Яичница с беконом", "Яйца, Бекон, Помидоры, Зелень", "250", 220.00, DishCategory.Breakfasts, 10, ["Классическое"]);
        Dish d3 = new("Брускетта", "Хлеб, Помидоры, Чеснок, Базилик, Оливковое масло", "150", 190.75, DishCategory.Appetizers, 5, ["Вегетарианское", "Итальянское"]);
        Dish d4 = new("Карпаччо", "Говядина, Пармезан, Руккола, Оливковое масло", "120", 350.00, DishCategory.Appetizers, 8, ["Итальянское"]);
        Dish d5 = new("Цезарь", "Курица, Салат айсберг, Пармезан, Сухарики, Соус цезарь", "200", 280.00, DishCategory.Salads, 10, ["Популярное"]);
        Dish d6 = new("Греческий", "Помидоры, Огурцы, Сыр фета, Маслины, Лук, Оливковое масло", "180", 240.50, DishCategory.Salads, 7, ["Вегетарианское", "Греческое"]);
        Dish d7 = new("Борщ", "Вода, Мясо, Свекла, Капуста, Картофель, Сметана", "250", 200.00, DishCategory.Soups, 45, ["Халяль"]);
        Dish d8 = new("Куриный суп", "Курица, Лапша, Морковь, Лук, Зелень", "300", 190.00, DishCategory.Soups, 30, ["Классическое"]);
        Dish d9 = new("Стейк Рибай", "Говядина, Специи, Соус", "350", 890.00, DishCategory.MainCourses, 25, ["Премиум", "Мясное"]);
        Dish d10 = new("Лосось на гриле", "Лосось, Лимон, Укроп, Оливковое масло", "280", 650.50, DishCategory.MainCourses, 20, ["Рыбное", "Полезное"]);
        Dish d11 = new("Картофель фри", "Картофель, Растительное масло, Соль", "200", 120.00, DishCategory.Sides, 12, ["Вегетарианское"]);
        Dish d12 = new("Гречка", "Гречневая крупа, Вода, Соль, Масло", "150", 100.50, DishCategory.Sides, 20, ["Вегетарианское", "Полезное"]);
        Dish d13 = new("Тирамису", "Сыр маскарпоне, Печенье савоярди, Кофе, Какао", "180", 320.00, DishCategory.Desserts, 15, ["Итальянское", "С кофе"]);
        Dish d14 = new("Чизкейк", "Творожный сыр, Печенье, Сливочное масло, Сахар", "200", 280.75, DishCategory.Desserts, 0, ["Классическое"]);
        Dish d15 = new("Капучино", "Кофе, Молоко", "200", 150.00, DishCategory.Drinks, 5, ["Горячее", "С кофе"]);
        Dish d16 = new("Мохито", "Лайм, Мята, Содовая, Лед, Сахарный сироп", "400", 220.50, DishCategory.Drinks, 3, ["Освежающее", "Алкогольное"]);
        List<Dish> dishList = [d1, d2, d3, d4, d5, d6, d7, d8, d9, d10, d11, d12, d13, d14, d15, d16];

        OrderManager orderManager = new(dishList);
        orderManager.ShowMainMenu();
    }
}

