using System;
using System.IO.MemoryMappedFiles;
using System.IO;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Timers;
using System.Collections.Specialized;

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
            int noExec = 10;
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
            int noOfThreads = 6;
            locker = new object();
            string inputStr = @"D:\Temporary Downloads\Rgn02.txt";
            FileInfo fil = new FileInfo(inputStr);
            long fileLength = fil.Length;
            length = fileLength / noOfThreads;
            BitArray[,,] bArrArr = new BitArray[26, 26, 1000];
            for (int i1 = 0; i1 < 26; i1++)
            {
                for (int i2 = 0; i2 < 26; i2++)
                {
                    for (int i3 = 0; i3 < 1000; i3++)
                    {
                        bArrArr[i1, i2, i3] = new BitArray(26);
                    }

                }
            }
            BitArray bitArr = new BitArray(17576000);
            int noOfRemainingTasks = noOfThreads;
            Task[] tasks = new Task[noOfThreads];
            MemoryMappedFile originalFile = MemoryMappedFile.CreateFromFile(inputStr, FileMode.Open, "mmFile");
            Parallel.For(0, noOfThreads, i =>
            {
                long offset = i * length;
                Task task = Task.Factory.StartNew
                    (() => threadWorker(bArrArr, offset, length), TaskCreationOptions.None);
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
            int[] k = new int[noOfThreads];
            int[] j = new int[noOfThreads];
            int j1 = 26;
            for (int i = 0; i < noOfThreads; i++) {
                j[i] = j1 / noOfThreads;
                if (j1 % noOfThreads > 0)
                {
                    j1--;
                    j[i]++;
                }

            }
            Task.WaitAll(tasks);
            Parallel.For(0, noOfThreads,
                index =>
                {
                    for (int m = 0; m < j[index]; m++)
                    {
                        for (int n = 0; n < 26; n++)
                        {
                            for(int o = 0; o < 1000; o++)
                            {
                                for (int p = 0; p < 26; p++) {
                                    if (bArrArr[m, n, o].Get(p))
                                        k[index]++;
                                }
                            }
                        }
                    }
                }
            );
            int sum = 0;
            foreach(int k1 in k)
            {
                sum += k1;
            }
            if (sum != fileLength / 8)
            {
                Console.WriteLine("Dubbletter");
                time.Stop();
                originalFile.Dispose();
                return (time.ElapsedMilliseconds);
            }
            Console.WriteLine("Ej Dubblett");
            time.Stop();
            originalFile.Dispose();
            return (time.ElapsedMilliseconds);
        }

        static void threadWorker(BitArray[,,] bitArr, long offset, long length) {
            //BitArray ba = (BitArray)bitArr.Clone();
            //BitArray ba = new BitArray(bitArr.Length);
            //int val;
            byte[] plate = new byte[length];
            //Stopwatch t = Stopwatch.StartNew();
            //Stopwatch s = Stopwatch.StartNew();
            MemoryMappedFile mmf = MemoryMappedFile.OpenExisting("mmFile");
            MemoryMappedViewStream stream = mmf.CreateViewStream(offset, length, MemoryMappedFileAccess.Read);
            //s.Stop();
            //times[0] += s.ElapsedMilliseconds;
            //s = Stopwatch.StartNew();
            stream.Read(plate, 0, (int)length);
            //s.Stop();
            //times[1] += s.ElapsedMilliseconds;
            //Stopwatch u = Stopwatch.StartNew();
            for (int i = 0; i < length; i += 8)//Each line is 8 bytes
            {
                //val = plate[i+0] * 676000;
                //val += plate[i+1] * 26000;
                //val += plate[i+2] * 1000;
                //val += plate[i+3] * 100;
                //val += plate[i+4] * 10;
                //val += plate[i+5];
                //val -= 45700328;
                //ba.Set(val, true);
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
                int v1, v2, v3, v4, v5, v0;
                v1 = plate[i + 1]-65;
                v2 = plate[i + 2]-65;
                v3 = (plate[i + 3]-48)*100;
                v4 = (plate[i + 4] - 48) * 10;
                v5 = (plate[i + 5] - 48);
                v0 = plate[i]-65;
                
                bitArr[v1, v2, v3+v4+v5].Set(v0, true);
            }
            mmf.Dispose();
            stream.Dispose();
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