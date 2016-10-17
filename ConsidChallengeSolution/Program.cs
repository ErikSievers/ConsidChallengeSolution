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
            int noExec = 10;
            long totalTime = 0;
            for (int i = 0; i < noExec; i++) {
                totalTime += doStuff();
            }
            Console.WriteLine("Total time was: " +totalTime / noExec);
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
            string inputStr = @"D:\Temporary Downloads\Rgn02.txt";
            locker = new object();

            FileInfo fil = new FileInfo(inputStr);
            long fileLength = fil.Length;
            length = fileLength / noOfThreads;
            //BitArray[,,,,] bitArr = new BitArray[26,26,26,10,10];
            BitArray bitArr = new BitArray(17576000);

            int noOfRemainingTasks = noOfThreads;
            Thread[] tasks = new Thread[noOfThreads];
            MemoryMappedFile originalFile = MemoryMappedFile.CreateFromFile(inputStr, FileMode.Open, "mmFile");

            Parallel.For(0, noOfThreads, i =>
            {
                long offset = i * length;
                Thread task = new Thread
                        (() => threadWorker(bitArr, offset, length));
                tasks[i] = task;
                task.Start();
            });
            foreach (Thread t in tasks) {
                t.Join();
            }
            //if (existsDuplicate)
            if (bitArr.Count != fileLength)
            {
                Console.WriteLine("Duplicate");
                time.Stop();
                originalFile.Dispose();
                return (time.ElapsedMilliseconds);
            }
            Console.WriteLine("No Duplicates Found");
            time.Stop();
            originalFile.Dispose();
            return (time.ElapsedMilliseconds);
        }

        static void threadWorker(BitArray bitArr, long offset, long length) {
            BitArray ba = new BitArray(bitArr.Length);
            Stopwatch t = Stopwatch.StartNew();
            Stopwatch s = Stopwatch.StartNew();
            MemoryMappedFile mmf = MemoryMappedFile.OpenExisting("mmFile", MemoryMappedFileRights.Read);
            MemoryMappedViewStream stream = mmf.CreateViewStream(offset, length, MemoryMappedFileAccess.Read);
            byte[] plate = new byte[length];
            int val;
            s.Stop();
            times[0] += s.ElapsedMilliseconds;
            s = Stopwatch.StartNew();
            stream.Read(plate, 0, (int)length);
            s.Stop();
            times[1] += s.ElapsedMilliseconds;
            Stopwatch u = Stopwatch.StartNew();
            for (int i = 0; i < length; i += 8)//Each line is 8 bytes
            {
                val = plate[i+0] * 676000;
                val += plate[i+1] * 26000;
                val += plate[i+2] * 1000;
                val += plate[i+3] * 100;
                val += plate[i+4] * 10;
                val += plate[i+5];
                val -= 45700328;
                tt(val, bitArr);
                //bitArr.Set(val, true);
            }
            u.Stop();
            times[5] += u.ElapsedMilliseconds;
            mmf.Dispose();
            stream.Dispose();
            s = Stopwatch.StartNew();
            bitArr.Or(ba);
            s.Stop();
            t.Stop();
            times[2] += s.ElapsedMilliseconds;
            times[4] += t.ElapsedMilliseconds;
        }


        static void tt(int val, BitArray ba) {
            Stopwatch s = Stopwatch.StartNew();
            ba.Set(val, true);
            s.Stop();
            times[5] += s.ElapsedMilliseconds;
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