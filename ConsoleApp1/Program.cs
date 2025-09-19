internal class Program
{
    private static void Cal()
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

    private static void Bank()
    {
        int[] banknote = { 5000, 2000, 1000, 500, 200, 100 };
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

    private static void Main(string[] args)
    {
        Cal();
        Bank();

    }
}