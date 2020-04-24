using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace ITF_PC_Service
{
    public static class DataParser
    {
        static int tempRead = 0;
        static int testcounterAmountMessages = 0;
        static int tempCounter = 0;
        static bool Baroread = false;
        static int desiredOffsetTemperature = 30;

        public struct databaseMessage
        {
            public int data;
            public byte Data_type;
            public byte Sensor_Id;
            public byte Attribute;
        };

        private const int numThreads = 1;

        private static DataProcessor dataProcessor = new DataProcessor();
        private static Influxdb1_7 influx17 = new Influxdb1_7();

        public static void initDataHandler()
        {
            influx17.initDB();
            AsyncReceive.ReceiveMessages();
            dataReceiver();
            temperatureCheck();
            dataProcessor.CalculateOffset();
            Console.WriteLine("OffsetInitialisation finished");
            dataProcessor.resetIMUs();
            dataProcessor.resetInclinos();

            makeThreads();
            InitTimer();
        }

        //Initialize timer which interrupts every second
        private static void InitTimer()
        {
            System.Timers.Timer timerS = new System.Timers.Timer();
            timerS.Interval = 1000;
            timerS.Elapsed += timer_ElapsedS;
            timerS.Start();
        }


        //Send all buffered data to the influx database
        private static void timer_ElapsedS(object sender, System.Timers.ElapsedEventArgs e)
        {
            influx17.addIMUs(dataProcessor.IMUS, dataProcessor.AmountIMU);
            influx17.addInclinos(dataProcessor.Inclinos, dataProcessor.AmountInclino);          

            dataProcessor.resetIMUs();
            dataProcessor.resetInclinos();

            tempRead = 0;
            Baroread = false;
        }

        private static void makeThreads()
        {
            for (int i = 0; i < numThreads; i++)
            {
                Thread newThread = new Thread(new ThreadStart(ThreadProcReceive));
                newThread.Name = String.Format("ThreadReceive{0}", i + 1);
                newThread.Start();
            }
        }
                              
        private static void ThreadProcReceive()
        {
            while (true)
            {
                dataReceiver();
            }
        }

        private static void temperatureCheck()
        {
            int ITFTemp = 0;
            while(desiredOffsetTemperature != ITFTemp) { 

                while (AsyncReceive.MessageReadbuffer == AsyncReceive.dataMessageCounter)
                {
                    Thread.Sleep(1);
                }
                AsyncReceive.dataMessage[] messages = AsyncReceive.messageBuffer[AsyncReceive.MessageReadbuffer];

                AsyncReceive.MessageReadbuffer++;
                if (AsyncReceive.MessageReadbuffer == 1000)
                {
                    AsyncReceive.MessageReadbuffer = 0;
                }
                if (messages != null)
                {
                    for (int i = 0; i < messages.Length; i++)
                    {
                        if ((enums.Data_type)messages[i].Data_type == enums.Data_type.TEMP)
                        {
                            enums.IC_type ic_type = determineSensorType(messages[i].Sensor_Id);
                            if (ic_type == enums.IC_type.MS5611_01BA03)
                            {
                                ITFTemp = (int)dataProcessor.calculateTempDegrees(messages[i].data, ic_type);
                            }
                        }
                    }
                }
            }
        }

        private static void dataReceiver()
        {
            while(AsyncReceive.MessageReadbuffer == AsyncReceive.dataMessageCounter)
            {
                Thread.Sleep(1);
            }
            AsyncReceive.dataMessage[] messages = AsyncReceive.messageBuffer[AsyncReceive.MessageReadbuffer];
           
            AsyncReceive.MessageReadbuffer++;
            if (AsyncReceive.MessageReadbuffer == 1000)
            {
                AsyncReceive.MessageReadbuffer = 0;
            }
            if(messages != null)
            {
                for (int i = 0; i < messages.Length; i++)
                {
                    if ((enums.Data_type)messages[i].Data_type == enums.Data_type.TEMP)
                    {
                        if ((tempRead & (1 << messages[i].Sensor_Id)) == 0)
                        {
                            enums.IC_type ic_type = determineSensorType(messages[i].Sensor_Id);
                            influx17.addData(messages[i].Sensor_Id, (enums.Data_type)messages[i].Data_type, dataProcessor.calculateTempDegrees(messages[i].data, ic_type), ic_type); 
                            tempRead |= (1 << (messages[i].Sensor_Id));
                        }
                    }
                    else if ((enums.Data_type)messages[i].Data_type == enums.Data_type.BARO)
                    {
                        if(!Baroread)
                        {
                            Baroread = true;
                            influx17.addData(messages[i].Sensor_Id, (enums.Data_type)messages[i].Data_type, (messages[i].data/100), determineSensorType(messages[i].Sensor_Id));
                        }
                    }
                    else if((messages[i].Sensor_Id > 0) & (messages[i].Sensor_Id < 20)){
                        dataProcessor.addData(messages[i].Sensor_Id, messages[i].Data_type, messages[i].data);
                    }
                    else{
                        Console.WriteLine("Data Invalid: Sensor_Id {0}, Data type {1}, data {2}", messages[i].Sensor_Id, messages[i].Data_type, messages[i].data);
                    }
                }
            }
        }

        private static enums.IC_type determineSensorType(int Sensor_id)
        {
            if(Sensor_id >=1 && Sensor_id <= 8)
            {
                return enums.IC_type.BMI55;
            }
            else if (Sensor_id >= 9 && Sensor_id <= 16)
            {
                return enums.IC_type.SCA103T;
            }
            else if((enums.Sensor_Id)Sensor_id == enums.Sensor_Id.BMI085)
            {
                return enums.IC_type.BMI085;
            }
            else if((enums.Sensor_Id)Sensor_id == enums.Sensor_Id.LMS6DSO)
            {
                return enums.IC_type.LMS6DSO;
            }
            else if((enums.Sensor_Id)Sensor_id == enums.Sensor_Id.MS5611_01BA03)
            {
                return enums.IC_type.MS5611_01BA03;
            }
            else
            {
                return 0;
            }
        }

        private static void resetTempRead()
        {
            tempRead = 0;
        }

    }
}


