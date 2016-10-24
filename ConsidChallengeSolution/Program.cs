using System;
using System.IO.MemoryMappedFiles;
using System.IO;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Timers;

namespace ConsidChallengeSolution
{
    class Program
    {
        private static long length;
        private static int noOfTasks = 8;

        static void Main(string[] args)
        {
            FileInfo fil = new FileInfo(args[0]);
            long fileLength = fil.Length;
            length = fileLength / noOfTasks;
            BitArray bitArr = new BitArray(17576000);
            Task[] tasks = new Task[noOfTasks];
            MemoryMappedFile originalFile = MemoryMappedFile.CreateFromFile(args[0], FileMode.Open, "mmFile");
            Parallel.For(0, noOfTasks, i =>
            {
                long offset = i * length;
                Task task = Task.Factory.StartNew
                    (() => worker(bitArr, offset, length), TaskCreationOptions.None);
                tasks[i] = task;
            });
            Task.WaitAll(tasks);
            int sum = GetCardinality(bitArr);
            if (sum != fileLength / 8)
            {
                Console.WriteLine("Dubbletter");
            }
            else
            {
                Console.WriteLine("Ej Dubblett");
            }
            originalFile.Dispose();
        }        

        static void worker(BitArray bitArr, long offset, long length) {
            int val;
            BitArray ba = new BitArray(bitArr.Length);
            byte[] plate = new byte[length];
            MemoryMappedFile mmf = MemoryMappedFile.OpenExisting("mmFile");
            MemoryMappedViewStream stream = mmf.CreateViewStream(offset, length, MemoryMappedFileAccess.Read);
            stream.Read(plate, 0, (int)length);
            for (int i = 0; i < length; i += 8)//Each line is 8 bytes
            {
                val = plate[i+0] * 676000;
                val += plate[i+1] * 26000;
                val += plate[i+2] * 1000;
                val += plate[i+3] * 100;
                val += plate[i+4] * 10;
                val += plate[i+5];
                val -= 45700328;
                ba.Set(val, true);
            }
            mmf.Dispose();
            stream.Dispose();
            lock (bitArr)
            {
                bitArr.Or(ba);
            }
        }

        //From Ronny Heuschkel on stackoverflow
        public static Int32 GetCardinality(BitArray bitArray)
        {
            Int32[] ints = new Int32[(bitArray.Count >> 5) + 1];

            bitArray.CopyTo(ints, 0);

            Int32 count = 0;

            // fix for not truncated bits in last integer that may have been set to true with SetAll()
            ints[ints.Length - 1] &= ~(-1 << (bitArray.Count % 32));

            for (Int32 i = 0; i < ints.Length; i++)
            {
                Int32 c = ints[i];
                // magic (http://graphics.stanford.edu/~seander/bithacks.html#CountBitsSetParallel)
                unchecked
                {
                    c = c - ((c >> 1) & 0x55555555);
                    c = (c & 0x33333333) + ((c >> 2) & 0x33333333);
                    c = ((c + (c >> 4) & 0xF0F0F0F) * 0x1010101) >> 24;
                }
                count += c;
            }
            return count;
        }

        static void calcPlate(byte[] plate) {
            foreach(byte b in plate)
            {
                Console.Write((char)b);
            }
            Console.WriteLine();
        }
    }
}
;