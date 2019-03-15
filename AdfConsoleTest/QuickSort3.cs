using System;
using System.Collections.Generic;
using System.Text;

namespace AdfConsoleTest
{
    public class QuickSort3
    {
        //https://zh.wikipedia.org/zh-hant/%E5%BF%AB%E9%80%9F%E6%8E%92%E5%BA%8F#C.23


        public static void Sort(double[] numbers)
        {
            Sort(numbers, 0, numbers.Length - 1);
        }

        private static void Sort(double[] numbers, int left, int right)
        {
            if (left < right)
            {
                double middle = numbers[(left + right) / 2];
                int i = left - 1;
                int j = right + 1;
                while (true)
                {
                    while (numbers[++i] < middle) ;

                    while (numbers[--j] > middle) ;

                    if (i >= j)
                        break;

                    Swap(numbers, i, j);
                }

                Sort(numbers, left, i - 1);
                Sort(numbers, j + 1, right);
            }
        }

        private static void Swap(double[] numbers, int i, int j)
        {
            double number = numbers[i];
            numbers[i] = numbers[j];
            numbers[j] = number;
        }
    }
}
