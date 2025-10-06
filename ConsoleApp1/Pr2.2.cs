using System;

class Pr2_2
{
    class Matrix
    {
        private List<List<int>> data = new List<List<int>>();

        public Matrix(int h, int m)
        {
            for (int i = 0; i < h; i++)
            {
                data.Add(new List<int>());
                for (int j = 0; j < m; j++)
                {
                    data[i].Add(0);
                }
            }
        }

        private Matrix(List<List<int>> inputList)
        {
            foreach (var r in inputList)
            {
                data.Add(new List<int>(r));
            }
        }

        public static Matrix operator +(Matrix a, Matrix b)
        {
            bool error = (a.data.Count != b.data.Count);
            for (int i = 0; i < a.data.Count; i++)        
                error = error || (a.data[i].Count != b.data[i].Count);
            if (error)
            {
                throw new Exception("Сложение невозможно - размер несовпадает");
            }

            List<List<int>> result = new List<List<int>>();
            for (int i = 0; i < a.data.Count; i++)
            {
                List<int> l = new List<int>();
                for (int j = 0; j < a.data[i].Count; j++)
                {
                    l.Add(a.data[i][j] + b.data[i][j]);
                }
                result.Add(l);
            }
            return new Matrix(result);
        }

        public void Print()
        {
            Console.WriteLine("\nВывод матрицы:");
            foreach (var row in data)
            {
                foreach (var element in row)
                {
                    Console.Write($"{element} ");
                }
                Console.WriteLine();
            }
        }

        public void FillFromConsole()
        {
            Console.WriteLine($"Заполнение матрицы [{data.Count}:{data[0].Count}]:");
            Console.WriteLine("Введите строки матрицы через пробел, разделяя строки Enter:");
            for (int i = 0; i < data.Count; i++)
            {
                Console.Write($"Строка {i + 1}: ");
                string[] values = Console.ReadLine().Split(' ');

                for (int j = 0; j < data[i].Count; j++)
                {
                    data[i][j] = int.Parse(values[j]);
                }
            }
            Print();
        }

        public void FillRandom(int a, int b)
        {
            Random random = new Random();
            for (int i = 0; i < data.Count; i++)
            {
                for (int j = 0; j < data[i].Count; j++)
                {
                    data[i][j] = random.Next(a, b + 1);
                }
            }
            Console.WriteLine($"Матрица заполнена случайными значениями в диапазоне [{a}:{b}]");
            Print();
        }
    }

    public Pr2_2()
    {
        Matrix m1 = new Matrix(3, 3);
        //m1.FillFromConsole();
        m1.FillRandom(1, 5);

        Matrix m2 = new Matrix(3, 3);

        //Console.WriteLine("Введите минимальное значение (a): ");
        //int a = Convert.ToInt32(Console.ReadLine());
        //Console.WriteLine("Введите максимальное значение (b): ");
        //int b = Convert.ToInt32(Console.ReadLine());
        //m2.FillRandom(a,b);
        m2.FillRandom(1, 5);

        Matrix o1 = m1 + m2;
        o1.Print();
    }
}

