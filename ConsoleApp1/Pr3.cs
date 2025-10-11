class Pr3
{

    public Pr3(int i)
    {
        switch (i)
        {
            case 1:
                Pr3.Z1();
                break;
            case 2:
                Pr3.Z2();
                break;

        }
    }

    private static int Z1R(int a, int b)
    {
        if (a == 0)
            return b;
        return Z1R(a / 10, b * 10 + (a % 10));
    }

    private static void Z1()
    {
        Console.WriteLine("Число n:");
        int a = Convert.ToInt32(Console.ReadLine());
        Console.WriteLine(Z1R(a, 0));
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static int Z2R(int m, int n)
    {
        if (m == 0) return n + 1;
        if (n == 0) return Z2R(m - 1, 1);
        return Z2R(m - 1, Z2R(m, n - 1));
    }

    private static void Z2()
    {

        Console.WriteLine("Введите m и n:");
        int m = Convert.ToInt32(Console.ReadLine());
        int n = Convert.ToInt32(Console.ReadLine());
        Console.WriteLine($"A({m},{n}): {Z2R(m,n)}");
    }
}