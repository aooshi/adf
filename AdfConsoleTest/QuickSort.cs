//https://www.microsoft.com/china/MSDN/library/langtool/VCSharp/USgetstart_vcsharp.mspx?mfr=true
//下面是 QuickSort C# .NET 示例应用程序的完整源代码。您可以复制、使用和分发这些代码（无版权费）。注意，这些源代码以"原样"提供并且不作任何保证。

//
//  QuickSort C# .NET Sample Application
//  Copyright 2001-2002 Microsoft Corporation. All rights reserved.
//
//  MSDN ACADEMIC ALLIANCE [http://www.msdn.microsoft.com/academic]
//  This sample is part of a vast collection of resources we developed for
//  faculty members in K-12 and higher education. Visit the MSDN AA web site for more!
//  The source code is provided "as is" without warranty.
//
// Import namespaces
using System;
using System.Collections;
using System.IO;
// Declare namespace
namespace MsdnAA
{
    // Declare application class
    public class QuickSortApp
    {
        //// Application initialization
        //static void Main(string[] szArgs)
        //{
        //    // Print startup banner
        //    Console.WriteLine("\nQuickSort C#.NET Sample Application");
        //    Console.WriteLine("Copyright (c)2001-2002 Microsoft Corporation. All rights reserved.\n");
        //    Console.WriteLine("MSDN ACADEMIC ALLIANCE [http://www.msdnaa.net/]\n");
        //    // Describe program function
        //    Console.WriteLine("This example demonstrates the QuickSort algorithm by reading an input file,");
        //    Console.WriteLine("sorting its contents, and writing them to a new file.\n");
        //    // Prompt user for filenames
        //    Console.Write("Source: ");
        //    string szSrcFile = Console.ReadLine();
        //    Console.Write("Output: ");
        //    string szDestFile = Console.ReadLine();
        //    // Read contents of source file
        //    string szSrcLine;
        //    ArrayList szContents = new ArrayList();
        //    FileStream fsInput = new FileStream(szSrcFile, FileMode.Open, FileAccess.Read);
        //    StreamReader srInput = new StreamReader(fsInput);
        //    while ((szSrcLine = srInput.ReadLine()) != null)
        //    {
        //        // Append to array
        //        szContents.Add(szSrcLine);
        //    }
        //    srInput.Close();
        //    fsInput.Close();
        //    // Pass to QuickSort function
        //    QuickSort(szContents, 0, szContents.Count - 1);
        //    // Write sorted lines
        //    FileStream fsOutput = new FileStream(szDestFile, FileMode.Create, FileAccess.Write);
        //    StreamWriter srOutput = new StreamWriter(fsOutput);
        //    for (int nIndex = 0; nIndex < szContents.Count; nIndex++)
        //    {
        //        // Write line to output file
        //        srOutput.WriteLine(szContents[nIndex]);
        //    }
        //    srOutput.Close();
        //    fsOutput.Close();
        //    // Report program success
        //    Console.WriteLine("\nThe sorted lines have been written to the output file.\n\n");
        //}

        // QuickSort implementation
        public static void QuickSort(double[] szArray, int nLower, int nUpper)
        {
            // Check for non-base case
            if (nLower < nUpper)
            {
                // Split and sort partitions
                int nSplit = Partition(szArray, nLower, nUpper);
               QuickSort(szArray, nLower, nSplit - 1);
               QuickSort(szArray, nSplit + 1, nUpper);
            }
        }
        
        // QuickSort partition implementation
        private static int Partition(double[] szArray, int nLower, int nUpper)
        {
            // Pivot with first element
            int nLeft = nLower + 1;
            double szPivot = szArray[nLower];
            int nRight = nUpper;
            // Partition array elements
            double szSwap;
            while (nLeft <= nRight)
            {
                //// Find item out of place
                //while (nLeft <= nRight && (szArray[nLeft]).CompareTo(szPivot) <= 0)
                //    nLeft = nLeft + 1;

                //while (nLeft <= nRight && (szArray[nRight]).CompareTo(szPivot) > 0)
                //    nRight = nRight - 1;

                // Find item out of place
                while (nLeft <= nRight && (szArray[nLeft] == szPivot || szArray[nLeft] < szPivot))
                    nLeft = nLeft + 1;

                while (nLeft <= nRight && szArray[nRight] > szPivot)
                    nRight = nRight - 1;

                // Swap values if necessary
                if (nLeft < nRight)
                {
                    szSwap = szArray[nLeft];
                    szArray[nLeft] = szArray[nRight];
                    szArray[nRight] = szSwap;
                    nLeft = nLeft + 1;
                    nRight = nRight - 1;
                }
            }
            // Move pivot element
            szSwap = szArray[nLower];
            szArray[nLower] = szArray[nRight];
            szArray[nRight] = szSwap;
            return nRight;
        }
    }
}