using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace AdfConsoleTest
{
    public class HeapSort
    {
        public static void Sort(double[] numbers)
        {
            // 得到大顶堆
            for (int i = numbers.Length / 2 - 1; i >= 0; i--)
            {
                HeapAdjust(numbers, i, numbers.Length - 1);
            }

            // 开始堆排序
            // 1. 即将堆顶元素（最大值）跟最后一个元素交换，此时最大元素已经就绪，放到了最后
            // 2. 现在只需要关注前n-1个结点就可了，由于上一步将取后一个元素放到了根结点，所以前n-1个结点不再是大顶堆了，
            //    所以现在要调整堆为一个大顶堆，即筛选
            // 3. 一次筛选完成之后把堆顶元素再和最后一个交换，次大数就绪
            // 4. 循环这个过程，最终得到有序序列
            double temp;
            for (int i = numbers.Length - 1; i > 0; )
            {
                temp = numbers[i];
                numbers[i] = numbers[0];
                numbers[0] = temp;
                i--;
                HeapAdjust(numbers, 0, i);
            }
        }

        static void HeapAdjust(double[] numbers, int index, int length)
        {
            double temp = numbers[index];
            for (int childIndex = 2 * index + 1; childIndex <= length; childIndex *= 2)
            {
                if (childIndex < length && numbers[childIndex] < numbers[childIndex + 1])
                {
                    childIndex++;
                }

                if (temp >= numbers[childIndex])
                {
                    break;
                }

                numbers[index] = numbers[childIndex];
                index = childIndex;
            }

            numbers[index] = temp;
        }
    }
}