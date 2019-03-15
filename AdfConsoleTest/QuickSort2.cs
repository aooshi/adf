using System;
using System.Collections.Generic;
using System.Text;

namespace AdfConsoleTest
{
    public class QuickSort2
    {
        public static void Qsort(double[] data, int low, int high)//递归实现
        {
            if (low >= high) return;
            int i, j;
            double pivot;
            i = low;
            j = high;
            pivot = data[low];
            while (i < j)
            {
                while (data[j] > pivot) j--;
                data[i] = data[j];
                while (i < j && data[i] <= pivot) i++;
                data[j] = data[i];
            }
            data[i] = pivot;
            Qsort(data, low, i - 1);
            Qsort(data, i + 1, high);
        }
    }
}
