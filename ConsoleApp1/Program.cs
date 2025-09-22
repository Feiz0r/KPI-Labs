using System.ComponentModel;
using System.Diagnostics;
using System.Numerics;

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
    static double Factorial(int n)
    {
        if (n <= 1)
            return 1;
        return n * Factorial(n - 1);
    }

    public static double Z1(double x, int n)
    {
        if (n == 0)       
            return x;
        double term = Math.Pow(-1, n) * Math.Pow(x, 2 * n + 1) / Factorial(2 * n + 1);
        return Z1(x, n - 1) + term;
    }
}

internal class Program
{
   
    private static void Main()
    {
        int p = 2;
        int z = 1;

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
                }
                break;
        }      
    }
}