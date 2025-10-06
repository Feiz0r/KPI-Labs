internal class Program
{
    private static void Main()
    {
        int p = 2;
        int z = 8;

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
        }      
    }
}