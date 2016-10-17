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
        private static bool existsDuplicate;
        private static object locker;
        private static long length;
        private volatile static long[] times;
        private static Stopwatch[] timers;

        static void Main(string[] args)
        {
            times = new long[8];
            int noExec = 20;
            long totalTime = 0;
            for (int i = 0; i < noExec; i++) {
                totalTime += doStuff();
            }
            Console.WriteLine("Total time was: " + (totalTime / noExec));
            for (int i = 0; i < times.Length; i++) {
                if (times[i] > 0)
                    Console.WriteLine("Timer " + i + " averaged " + times[i] / noExec + " which is " + times[i] / totalTime + " of the total execution time");
            }
            Console.ReadLine();
        }

        static long doStuff()
        {
            Stopwatch time = Stopwatch.StartNew();
            int noOfThreads = 4;
            locker = new object();
            string inputStr = @"D:\Temporary Downloads\Rgn02.txt";
            FileInfo fil = new FileInfo(inputStr);
            long fileLength = fil.Length;
            length = fileLength / noOfThreads;
            BitArray bitArr = new BitArray(17576000);
            int noOfRemainingTasks = noOfThreads;
            Task[] tasks = new Task[noOfThreads];
            MemoryMappedFile originalFile = MemoryMappedFile.CreateFromFile(inputStr, FileMode.Open, "mmFile");
            Parallel.For(0, noOfThreads, i =>
            {
                long offset = i * length;
                Task task = Task.Factory.StartNew
                    (() => threadWorker(bitArr, offset, length), TaskCreationOptions.None);
                tasks[i] = task;
            });
            //while (noOfRemainingTasks > 0)
            //{
            //    Task.WaitAny(tasks);
            //    if (existsDuplicate)
            //    {
            //        Console.WriteLine("Dubbletter");
            //        time.Stop();
            //        originalFile.Dispose();
            //        return (time.ElapsedMilliseconds);
            //    }
            //    noOfRemainingTasks--;
            //}
            Task.WaitAll(tasks);
            int sum = 0;
            object sumLock = sum;
            int j = bitArr.Count / noOfThreads;
            //Parallel.For(0, noOfThreads,
            //    i =>
            //    {
            for (int index = 0; index < noOfThreads; index++)
            {
                int count = 0;
                int end = j * (index + 1);
                Task task = Task.Factory.StartNew
                (() =>
                {
                    for (int m = end-j; m < end; m++)
                    {
                        if (bitArr.Get(m))
                            count++;
                    }
                    lock (sumLock)
                    {
                        sum += count;
                    }
                });
                tasks[index] = task;
            }

            //});
            Task.WaitAll(tasks);
            if (sum != fileLength / 8)
            {
                Console.WriteLine("Dubbletter");
                Console.WriteLine(sum);
                time.Stop();
                originalFile.Dispose();
                return (time.ElapsedMilliseconds);
            }
            Console.WriteLine("Ej Dubblett");
            time.Stop();
            originalFile.Dispose();
            return (time.ElapsedMilliseconds);
        }

        static void threadWorker(BitArray bitArr, long offset, long length) {
            bool[] boolArr = new bool[17576000];
            //BitArray ba = new BitArray(bitArr.Length);
            int val;
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
                //lock (bitArr)
                //{
                //    if (bitArr.Get(val))
                //    {
                //        mmf.Dispose();
                //        stream.Dispose();
                //        existsDuplicate = true;
                //        return;
                //    }
                //    bitArr.Set(val, true);
                //}
                boolArr[val] = true;
            }
            mmf.Dispose();
            stream.Dispose();
            BitArray ba = new BitArray(boolArr);
            lock (bitArr)
            {
                bitArr.Or(ba);
            }
        }


        static void block(BitArray bitArr, BitArray ba) {
            lock (bitArr)
            {
                bitArr.Or(ba);
            }
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