using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SUB5
{
    class Program
    {
        class BusInfo
        {
            public String time;
            public String busId;
            public double distance;
            public double busMPS;
            public BusInfo() { }
            public BusInfo(BusInfo bi)
            {
                time = bi.time;
                busId = bi.busId;
                distance = bi.distance;
                busMPS = bi.busMPS;
            }
            public BusInfo(String t, String b, double d, double m)
            {
                time = t;
                busId = b;
                distance = d;
                busMPS = m;
            }
        }

        class StationInfo
        {
            public String staName;
            public double staDist;
            public double limitMPS;

            public StationInfo() { }
            public StationInfo(String n, double d, double m)
            {
                staName = n;
                staDist = d;
                limitMPS = m;
            }
        }

        static void fillListBusInfo(String line)
        {
            listBusInfo = new List<BusInfo>();
            String[] words = line.Split('#');
            String time = words[0];

            for (int i = 1; i < words.Length; i++)
            {
                String[] words2 = words[i].Split(',');
                String busId = words2[0];
                double distance = double.Parse(words2[1]);
                BusInfo bi = new BusInfo(time, busId, distance, 0);
                listBusInfo.Add(bi);
            }
        }

        static int getBusInfoIdx(String id)
        {
            for (int i = 0; i < listBusInfo.Count; i++)
            {
                if (listBusInfo[i].busId == id)
                    return i;
            }

            return -1;
        }

        static double getStationLimitMPS(double dist)
        {
            int idx = 0;
            for (int i = 0; i < listStation.Count; i++)
            {
                if (dist < listStation[i].staDist)
                    break;
                idx = i;
            }
            return listStation[idx].limitMPS;
        }

        static void updateListBusInfo(String line)
        {
            String[] words = line.Split('#');
            String time = words[0];

            for (int i = 1; i < listBusInfo.Count + 1; i++)
            {
                if (words.Length == 1) // 복원
                {
                    int busIdx = i - 1;
                    listBusInfo[busIdx].distance += Math.Min(listBusInfo[busIdx].busMPS, getStationLimitMPS(listBusInfo[busIdx].distance)); // 속도만큼 거리 더하기 

                    listBusInfo[busIdx].time = time;
                }
                else // 단순 업데이트
                {
                    String[] words2 = words[i].Split(',');
                    String busId = words2[0];
                    double distance = double.Parse(words2[1]);
                    int busIdx = i - 1;
                    double MPS = distance - listBusInfo[busIdx].distance;

                    listBusInfo[busIdx].time = time;
                    listBusInfo[busIdx].distance = distance;
                    listBusInfo[busIdx].busMPS = MPS;
                }
            }
        }

        static void loadStationInfo()
        {
            listStation = new List<StationInfo>();
            StreamReader sr = new StreamReader("./INFILE/STATION.TXT");
            while (true)
            {
                String line = sr.ReadLine();
                if (line == null || line == "")
                {
                    break;
                }
                String[] words = line.Split('#');
                String staName = words[0];
                double staDist = double.Parse(words[1]);
                double staMPS = double.Parse(words[2]) * 1000.0 / 3600.0;
                listStation.Add(new StationInfo(staName, staDist, staMPS));
            }
            sr.Close();
        }

        static int comingBus(int staIdx)
        {
            double staDist = listStation[staIdx].staDist;
            double maxDist = 0;
            int idx = -1;
            for (int i = 0; i < listBusInfo.Count; i++)
            {
                if (listBusInfo[i].distance <= staDist)
                {
                    if (maxDist < listBusInfo[i].distance)
                    {
                        idx = i;
                        maxDist = listBusInfo[i].distance;
                    }
                }
            }

            return idx;
        }

        static void listWriteFile(List<String> listResult, String path)
        {
            StreamWriter sw = new StreamWriter(path);
            foreach (var item in listResult)
            {
                sw.WriteLine(item);
            }
            sw.Close();
        }

        static void MakeArrival()
        {
            List<String> listResult = new List<String>();

            String time = listBusInfo[0].time;
            for (int i = 0; i < listStation.Count; i++)
            {
                int busIdx = comingBus(i);
                String res = "";
                if (busIdx == -1)
                {
                    res = time + "#" + listStation[i].staName + "#" + "NOBUS,00000";
                }
                else
                {
                    double diffDist = listStation[i].staDist - listBusInfo[busIdx].distance;
                    res = string.Format("{0}#{1}#{2},{3:D5}", time, listStation[i].staName, listBusInfo[busIdx].busId, (int)diffDist);
                }
                listResult.Add(res);
            }
            listWriteFile(listResult, "./OUTFILE/ARRIVAL.TXT");
        }

        static void MakePrePost()
        {
            List<String> listResult = new List<String>();

            listBusInfo.Sort((x, y) => x.distance.CompareTo(y.distance));

            for (int i = 0; i < listBusInfo.Count; i++)
            {
                String resLine = "";
                String prev = "";
                String next = "";
                if (i == 0)
                {
                    prev = "NOBUS,00000";
                    double nextDiff = listBusInfo[i + 1].distance - listBusInfo[i].distance;
                    next = listBusInfo[i + 1].busId + "," + string.Format("{0:D5}", (int)nextDiff);
                }
                else if (i == listBusInfo.Count - 1)
                {
                    next = "NOBUS,00000";
                    double prevDiff = listBusInfo[i].distance - listBusInfo[i - 1].distance;
                    prev = listBusInfo[i - 1].busId + "," + string.Format("{0:D5}", (int)prevDiff);
                }
                else
                {
                    double prevDiff = listBusInfo[i].distance - listBusInfo[i - 1].distance;
                    double nextDiff = listBusInfo[i + 1].distance - listBusInfo[i].distance;
                    prev = listBusInfo[i - 1].busId + "," + string.Format("{0:D5}", (int)prevDiff);
                    next = listBusInfo[i + 1].busId + "," + string.Format("{0:D5}", (int)nextDiff);
                }

                resLine = string.Format("{0}#{1}#{2}#{3}", listBusInfo[i].time, listBusInfo[i].busId, next, prev);
                listResult.Add(resLine);
            }

            listResult.Sort();
            listWriteFile(listResult, "./OUTFILE/PREPOST.TXT");
        }

        static int findRightStation(double dist)
        {
            int idx = 0;
            for (int i = 0; i < listStation.Count; i++)
            {
                if (listStation[i].staDist > dist)
                {
                    idx = i;
                    break;
                }
            }

            return idx;
        }

        static int findLeftStation(double dist)
        {
            int idx = 0;
            for (int i = 0; i < listStation.Count; i++)
            {
                if (listStation[i].staDist > dist)
                {
                    break;
                }
                idx = i;
            }

            return idx;
        }

        static double calcSecBetweenStation(int staIdx1, int staIdx2, double defaultMPS)
        {
            double totalSec = 0;
            for (int i = staIdx1; i < staIdx1 + (staIdx2 - staIdx1); i++)
            {
                totalSec += (listStation[i + 1].staDist - listStation[i].staDist) / Math.Min(defaultMPS, listStation[i].limitMPS);
            }
            return totalSec;
        }

        static double calcBusSec(double busDist, int staIdx2, double defaultMPS)
        {
            int staIdx1 = findRightStation(busDist);
            double totalSec = 0;

            totalSec += (listStation[staIdx1].staDist - busDist) / Math.Min(defaultMPS, getStationLimitMPS(busDist));
            totalSec += calcSecBetweenStation(staIdx1, staIdx2, defaultMPS);

            return totalSec;
        }

        static bool comingBusByTime(List<BusInfo> listBusInfo, int staIdx, out int busIdx, out double dSec)
        {
            double dMinSec = double.MaxValue;
            int foundIdx = -1;

            for (int i = 0; i < listBusInfo.Count; i++)
            {
                if (listStation[staIdx].staDist >= listBusInfo[i].distance) // 버스가 정류장쪽으로 
                {
                    double sec = calcBusSec(listBusInfo[i].distance, staIdx, listBusInfo[i].busMPS);
                    if (dMinSec > sec)
                    {
                        dMinSec = sec;
                        foundIdx = i;
                    }
                }
            }

            busIdx = foundIdx;
            dSec = dMinSec;

            if (foundIdx == -1)
                return false;
            else
                return true;
        }

        static void MakeSignage()
        {
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = "SIGNAGE.EXE";
            start.UseShellExecute = false;
            start.RedirectStandardInput = true;
            start.RedirectStandardOutput = true;
            start.CreateNoWindow = true;

            Process process = Process.Start(start);         // test.exe를 실행시키고 
            StreamWriter wr = process.StandardInput;   // 출력되는 값을 가져오기 위해 StreamReader에 연결  

            ///////////////////////////////////////////////////////
            int busIdx = 0;
            double dSec = 0.0;
            for (int i = 0; i < listStation.Count; i++)
            {
                String res = "";
                bool bFind = comingBusByTime(listBusInfo, i, out busIdx, out dSec);
                if (!bFind)
                {
                    res = string.Format("{0}#{1}#NOBUS,00:00:00", listBusInfo[0].time, listStation[i].staName);
                }
                else
                {
                    DateTime dt = DateTime.ParseExact(listBusInfo[busIdx].time, "HH:mm:ss", null);
                    dt = dt.AddSeconds(dSec);
                    res = string.Format("{0}#{1}#{2},{3}", listBusInfo[busIdx].time, listStation[i].staName, listBusInfo[busIdx].busId, dt.ToString("HH:mm:ss"));
                }
                Console.WriteLine(res);
                wr.WriteLine(res);
            }

            process.Close();
        }

        static List<BusInfo> listBusInfo;
        static List<StationInfo> listStation;

        static void Main(string[] args)
        {
            listBusInfo = new List<BusInfo>();

            // load Station
            loadStationInfo();


            const string strIP = "127.0.0.1";
            const int PORT = 9876;

            IPAddress ipAddress = IPAddress.Parse(strIP);
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, PORT);

            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(localEndPoint);
            listener.Listen(10);

            // Start listening for connections. 
            try
            {
                while (true)
                {
                    Socket handler = listener.Accept();

                    Thread th = new Thread(doWork);
                    th.Start(handler);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        static int getStationIdx(String staName)
        {
            for (int i = 0; i < listStation.Count; i++)
            {
                if (listStation[i].staName == staName)
                    return i;
            }
            return -1;
        }

        static void updateBusInfo(List<BusInfo> listUpdate, double sec)
        {
            for (int i = 0; i < listBusInfo.Count; i++)
            {
                listUpdate.Add(new BusInfo(listBusInfo[i]));
            }

            for (int t = 1; t <= (int)sec; t++)
            {
                for (int i = 0; i < listUpdate.Count; i++)
                {
                    DateTime dt = DateTime.ParseExact(listUpdate[i].time, "HH:mm:ss", null);
                    dt = dt.AddSeconds(1);
                    listUpdate[i].time = dt.ToString("HH:mm:ss");
                    listUpdate[i].distance += Math.Min(listBusInfo[i].busMPS, getStationLimitMPS(listBusInfo[i].distance));
                }
            }
        }

        static String calcExpectTime(String toStation, double personDist)
        {
            int staIdx = getStationIdx(toStation);

            int leftStaIdx = findLeftStation(personDist);
            int rightStaIdx = findRightStation(personDist);

            // Left Station으로 걸어갔을 경우
            double totalLeftSec = 0;
            totalLeftSec += personDist - listStation[leftStaIdx].staDist; // 1 m/s
            List<BusInfo> listBusInfoUpdate = new List<BusInfo>();
            updateBusInfo(listBusInfoUpdate, totalLeftSec); // 걸어간 시간만큼 업데이트
            // 현 시점에서 toStation까지 가장 빨리가는 버스 찾기
            int busIdx = -1;
            double dSec = -1;
            bool bFind = comingBusByTime(listBusInfoUpdate, staIdx, out busIdx, out dSec);
            totalLeftSec += dSec;//총 걸린 시간

            // Right Station으로 걸어갔을 경우
            double totalRightSec = 0;
            totalRightSec += listStation[rightStaIdx].staDist - personDist; // 1 m/s
            listBusInfoUpdate.Clear();
            updateBusInfo(listBusInfoUpdate, totalRightSec); // 걸어간 시간만큼 업데이트
            // 현 시점에서 toStation까지 가장 빨리가는 버스 찾기
            busIdx = -1;
            dSec = -1;
            bFind = comingBusByTime(listBusInfoUpdate, staIdx, out busIdx, out dSec);
            totalRightSec += dSec;//총 걸린 시간

            double totalSec = Math.Min(totalLeftSec, totalRightSec);

            DateTime dt = DateTime.ParseExact(listBusInfo[0].time, "HH:mm:ss", null);
            dt = dt.AddSeconds(totalSec);
            return dt.ToString("HH:mm:ss");
        }

        static double getSecBetweenBus(double fromDist, double toDist, double busMPS)
        {
            double totalSec = 0;
            int rStaIdx = findRightStation(fromDist);
            int lStaIdx = findLeftStation(fromDist);
            totalSec += (listStation[rStaIdx].staDist - fromDist) / Math.Min(listStation[lStaIdx].limitMPS, busMPS);

            int lStaIdx2 = findLeftStation(toDist);
            totalSec += calcSecBetweenStation(rStaIdx, lStaIdx2, busMPS);

            totalSec += (toDist - listStation[lStaIdx2].staDist) / Math.Min(listStation[lStaIdx2].limitMPS, busMPS);

            return totalSec;
        }

        static String getPrePostTime(String busId)
        {
            int busIdx = getBusInfoIdx(busId);
            double minPrevSec = double.MaxValue;
            double minNextSec = double.MaxValue;
            int minPrevBusIdx = -1;
            int minNextBusIdx = -1;
            for (int i=0; i<listBusInfo.Count; i++)
            {
                if (busIdx == i)
                    continue;

                if (listBusInfo[i].distance < listBusInfo[busIdx].distance)
                {
                    double dSec = getSecBetweenBus(listBusInfo[i].distance, listBusInfo[busIdx].distance, listBusInfo[i].busMPS);
                    if (minPrevSec > dSec)
                    {
                        minPrevSec = dSec;
                        minPrevBusIdx = i;
                    }
                }
                else
                {
                    double dSec = getSecBetweenBus(listBusInfo[busIdx].distance, listBusInfo[i].distance, listBusInfo[busIdx].busMPS);
                    if (minNextSec > dSec)
                    {
                        minNextSec = dSec;
                        minNextBusIdx = i;
                    }
                }
            }

            String prev = "";
            String next = "";
            if (minPrevBusIdx == -1)
            {
                prev = string.Format("NOBUS#00:00:00");
            }
            else
            {
                DateTime dt = DateTime.ParseExact("00:00:00", "HH:mm:ss", null);
                dt = dt.AddSeconds(minPrevSec);
                prev = string.Format("{0}#{1}", listBusInfo[minPrevBusIdx].busId, dt.ToString("HH:mm:ss"));
            }

            if (minNextBusIdx == -1)
            {
                next = string.Format("NOBUS#00:00:00");
            }
            else
            {
                DateTime dt = DateTime.ParseExact("00:00:00", "HH:mm:ss", null);
                dt = dt.AddSeconds(minNextSec);
                next = string.Format("{0}#{1}", listBusInfo[minNextBusIdx].busId, dt.ToString("HH:mm:ss"));
            }

            return (next + "#" + prev);
        }

        static void doWork(Object obj)
        {
            Socket handler = (Socket)obj;
            byte[] buf = new byte[100];
            int len = handler.Receive(buf);
            String name = Encoding.Default.GetString(buf, 0, len).Replace("\n", "");
            Console.WriteLine(name);


            // listBusInfo에 일단 개수만큼 채워 넣는다
            if (name.StartsWith("BUS"))
            {
                BusInfo bi = new BusInfo(null, name, 0, 0);
                listBusInfo.Add(bi);
            }

            int busIdx = getBusInfoIdx(name);

            while (true)
            {
                len = handler.Receive(buf);
                if (len == 0)
                    break;
                String recv = Encoding.Default.GetString(buf, 0, len);
                recv = recv.Replace("\n", "");
                Console.WriteLine(name + " : " + recv);

                if (name.StartsWith("BUS"))
                {
                    if (recv == "PRINT")
                    {
                        String strPrePostTime = getPrePostTime(name);
                        byte[] sendBuf = Encoding.Default.GetBytes(strPrePostTime + "\n");
                        handler.Send(sendBuf);
                    }
                    else
                    {
                        String[] words = recv.Split('#');
                        String time = words[0];
                        listBusInfo[busIdx].time = time;
                        if (words.Length == 1) // 복구 필요
                        {
                            listBusInfo[busIdx].distance += Math.Min(listBusInfo[busIdx].busMPS, getStationLimitMPS(listBusInfo[busIdx].distance));
                        }
                        else
                        {
                            double dist = double.Parse(words[1]);
                            double MPS = dist - listBusInfo[busIdx].distance;

                            listBusInfo[busIdx].distance = dist;
                            listBusInfo[busIdx].busMPS = MPS;
                        }
                    }
                }
                else if (name.StartsWith("MOBILE"))
                {
                    if (recv == "PRINT")
                    {
                        // No.1
                        MakePrePost();

                        // No.2
                        MakeArrival();

                        // No.3
                        MakeSignage();
                    }
                    else
                    {
                        String[] words = recv.Split('#');
                        String toStation = words[0];
                        double personDist = double.Parse(words[1]);

                        String expectTime = calcExpectTime(toStation, personDist);
                        byte[] sendBuf = Encoding.Default.GetBytes(expectTime + "\n");
                        handler.Send(sendBuf);
                    }
                }
            }
        }

    }
}
