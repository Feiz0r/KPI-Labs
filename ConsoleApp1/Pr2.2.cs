using System.Numerics;

class Pr2_2
{
    class Matrix
    {
        private readonly List<List<double>> data = [];

        public Matrix(int h, int m)
        {
            bool error = (h <= 0 || m <= 0);
            if (error)
                throw new Exception("Размерность матрицы должна быть больше 0");

            for (int i = 0; i < h; i++)
            {
                data.Add([]);
                for (int j = 0; j < m; j++)
                {
                    data[i].Add(0);
                }
            }
        }

        private Matrix(List<List<double>> inputList)
        {
            foreach (var r in inputList)
            {
                data.Add(new List<double>(r));
            }
        }

        public static Matrix operator +(Matrix a, Matrix b)
        {
            bool error = (a.data.Count != b.data.Count) || (a.data[0].Count != b.data[0].Count);
            if (error)
                throw new Exception("Сложение невозможно - размерность несовпадает");

            List<List<double>> result = [];
            for (int i = 0; i < a.data.Count; i++)
            {
                List<double> l = [];
                for (int j = 0; j < a.data[0].Count; j++)
                {
                    l.Add(a.data[i][j] + b.data[i][j]);
                }
                result.Add(l);
            }
            return new Matrix(result);
        }

        public static Matrix operator *(Matrix a, Matrix b)
        {
            bool error = a.data.Count != b.data[0].Count;
            if (error)
                throw new Exception("Умножение невозможно - матрицы несогласованны");


            List<List<double>> result = [];
            for (int i = 0; i < a.data.Count; i++)
            {
                List<double> l = [];
                for (int j = 0; j < b.data[0].Count; j++)
                {
                    double c = 0;
                    for (int k = 0; k < a.data[i].Count; k++)
                    {
                        c += a.data[i][k] * b.data[k][j];
                    }
                    l.Add(c);
                }
                result.Add(l);
            }

            return new Matrix(result);
        }

        public static Matrix operator *(Matrix a, double b)
        {
            List<List<double>> result = [];
            for (int i = 0; i < a.data.Count; i++)
            {
                List<double> l = [];
                for (int j = 0; j < a.data[0].Count; j++)
                {
                    l.Add(a.data[i][j] * b);
                }
                result.Add(l);
            }
            return new Matrix(result);
        }

        public int SizeI() => data.Count;
        public int SizeJ() => data[0].Count;

        public Matrix Clean(double tolerance = 1e-10)
        {
            List<List<double>> cleaned = [];
            for (int i = 0; i < data.Count; i++)
            {
                List<double> row = [];
                for (int j = 0; j < data[0].Count; j++)
                {
                    double value = data[i][j];
                    if (Math.Abs(value) < tolerance)
                        row.Add(0.0);
                    else
                        row.Add(value);
                }
                cleaned.Add(row);
            }
            return new Matrix(cleaned);
        }

        public Matrix Round(bool clean = false, int decimals = 12)
        {
            List<List<double>> rounded = [];
            for (int i = 0; i < data.Count; i++)
            {
                List<double> row = [];
                for (int j = 0; j < data[0].Count; j++)
                {
                    row.Add(Math.Round(data[i][j], decimals));
                }
                rounded.Add(row);
            }
            Matrix result = new(rounded);
            if (clean)
                return result.Clean();
            else
                return result;
        }

        public Matrix GetMinor(int row, int col)
        {
            List<List<double>> minor = [];
            for (int i = 0; i < data.Count; i++)
            {
                if (i == row) continue;
                List<double> newRow = [];
                for (int j = 0; j < data[i].Count; j++)
                {
                    if (j == col) continue;
                    newRow.Add(data[i][j]);
                }
                minor.Add(newRow);
            }
            return new Matrix(minor);
        }

        static private List<List<double>> GetMinor(List<List<double>> m, int row, int col)
        {
            List<List<double>> minor = [];
            for (int i = 0; i < m.Count; i++)
            {
                if (i == row) continue;
                List<double> newRow = [];
                for (int j = 0; j < m[i].Count; j++)
                {
                    if (j == col) continue;
                    newRow.Add(m[i][j]);
                }
                minor.Add(newRow);
            }
            return minor;
        }

        public double GetDet(List<List<double>>? m = null)
        {
            if (m == null)
            {
                m = data;
                if (m.Count != m[0].Count)
                    throw new Exception("Определитель невозможно вычеслить");
            }
            if (m.Count == 1)
                return m[0][0];
            else
            {
                double result = 0;
                for (int i = 0; i < m.Count; i++)
                {
                    
                    List<List<double>> minor = GetMinor(m, 0, i);
                    result += ((i % 2 == 0) ? 1 : -1) * m[0][i] * GetDet(minor);
                }
                return result;
            }
        }

        public Matrix GetTranspose()
        {
            List<List<double>> t = [];
            for (int i = 0; i < data.Count; i++)
            {
                List<double> row = [];
                for (int j = 0; j < data[0].Count; j++)
                {
                    row.Add(data[j][i]);
                }
                t.Add(row);
            }
            return new Matrix(t);
        }

        public Matrix GetInverse()
        {
            if (data.Count != data[0].Count)
                throw new Exception("Обратную матрицу невозможно вычислить - неквадратная матрица");

            double determinant = GetDet();
            if (Math.Abs(determinant) < 1e-10)
                throw new Exception("Обратную матрицу невозможно вычислить - вырожденная матрица");

            List<List<double>> result = [];
            for (int i = 0; i < data.Count; i++)
            {
                List<double> row = [];
                for (int j = 0; j < data[0].Count; j++)
                {
                    double minorDet = GetMinor(i, j).GetDet();
                    double cofactor = Math.Pow(-1, i + j) * minorDet;
                    row.Add(cofactor);
                }
                result.Add(row);
            }

            Matrix cofactorMatrix = new(result);
            Matrix adjugateMatrix = cofactorMatrix.GetTranspose();
            return (adjugateMatrix * (1/determinant));
        }

        public Matrix Solve(Matrix a, Matrix b)
        {
            if (a.GetDet() == 0)
                throw new Exception("Решить СЛАУ невозможно - вырожденная матрица");
            if (a.SizeI() != b.SizeI() )
                throw new Exception("Решить СЛАУ невозможно - разная размерность");
            return a.GetInverse() * b;
        }

        public void Print(string s = "")
        {
            Console.WriteLine($"\nВывод матрицы{s}:");
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
            Random random = new();
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
        Matrix m1 = new(5, 5);
        //m1.FillFromConsole();
        m1.FillRandom(1, 5);

        Matrix m2 = new(5, 5);

        //Console.WriteLine("Введите минимальное значение (a): ");
        //int a = Convert.ToInt32(Console.ReadLine());
        //Console.WriteLine("Введите максимальное значение (b): ");
        //int b = Convert.ToInt32(Console.ReadLine());
        //m2.FillRandom(a,b);
        m2.FillRandom(1, 5);

        Matrix o1 = m1 + m2;
        o1.Print(" сложения");
        Matrix o2 = m1 * m2;
        o2.Print(" умножения");
        Matrix o3 = m1.GetTranspose();
        o3.Print(" транспонирования m1");
        Matrix o4 = m1.GetInverse();
        o4.Round(true).Print(" обратной m1");

        Matrix o5 = m1 * o4;
        o5.Round(true).Print(" проверки обратной матрицы m1");

        double det = m1.GetDet();
        Console.WriteLine($"Определитель: {det}");
    }
}

