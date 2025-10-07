using System;
using System.Reflection;

class Pr2
{

    public Pr2(int i)
    {
        switch (i)
        {
            case 1:
                Pr2.Z1();
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
            case 6:
                Pr2.Z6();
                break;
            case 7:
                Pr2.Z7();
                break;
        }
    }

    static double Factorial(int n) //1
    {
        double result = 1;
        for (int i = 2; i <= n; i++)
        {
            result *= i;
        }
        return result;
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
            if (i < components.Count - 1) return c[i] >= components[i] && Chek(c, i + 1);
            return c[i] >= components[i];
        }
    }

    private static void Z1()
    {
        Console.WriteLine("Введите x (в градусах): ");
        double x = Convert.ToDouble(Console.ReadLine());
        Console.WriteLine("Введите n: ");
        int n = Convert.ToInt32(Console.ReadLine());

        x = x * Math.PI / 180;
        double term = Math.Pow(-1, n) * Math.Pow(x, 2 * n + 1) / Factorial(2 * n + 1);
        Console.WriteLine($"f({x}, {n}): {Math.Round(term, 8)}");

        Console.WriteLine("Введите точность e: ");
        double e = Convert.ToDouble(Console.ReadLine());

        double sum = 0;
        double f;
        int k = 0;
        do
        {
            f = Math.Pow(-1, k) * Math.Pow(x, 2 * k + 1) / Factorial(2 * k + 1);
            sum += f;
            k++;
        } while (Math.Abs(f) >= e);

        Console.WriteLine($"Сумма: {Math.Round(sum,8)}, Членов ряда: {k}");
    }

    private static void Z2()
    {
        Console.WriteLine("Введите номер билета: ");
        int n = Convert.ToInt32(Console.ReadLine());
        int c1 = (n % 10) + (n % 100) / 10 + (n % 1000) / 100;
        int c2 = (n % 10000) / 1000 + (n % 100000) / 10000 + (n % 1000000) / 100000;
        Console.WriteLine($"Вы счастливы? {c1 == c2}");
    }

    private static void Z3()
    {
        Console.WriteLine("Введите числитель: ");
        int n1 = Convert.ToInt32(Console.ReadLine());
        Console.WriteLine("Введите знаменатель: ");
        int n2 = Convert.ToInt32(Console.ReadLine());
        if (n2 == 0)
        {
            Console.WriteLine("На 0 делить нельзя");
        }
        else if (n1 == 0)
        {
            Console.WriteLine("0/x --> 0");
        }
        else
        {
            bool negative = (n1 < 0) ^ (n2 < 0);
            n1 = Math.Abs(n1);
            n2 = Math.Abs(n2);
            int gcd = GCD(n1, n2);
            n1 /= gcd;
            n2 /= gcd;
            Console.WriteLine($"Результат: {(negative ? "-(" : "(")}{n1}/{n2}) {(n2 == 1 ? ("--> " + n1) :"")}");
        }
    }

    private static void Z4()
    {
        int n = 32;
        int n2 = 16;
        while (n2 != 0)
        {
            Console.WriteLine($"»Ваше число: {n}? («да-3» «больше-2» «меньше-1)");
            int a = Convert.ToInt16(Console.ReadLine());
            if (a == 3) break;
            if (a == 2) n += n2;
            else n -= n2;
            n2 /= 2;
        }
        Console.WriteLine($"Ваше число: {n}");
    }

    private static void Z5()
    {
        List<string> componentsN = new(["вода", "молоко"]);
        List<int> componentsM = [];
        for (int i = 0; i < componentsN.Count; i++)
        {
            Console.WriteLine($"Введите количество {componentsN[i]} в мл:");
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

        while (VarietyDrinks.Any(i => i.Chek(componentsM)))
        {
            Console.Write("\nВыберите напиток ");
            for (int i = 0; i < VarietyDrinks.Count; i++)
            {
                Console.Write($"{i+1} - {VarietyDrinks[i].name}, ");
            }
            Console.Write("\n");
            int selectedItem = Convert.ToInt32(Console.ReadLine()) - 1;
            if (selectedItem < 0 || selectedItem >= VarietyDrinks.Count)
            {
                Console.WriteLine("Такого напитка пока нет :(");
                continue;
            }

            Drink d = VarietyDrinks[selectedItem];
            if (!d.Chek(componentsM))
            {
                for (int i = 0; i < d.components.Count; i++)
                    if (componentsM[i] < d.components[i]) Console.WriteLine($"Не хватает: {componentsN[i]}");
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
            Console.WriteLine($"{componentsN[i]}: {componentsM[i]}мл");
        }
        Console.Write("\n");
        for (int i = 0; i < VarietyDrinks.Count; i++)
        {
            Console.Write($"Приготовлено кружек {VarietyDrinks[i].name}: {VarietyDrinks[i].ReturnNP()}");
            Console.Write($", что принесло: {VarietyDrinks[i].ReturnProfit()}р\n");
        }
        Console.Write("\n");
        Console.WriteLine($"Суммарный доход: {VarietyDrinks.Sum(i => i.ReturnProfit())}р");
    }

    private static void Z6()
    {
        Console.WriteLine("Введите количество бактерий:");
        Int64 n = Convert.ToInt64(Console.ReadLine());
        Console.WriteLine("Введите количество антибиотика:");
        Int64 x = Convert.ToInt64(Console.ReadLine());
        Int64 time = 0;
        while ((n > 0 && x > 0) || time < 5)
        {
            time++;
            n *= 2;
            n -= x;
            if (n < 0) n = 0;
            x--;
            Console.WriteLine($"После {time} часа бактерий осталось {n}");     
        }
    }

    private static void Z7()
    {
        Console.WriteLine("Введите n:");
        int n = Convert.ToInt32(Console.ReadLine());
        Console.WriteLine("Введите a:");
        int a = Convert.ToInt32(Console.ReadLine());
        Console.WriteLine("Введите b:");
        int b = Convert.ToInt32(Console.ReadLine());
        Console.WriteLine("Введите w:");
        int w = Convert.ToInt32(Console.ReadLine());
        Console.WriteLine("Введите h:");
        int h = Convert.ToInt32(Console.ReadLine());

        int l = 0;
        int r = Math.Min(w, h) / 2;
        int d = 0;
        while (l <= r)
        {
            int m = (l + r) / 2;
            if ((w / (a + 2 * m)) * (h / (b + 2 * m)) >= n)
            {
                d = m;
                l = m + 1;
            }
            else
            {
                r = m - 1;
            }
        }
        Console.WriteLine($"Ответ d: {d}");
    }
}
