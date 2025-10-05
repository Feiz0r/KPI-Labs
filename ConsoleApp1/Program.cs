internal class Program
{
    private static void Main()
    {
        int p = 2;
        int z = 4;

        switch (p)
        {
            case 1:
                new Pr1(z);
                break;
            case 2:
                new Pr2(z);
                break;
        }      
    }
}