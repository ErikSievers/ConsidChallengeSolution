using System;
using System.IO.MemoryMappedFiles;
using System.IO;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using System.ComponentModel;
using System.Threading.Tasks;

namespace ConsidChallengeSolution
{
    class Program
    {
        private static bool existsDuplicate;
        private static object locker;
        private static long length;

        static void Main(string[] args)
        {
            int noExec = 10;
            long totalTime = 0;
            for (int i = 0; i < noExec; i++) {
                totalTime += doStuff();
                Thread.Sleep(1000);
            }
            Console.WriteLine(totalTime / noExec);
            Console.ReadLine();
        }

        static long doStuff()
        {
            Stopwatch time = Stopwatch.StartNew();
            int noOfThreads = 8;
            string inputStr = @"D:\Temporary Downloads\Rgn02.txt";
            locker = new object();

            FileInfo fil = new FileInfo(inputStr);
            long fileLength = fil.Length;
            length = fileLength / noOfThreads;
            BitArray bitArr = new BitArray(17576000);//The number of possible licence plates

            int noOfRemainingTasks = noOfThreads;
            Task[] tasks = new Task[noOfThreads];
            MemoryMappedFile originalFile = MemoryMappedFile.CreateFromFile(inputStr, FileMode.Open, "mmFile");
            CancellationToken cToken = new CancellationTokenSource().Token;

            for (long i = 0; i < noOfThreads; i++)
            {
                long offset = i * length;
                Task task = Task.Factory.StartNew
                        (() => threadWorker(bitArr, offset, length));
                tasks[i] = task;
            }            
            while (noOfRemainingTasks > 0)
            {
                Task.WaitAny(tasks);
                if (existsDuplicate)
                {
                    Console.WriteLine("Duplicate");
                    time.Stop();
                    originalFile.Dispose();
                    return (time.ElapsedMilliseconds);
                }
                noOfRemainingTasks--;
            }
            Console.WriteLine("No Duplicates Found");
            time.Stop();
            originalFile.Dispose();
            return (time.ElapsedMilliseconds);
        }

        static void threadWorker(BitArray bitArr, long offset, long length) {
            MemoryMappedFile mmf = MemoryMappedFile.OpenExisting("mmFile", MemoryMappedFileRights.Read);
            MemoryMappedViewStream stream = mmf.CreateViewStream(offset, length, MemoryMappedFileAccess.Read);
            byte[] plate = new byte[8];
            int val;
            for (int i = 0; i < length; i += 8)//Each line is 8 bytes
            {
                
                stream.Read(plate, 0, 8);
                val = plate[0] * 676000;
                val += plate[1] * 26000;
                val += plate[2] * 1000;
                val += plate[3] * 100;
                val += plate[4] * 10;
                val += plate[5];
                val -= 45700328;
                lock (locker)
                {
                    if (bitArr.Get(val))
                    {
                        mmf.Dispose();
                        stream.Dispose();
                        existsDuplicate = true;
                        return;
                    }
                    bitArr.Set(val, true);
                }
            }
            mmf.Dispose();
            stream.Dispose();
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