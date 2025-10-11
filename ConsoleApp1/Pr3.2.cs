class Pr3_2
{
    class Table
    {
        public int TableNumber;
        public string TableLocation;
        public int SeatsCount;
        public Dictionary<int, Booking> TimeSlots = [];

        private static int _lastTableNumber = 0;
        private static List<Table> _tables = [];

        public Table(string tableLocation, int seatsCount)
        {
            _lastTableNumber++;
            TableNumber = _lastTableNumber;
            TableLocation = tableLocation;
            SeatsCount = seatsCount;
            _tables.Add(this);
        }

        ~Table()
        {
            _tables.Remove(this);
        }

        private static Table? FindById(int id)
        {
            foreach (Table item in _tables)
            {
                if (item.TableNumber == id)
                    return item;
            }
            Console.WriteLine($"\nОбъект с ID {id} не найден");
            return null;
        }


        public void DisplayInfo()
        {
            Console.WriteLine(
                $"\nID: {TableNumber}" +
                $"\nРасположение: {TableLocation}" +
                $"\nКоличество мест: {SeatsCount}" +
                "\nРасписание: ");
            for (int i = 0; i < TimeSlots.Count; i++)
            {
                Console.WriteLine($"{i}:00-{i+1}:00 - {TimeSlots[i]}");
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
    }

    public Pr3_2()
    {
        Table a = new("У окна", 3);
        Table b = new("У входа", 5);
        List<Table> TableList = [a, b];

        for (int i = 0;i < TableList.Count;i++)
            TableList[i].DisplayInfo();

        Table.DisplayInfo(123);
    }
}