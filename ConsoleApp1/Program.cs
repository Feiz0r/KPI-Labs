using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

class Pr1
{
    public static void Z1()
    {
        Console.WriteLine("Введите номер дня недели, с которого начинается месяц (1-пн,...7-вс)");
        int a = Convert.ToInt16(Console.ReadLine());
        Console.WriteLine("Введите день месяца");
        int b = Convert.ToInt16(Console.ReadLine());
        int c = (a + b - 2) % 7;
        string[] weekDay = [
          "Понедельник","Вторник","Среда","Четверг","Пятница","Суббота","Воскресенье"
          ];
        Console.WriteLine("Это будет: " + weekDay[c]);
    }

    public static void Z2()
    {
        int[] banknote = [5000, 2000, 1000, 500, 200, 100];
        int[] banknoteCount = new int[banknote.Length];
        Console.Write("Введите кол-во для снятия: ");
        int sum = Convert.ToInt32(Console.ReadLine());
        if (sum % 100 != 0 || sum > 150000)
        {
            Console.WriteLine("Невозможно");
            return;
        }

        for (int i = 0; i < banknote.Length; i++)
        {
            banknoteCount[i] = sum / banknote[i];
            sum %= banknote[i];
        }

        for (int i = 0; i < banknote.Length; i++)
        {
            Console.WriteLine(banknote[i] + ": " + banknoteCount[i]);
        }
    }
}

class Pr2
{
    static double Factorial(int n) //1
    {
        if (n <= 1)
            return 1;
        return n * Factorial(n - 1);
    }

    static int GCD(int a, int b) //3
    {
        while (b != 0)
        {
            int temp = b;
            b = a % b;
            a = temp;
        }
        return a;
    }

    class Drink(string name, int price, List<int> components)
    {
        public readonly string name = name;
        public readonly int price = price;
        private int np = 0;
        public readonly List<int> components = components; //0-water,1-milk,3-...

        public void Use() => np++;
        public int ReturnNP() => (int)(np);
        public int ReturnProfit() => (int)(price * np);
        public bool Chek(List<int> c, int i = 0)
        {
            if (c.Count < components.Count) return false;
            if (i < components.Count-1) return c[i] >= components[i] && Chek(c, i + 1);
            return c[i] >= components[i];
        }
    }

    public static double Z1(double x, int n)
    {
        if (n == 0)       
            return x;
        double term = Math.Pow(-1, n) * Math.Pow(x, 2 * n + 1) / Factorial(2 * n + 1);
        return Z1(x, n - 1) + term;
    }

    public static void Z2()
    {
        Console.WriteLine("Введите номер билета: ");
        int n = Convert.ToInt32(Console.ReadLine());
        int c1 = (n % 10) + (n % 100) / 10 + (n % 1000) / 100;
        int c2 = (n % 10000) / 1000 + (n % 100000) / 10000 + (n % 1000000) / 100000;
        Console.WriteLine(c1 == c2);
    }

    public static void Z3()
    {
        Console.WriteLine("Введите числитель: ");
        int n1 = Convert.ToInt32(Console.ReadLine());
        Console.WriteLine("Введите знаменатель: ");
        int n2 = Convert.ToInt32(Console.ReadLine());
        if (n2 == 0)
        {
            Console.WriteLine("На 0 делить нельзя");
        }
        else if(n1 == 0) {
            Console.WriteLine("0/0 --> 0");
        }
        else
        {
            bool negative = (n1 < 0) ^ (n2 < 0);
            n1 = Math.Abs(n1);
            n2 = Math.Abs(n2);
            int gcd = GCD(n1, n2);
            n1 /= gcd;
            n2 /= gcd;
            Console.WriteLine("Результат: " + (negative ? "-(":"(") + Convert.ToString(n1) + " / " + Convert.ToString(n2)+")" + (n1==n2?(" --> "+ Convert.ToString(n1)):""));
        }
    }

    public static void Z4()
    {
        int n = 32;
        int n2 = 16;
        while (n2 != 0)
        {
            Console.WriteLine("«Ваше число: " + Convert.ToString(n) + "? («да-2» «больше-1» «меньше-0)");
            int a = Convert.ToInt16(Console.ReadLine());
            if (a == 2) break;
            if (a==1) n += n2;
            else n -= n2;
            n2 /= 2;
        }
        Console.WriteLine("Ваше число: " + Convert.ToString(n));
    }

    

    public static void Z5()
    {
        List<string> componentsN = new(["вода", "молоко"]);
        List<int> componentsM = [];
        for (int i = 0; i < componentsN.Count; i++)
        {
            Console.WriteLine("Введите количество " + componentsN[i] + " в мл:");
            int r = Convert.ToInt32(Console.ReadLine());
            if (r < 0)
            {
                Console.WriteLine("Не воруйте у аппарата!");
                return;
            }
            componentsM.Add(r);
        }

        Drink d1 = new("американо", 150, [300]);
        Drink d2 = new("латте", 170, [30, 270]);
        List<Drink> VarietyDrinks = [d1, d2];

        while (VarietyDrinks.Any(i => i.Chek(componentsM))) {
            Console.Write("\nВыберите напиток ");
            for (int i = 0; i < VarietyDrinks.Count; i++)
            {
                Console.Write(Convert.ToString(i+1) +" - "+ VarietyDrinks[i].name +", ");
            }
            Console.Write("\n");
            int selectedItem = Convert.ToInt32(Console.ReadLine())-1;
            if (selectedItem<0 || selectedItem>=VarietyDrinks.Count)
            {
                Console.WriteLine("Такого напитка пока нет :(");
                continue;
            }

            Drink d = VarietyDrinks[selectedItem];
            if (!d.Chek(componentsM))
            {
                for (int i = 0; i < d.components.Count; i++)
                    if (componentsM[i] < d.components[i]) Console.WriteLine("Не хватает: " + componentsN[i]);
                continue;
            }
            for (int i = 0; i < d.components.Count; i++)
            {
                componentsM[i] -= d.components[i];
            }
            VarietyDrinks[selectedItem].Use();
            Console.WriteLine("Ваш напиток готов");
        }

        Console.WriteLine("\n\n=====================================ОТЧЕТ=====================================");
        Console.WriteLine("Ингредиентов осталось: ");
        for (int i = 0; i < componentsM.Count; i++)
        {
            Console.WriteLine(Convert.ToString(componentsN[i]) + ": " + Convert.ToString(componentsM[i])+"мл");
        }
        Console.Write("\n");
        for (int i = 0; i < VarietyDrinks.Count; i++) {
            Console.Write("Приготовлено кружек "+VarietyDrinks[i].name+": "+Convert.ToString(VarietyDrinks[i].ReturnNP()));
            Console.Write(", что принесло: " + Convert.ToString(VarietyDrinks[i].ReturnProfit())+"р\n");
        }
        Console.Write("\n");
        Console.WriteLine("Суммарный доход: " +Convert.ToString(VarietyDrinks.Sum(i => i.ReturnProfit()))+"р");
    }
}

internal class Program
{
   
    private static void Main()
    {
        int p = 2;
        int z = 5;

        switch (p)
        {
            case 1:
                switch (z)
                {
                    case 1:
                        Pr1.Z1();
                        break;
                    case 2:
                        Pr1.Z2();
                        break;
                }
                break;
            case 2:
                switch (z)
                {
                    case 1:
                        double term = Pr2.Z1(Math.PI / 6, 15);
                        Console.WriteLine(term);
                        break;
                    case 2:
                        Pr2.Z2();
                        break;
                    case 3:
                        Pr2.Z3();
                        break;
                    case 4:
                        Pr2.Z4();
                        break;
                    case 5:
                        Pr2.Z5();
                        break;
                }
                break;
        }      
    }
}