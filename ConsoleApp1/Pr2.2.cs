using System.Collections;
using System.Numerics;
using System.Text;

class Pr2_2
{
    class Matrix
    {
        private readonly List<List<double>> data = [];      

        public Matrix(int h, int m)
        {
            ValidateDimensions(h, m);

            for (int i = 0; i < h; i++)
            {
                data.Add([]);
                for (int j = 0; j < m; j++)
                {
                    data[i].Add(0);
                }
            }
        }
        public Matrix(List<List<double>> inputList)
        {
            ValidateDimensions(inputList);

            foreach (var r in inputList)
            {
                data.Add(new List<double>(r));
            }
        }

        public int SizeI() => data.Count;
        public int SizeJ() => data[0].Count;
        public double this[int i, int j]
        {
            get => data[i][j];
            set => data[i][j] = value;
        }

        public static Matrix operator +(Matrix a, Matrix b)
        {
            bool error = (a.SizeI() != b.SizeI()) || (a.SizeJ() != b.SizeJ());
            if (error)
                throw new Exception("Сложение невозможно - размерность несовпадает");

            List<List<double>> result = [];
            for (int i = 0; i < a.SizeI(); i++)
            {
                List<double> l = [];
                for (int j = 0; j < a.SizeJ(); j++)
                {
                    l.Add(a[i,j] + b[i,j]);
                }
                result.Add(l);
            }
            return new Matrix(result);
        }
        public static Matrix operator *(Matrix a, Matrix b)
        {
            bool error = a.SizeJ() != b.SizeI();
            if (error)
                throw new Exception("Умножение невозможно - матрицы несогласованны");


            List<List<double>> result = [];
            for (int i = 0; i < a.SizeI(); i++)
            {
                List<double> l = [];
                for (int j = 0; j < b.SizeJ(); j++)
                {
                    double c = 0;
                    for (int k = 0; k < a.SizeJ(); k++)
                    {
                        c += a[i,k] * b[k,j];
                    }
                    l.Add(c);
                }
                result.Add(l);
            }

            return new Matrix(result);
        }
        public static Matrix operator -(Matrix a, Matrix b) => a + (b * -1);
        public static Matrix operator /(Matrix a, double b) => a * (1 / b);
        public static Matrix operator *(Matrix a, double b)
        {
            List<List<double>> result = [];
            for (int i = 0; i < a.SizeI(); i++)
            {
                List<double> l = [];
                for (int j = 0; j < a.SizeJ(); j++)
                {
                    l.Add(a[i,j] * b);
                }
                result.Add(l);
            }
            return new Matrix(result);
        }
        public static Matrix operator |(Matrix a, Matrix b) => Augment(a, b);

        public Matrix GetTranspose()
        {
            List<List<double>> t = [];
            for (int i = 0; i < SizeI(); i++)
            {
                List<double> row = [];
                for (int j = 0; j < SizeJ(); j++)
                {
                    row.Add(this[j,i]);
                }
                t.Add(row);
            }
            return new Matrix(t);
        }
        public Matrix GetInverse()
        {
            if (SizeI() != SizeJ())
                throw new Exception("Обратную матрицу невозможно вычислить - неквадратная матрица");

            double det = GetDet();
            if (Math.Abs(det) < 1e-10)
                throw new Exception("Обратную матрицу невозможно вычислить - вырожденная матрица");

            List<List<double>> result = [];
            for (int i = 0; i < SizeI(); i++)
            {
                List<double> row = [];
                for (int j = 0; j < SizeJ(); j++)
                {
                    double minorDet = GetMinor(i, j).GetDet();
                    double cofactor = Math.Pow(-1, i + j) * minorDet;
                    row.Add(cofactor);
                }
                result.Add(row);
            }

            Matrix cMatrix = new(result);
            Matrix aMatrix = cMatrix.GetTranspose();
            return (aMatrix * (1 / det));
        }
        public double GetDet(List<List<double>>? m = null)
        {
            if (m == null)
            {
                m = data;
                if (m.Count != m[0].Count)
                    throw new Exception("Определитель невозможно вычислить");
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
        public Matrix GetMinor(int row, int col)
        {
            List<List<double>> minor = [];
            for (int i = 0; i < SizeI(); i++)
            {
                if (i == row) continue;
                List<double> newRow = [];
                for (int j = 0; j < SizeJ(); j++)
                {
                    if (j == col) continue;
                    newRow.Add(this[i,j]);
                }
                minor.Add(newRow);
            }
            return new Matrix(minor);
        }
        public Matrix GetRREF()
        {
            var result = new Matrix(data);
            int lead = 0;
            int rowCount = SizeI();
            int columnCount = SizeJ();

            for (int r = 0; r < rowCount; r++)
            {
                if (lead >= columnCount) break;

                int i = r;
                while (Math.Abs(result[i, lead]) < 1e-10)
                {
                    i++;
                    if (i == rowCount)
                    {
                        i = r;
                        lead++;
                        if (lead == columnCount) return result;
                    }
                }
                for (int j = 0; j < columnCount; j++)
                {
                    double temp = result[i, j];
                    result[i, j] = result[r, j];
                    result[r, j] = temp;
                }
                double div = result[r, lead];
                if (Math.Abs(div) > 1e-10)
                    for (int j = 0; j < columnCount; j++)
                        result[r, j] /= div;
                for (int j = 0; j < rowCount; j++)
                {
                    if (j != r)
                    {
                        double mult = result[j, lead];
                        for (int k = 0; k < columnCount; k++)
                            result[j, k] -= mult * result[r, k];
                    }
                }
                lead++;
            }
            return result;
        }
        public int GetRank()
        {
            var rref = GetRREF();
            int rank = 0;
            for (int i = 0; i < SizeI(); i++)
                if (!IsZeroRow(rref.data[i]))
                    rank++;
            return rank;
        }

        public Matrix Clean(double tolerance = 1e-10)
        {
            List<List<double>> cleaned = [];
            for (int i = 0; i < SizeI(); i++)
            {
                List<double> row = [];
                for (int j = 0; j < SizeJ(); j++)
                {
                    double value = this[i,j];
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
            for (int i = 0; i < SizeI(); i++)
            {
                List<double> row = [];
                for (int j = 0; j < SizeJ(); j++)
                {
                    row.Add(Math.Round(this[i,j], decimals));
                }
                rounded.Add(row);
            }
            Matrix result = new(rounded);
            if (clean)
                return result.Clean();
            else
                return result;
        }

        public static Matrix Augment(Matrix a, Matrix b)
        {
            if (a.SizeI() != b.SizeI())
                throw new ArgumentException("Расширение невозможно - разное количество строк");

            int rows = a.SizeI();
            int colsA = a.SizeJ();
            int colsB = b.SizeJ();

            List<List<double>> augmentedData = [];

            for (int i = 0; i < rows; i++)
            {
                List<double> newRow = [];
                for (int j = 0; j < colsA; j++)
                    newRow.Add(a[i, j]);
                for (int j = 0; j < colsB; j++)
                    newRow.Add(b[i, j]);
                augmentedData.Add(newRow);
            }
            return new Matrix(augmentedData);
        }
        public static Matrix Solve(Matrix a, Matrix b)
        {
            if (a.GetDet() == 0)
                throw new Exception("Решить СЛАУ невозможно - вырожденная матрица");
            if (a.SizeI() != b.SizeI())
                throw new Exception("Решить СЛАУ невозможно - разная размерность");
            int rankA = a.GetRank();
            int rankAB = (a | b).GetRank();
            if (rankA < rankAB)
                throw new Exception("Решить СЛАУ невозможно - нет решений");
            if ((rankA == rankAB) && (rankA < b.SizeI()))
                throw new Exception("Решить СЛАУ невозможно - бесконечно много решений");

            return a.GetInverse() * b;
        }
        public static Matrix Identity(int size)
        {
            var result = new Matrix(size, size);
            for (int i = 0; i < size; i++)
                result[i, i] = 1;
            return result;
        }
        public static Matrix Zero(int rows, int cols) => new(rows, cols);

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
        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var row in data)
            {
                sb.AppendLine(string.Join(" ", row));
            }
            return sb.ToString();
        }
        public void FillFromConsole()
        {
            Console.WriteLine($"Заполнение матрицы [{data.Count}:{data[0].Count}]:");
            Console.WriteLine("Введите строки матрицы через пробел, разделяя строки Enter:");
            for (int i = 0; i < SizeI(); i++)
            {
                Console.Write($"Строка {i + 1}: ");
                string[] values = Console.ReadLine().Split(' ');

                for (int j = 0; j < SizeJ(); j++)
                {
                    this[i,j] = int.Parse(values[j]);
                }
            }
        }
        public void FillRandom(int a, int b)
        {
            Random random = new();
            for (int i = 0; i < SizeI(); i++)
            {
                for (int j = 0; j < SizeJ(); j++)
                {
                    this[i,j] = random.Next(a, b + 1);
                }
            }
            Console.WriteLine($"Матрица заполнена случайными значениями в диапазоне [{a}:{b}]");
        }

        private static bool IsZeroRow(List<double> row)
        {
            return row.All(x => Math.Abs(x) < 1e-10);
        }
        private static List<List<double>> GetMinor(List<List<double>> m, int row, int col)
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
        private static void ValidateDimensions(int rows, int cols)
        {
            if (rows <= 0 || cols <= 0)
                throw new ArgumentException("Размеры матрицы должны быть положительными");
        }
        private static void ValidateDimensions(List<List<double>> m)
        {
            bool error = m.Count <= 0 || m[0].Count <= 0;
            for (int i = 0; i < m.Count; i++)
                error = error || (m[0].Count != m[i].Count);
            if (error)
                throw new ArgumentException("Нарушение размерности матрицы");
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
        Matrix o2 = m1 * m2;
        Matrix o3 = m1.GetTranspose();
        Matrix o4 = m1.GetInverse();
        Matrix o5 = m1 * o4;
        Matrix m3 = new([
            [1],
            [2],
            [3],
            [4],
            [5]
            ]);
        Matrix o6 = Matrix.Solve(m1, m3);
        double det = m1.GetDet();

        Console.Write(
            $"матрица m1:\n{m1}\n" +
            $"матрица m2:\n{m2}\n" +
            $"m1 + m2:\n{o1}\n" +
            $"m1 * m2:\n{o2}\n" +
            $"матрица m1 транспонирования:\n{o3}\n" +
            $"матрица обратная m1:\n{o4.Round(true)}\n" +
            $"проверка обратной матрицы m1:\n{o5.Round(true)}\n" +
            $"матрица m3:\n{m3}\n" +
            $"решения СЛАУ[m1|m3]:\n{o6.Round(true)}\n" +
            $"проверка решения СЛАУ(m1*решение):\n{(m1*o6).Round(true)}\n" +
            $"Определитель m1:\n{det}\n"
            );
    }
}

