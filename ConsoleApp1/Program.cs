internal class Program
{
    private static void Main()
    {
        int p = 6;
        int z = 1;

        switch (p)
        {
            case 1:
                _ = new Pr1(z);
                break;
            case 2:
                if (z != 8)
                    _ = new Pr2(z);
                else
                    _ = new Pr2_2();
                break;
            case 3:
                if (z != 3)
                    _ = new Pr3(z);
                else
                    _ = new Pr3_2();
                break;
            case 4:
                _ = new Pr4();
                break;
            case 5:
                _ = new Pr5();
                break;
            case 6:
                _ = new Pr6(z);
                break;
        }
    }
}
