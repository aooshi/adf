using System;
using System.Collections.Generic;
using System.Text;

namespace AdfConsoleTest
{
   public class DataSerializableTest
    {
       public void Test()
       {
           var data = new int[2][];
           data[0] = new int[] { 1,2 };
           data[1] = new int[] { 2,2 };

           var data2 = new double[2];
           data2[0] = 10;
           data2[1] = 1000;
           
           Console.WriteLine(data is Int32[]);
           Console.WriteLine(data.GetType());


           var bytes1 = Adf.DataSerializable.DefaultInstance.Serialize(data);

           var result1 = Adf.DataSerializable.DefaultInstance.Deserialize(data.GetType(), bytes1);
           Console.WriteLine(result1 != null);


           var bytes2 = Adf.DataSerializable.DefaultInstance.Serialize(data2);

           var result2 = Adf.DataSerializable.DefaultInstance.Deserialize(data2.GetType(), bytes2);
           Console.WriteLine(result2 != null);

           Console.Read();
       }
    }
}
