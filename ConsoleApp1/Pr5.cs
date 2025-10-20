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

        internal bool CanAcceptProducts(IEnumerable<Product> products)
        {
            var totalVolume = products?.Sum(p => p.VolumePerUnit) ?? 0;
            return totalVolume <= FreeVolume;
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

        internal bool RemoveProduct(int productId)
        {
            var product = _products.FirstOrDefault(p => p.Id == productId);
            return product != null && RemoveProduct(product);
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

            List<Product> productsToMoveFromCurrent = new();
            List<Warehouse> suitableWarehouses = new();
            List<Product> suitableProducts = new();

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

            if (!suitableWarehouses.Any() && suitableProducts.Any())
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
                        new List<Product> { product }).FirstOrDefault();

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

    public Pr5()
    {

    }
}

