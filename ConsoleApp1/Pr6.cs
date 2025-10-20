class Pr6
{
    public Pr6(int i)
    {
        switch (i)
        {
            case 1:
                _ = new Z1();
                break;
        }
    }

    class Z1
    {
        public delegate int StringComparisonDelegate(string x, string y);

        static int CompareByLength(string x, string y) => x.Length.CompareTo(y.Length);

        static int CompareAlphabetically(string x, string y) => string.Compare(x, y, StringComparison.OrdinalIgnoreCase);

        static int CompareByVowelCount(string x, string y)
        {
            int vowelsX = CountVowels(x);
            int vowelsY = CountVowels(y);
            return vowelsX.CompareTo(vowelsY);
        }

        static int CountVowels(string str)
        {
            int count = 0;
            string vowels = "аеёиоуыэюяaeiou";

            foreach (char c in str.ToLower())
            {
                if (vowels.Contains(c))
                {
                    count++;
                }
            }
            return count;
        }

        public Z1()
        {
            List<string> strings =
            [
                "яблоко",
                "программирование",
                "кот",
                "делегат",
                "электричество",
                "дом",
                "алгоритм",
                "зима"
            ];

            Console.WriteLine("1 - По длине строки (от короткой к длинной)");
            Console.WriteLine("2 - По алфавиту");
            Console.WriteLine("3 - По количеству гласных букв");
            int choice = Convert.ToInt32(Console.ReadLine());

            StringComparisonDelegate? comparisonDelegate = null;
            switch (choice)
            {
                case 1:
                    comparisonDelegate = CompareByLength;
                    break;
                case 2:
                    comparisonDelegate = CompareAlphabetically;
                    break;
                case 3:
                    comparisonDelegate = CompareByVowelCount;
                    break;
            }

            strings.Sort((x, y) => comparisonDelegate(x, y));

            foreach (string s in strings)
            { Console.WriteLine(s); }
        }
    }

    class Z2
    {


        public Z2()
        {

        }
    }
}
