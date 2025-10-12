class Pr3_2
{
    class Table
    {
        public int TableId;
        public string TableLocation;
        public int SeatsCount;
        public Dictionary<int, Booking?> TimeSlots = [];

        private static int _lastTableId = 0;
        private static readonly List<Table> _tables = [];

        public Table(string tableLocation, int seatsCount)
        {
            _lastTableId++;
            TableId = _lastTableId;
            TableLocation = tableLocation;
            SeatsCount = seatsCount;
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
            Console.WriteLine("Введите количесво мест:");
            int count = Convert.ToInt32(Console.ReadLine()!);
            return new Table(location, count);
        }
        
        private void InitializationTimeSlots()
        {
            for (int i = 0; i < 24; i++)
                TimeSlots.Add(i, null);
        }

        private static Table? FindById(int id)
        {
            foreach (Table item in _tables)
            {
                if (item.TableId == id)
                    return item;
            }
            Console.WriteLine($"\nОбъект с ID {id} не найден");
            return null;
        }

        public static bool ContainsId(int id)
        {
            if (FindById(id) == null)
                return false;
            else
                return true;
        }

        public void DisplayInfo()
        {
            Console.WriteLine(
                $"\nID стола: {TableId}" +
                $"\nРасположение стола: {TableLocation}" +
                $"\nКоличество мест: {SeatsCount}" +
                "\nРасписание: "
                );
            for (int i = 0; i < TimeSlots.Count; i++)
            {
                Console.WriteLine($"{i}:00-{i+1}:00 - " + (TimeSlots[i] == null ? "свободно":"занято"));
            }
        }

        public static void DisplayInfo(int id)
        {
            var a = FindById(id);
            if (a == null) return;
            a.DisplayInfo();
        }

        public void Edit(string tableLocation = "", int seatsCount = -1)
        {
            if (tableLocation != string.Empty)
                TableLocation = tableLocation;
            if (seatsCount != -1)
                SeatsCount = seatsCount;
        }

        public static void Edit(int id, string tableLocation = "", int seatsCount = -1)
        {
            var a = FindById(id);
            if (a == null) return;
            a.Edit(tableLocation, seatsCount);
        }
    }

    class Booking
    {
        public int BookingId;
        public string ClientName;
        public string PhoneNumber;
        public int StartTime;
        public int EndTime;
        public string Comment;
        public Table AssignedTable;

        private static int _lastBookingId = 0;
        private static readonly List<Booking> _booking = [];

        public Booking(string clientName, string phoneNumber, int startTime, int endTime, string comment, Table assignedTable)
        {
            _lastBookingId++;
            BookingId = _lastBookingId;
            ClientName = clientName;
            PhoneNumber = phoneNumber;
            StartTime = startTime;
            EndTime = endTime;
            Comment = comment;
            AssignedTable = assignedTable;
            _booking.Add(this);
        }

        public void DisplayInfo()
        {
            Console.WriteLine(
                $"\nID клиента: {BookingId}" +
                $"\nИмя клиента: {ClientName}" +
                $"\nНомер телефона клиента: {PhoneNumber}" +
                $"\nВремя начала брони: {StartTime}:00" +
                $"\nВремя окончания брони: {EndTime}:00" +
                $"\nКомментарий: {Comment}" +
                $"\nНазначенный столик: {AssignedTable.TableId}"
                );
        }

    }

    public Pr3_2()
    {
        Table t1 = new("У окна", 3);
        Table t2 = new("У входа", 5);
        List<Table> TableList = [t1, t2];

        Booking b1 = new("Василий", "+79708883535", 10, 13, "ХАХАХАХ", t1);
        Booking b2 = new("Георгий", "+79708883536", 15, 19, "ХАХАХАХ", t2);
        List<Booking> BookingList = [b1, b2];

        void tableMenu()
        {
            Console.WriteLine("\n 1.Создать \n 2.Редактировать \n 3.Информация \n 4.Список всех столов");
            int m = Convert.ToInt32(Console.ReadLine());
            int id;
            switch (m)
            {
                case 1:
                    Table t = Table.Create();
                    TableList.Add(t);
                    break;
                case 2:
                    Console.WriteLine("\nВведите id стола:");
                    id = Convert.ToInt32(Console.ReadLine());

                    if (!Table.ContainsId(id))
                        return;

                    Console.WriteLine("\nВведите новое расположение (или оставьте пустым):");
                    string loc = Console.ReadLine()!;
                    Console.WriteLine("\nВведите новое количесво мест (или оставьте пустым):");
                    string scs = Console.ReadLine()!;
                    int sc = -1;
                    if (scs != null)
                        sc = Convert.ToInt32(scs);

                    Table.Edit(id, loc, sc);
                    break;
                case 3:
                    Console.WriteLine("\nВведите id стола:");
                    id = Convert.ToInt32(Console.ReadLine());
                    Table.DisplayInfo(id);
                    break;
                case 4:
                    foreach (Table v in TableList)
                        v.DisplayInfo();
                    break;
            }
        }

        void bookingMenu()
        {
            Console.WriteLine("\n 1.Создать \n 2.Редактировать \n 3.Удалить \n3.Информация \n 4.Список бронирований");
            int m = Convert.ToInt32(Console.ReadLine());
            int id;
            switch (m)
            {
                case 1:
                    break;
            }
        }

        foreach (Booking b in BookingList)
            b.DisplayInfo();

        Table.DisplayInfo(123);

        for (; ; )
        {
            Console.WriteLine("\n1.Стол \n2.Бронь");
            int n = Convert.ToInt32(Console.ReadLine());
            switch (n)
            {
                case 1:
                    tableMenu();
                    break;
                case 2:
                    bookingMenu();
                    break;
            }
        }
        
    }
}