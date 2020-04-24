using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;


namespace ITF_PC_Service
{
    public class DataProcessor
    {

        private const string path = @"C:\Users\Testfacility\Documents\Github\Inclino_Test_Facility_PC_Service\offsets\BMI_offset.txt";

        private const int sensorOffset = 1;

        private const int amountBMI055 = 8;
        private const int amountBMI085 = 1;
        private const int amountLMS6DSO = 1;

        const int amountIMU = amountBMI055 + amountBMI085 + amountLMS6DSO;

        //BMI055 Gyro
        private const int BMI055_Gyr_rang = 125;
        private double BMI055_Gyr_scale;

        //BMI055 Accelerometer
        private const int BMI055_Acc_rang = 2;//g
        private double BMI055_acc_Scale;

        //BMI085 Gyro
        private const int BMI085_Gyr_Angular_Rate = 125;

        //BMI085 Accelerometer
        private const int BMI085_Acc_rang = 0;//2g
        private double BMI085_acc_Scale;

        //LSM6DSM gyro
        private const int LSM6DSM_angular_rate = 125;
        private double LSM6DSM_Gyr_scale;

        //LSM6DSM Accelerometer
        private const int LSM6DSM_Acc_rang = 2;//g
        private double LSM6DSM_acc_Scale;


        private const int amountInclino = 8;

        public int AmountInclino
        {
            get { return amountInclino; }
        }
        public int AmountIMU
        {
            get { return amountIMU; }
        }

        private IMU[] imus;
        private Inclino[] inclinos;

        public Inclino[] Inclinos
        {
            get { return inclinos; }
            set { inclinos = Inclinos; }
        }

        public IMU[] IMUS
        {
            get { return imus; }
            set { imus = IMUS; }
        }

        static int offsetTimerInterval = 2000;
        static bool offsetTimerDone = false;
        static System.Timers.Timer OffsetTimer = new System.Timers.Timer();

        public DataProcessor()
        {
            //Initialize IMU values that are needed for the calculations
            initIMUs();

            //Initialize the amount of IMU that are needed 
            int j = 0;
            imus = new IMU[amountIMU];
            inclinos = new Inclino[amountInclino];
            for (int i = 0; i < amountBMI055; i++)
            {
                imus[i] = new IMU(enums.IC_type.BMI55);
                imus[i].SensorId = i + 1;
            }
            for (int i = 0; i < amountBMI085; i++)
            {
                imus[amountBMI055 + i] = new IMU(enums.IC_type.BMI085);
                imus[amountBMI055 + i].SensorId = (int)enums.Sensor_Id.BMI085;
            }
            for (int i = 0; i < amountLMS6DSO; i++)
            {
                imus[amountBMI055 + amountBMI085 + i] = new IMU(enums.IC_type.LMS6DSO);
                imus[amountBMI055 + amountBMI085 + i].SensorId = (int)enums.Sensor_Id.LMS6DSO;
            }
            for (int i = (int)enums.Sensor_Id.SCA103T_0; i < (int)enums.Sensor_Id.SCA103T_0 + amountInclino; i++)
            {
                inclinos[j] = new Inclino(enums.IC_type.SCA103T);
                inclinos[j].SensorId = i;
                j++;
            }
        }

        public void initIMUs()
        {
            //BMI055 Gyroscoop
            if (BMI055_Gyr_rang == 125)
            {
                BMI055_Gyr_scale = 3.8/1000;
            }
            else if (BMI055_Gyr_rang == 250)
            {
                BMI055_Gyr_scale = 7.6/1000;
            }
            else if (BMI055_Gyr_rang == 500)
            {
                BMI055_Gyr_scale = 15.3/1000;
            }
            else if (BMI055_Gyr_rang == 1000)
            {
                BMI055_Gyr_scale = 30.5/1000;
            }
            else if (BMI055_Gyr_rang == 2000)
            {
                BMI055_Gyr_scale = 61.0/1000;
            }
            else
            {
                throw new System.ArgumentException("BMI055_Gyro_rang value is not correct", "original");
            }

            //BMI055 Accelerometer
            if (BMI055_Acc_rang == 2)
            {
                BMI055_acc_Scale = 0.97;
            }
            else if (BMI055_Acc_rang == 4)
            {
                BMI055_acc_Scale = 1.95;
            }
            else if (BMI055_Acc_rang == 8)
            {
                BMI055_acc_Scale = 3.91;
            }
            else if (BMI055_Acc_rang == 16)
            {
                BMI055_acc_Scale = 7.81;
            }
            else
            {
                throw new System.ArgumentException("BMI055_Acc_rang value is not correct", "original");
            }
            

            //BMI085 Accelerometer
            if (BMI055_Acc_rang == 2)
            {
                BMI055_acc_Scale = 0.98;
            }
            else if (BMI055_Acc_rang == 4)
            {
                BMI055_acc_Scale = 1.95;
            }
            else if (BMI055_Acc_rang == 8)
            {
                BMI055_acc_Scale = 3.91;
            }
            else if (BMI055_Acc_rang == 16)
            {
                BMI055_acc_Scale = 7.81;
            }
            else
            {
                throw new System.ArgumentException("BMI055_Acc_rang value is not correct", "original");
            }



            //LSM6DSM Accelerometer
            if (LSM6DSM_Acc_rang == 2)
            {
                LSM6DSM_acc_Scale = 0.061;
            }
            else if (LSM6DSM_Acc_rang == 4)
            {
                LSM6DSM_acc_Scale = 0.122;
            }
            else if (LSM6DSM_Acc_rang == 8)
            {
                LSM6DSM_acc_Scale = 0.244;
            }
            else if (LSM6DSM_Acc_rang == 16)
            {
                LSM6DSM_acc_Scale = 0.488;
            }
            else
            {
                throw new System.ArgumentException("LSM6DSM_Acc_rang value is not correct", "original");
            }
        }

        public void addData(int Sensor_Id, int Data_type, int data)
        {
            int index = Sensor_Id;
            double returnValue = 0;
            if (//IMU data
                (enums.Data_type)Data_type >= enums.Data_type.GYRO_X &&
                (enums.Data_type)Data_type <= enums.Data_type.ACC_Z)
            {
                if ((enums.Sensor_Id)Sensor_Id == enums.Sensor_Id.LMS6DSO )
                {
                    returnValue = calculateLSM6DSM(data, Data_type);
                }
                else if ((enums.Sensor_Id)Sensor_Id == enums.Sensor_Id.BMI085)
                {
                    returnValue = calculateBMI085(data, Data_type);
                }
                else 
                {
                    returnValue = calculateBMI055(data, Data_type);
                }
                imus[determineIndexIMU(Sensor_Id)].addIMUData(Data_type, returnValue);
            }
            else if (//Inclino data
                (enums.Data_type)Data_type == enums.Data_type.INCL_A ||
                (enums.Data_type)Data_type == enums.Data_type.INCL_B ||
                (enums.Data_type)Data_type == enums.Data_type.VREF)
            {
                inclinos[determineIndexInclino(Sensor_Id)].addInclinoData(Data_type, data);
            }
            else
            {
                Console.WriteLine("Error Data_type with this Id does not exist");
            }
        }

        public void resetIMUs()
        {
            for (int i = 0; i < amountIMU; i++)
            {
                imus[i].resetIMUData();
            }
        }

        public void resetInclinos()
        {
            for (int i = 0; i < amountInclino; i++)
            {
                inclinos[i].resetInclinoData();
            }
        }

        //Convert a sensor id to an index for the inclinosensor
        public int determineIndexInclino(int Sensor_Id)
        {
            return Sensor_Id - amountBMI055 - sensorOffset;
        }

        //Convert a sensor id to an index for the IMUs
        public int determineIndexIMU(int Sensor_Id)
        {
            int index = 0;


            if ((enums.Sensor_Id)Sensor_Id == enums.Sensor_Id.BMI085)
            {
                index = amountBMI055;
            }
            else if ((enums.Sensor_Id)Sensor_Id == enums.Sensor_Id.LMS6DSO)
            {
                index = amountBMI055 + amountBMI085;
            }
            else
            {
                index = Sensor_Id - sensorOffset;
            }
            return index;
        }

        public double calculateTempDegrees(int temp, enums.IC_type ic_type)
        {
            // BMI055
            //The slope of the temperature sensor is 0.5K/LSB, it's center temperature is 23 degrees Celcius when temp = 0x00
            //Temp value is 8 bits 
            if (ic_type == enums.IC_type.BMI55)
            {
                if(temp > 127)
                {
                    temp -= 256;
                }
                return 23 + (0.5 * temp);

            }
            else if (ic_type == enums.IC_type.BMI085)
            {
                // Temperature Sensor slope
                // typ = 0.125
                // Units K/LSB
                if (temp > 1023)
                {
                    temp -= 2048;
                }
                return ((double)temp * 0.125) + 23;
            }
            else if (ic_type == enums.IC_type.LMS6DSO)
            {
                //16bit Resolution
                //Temperature sensitivity 256 LSB/°C
                //The output of the temperature sensor is 0 LSB (typ.) at 25 °C
                short LMS6DSOTemp_int16 = (short)temp;
                double LMS6DSOTemp_double = (double)LMS6DSOTemp_int16;
                return 25 + (LMS6DSOTemp_double / 256);
            }
            else if (ic_type == enums.IC_type.MS5611_01BA03)
            {
                double double_temp = (double)temp;
                if(double_temp < 0)
                {

                }
                return (double_temp / 100);
            }

                return 1;
        }

        public void CalculateOffset()
        {

            if (!File.Exists(path))
            {
                startoffsetTimer();

                while (!offsetTimerDone)
                {

                }

                int j = 0;
                for (int i = 0; i < amountBMI055; i++)
                {
                    imus[i].calculateIMUOffset();
                    imus[i].saveIMUOffsets(path);
                }
                for (int i = 0; i < amountBMI085; i++)
                {
                    imus[amountBMI055 + i].calculateIMUOffset();
                    imus[amountBMI055 + i].saveIMUOffsets(path);
                }
                for (int i = 0; i < amountLMS6DSO; i++)
                {
                    imus[amountBMI055 + amountBMI085 + i].calculateIMUOffset();
                    imus[amountBMI055 + amountBMI085 + i].saveIMUOffsets(path);
                }
                for (int i = (int)enums.Sensor_Id.SCA103T_0; i < (int)enums.Sensor_Id.SCA103T_0 + amountInclino; i++)
                {
                    inclinos[j].calculateInclinoOffset();
                    inclinos[j].saveInclinoOffsets(path);
                    j++;
                }
            }
            else
            {
                int j = 0;
                for (int i = 0; i < amountBMI055; i++)
                {
                    imus[i].readIMUOffsets(path);
                }
                for (int i = 0; i < amountBMI085; i++)
                {
                    imus[i].readIMUOffsets(path);
                }
                for (int i = 0; i < amountLMS6DSO; i++)
                {
                    imus[i].readIMUOffsets(path);
                }
                for (int i = (int)enums.Sensor_Id.SCA103T_0; i < (int)enums.Sensor_Id.SCA103T_0 + amountInclino; i++)
                {
                    inclinos[j].readInclinoOffsets(path);
                    j++;
                }
            }
        }

        private double calculateLSM6DSM(int data, int Data_type)
        {
            data = (Int16)data;
            double returnValue;
            if (
                (enums.Data_type)Data_type >= enums.Data_type.GYRO_X &&
                (enums.Data_type)Data_type <= enums.Data_type.GYRO_Z)
            {
                returnValue = data / 1000;
                //returnValue = (LSM6DSM_angular_rate / 32767) * data;
            }
            else
            {
                returnValue = (LSM6DSM_acc_Scale * data);
                //returnValue = (data / 32768) * 1000 * 2 ^ (LSM6DSM_Acc_rang + 1);
            }
            return returnValue;
        }
        private double calculateBMI085(int data, int Data_type)
        {
            double returnValue;
            if (
                (enums.Data_type)Data_type >= enums.Data_type.GYRO_X &&
                (enums.Data_type)Data_type <= enums.Data_type.GYRO_Z)
            {
                data = (Int16)data;
                double double_data = (double)data;
                returnValue = (double_data / 32767) * BMI085_Gyr_Angular_Rate;
            }
            else
            {
                data = (Int16)data;
                double double_data = (double)data;
                returnValue = (double_data / 32768) * 1000 * Math.Pow(2 , (BMI085_Acc_rang + 1));

            }
            return returnValue;
        }

        private double calculateBMI055(int data, int Data_type)
        {
            double returnValue;

            if (
                (enums.Data_type)Data_type >= enums.Data_type.GYRO_X &&
                (enums.Data_type)Data_type <= enums.Data_type.GYRO_Z)
            {
                data = (data >> 15) == 0 ? data : -1 ^ 0xFFFF | data;
                returnValue = (BMI055_Gyr_scale * data);
                //(dev->gyrX * dev->gyrScale) / 1000.0;
            }
            else
            {
                data = (data >> 11) == 0 ? data : -1 ^ 0xFFF | data;
                //1000 is translation to g
                returnValue = data * BMI055_acc_Scale;
            }

            return returnValue;
        }

        private static void startoffsetTimer()
        {
            OffsetTimer.Interval = offsetTimerInterval;
            OffsetTimer.Elapsed += offsettimer_Elapsed;
            OffsetTimer.Start();
        }

        private static void offsettimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            offsetTimerDone = true;
            OffsetTimer.Stop();
        }
    }
}
