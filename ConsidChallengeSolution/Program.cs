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
            //Stopwatch s = Stopwatch.StartNew();
            //string inputStr = @"D:\Temporary Downloads\Rgn02.txt";
            //MemoryMappedFile originalFile = MemoryMappedFile.CreateFromFile(inputStr, FileMode.Open, "mmFile");
            //s.Stop();
            times = new long[8];
            int noExec = 20;
            long totalTime = 0;
            //totalTime = s.ElapsedMilliseconds*noExec;
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
            //Thread[] tasks = new Thread[noOfThreads];
            Task[] tasks = new Task[noOfThreads];
            MemoryMappedFile originalFile = MemoryMappedFile.CreateFromFile(inputStr, FileMode.Open, "mmFile");
            Parallel.For(0, noOfThreads, i =>
            {
                long offset = i * length;
                //Thread task = new Thread
                //        (() => threadWorker(bitArr, offset, length));
                Task task = Task.Factory.StartNew
                    (() => threadWorker(bitArr, offset, length), TaskCreationOptions.None);
                tasks[i] = task;
                //task.Start();
            });
            //foreach (Thread t in tasks) {
            //    t.Join();
            //}
            while (noOfRemainingTasks > 0)
            {
                Task.WaitAny(tasks);
                if (existsDuplicate)
                {
                    Console.WriteLine("Dubbletter");
                    time.Stop();
                    //originalFile.Dispose();
                    return (time.ElapsedMilliseconds);
                }
                noOfRemainingTasks--;
            }
            int j = bitArr.Count / noOfThreads;
            int k = 0;
            Task.WaitAll(tasks);
            for (int i = 0; i < bitArr.Count; i++) {
                if (bitArr.Get(i))
                    k++;
            }
            if (k == fileLength/8)
            {
                Console.WriteLine("Ej dubblett");
            }
            else {
                Console.WriteLine("Dubbletter");
                Console.WriteLine(k);
            }
            time.Stop();
            originalFile.Dispose();
            return (time.ElapsedMilliseconds);
        }

        static void threadWorker(BitArray bitArr, long offset, long length) {
            BitArray ba = (BitArray)bitArr.Clone();
            //BitArray ba = new BitArray(bitArr.Length);
            int val;
            byte[] plate = new byte[length];
            //Stopwatch t = Stopwatch.StartNew();
            //Stopwatch s = Stopwatch.StartNew();
            MemoryMappedFile mmf = MemoryMappedFile.OpenExisting("mmFile");
            MemoryMappedViewStream stream = mmf.CreateViewStream(offset, length, MemoryMappedFileAccess.Read);
            //s.Stop();
            //times[0] += s.ElapsedMilliseconds;
            //s = Stopwatch.StartNew();
            stream.Read(plate, 0, (int)length);
            mmf.Dispose();
            stream.Dispose();
            //s.Stop();
            //times[1] += s.ElapsedMilliseconds;
            //Stopwatch u = Stopwatch.StartNew();
            for (int i = 0; i < length; i += 8)//Each line is 8 bytes
            {
                val = plate[i+0] * 676000;
                val += plate[i+1] * 26000;
                val += plate[i+2] * 1000;
                val += plate[i+3] * 100;
                val += plate[i+4] * 10;
                val += plate[i+5];
                val -= 45700328;
                lock (bitArr)
                {
                    if (bitArr.Get(val))
                    {
                        mmf.Dispose();
                        stream.Dispose();
                        existsDuplicate = true;
                        return;
                    }
                    //ba.Set(val, true);
                }
                bitArr.Set(val, true);
            }
            //u.Stop();
            //times[5] += u.ElapsedMilliseconds;
            //s = Stopwatch.StartNew();
            //lock (bitArr)
            //{
            //    bitArr.Or(ba);
            //}
            //s.Stop();
            //t.Stop();
            //times[2] += s.ElapsedMilliseconds;
            //times[4] += t.ElapsedMilliseconds;
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