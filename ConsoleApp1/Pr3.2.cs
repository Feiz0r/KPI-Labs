class Pr3_2
{
    class Table
    {
        public int TableId { get; private set; }
        public string TableLocation { get; private set; }
        public int SeatsCount { get; private set; }
        public Dictionary<int, Booking?> TimeSlots { get; private set; }

        private static int _lastTableId = 0;
        private static readonly List<Table> _tables = [];

        public Table(string tableLocation, int seatsCount)
        {
            if (string.IsNullOrWhiteSpace(tableLocation))
                throw new ArgumentException("Расположение стола не может быть пустым");

            if (seatsCount <= 0)
                throw new ArgumentException("Количество мест должно быть положительным числом");

            _lastTableId++;
            TableId = _lastTableId;
            TableLocation = tableLocation.Trim();
            SeatsCount = seatsCount;
            TimeSlots = [];
            InitializationTimeSlots();
            _tables.Add(this);
        }

        ~Table()
        {
            _tables.Remove(this);
        }

        public static Table Create()
        {
            Console.WriteLine("Введите расположение стола:");
            string location = Console.ReadLine()!;
            Console.WriteLine("Введите количество мест:");
            string countInput = Console.ReadLine()!;

            if (!int.TryParse(countInput, out int count))
                throw new FormatException("Некорректный формат количества мест");

            return new Table(location, count);
        }

        private void InitializationTimeSlots()
        {
            for (int i = 0; i < 24; i++)
                TimeSlots[i] = null;
        }

        private static Table FindById(int id)
        {
            var table = _tables.Find(t => t.TableId == id);
            return table ?? throw new KeyNotFoundException($"Стол с ID {id} не найден");
        }

        public bool IsAvailable(int startTime, int endTime)
        {
            if (startTime < 0 || startTime >= 24)
                throw new ArgumentException("Время начала должно быть в диапазоне 0-23");

            if (endTime <= startTime || endTime > 24)
                throw new ArgumentException("Время окончания должно быть больше времени начала и не превышать 24");

            for (int i = startTime; i < endTime; i++)
            {
                if (TimeSlots.TryGetValue(i, out Booking? value) && value != null)
                    return false;
            }
            return true;
        }

        public void DisplayInfo()
        {
            Console.WriteLine(
                $"\nID стола: {TableId}" +
                $"\nРасположение стола: {TableLocation}" +
                $"\nКоличество мест: {SeatsCount}" +
                "\nРасписание:"
            );

            for (int i = 0; i < 24; i++)
            {
                string timeSlot = $"{i:00}:00-{(i + 1):00}:00";
                var booking= TimeSlots[i];
                string status = TimeSlots[i] == null ? "свободно" : $"Id: {booking.BookingId}, {booking.ClientName}, {booking.PhoneNumber}";
                Console.WriteLine($"{timeSlot} - {status}");
            }
        }

        public static void DisplayInfo(int id)
        {
            var table = FindById(id);
            table.DisplayInfo();
        }

        public void Edit(string tableLocation = "", int seatsCount = -1)
        {
            if (!string.IsNullOrEmpty(tableLocation))
                TableLocation = tableLocation;
            if (seatsCount > 0)
                SeatsCount = seatsCount;
        }

        public bool HasActiveBookings()
        {
            return TimeSlots.Any(slot => slot.Value != null);
        }

        public static void EditTableWithChecks(int id, string tableLocation = "", int seatsCount = -1)
        {
            var table = FindById(id);

            if (table.HasActiveBookings())
            {
                throw new InvalidOperationException(
                    $"Невозможно редактировать стол ID {id}. " +
                    "У стола есть активные бронирования. "
                );
            }

            table.Edit(tableLocation, seatsCount);
            Console.WriteLine($"Стол ID {id} успешно отредактирован");
        }

        public static List<Table> FindAvailableTables(int startTime, int endTime, int requiredSeats = 1)
        {
            if (startTime < 0 || startTime >= 24)
                throw new ArgumentException("Время начала должно быть в диапазоне 0-23");

            if (endTime <= startTime || endTime > 24)
                throw new ArgumentException("Время окончания должно быть больше времени начала и не превышать 24");

            if (requiredSeats <= 0)
                throw new ArgumentException("Количество требуемых мест должно быть положительным числом");

            var availableTables = _tables.Where(t =>
                t.SeatsCount >= requiredSeats &&
                t.IsAvailable(startTime, endTime)
            ).ToList();

            return availableTables;
        }

        public static Table GetById(int id)
        {
            return FindById(id);
        }

        public static List<Table> GetAllTables()
        {
            if (_tables.Count == 0)
                throw new InvalidOperationException("Нет доступных столов");

            return new List<Table>(_tables);
        }

        public void AssignBooking(Booking booking, int startTime, int endTime)
        {
            if (booking == null)
                throw new ArgumentNullException(nameof(booking), "Бронь не может быть null");

            if (!IsAvailable(startTime, endTime))
                throw new InvalidOperationException("Стол не доступен в указанное время");

            for (int i = startTime; i < endTime; i++)
            {
                TimeSlots[i] = booking;
            }
        }

        public void ReleaseTimeSlots(int startTime, int endTime)
        {
            if (startTime < 0 || startTime >= 24)
                throw new ArgumentException("Время начала должно быть в диапазоне 0-23");

            if (endTime <= startTime || endTime > 24)
                throw new ArgumentException("Время окончания должно быть больше времени начала и не превышать 24");

            for (int i = startTime; i < endTime; i++)
            {
                if (TimeSlots.ContainsKey(i))
                    TimeSlots[i] = null;
            }
        }

    }

    class Booking
    {
        public int BookingId { get; private set; }
        public string ClientName { get; private set; }
        public string PhoneNumber { get; private set; }
        public int StartTime { get; private set; }
        public int EndTime { get; private set; }
        public string Comment { get; private set; }
        public Table AssignedTable { get; private set; }

        private static int _lastBookingId = 0;
        private static readonly List<Booking> _bookings = [];

        public Booking(string clientName, string phoneNumber, int startTime, int endTime, string comment, Table assignedTable)
        {
            if (startTime < 0 || startTime >= 24)
                throw new ArgumentException("Время начала должно быть в диапазоне 0-23");

            if (endTime <= startTime || endTime > 24)
                throw new ArgumentException("Время окончания должно быть больше времени начала и не превышать 24");

            if (string.IsNullOrWhiteSpace(clientName))
                throw new ArgumentException("Имя клиента не может быть пустым");

            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentException("Номер телефона не может быть пустым");

            if (!assignedTable.IsAvailable(startTime, endTime))
                throw new InvalidOperationException($"Стол не доступен в указанное время ({startTime}:00-{endTime}:00)");

            _lastBookingId++;
            BookingId = _lastBookingId;
            ClientName = clientName.Trim();
            PhoneNumber = phoneNumber.Trim();
            StartTime = startTime;
            EndTime = endTime;
            Comment = comment ?? "";
            AssignedTable = assignedTable;

            assignedTable.AssignBooking(this, startTime, endTime);

            _bookings.Add(this);
        }

        ~Booking()
        {
            AssignedTable.ReleaseTimeSlots(StartTime, EndTime);
            _bookings.Remove(this);
        }

        public void DisplayInfo()
        {
            Console.WriteLine(
                $"\nID брони: {BookingId}" +
                $"\nИмя клиента: {ClientName}" +
                $"\nНомер телефона: {PhoneNumber}" +
                $"\nВремя брони: {StartTime:00}:00-{EndTime:00}:00" +
                $"\nПродолжительность: {EndTime - StartTime} час(ов)" +
                $"\nКомментарий: {Comment}" +
                $"\nСтол: #{AssignedTable.TableId} ({AssignedTable.TableLocation}, {AssignedTable.SeatsCount} мест)"
            );
        }

        public void Cancel()
        {
            AssignedTable.ReleaseTimeSlots(StartTime, EndTime);
            _bookings.Remove(this);
        }

        public static Booking Create(Table table)
        {
            Console.WriteLine("Введите имя клиента:");
            string name = Console.ReadLine()!;

            Console.WriteLine("Введите номер телефона:");
            string phone = Console.ReadLine()!;

            Console.WriteLine("Введите время начала брони (0-23):");
            if (!int.TryParse(Console.ReadLine(), out int start) || start < 0 || start >= 24)
                throw new FormatException("Некорректное время начала");

            Console.WriteLine("Введите время окончания брони (1-24):");
            if (!int.TryParse(Console.ReadLine(), out int end) || end <= start || end > 24)
                throw new FormatException("Некорректное время окончания");

            Console.WriteLine("Введите комментарий:");
            string comment = Console.ReadLine()!;

            return new Booking(name, phone, start, end, comment, table);
        }

        private static Booking FindById(int id)
        {
            var booking = _bookings.Find(b => b.BookingId == id);
            return booking ?? throw new KeyNotFoundException($"Бронь с ID {id} не найдена");
        }

        public static void Cancel(int id)
        {
            var booking = FindById(id);
            booking.Cancel();
        }

        public static void DisplayInfo(int id)
        {
            var booking = FindById(id);
            booking.DisplayInfo();
        }

        public static List<Booking> GetAllBookings()
        {
            if (_bookings.Count == 0)
                throw new InvalidOperationException("Нет активных бронирований");

            return new List<Booking>(_bookings);
        }

        public static List<Booking> FindBookingsFilter(string lastDigits, string clientName)
        {
            if (string.IsNullOrWhiteSpace(lastDigits) || lastDigits.Length != 4)
                throw new ArgumentException("Последние 4 цифры номера телефона должны быть указаны");

            if (string.IsNullOrWhiteSpace(clientName))
                throw new ArgumentException("Имя клиента не может быть пустым");

            if (!lastDigits.All(char.IsDigit))
                throw new ArgumentException("Последние 4 цифры должны содержать только числа");

            var foundBookings = _bookings.Where(t =>
                t.ClientName.Trim().Equals(clientName.Trim(), StringComparison.OrdinalIgnoreCase) &&
                t.PhoneNumber.EndsWith(lastDigits)
                ).ToList();
            
            return foundBookings;
        }

        public static void DisplayBookingsFilter(string lastDigits, string clientName)
        {
            try
            {
                var bookings = FindBookingsFilter(lastDigits, clientName);

                if (bookings.Count == 0)
                {
                    Console.WriteLine($"Бронирования для клиента '{clientName}' с последними цифрами '{lastDigits}' не найдены");
                    return;
                }

                Console.WriteLine($"\nНАЙДЕННЫЕ БРОНИРОВАНИЯ:");
                Console.WriteLine($"Клиент: {clientName}, последние цифры телефона: {lastDigits}");
                Console.WriteLine($"Найдено бронирований: {bookings.Count}\n");

                foreach (var booking in bookings)
                {
                    booking.DisplayInfo();
                    Console.WriteLine("---");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка поиска: {ex.Message}");
            }
        }

    }

    public Pr3_2()
    {
        Table t1 = new("У окна", 3);
        Table t2 = new("У входа", 5);
        List<Table> tableList = [t1, t2];

        Booking b1 = new("Василий", "+79708883535", 3, 13, "ХАХАХАХ", t1);
        Booking b2 = new("Георгий", "+79708883536", 15, 19, "ХАХАХАХ", t1);
        List<Booking> BookingList = [b1, b2];

        void TableMenu()
        {
            Console.WriteLine("\n1. Создать\n2. Редактировать\n3. Информация\n4. Список всех столов\n5. Поиск доступных столов");
            int choice = Convert.ToInt32(Console.ReadLine());

            try
            {
                switch (choice)
                {
                    case 1:
                        Table table = Table.Create();
                        tableList.Add(table);
                        Console.WriteLine($"Стол создан с ID: {table.TableId}");
                        break;
                    case 2:
                        Console.WriteLine("Введите ID стола:");
                        int id = Convert.ToInt32(Console.ReadLine());
                        try
                        {
                            Console.WriteLine("Введите новое расположение (или Enter для пропуска):");
                            string loc = Console.ReadLine()!;
                            Console.WriteLine("Введите новое количество мест (или Enter для пропуска):");
                            string scs = Console.ReadLine()!;
                            int sc = -1;
                            if (!string.IsNullOrEmpty(scs))
                                sc = Convert.ToInt32(scs);

                            Table.EditTableWithChecks(id, loc, sc);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Ошибка: {ex.Message}");
                        }
                        break;
                    case 3:
                        Console.WriteLine("Введите ID стола:");
                        id = Convert.ToInt32(Console.ReadLine());
                        Table.DisplayInfo(id);
                        break;
                    case 4:
                        var allTables = Table.GetAllTables();
                        foreach (Table tableItem in allTables)
                            tableItem.DisplayInfo();
                        break;
                    case 5:
                        Console.WriteLine("Введите время начала брони (0-23):");
                        int start = Convert.ToInt32(Console.ReadLine());
                        Console.WriteLine("Введите время окончания брони (0-23):");
                        int end = Convert.ToInt32(Console.ReadLine());
                        Console.WriteLine("Введите необходимое количество мест (или Enter для любого):");
                        string seatsInput = Console.ReadLine()!;
                        int seats = 1;
                        if (!string.IsNullOrEmpty(seatsInput))
                            seats = Convert.ToInt32(seatsInput);

                        var availableTables = Table.FindAvailableTables(start, end, seats);
                        if (availableTables.Count == 0)
                        {
                            Console.WriteLine("Нет доступных столов");
                        }
                        else
                        {
                            Console.WriteLine("Доступные столы:");
                            foreach (var tableItem in availableTables)
                            {
                                Console.WriteLine($"ID: {tableItem.TableId}, Мест: {tableItem.SeatsCount}, Расположение: {tableItem.TableLocation}");
                            }
                        }
                        break;
                    default:
                        Console.WriteLine("Неверный выбор");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }

        void BookingMenu()
        {
            Console.WriteLine("\n1. Создать\n2. Отменить\n3. Информация\n4. Список бронирований\n5. Поиск бронирования");
            int choice = Convert.ToInt32(Console.ReadLine());

            try
            {
                switch (choice)
                {
                    case 1:
                        Console.WriteLine("Введите ID стола для брони:");
                        int tableId = Convert.ToInt32(Console.ReadLine());
                        var tableToBook = Table.GetById(tableId);
                        Booking newBooking = Booking.Create(tableToBook);
                        Console.WriteLine($"Бронь создана с ID: {newBooking.BookingId}");
                        BookingList.Add(newBooking);
                        break;
                    case 2:
                        Console.WriteLine("Введите ID брони для отмены:");
                        int bookingId = Convert.ToInt32(Console.ReadLine());
                        Booking.Cancel(bookingId);
                        Console.WriteLine("Бронь отменена");
                        break;
                    case 3:
                        Console.WriteLine("Введите ID брони:");
                        bookingId = Convert.ToInt32(Console.ReadLine());
                        Booking.DisplayInfo(bookingId);
                        break;
                    case 4:
                        foreach (Booking b in Booking.GetAllBookings())
                            b.DisplayInfo();
                        break;
                    case 5:
                        Console.WriteLine("Введите имя клиента:");
                        string name = Console.ReadLine()!;
                        Console.WriteLine("Введите последние 4 цифры номера телефона:");
                        string digits = Console.ReadLine()!;

                        Booking.DisplayBookingsFilter(digits, name);
                        break;
                    default:
                        Console.WriteLine("Неверный выбор");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }

        while (true)
        {
            try
            {
                Console.WriteLine("\n1. Столы\n2. Брони\n3. Выход");
                int mainChoice = Convert.ToInt32(Console.ReadLine());

                switch (mainChoice)
                {
                    case 1:
                        TableMenu();
                        break;
                    case 2:
                        BookingMenu();
                        break;
                    case 3:
                        Console.WriteLine("Выход из программы");
                        return;
                    default:
                        Console.WriteLine("Неверный выбор");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }

    }
}