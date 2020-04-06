using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace UDP_Test
{
    public class DataHandler
    {
        byte desiredTemperature = 40;
        int tempRead = 0;
        int testcounterAmountMessages = 0;
        int tempCounter = 0;
        bool Baroread = false;
        int offsetTimerInterval = 2000;
        bool offsetTimerDone = false;
        System.Timers.Timer OffsetTimer = new System.Timers.Timer();

        public struct databaseMessage
        {
            public int data;
            public byte Data_type;
            public byte Sensor_Id;
            public byte Attribute;
        };

        private const int numThreads = 1;

        private static DataProcessor dataProcessor = new DataProcessor();
        private Influxdb1_7 influx17 = new Influxdb1_7();
        private UDPSend udpSendserver = new UDPSend();

        public DataHandler()
        {
            initDataHandler();
        }

        public void initDataHandler()
        {
            influx17.initDB();
            AsyncReceive.ReceiveMessages();
            startoffsetTimer();
            while (!offsetTimerDone)
            {
                dataReceiver();
            }
            dataProcessor.CalculateOffset();
            Console.WriteLine("OffsetInitialisation finished");
            dataProcessor.resetIMUs();
            dataProcessor.resetInclinos();

            udpSendserver.initUDPSend();

            makeThreads();
            InitTimer();
        }

        //Initialize timer which interrupts every second
        private void InitTimer()
        {
            System.Timers.Timer timerS = new System.Timers.Timer();
            timerS.Interval = 1000;
            timerS.Elapsed += timer_ElapsedS;
            timerS.Start();
        }

        private void startoffsetTimer()
        {
            OffsetTimer.Interval = offsetTimerInterval;
            OffsetTimer.Elapsed += offsettimer_Elapsed;
            OffsetTimer.Start();
        }

        void offsettimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            offsetTimerDone = true;
            OffsetTimer.Stop();
        }

        //Send all buffered data to the influx database
        void timer_ElapsedS(object sender, System.Timers.ElapsedEventArgs e)
        {

            influx17.addIMUs(dataProcessor.IMUS, dataProcessor.AmountIMU);
            influx17.addInclinos(dataProcessor.Inclinos, dataProcessor.AmountInclino);          

            dataProcessor.resetIMUs();
            dataProcessor.resetInclinos();

            udpSendserver.sendTemp(desiredTemperature);

            Console.WriteLine("amount packages{0}", testcounterAmountMessages);
            Console.WriteLine("amount temperature{0}", tempCounter);
            tempCounter = 0;
            tempRead = 0;
            testcounterAmountMessages = 0;
            Baroread = false;
        }

        private void makeThreads()
        {
            for (int i = 0; i < numThreads; i++)
            {
                Thread newThread = new Thread(new ThreadStart(ThreadProcReceive));
                newThread.Name = String.Format("ThreadReceive{0}", i + 1);
                newThread.Start();
            }
        }
                              
        private  void ThreadProcReceive()
        {
            while (true)
            {
                dataReceiver();
            }
        }

        void dataReceiver()
        {
            while(AsyncReceive.MessageReadbuffer + 1 == AsyncReceive.dataMessageCounter)
            {
                Thread.Sleep(1);
            }
            AsyncReceive.dataMessage[] messages = AsyncReceive.messageBuffer[AsyncReceive.MessageReadbuffer];
           
            AsyncReceive.MessageReadbuffer++;
            if (AsyncReceive.MessageReadbuffer == 1000)
            {
                AsyncReceive.MessageReadbuffer = 0;
            }
            for (int i = 0; i < messages.Length; i++)
            {
                if (messages[i].Sensor_Id == 0 || messages[i].Data_type == 0)
                {
                    break;
                }
                if(messages[i].data == 0)
                {

                }
                else if ((enums.Data_type)messages[i].Data_type == enums.Data_type.TEMP)
                {
                    if ((tempRead & (1 << messages[i].Sensor_Id)) == 0)
                    {
                        tempCounter++;
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
                    //Console.WriteLine("Sensor data: Sensor_Id {0}, Data type {1}, data {2:X}", messages[i].Sensor_Id, messages[i].Data_type, messages[i].data);
                }
                else{
                    Console.WriteLine("Data Invalid: Sensor_Id {0}, Data type {1}, data {2}", messages[i].Sensor_Id, messages[i].Data_type, messages[i].data);
                }
            }
            testcounterAmountMessages++;
        }

        private enums.IC_type determineSensorType(int Sensor_id)
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

        private void resetTempRead()
        {
            tempRead = 0;
        }

    }
}


