using System.Diagnostics;
using System.Net;
using static System.Runtime.InteropServices.JavaScript.JSType;

class Pr5
{
    enum WarehouseType
    {
        Cold,
        Sorting,
        Shared,
        Recycling
    }

    class Warehouse
    {
        public int Id { get; private set; }
        public WarehouseType Type { get; private set; }
        public double Volume { get; private set; }
        public double FreeVolume => Volume - OccupiedVolume;
        public double OccupiedVolume => _products.Sum(p => p.VolumePerUnit);
        public string Address { get; private set; }
        public int ProductsCount => _products.Count;
        public IReadOnlyList<Product> Products => _products.AsReadOnly();

        private readonly List<Product> _products;
        private static int _lastWarehouseId = 0;

        internal Warehouse(WarehouseType type, double volume, string address)
        {
            Id = ++_lastWarehouseId;
            _products = [];
            Type = type;
            Volume = volume;
            Address = address ?? throw new ArgumentNullException(nameof(address));
        }

        internal void Edit(WarehouseType? type = null, double? volume = null, string? address = null)
        {
            if (type != null)
                Type = type.Value;

            if (volume.HasValue)
            {
                if (volume.Value < OccupiedVolume)
                    throw new InvalidOperationException($"Новый объем ({volume.Value}) меньше занятого объема ({OccupiedVolume})");
                Volume = volume.Value;
            }

            if (!string.IsNullOrWhiteSpace(address))
                Address = address;
        }

        internal string GetInformation()
        {
            return $"""
            ID: {Id}
            Тип: {Type}
            Адрес: {Address}
            Объем: {Volume:F2}
            Занятый объем: {OccupiedVolume:F2}
            Свободный объем: {FreeVolume:F2}
            Количество товаров: {ProductsCount}
            Общая стоимость товаров: {CalculateTotalProductsValue():C2}
            """;
        }

        internal bool CanAcceptProduct(Product product)
        {
            return FreeVolume >= product.VolumePerUnit;
        }

        internal bool AddProduct(Product product)
        {
            if (!CanAcceptProduct(product))
                return false;

            _products.Add(product);
            return true;
        }

        internal int AddProducts(IEnumerable<Product> products)
        {
            var addedCount = 0;
            foreach (var product in products ?? [])
            {
                if (AddProduct(product))
                    addedCount++;
            }
            return addedCount;
        }

        internal bool RemoveProduct(Product product)
        {
            return _products.Remove(product);
        }

        internal void ClearProducts()
        {
            _products.Clear();
        }

        internal bool ContainsProduct(int productId)
        {
            return _products.Any(p => p.Id == productId);
        }

        internal IEnumerable<Product> GetExpiredProducts()
        {
            return _products.Where(p => p.IsExpired);
        }

        internal IEnumerable<Product> GetProductsBySupplier(int supplierId)
        {
            return _products.Where(p => p.SupplierId == supplierId);
        }

        internal decimal CalculateTotalProductsValue()
        {
            return _products.Sum(p => p.PricePerUnit);
        }
    }

    class Product
    {
        public int Id { get; }
        public int SupplierId { get; private set; }
        public string Name { get; private set; }
        public double VolumePerUnit { get; private set; }
        public decimal PricePerUnit { get; private set; }
        public int ExpirationDateRemaining { get; private set; }

        private static int _lastProductId = 0;

        public Product(int supplierId, string name, double volumePerUnit, decimal pricePerUnit, int expirationDateRemaining)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Название товара не может быть пустым");
            if (volumePerUnit <= 0)
                throw new ArgumentException("Объем товара должен быть положительным");
            if (pricePerUnit < 0)
                throw new ArgumentException("Цена не может быть отрицательной");

            Id = ++_lastProductId;
            SupplierId = supplierId;
            Name = name;
            VolumePerUnit = volumePerUnit;
            PricePerUnit = pricePerUnit;
            ExpirationDateRemaining = expirationDateRemaining;
        }

        internal void Edit(int? supplierId = null, string? name = null, double? volumePerUnit = null, decimal? pricePerUnit = null, int? expirationDateRemaining = null)
        {
            if (supplierId.HasValue)
                SupplierId = supplierId.Value;

            if (!string.IsNullOrWhiteSpace(name))
                Name = name;

            if (volumePerUnit.HasValue)
            {
                if (volumePerUnit.Value <= 0)
                    throw new ArgumentException("Объем товара должен быть положительным");
                VolumePerUnit = volumePerUnit.Value;
            }

            if (pricePerUnit.HasValue)
            {
                if (pricePerUnit.Value < 0)
                    throw new ArgumentException("Цена не может быть отрицательной");
                PricePerUnit = pricePerUnit.Value;
            }

            if (expirationDateRemaining.HasValue)
                ExpirationDateRemaining = expirationDateRemaining.Value;
        }

        internal string GetInformation()
        {
            return $"""
            ID: {Id}
            ID Поставщика: {SupplierId}
            Название: {Name}
            Объем на единицу: {VolumePerUnit:F2}
            Цена на единицу: {PricePerUnit:C2}
            Срок годности: {ExpirationDateRemaining} дней
            """;
        }

        internal bool IsExpired => ExpirationDateRemaining <= 0;
    }

    class LogisticsManager
    {
        private readonly List<Warehouse> _warehouses;
        private readonly List<LogEntry> _logs;

        public LogisticsManager()
        {
            _warehouses = [];
            _logs = [];
        }

        public bool ProcessTheDelivery(List<Product> delivery)
        {
            if (delivery == null || delivery.Count == 0)
            {
                AddLog("Поставка", "Ошибка: пустая поставка", "Отказ", 0);
                return false;
            }

            int countPerishable = delivery.Count(p => p.ExpirationDateRemaining < 30);
            int countLongTerm = delivery.Count - countPerishable;

            WarehouseType targetType;
            if (countPerishable == delivery.Count)
                targetType = WarehouseType.Cold;
            else if (countLongTerm == delivery.Count)
                targetType = WarehouseType.Shared;
            else
                targetType = WarehouseType.Sorting;

            var suitableWarehouses = FindSuitableWarehouses(targetType, delivery);
            if (suitableWarehouses.Count == 0)
            {
                AddLog("Поставка", "Нет подходящих складов", "Отказ", delivery.Sum(p => p.VolumePerUnit));
                return false;
            }

            return DistributeProducts(delivery, suitableWarehouses, "Поставка");
        }

        private List<Warehouse> FindSuitableWarehouses(WarehouseType type, List<Product> delivery)
        {
            var totalVolume = delivery.Sum(p => p.VolumePerUnit);
            var warehouses = _warehouses
                .Where(w => w.Type == type && w.FreeVolume >= totalVolume)
                .OrderByDescending(w => w.FreeVolume)
                .ToList();

            if (warehouses.Count == 0 && type != WarehouseType.Recycling)
            {
                warehouses = FindWarehouseCombination(type, delivery);
            }

            return warehouses;
        }

        private List<Warehouse> FindWarehouseCombination(WarehouseType type, List<Product> delivery)
        {
            var result = new List<Warehouse>();
            var remainingProducts = new List<Product>(delivery);
            var availableWarehouses = _warehouses
                .Where(w => w.Type == type && w.FreeVolume > 0)
                .OrderByDescending(w => w.FreeVolume)
                .ToList();

            foreach (var warehouse in availableWarehouses)
            {
                var productsThatFit = remainingProducts
                    .Where(p => p.VolumePerUnit <= warehouse.FreeVolume)
                    .ToList();

                if (productsThatFit.Count != 0)
                {
                    result.Add(warehouse);
                    remainingProducts = remainingProducts.Except(productsThatFit).ToList();
                }

                if (remainingProducts.Count == 0) break;
            }

            return remainingProducts.Count != 0 ? [] : result;
        }

        private bool DistributeProducts(List<Product> products, List<Warehouse> warehouses, string source)
        {
            bool success = true;
            List<Product> remainingProducts = new(products);

            foreach (var warehouse in warehouses)
            {
                if (remainingProducts.Count == 0) break;

                List<Product> productsToAdd = remainingProducts
                    .Where(p => warehouse.CanAcceptProduct(p))
                    .ToList();

                if (productsToAdd.Count != 0)
                {
                    int addedCount = warehouse.AddProducts(productsToAdd);
                    if (addedCount > 0)
                    {
                        foreach (var product in productsToAdd)
                        {
                            AddLog(product.Name, source, $"Склад {warehouse.Id} ({warehouse.Type})",
                                  product.VolumePerUnit);
                        }

                        remainingProducts = remainingProducts.Except(productsToAdd).ToList();
                    }
                }
            }

            if (remainingProducts.Count != 0)
            {
                AddLog("Поставка", source, "Не распределено", remainingProducts.Sum(p => p.VolumePerUnit));
                success = false;
            }

            return success;
        }

        public bool OptimizeWarehouse(Warehouse warehouse)
        {
            if (warehouse == null)
                return false;

            if (warehouse.Type == WarehouseType.Sorting)
            {
                bool success = true;
                var productsToMove = warehouse.Products.ToList();

                foreach (var product in productsToMove)
                {
                    WarehouseType targetType = product.ExpirationDateRemaining < 30
                        ? WarehouseType.Cold
                        : WarehouseType.Shared;

                    var targetWarehouse = FindSuitableWarehouses(targetType, [product])
                        .FirstOrDefault(w => w.CanAcceptProduct(product));

                    if (targetWarehouse != null)
                    {
                        if (MoveProduct(product, warehouse, targetWarehouse))
                        {
                            AddLog(product.Name, $"Склад {warehouse.Id} ({warehouse.Type})",
                                  $"Склад {targetWarehouse.Id} ({targetType})", product.VolumePerUnit);
                        }
                        else
                        {
                            success = false;
                            AddLog(product.Name, $"Склад {warehouse.Id} ({warehouse.Type})",
                                  $"Ошибка перемещения на склад {targetWarehouse.Id}", product.VolumePerUnit);
                        }
                    }
                    else
                    {
                        success = false;
                        AddLog(product.Name, $"Склад {warehouse.Id} ({warehouse.Type})",
                              "Не найден подходящий склад", product.VolumePerUnit);
                    }
                }
                return success;
            }

            List<Product> productsToMoveFromCurrent = [];
            List<Warehouse> suitableWarehouses = [];
            List<Product> suitableProducts = [];

            switch (warehouse.Type)
            {
                case WarehouseType.Cold:
                    productsToMoveFromCurrent = warehouse.Products
                        .Where(p => p.ExpirationDateRemaining >= 30)
                        .ToList();
                    suitableWarehouses = FindSuitableWarehouses(WarehouseType.Shared, productsToMoveFromCurrent);
                    suitableProducts = productsToMoveFromCurrent;
                    break;

                case WarehouseType.Shared:
                    productsToMoveFromCurrent = warehouse.Products
                        .Where(p => p.ExpirationDateRemaining < 30)
                        .ToList();
                    suitableWarehouses = FindSuitableWarehouses(WarehouseType.Cold, productsToMoveFromCurrent);
                    suitableProducts = productsToMoveFromCurrent;
                    break;

                case WarehouseType.Recycling:
                    AddLog("Утилизация", $"Склад {warehouse.Id} ({warehouse.Type})", "Оптимизация не требуется", 0);
                    return true;

                default:
                    return false;
            }

            if (suitableWarehouses.Count == 0 && suitableProducts.Count != 0)
            {
                AddLog(warehouse.Type.ToString(), $"Склад {warehouse.Id} ({warehouse.Type})",
                      "Нет подходящих складов для оптимизации", suitableProducts.Sum(p => p.VolumePerUnit));
                return false;
            }

            return DistributeProducts(suitableProducts, suitableWarehouses, $"Склад {warehouse.Id} ({warehouse.Type})");
        }

        private bool MoveProduct(Product product, Warehouse fromWarehouse, Warehouse toWarehouse)
        {
            if (product == null || fromWarehouse == null || toWarehouse == null)
                return false;

            if (!fromWarehouse.ContainsProduct(product.Id))
            {
                AddLog(product.Name, $"Склад {fromWarehouse.Id}", "Ошибка: товар не найден", product.VolumePerUnit);
                return false;
            }

            if (!toWarehouse.CanAcceptProduct(product))
            {
                AddLog(product.Name, $"Склад {fromWarehouse.Id} ({fromWarehouse.Type})",
                      $"Склад {toWarehouse.Id} ({toWarehouse.Type}) (недостаточно места)", product.VolumePerUnit);
                return false;
            }

            if (fromWarehouse.RemoveProduct(product) && toWarehouse.AddProduct(product))
            {
                return true;
            }
            else
            {
                fromWarehouse.AddProduct(product);
                AddLog(product.Name, $"Склад {fromWarehouse.Id} ({fromWarehouse.Type})", "Ошибка перемещения", product.VolumePerUnit);
                return false;
            }
        }

        public bool OptimizeAllWarehousesByType(WarehouseType type)
        {
            var warehousesToOptimize = _warehouses.Where(w => w.Type == type).ToList();
            bool overallSuccess = true;

            foreach (var warehouse in warehousesToOptimize)
            {
                if (!OptimizeWarehouse(warehouse))
                {
                    overallSuccess = false;
                }
            }

            return overallSuccess;
        }

        public bool OptimizeAllWarehouses()
        {
            bool overallSuccess = true;

            overallSuccess &= OptimizeAllWarehousesByType(WarehouseType.Sorting);
            overallSuccess &= OptimizeAllWarehousesByType(WarehouseType.Cold);
            overallSuccess &= OptimizeAllWarehousesByType(WarehouseType.Shared);

            return overallSuccess;
        }

        public string AnalyzeWarehouse(Warehouse warehouse)
        {
            if (warehouse == null) return "Склад не найден";

            var violations = new List<string>();

            switch (warehouse.Type)
            {
                case WarehouseType.Cold:
                    var longTermOnCold = warehouse.Products.Count(p => p.ExpirationDateRemaining >= 30);
                    if (longTermOnCold > 0)
                        violations.Add($"Найдено {longTermOnCold} товаров с длительным сроком хранения");
                    break;

                case WarehouseType.Shared:
                    var perishableOnShared = warehouse.Products.Count(p => p.ExpirationDateRemaining < 30);
                    if (perishableOnShared > 0)
                        violations.Add($"Найдено {perishableOnShared} скоропортящихся товаров");
                    break;

                case WarehouseType.Sorting:
                    var productsOnSorting = warehouse.Products.Count;
                    if (productsOnSorting > 0)
                        violations.Add($"Найдено {productsOnSorting} товаров, требующих распределения");
                    break;
            }

            var expiredProducts = warehouse.GetExpiredProducts().Count();
            if (expiredProducts > 0)
                violations.Add($"Найдено {expiredProducts} просроченных товаров");

            if (violations.Count == 0)
                return $"Склад {warehouse.Id} ({warehouse.Type}): Нарушений нет";

            return $"Склад {warehouse.Id} ({warehouse.Type}): {string.Join("; ", violations)}";
        }

        public void MoveExpiredProductsToRecycling()
        {
            foreach (var warehouse in _warehouses.Where(w => w.Type != WarehouseType.Recycling))
            {
                var expiredProducts = warehouse.GetExpiredProducts().ToList();

                foreach (var product in expiredProducts)
                {
                    var recyclingWarehouse = FindSuitableWarehouses(WarehouseType.Recycling,
                        [product]).FirstOrDefault();

                    if (recyclingWarehouse != null && warehouse.RemoveProduct(product))
                    {
                        recyclingWarehouse.AddProduct(product);
                        AddLog(product.Name, $"Склад {warehouse.Id} ({warehouse.Type})",
                              $"Склад {recyclingWarehouse.Id} (Утилизация)", product.VolumePerUnit);
                    }
                }
            }
        }

        public void AddLog(string productName, string from, string to, double volume)
        {
            var logEntry = new LogEntry
            {
                Timestamp = DateTime.Now,
                ProductName = productName,
                From = from,
                To = to,
                Volume = volume
            };
            _logs.Add(logEntry);
        }

        public IReadOnlyList<Warehouse> GetWarehouses()
        {
            return _warehouses.AsReadOnly();
        }

        public IReadOnlyList<LogEntry> GetLogs()
        {
            return _logs.AsReadOnly();
        }

        public void AddWarehouse(Warehouse warehouse)
        {
            _warehouses.Add(warehouse);
        }
    }

    class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public required string ProductName { get; set; }
        public required string From { get; set; }
        public required string To { get; set; }
        public double Volume { get; set; }
        public override string ToString()
        {
            string shortenedProductName = ProductName.Length > 20 ?
                string.Concat(ProductName.AsSpan(0, 15), "...") : ProductName;

            string shortenedFrom = From.Length > 22 ?
                string.Concat(From.AsSpan(0, 19), "...") : From;

            string shortenedTo = To.Length > 22 ?
                string.Concat(To.AsSpan(0, 19), "...") : To;

            return $"{Timestamp:yyyy-MM-dd HH:mm:ss} | " +
                   $"{shortenedProductName,-20} | " +
                   $"Объем: {Volume,7:F2} | " +
                   $"От: {shortenedFrom,-22} | " +
                   $"К: {shortenedTo,-22}";
        }
    }

    class WarehouseConsoleMenu
    {
        private readonly LogisticsManager _logisticsManager;

        public WarehouseConsoleMenu()
        {
            _logisticsManager = new LogisticsManager();
            InitializeSampleData();
        }

        private void InitializeSampleData()
        {
            _logisticsManager.AddWarehouse(new Warehouse(WarehouseType.Cold, 500, "Холодильная, 1"));
            _logisticsManager.AddWarehouse(new Warehouse(WarehouseType.Sorting, 800, "Сортировочная, 2"));
            _logisticsManager.AddWarehouse(new Warehouse(WarehouseType.Shared, 1200, "Основная, 3"));
            _logisticsManager.AddWarehouse(new Warehouse(WarehouseType.Recycling, 300, "Утилизационная, 4"));
        }

        public void ShowMainMenu()
        {
            while (true)
            {
                try
                {
                    Console.WriteLine("\n=== СИСТЕМА УПРАВЛЕНИЯ СКЛАДАМИ ===");
                    Console.WriteLine("1. Управление складами");
                    Console.WriteLine("2. Управление товарами");
                    Console.WriteLine("3. Логистические операции");
                    Console.WriteLine("4. Отчеты и аналитика");
                    Console.WriteLine("5. Просмотр логов");
                    Console.WriteLine("0. Выход");

                    Console.Write("Выберите действие: ");
                    int choice = Convert.ToInt32(Console.ReadLine());

                    switch (choice)
                    {
                        case 1:
                            ShowWarehouseMenu();
                            break;
                        case 2:
                            ShowProductMenu();
                            break;
                        case 3:
                            ShowLogisticsMenu();
                            break;
                        case 4:
                            ShowReportsMenu();
                            break;
                        case 5:
                            ShowAllLogs();
                            break;
                        case 0:
                            Console.WriteLine("Выход из программы");
                            return;
                        default:
                            Console.WriteLine("Неверный выбор!");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                }
            }
        }

        private void ShowWarehouseMenu()
        {
            while (true)
            {
                Console.WriteLine("\n--- УПРАВЛЕНИЕ СКЛАДАМИ ---");
                Console.WriteLine("1. Создать склад");
                Console.WriteLine("2. Редактировать склад");
                Console.WriteLine("3. Информация о складе");
                Console.WriteLine("4. Список всех складов");
                Console.WriteLine("5. Удалить товары со склада");
                Console.WriteLine("0. Назад");

                Console.Write("Выберите действие: ");
                int choice = Convert.ToInt32(Console.ReadLine());

                try
                {
                    switch (choice)
                    {
                        case 1:
                            CreateWarehouse();
                            break;
                        case 2:
                            EditWarehouse();
                            break;
                        case 3:
                            ShowWarehouseInfo();
                            break;
                        case 4:
                            ListAllWarehouses();
                            break;
                        case 5:
                            ClearWarehouseProducts();
                            break;
                        case 0:
                            return;
                        default:
                            Console.WriteLine("Неверный выбор!");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                }
            }
        }

        private void ShowProductMenu()
        {
            while (true)
            {
                Console.WriteLine("\n--- УПРАВЛЕНИЕ ТОВАРАМИ ---");
                Console.WriteLine("1. Создать товар");
                Console.WriteLine("2. Редактировать товар");
                Console.WriteLine("3. Информация о товаре");
                Console.WriteLine("4. Найти товар по ID");
                Console.WriteLine("5. Список просроченных товаров");
                Console.WriteLine("0. Назад");

                Console.Write("Выберите действие: ");
                int choice = Convert.ToInt32(Console.ReadLine());

                try
                {
                    switch (choice)
                    {
                        case 1:
                            CreateProduct();
                            break;
                        case 2:
                            EditProduct();
                            break;
                        case 3:
                            ShowProductInfo();
                            break;
                        case 4:
                            FindProductById();
                            break;
                        case 5:
                            ShowExpiredProducts();
                            break;
                        case 0:
                            return;
                        default:
                            Console.WriteLine("Неверный выбор!");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                }
            }
        }

        private void ShowLogisticsMenu()
        {
            while (true)
            {
                Console.WriteLine("\n--- ЛОГИСТИЧЕСКИЕ ОПЕРАЦИИ ---");
                Console.WriteLine("1. Обработать поставку");
                Console.WriteLine("2. Оптимизировать склад");
                Console.WriteLine("3. Оптимизировать все склады");
                Console.WriteLine("4. Переместить товар между складами");
                Console.WriteLine("5. Переместить просроченные товары в утилизацию");
                Console.WriteLine("0. Назад");

                Console.Write("Выберите действие: ");
                int choice = Convert.ToInt32(Console.ReadLine());

                try
                {
                    switch (choice)
                    {
                        case 1:
                            ProcessDelivery();
                            break;
                        case 2:
                            OptimizeWarehouse();
                            break;
                        case 3:
                            OptimizeAllWarehouses();
                            break;
                        case 4:
                            MoveProductBetweenWarehouses();
                            break;
                        case 5:
                            MoveExpiredToRecycling();
                            break;
                        case 0:
                            return;
                        default:
                            Console.WriteLine("Неверный выбор!");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                }
            }
        }

        private void ShowReportsMenu()
        {
            while (true)
            {
                Console.WriteLine("\n--- ОТЧЕТЫ И АНАЛИТИКА ---");
                Console.WriteLine("1. Анализ склада");
                Console.WriteLine("2. Анализ всех складов");
                Console.WriteLine("3. Общая статистика");
                Console.WriteLine("4. Финансовый отчет");
                Console.WriteLine("5. Товары по поставщику");
                Console.WriteLine("0. Назад");

                Console.Write("Выберите действие: ");
                int choice = Convert.ToInt32(Console.ReadLine());

                try
                {
                    switch (choice)
                    {
                        case 1:
                            AnalyzeWarehouse();
                            break;
                        case 2:
                            AnalyzeAllWarehouses();
                            break;
                        case 3:
                            ShowGeneralStatistics();
                            break;
                        case 4:
                            ShowFinancialReport();
                            break;
                        case 5:
                            ShowProductsBySupplier();
                            break;
                        case 0:
                            return;
                        default:
                            Console.WriteLine("Неверный выбор!");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                }
            }
        }

        private void CreateWarehouse()
        {
            Console.WriteLine("Типы складов: 1-Холодильный, 2-Сортировочный, 3-Общий, 4-Утилизация");
            Console.Write("Выберите тип склада: ");
            int typeChoice = Convert.ToInt32(Console.ReadLine());

            WarehouseType type = typeChoice switch
            {
                1 => WarehouseType.Cold,
                2 => WarehouseType.Sorting,
                3 => WarehouseType.Shared,
                4 => WarehouseType.Recycling,
                _ => throw new ArgumentException("Неверный тип склада")
            };

            Console.Write("Введите объем склада: ");
            double volume = Convert.ToDouble(Console.ReadLine());

            Console.Write("Введите адрес склада: ");
            string address = Console.ReadLine()!;

            var warehouse = new Warehouse(type, volume, address);
            _logisticsManager.AddWarehouse(warehouse);
            Console.WriteLine($"Склад создан с ID: {warehouse.Id}");
        }

        private void EditWarehouse()
        {
            Console.Write("Введите ID склада: ");
            int id = Convert.ToInt32(Console.ReadLine());

            var warehouse = _logisticsManager.GetWarehouses().FirstOrDefault(w => w.Id == id);
            if (warehouse == null)
            {
                Console.WriteLine("Склад не найден");
                return;
            }

            Console.WriteLine("Типы складов: 1-Холодильный, 2-Сортировочный, 3-Общий, 4-Утилизация");
            Console.Write("Введите новый тип склада (или Enter для пропуска): ");
            string typeInput = Console.ReadLine()!;
            WarehouseType? newType = null;
            if (!string.IsNullOrEmpty(typeInput))
            {
                int typeChoice = Convert.ToInt32(typeInput);
                newType = typeChoice switch
                {
                    1 => WarehouseType.Cold,
                    2 => WarehouseType.Sorting,
                    3 => WarehouseType.Shared,
                    4 => WarehouseType.Recycling,
                    _ => throw new ArgumentException("Неверный тип склада")
                };
            }

            Console.Write("Введите новый объем (или Enter для пропуска): ");
            string volumeInput = Console.ReadLine()!;
            double? newVolume = null;
            if (!string.IsNullOrEmpty(volumeInput))
            {
                newVolume = Convert.ToDouble(volumeInput);
            }

            Console.Write("Введите новый адрес (или Enter для пропуска): ");
            string newAddress = Console.ReadLine()!;

            warehouse.Edit(newType, newVolume, newAddress);
            Console.WriteLine("Склад успешно отредактирован");
        }

        private void ShowWarehouseInfo()
        {
            Console.Write("Введите ID склада: ");
            int id = Convert.ToInt32(Console.ReadLine());

            var warehouse = _logisticsManager.GetWarehouses().FirstOrDefault(w => w.Id == id);
            if (warehouse == null)
            {
                Console.WriteLine("Склад не найден");
                return;
            }

            Console.WriteLine(warehouse.GetInformation());

            if (warehouse.ProductsCount > 0)
            {
                Console.WriteLine("\nТовары на складе:");
                foreach (var product in warehouse.Products)
                {
                    Console.WriteLine($"- {product.Name} (ID: {product.Id}, Объем: {product.VolumePerUnit:F2})");
                }
            }
        }

        private void ListAllWarehouses()
        {
            var warehouses = _logisticsManager.GetWarehouses();
            if (!warehouses.Any())
            {
                Console.WriteLine("Нет созданных складов");
                return;
            }

            foreach (var warehouse in warehouses)
            {
                Console.WriteLine(warehouse.GetInformation());
                Console.WriteLine("---");
            }
        }

        private void ClearWarehouseProducts()
        {
            Console.Write("Введите ID склада: ");
            int id = Convert.ToInt32(Console.ReadLine());

            var warehouse = _logisticsManager.GetWarehouses().FirstOrDefault(w => w.Id == id);
            if (warehouse == null)
            {
                Console.WriteLine("Склад не найден");
                return;
            }

            warehouse.ClearProducts();
            Console.WriteLine("Все товары удалены со склада");
        }

        private void CreateProduct()
        {
            Console.Write("Введите ID поставщика: ");
            int supplierId = Convert.ToInt32(Console.ReadLine());

            Console.Write("Введите название товара: ");
            string name = Console.ReadLine()!;

            Console.Write("Введите объем единицы товара: ");
            double volume = Convert.ToDouble(Console.ReadLine());

            Console.Write("Введите цену единицы товара: ");
            decimal price = Convert.ToDecimal(Console.ReadLine());

            Console.Write("Введите срок годности (в днях): ");
            int expiration = Convert.ToInt32(Console.ReadLine());

            var product = new Product(supplierId, name, volume, price, expiration);
            Console.WriteLine($"Товар создан с ID: {product.Id}");

            Console.Write("Добавить товар на склад? (д/н): ");
            if (Console.ReadLine()!.ToLower() == "д")
            {
                AddProductToWarehouse(product);
            }
        }

        private void EditProduct()
        {
            Console.Write("Введите ID товара: ");
            int id = Convert.ToInt32(Console.ReadLine());

            var product = FindProductInAnyWarehouse(id);
            if (product == null)
            {
                Console.WriteLine("Товар не найден");
                return;
            }

            Console.Write("Введите новый ID поставщика (или Enter для пропуска): ");
            string supplierInput = Console.ReadLine()!;
            int? newSupplierId = null;
            if (!string.IsNullOrEmpty(supplierInput))
            {
                newSupplierId = Convert.ToInt32(supplierInput);
            }

            Console.Write("Введите новое название (или Enter для пропуска): ");
            string newName = Console.ReadLine()!;

            Console.Write("Введите новый объем (или Enter для пропуска): ");
            string volumeInput = Console.ReadLine()!;
            double? newVolume = null;
            if (!string.IsNullOrEmpty(volumeInput))
            {
                newVolume = Convert.ToDouble(volumeInput);
            }

            Console.Write("Введите новую цену (или Enter для пропуска): ");
            string priceInput = Console.ReadLine()!;
            decimal? newPrice = null;
            if (!string.IsNullOrEmpty(priceInput))
            {
                newPrice = Convert.ToDecimal(priceInput);
            }

            Console.Write("Введите новый срок годности (или Enter для пропуска): ");
            string expirationInput = Console.ReadLine()!;
            int? newExpiration = null;
            if (!string.IsNullOrEmpty(expirationInput))
            {
                newExpiration = Convert.ToInt32(expirationInput);
            }

            product.Edit(newSupplierId, newName, newVolume, newPrice, newExpiration);
            Console.WriteLine("Товар успешно отредактирован");
        }

        private void ShowProductInfo()
        {
            Console.Write("Введите ID товара: ");
            int id = Convert.ToInt32(Console.ReadLine());

            var product = FindProductInAnyWarehouse(id);
            if (product == null)
            {
                Console.WriteLine("Товар не найден");
                return;
            }

            Console.WriteLine(product.GetInformation());

            var warehouse = FindWarehouseWithProduct(id);
            if (warehouse != null)
            {
                Console.WriteLine($"Находится на складе: {warehouse.Id} ({warehouse.Type}) - {warehouse.Address}");
            }
        }

        private void FindProductById()
        {
            Console.Write("Введите ID товара: ");
            int id = Convert.ToInt32(Console.ReadLine());

            var product = FindProductInAnyWarehouse(id);
            if (product == null)
            {
                Console.WriteLine("Товар не найден");
                return;
            }

            Console.WriteLine(product.GetInformation());
        }

        private void ShowExpiredProducts()
        {
            var expiredProducts = new List<(Product product, Warehouse warehouse)>();

            foreach (var warehouse in _logisticsManager.GetWarehouses())
            {
                foreach (var product in warehouse.GetExpiredProducts())
                {
                    expiredProducts.Add((product, warehouse));
                }
            }

            if (!expiredProducts.Any())
            {
                Console.WriteLine("Просроченных товаров не найдено");
                return;
            }

            Console.WriteLine("Просроченные товары:");
            foreach (var (product, warehouse) in expiredProducts)
            {
                Console.WriteLine($"- {product.Name} (ID: {product.Id}) на складе {warehouse.Id} ({warehouse.Type})");
            }
        }

        private void ProcessDelivery()
        {
            var delivery = new List<Product>();

            Console.WriteLine("Добавление товаров в поставку (для завершения введите пустую строку):");

            while (true)
            {
                try
                {
                    Console.Write("Введите название товара (или Enter для завершения): ");
                    string name = Console.ReadLine()!;
                    if (string.IsNullOrEmpty(name)) break;

                    Console.Write("ID поставщика: ");
                    int supplierId = Convert.ToInt32(Console.ReadLine());

                    Console.Write("Объем единицы: ");
                    double volume = Convert.ToDouble(Console.ReadLine());

                    Console.Write("Цена единицы: ");
                    decimal price = Convert.ToDecimal(Console.ReadLine());

                    Console.Write("Срок годности (дней): ");
                    int expiration = Convert.ToInt32(Console.ReadLine());

                    var product = new Product(supplierId, name, volume, price, expiration);
                    delivery.Add(product);
                    Console.WriteLine($"Товар добавлен в поставку (ID: {product.Id})");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка ввода: {ex.Message}");
                }
            }

            if (!delivery.Any())
            {
                Console.WriteLine("Поставка пуста");
                return;
            }

            bool result = _logisticsManager.ProcessTheDelivery(delivery);
            Console.WriteLine($"Результат обработки поставки: {(result ? "УСПЕХ" : "НЕУДАЧА")}");
        }

        private void OptimizeWarehouse()
        {
            Console.Write("Введите ID склада для оптимизации: ");
            int id = Convert.ToInt32(Console.ReadLine());

            var warehouse = _logisticsManager.GetWarehouses().FirstOrDefault(w => w.Id == id);
            if (warehouse == null)
            {
                Console.WriteLine("Склад не найден");
                return;
            }

            bool result = _logisticsManager.OptimizeWarehouse(warehouse);
            Console.WriteLine($"Результат оптимизации: {(result ? "УСПЕХ" : "ЕСТЬ ПРОБЛЕМЫ")}");
        }

        private void OptimizeAllWarehouses()
        {
            bool result = _logisticsManager.OptimizeAllWarehouses();
            Console.WriteLine($"Результат оптимизации всех складов: {(result ? "УСПЕХ" : "ЕСТЬ ПРОБЛЕМЫ")}");
        }

        private void MoveProductBetweenWarehouses()
        {
            Console.Write("Введите ID товара: ");
            int productId = Convert.ToInt32(Console.ReadLine());

            var product = FindProductInAnyWarehouse(productId);
            if (product == null)
            {
                Console.WriteLine("Товар не найден");
                return;
            }

            var fromWarehouse = FindWarehouseWithProduct(productId);
            if (fromWarehouse == null)
            {
                Console.WriteLine("Не удалось определить текущий склад товара");
                return;
            }

            Console.Write("Введите ID целевого склада: ");
            int toWarehouseId = Convert.ToInt32(Console.ReadLine());

            var toWarehouse = _logisticsManager.GetWarehouses().FirstOrDefault(w => w.Id == toWarehouseId);
            if (toWarehouse == null)
            {
                Console.WriteLine("Целевой склад не найден");
                return;
            }

            if (fromWarehouse.RemoveProduct(product) && toWarehouse.AddProduct(product))
            {
                _logisticsManager.AddLog(product.Name, $"Склад {fromWarehouse.Id}", $"Склад {toWarehouse.Id}", product.VolumePerUnit);
                Console.WriteLine("Товар успешно перемещен");
            }
            else
            {
                Console.WriteLine("Не удалось переместить товар");
                fromWarehouse.AddProduct(product);
            }
        }

        private void MoveExpiredToRecycling()
        {
            _logisticsManager.MoveExpiredProductsToRecycling();
            Console.WriteLine("Перемещение просроченных товаров завершено");
        }

        private void AnalyzeWarehouse()
        {
            Console.Write("Введите ID склада: ");
            int id = Convert.ToInt32(Console.ReadLine());

            var warehouse = _logisticsManager.GetWarehouses().FirstOrDefault(w => w.Id == id);
            if (warehouse == null)
            {
                Console.WriteLine("Склад не найден");
                return;
            }

            string analysis = _logisticsManager.AnalyzeWarehouse(warehouse);
            Console.WriteLine(analysis);
        }

        private void AnalyzeAllWarehouses()
        {
            foreach (var warehouse in _logisticsManager.GetWarehouses())
            {
                string analysis = _logisticsManager.AnalyzeWarehouse(warehouse);
                Console.WriteLine(analysis);
                Console.WriteLine("---");
            }
        }

        private void ShowGeneralStatistics()
        {
            var warehouses = _logisticsManager.GetWarehouses();

            Console.WriteLine("\n=== ОБЩАЯ СТАТИСТИКА ===");
            Console.WriteLine($"Всего складов: {warehouses.Count}");
            Console.WriteLine($"Всего товаров: {warehouses.Sum(w => w.ProductsCount)}");
            Console.WriteLine($"Общий объем: {warehouses.Sum(w => w.Volume):F2}");
            Console.WriteLine($"Занятый объем: {warehouses.Sum(w => w.OccupiedVolume):F2}");
            Console.WriteLine($"Свободный объем: {warehouses.Sum(w => w.FreeVolume):F2}");
            Console.WriteLine($"Загрузка: {warehouses.Sum(w => w.OccupiedVolume) / warehouses.Sum(w => w.Volume) * 100:F1}%");

            Console.WriteLine("\nПо типам складов:");
            foreach (WarehouseType type in Enum.GetValues<WarehouseType>())
            {
                var typeWarehouses = warehouses.Where(w => w.Type == type).ToList();
                if (typeWarehouses.Any())
                {
                    Console.WriteLine($"  {type}: {typeWarehouses.Count} складов, {typeWarehouses.Sum(w => w.ProductsCount)} товаров");
                }
            }
        }

        private void ShowFinancialReport()
        {
            var warehouses = _logisticsManager.GetWarehouses();

            Console.WriteLine("\n=== ФИНАНСОВЫЙ ОТЧЕТ ===");
            decimal totalValue = 0;

            foreach (var warehouse in warehouses.Where(w => w.ProductsCount > 0))
            {
                decimal value = warehouse.CalculateTotalProductsValue();
                totalValue += value;
                Console.WriteLine($"Склад {warehouse.Id} ({warehouse.Type}): {value:C2}");
            }

            Console.WriteLine($"\nОбщая стоимость товаров: {totalValue:C2}");
        }

        private void ShowProductsBySupplier()
        {
            Console.Write("Введите ID поставщика: ");
            int supplierId = Convert.ToInt32(Console.ReadLine());

            var supplierProducts = new List<(Product product, Warehouse warehouse)>();

            foreach (var warehouse in _logisticsManager.GetWarehouses())
            {
                foreach (var product in warehouse.GetProductsBySupplier(supplierId))
                {
                    supplierProducts.Add((product, warehouse));
                }
            }

            if (!supplierProducts.Any())
            {
                Console.WriteLine("Товары данного поставщика не найдены");
                return;
            }

            Console.WriteLine($"Товары поставщика {supplierId}:");
            foreach (var (product, warehouse) in supplierProducts)
            {
                Console.WriteLine($"- {product.Name} (ID: {product.Id}) - {product.PricePerUnit:C2} на складе {warehouse.Id}");
            }
        }

        private void ShowAllLogs()
        {
            var logs = _logisticsManager.GetLogs();
            if (!logs.Any())
            {
                Console.WriteLine("Логи пусты");
                return;
            }

            Console.WriteLine("Все логи операций:");
            foreach (var log in logs)
            {
                Console.WriteLine(log.ToString());
            }
        }

        private Product? FindProductInAnyWarehouse(int productId)
        {
            return _logisticsManager.GetWarehouses()
                .SelectMany(w => w.Products)
                .FirstOrDefault(p => p.Id == productId);
        }

        private Warehouse? FindWarehouseWithProduct(int productId)
        {
            return _logisticsManager.GetWarehouses()
                .FirstOrDefault(w => w.ContainsProduct(productId));
        }

        private void AddProductToWarehouse(Product product)
        {
            Console.Write("Введите ID склада: ");
            int warehouseId = Convert.ToInt32(Console.ReadLine());

            var warehouse = _logisticsManager.GetWarehouses().FirstOrDefault(w => w.Id == warehouseId);
            if (warehouse == null)
            {
                Console.WriteLine("Склад не найден");
                return;
            }

            if (warehouse.AddProduct(product))
            {
                _logisticsManager.AddLog(product.Name, "Создание", $"Склад {warehouse.Id}", product.VolumePerUnit);
                Console.WriteLine("Товар добавлен на склад");
            }
            else
            {
                Console.WriteLine("Не удалось добавить товар на склад (недостаточно места)");
            }
        }
    }

    public Pr5()
    {
        var menu = new WarehouseConsoleMenu();
        menu.ShowMainMenu();
    }
}

