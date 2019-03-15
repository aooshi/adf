using System;
using System.Collections.Generic;
using System.Text;

namespace AdfConsoleTest
{
    public class HashSetTest
    {
        public void Test()
        {
            var hash = new HashSet();

            hash.Add("1");
            hash.Add("2");
            hash.Add("3");
            hash.Add("4");
            hash.Add("5");
            hash.Remove("5");
            hash.Remove("4");
            hash.Add("4");
            hash.Add("5");

        }



        class HashSet
        {
            const int SIZE = 10;

            private int[] buckets = new int[SIZE];

            //slots
            private int[] hashCodes = new int[SIZE];
            private int[] nexts = new int[SIZE];
            private string[] values = new string[SIZE];

            //
            private int count = 0;
            private int lastIndex = 0;
            private int freeIndex = -1;


            public bool Contains(string item)
            {
                int hashCode = item.GetHashCode();
                //
                for (int i = this.buckets[hashCode % this.buckets.Length] - 1; i >= 0; i = this.nexts[i])
                {
                    if (this.hashCodes[i] == hashCode && string.Equals(this.values[i], item))
                    {
                        return true;
                    }
                }

                return false;
            }


            public bool Remove(string item)
            {
                int hashCode = item.GetHashCode();
                int bucket = hashCode % this.buckets.Length;
                int last = -1;
                for (int i = this.buckets[bucket] - 1; i >= 0; last = i, i = this.nexts[i])
                {
                    if (this.hashCodes[i] == hashCode && string.Equals(this.values[i], item))
                    {
                        if (last < 0)
                        {

                            this.buckets[bucket] = this.nexts[i] + 1;
                        }
                        else
                        {

                            this.nexts[last] = this.nexts[i];
                        }
                        this.hashCodes[i] = -1;
                        this.values[i] = null;
                        this.nexts[i] = this.freeIndex;

                        this.count--;
                        if (this.count == 0)
                        {
                            this.lastIndex = 0;
                            this.freeIndex = -1;
                        }
                        else
                        {
                            this.freeIndex = i;
                        }
                        return true;
                    }
                }


                return false;
            }


            public int Count
            {
                get { return this.count; }
            }

            public bool Add(string value)
            {
                int hashCode = value.GetHashCode();
                int bucket = hashCode % this.buckets.Length;
                for (int i = this.buckets[bucket] - 1; i >= 0; i = this.nexts[i])
                {
                    if (this.hashCodes[i] == hashCode && string.Equals(this.values[i], value))
                    {
                        return false;
                    }
                }

                int index;
                if (this.freeIndex >= 0)
                {
                    index = this.freeIndex;
                    this.freeIndex = this.nexts[index];
                }
                else
                {
                    if (this.lastIndex == this.nexts.Length)
                    {
                        //SetCapacity(this.values.length,false);
                        //bucket = hashCode % this.buckets.Length;
                        throw new OutOfMemoryException("out allow size," + SIZE);
                    }
                    index = this.lastIndex;
                    this.lastIndex++;
                }
                this.hashCodes[index] = hashCode;
                this.values[index] = value;
                this.nexts[index] = this.buckets[bucket] - 1;
                this.buckets[bucket] = index + 1;
                this.count++;

                return true;
            }

        }

    }
}