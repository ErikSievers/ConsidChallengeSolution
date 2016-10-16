using System;
using System.IO.MemoryMappedFiles;
using System.IO;
using System.Collections;
using System.Diagnostics;
using System.Threading;

namespace ConsidChallengeSolution
{
    class Program
    {
        static void Main(string[] args)
        {
            int noExec = 100;
            long totalTime = 0;
            for (int i = 0; i < noExec; i++) {
                totalTime += doStuff();
            }
            Console.WriteLine(totalTime / noExec);
            Console.ReadLine();
        }

        static long doStuff()
        {
            Stopwatch time = Stopwatch.StartNew();

            int noOfThreads = 8;
            string inputStr = @"D:\Temporary Downloads\Rgn01.txt";
            FileInfo fi1 = new FileInfo(inputStr);
            long fileLength = fi1.Length;
            long offsetPerThread = fileLength / noOfThreads;
            long length = fileLength / noOfThreads;
            BitArray bitArr = new BitArray(17576000);//The number of possible licence plates

            using (var mmf = MemoryMappedFile.CreateFromFile(inputStr, FileMode.Open, "mmFile"))
            {
                for (int i = 0; i < noOfThreads; i++)
                {
                    Thread t = new Thread(() => threadWorker(bitArr, offsetPerThread * i, length, mmf));
                    t.Start();
                }
            }
            Console.WriteLine("No Duplicates Found");
            time.Stop();
            return (time.ElapsedMilliseconds);
        }

        static void threadWorker(BitArray bitArr, long offset, long length, MemoryMappedFile mmf) {
            using (var stream = mmf.CreateViewStream(offset, length))
            {
                byte[] plate = new byte[8];
                int val;
                for (int i = 0; i < length; i += 8)//Each line is 8 bytes
                {
                    stream.Read(plate, 0, 8);
                    //val = plate[0] - 0x41;
                    //val *= 26;
                    //val += plate[1] - 0x41;
                    //val *= 26;
                    //val += plate[2] - 0x41;
                    //val *= 10;
                    //val += plate[3] - 0x30;
                    //val *= 10;
                    //val += plate[4] - 0x30;
                    //val *= 10;
                    //val += plate[5] - 0x30;

                    val = plate[0] * 676000;
                    val += plate[1] * 26000;
                    val += plate[2] * 1000;
                    val += plate[3] * 100;
                    val += plate[4] * 10;
                    val += plate[5];
                    val -= 45700328;
                    if (bitArr.Get(val))
                    {
                        Console.WriteLine("Duplicate found");
                        Environment.Exit(1);
                    }
                    bitArr.Set(val, true);
                }
            }
        }

        static void calcPlate(int val) {
            Console.Write((char)val / 676000);
            Console.Write((char)(val % 676000)/26000);
            Console.Write((char)(val % 26000) / 1000);
            Console.Write((char)(val % 1000) / 100);
            Console.Write((char)(val % 100) / 10);
            Console.Write((char)(val % 10));
            Console.WriteLine("");
        }
    }
}
;