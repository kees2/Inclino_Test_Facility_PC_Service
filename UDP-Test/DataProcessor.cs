using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;


namespace UDP_Test
{
    public class DataProcessor
    {
        private const int sensorOffset = 1;


        private const int amountBMI055 = 8;
        private const int amountBMI085 = 1;
        private const int amountLMS6DSO = 1;

        const int amountIMU = amountBMI055 + amountBMI085 + amountLMS6DSO;

        private const int BMI055_angular_rate = 2000;

        private const int BMI085_Acc_rang = 4;//g
        private const int BMI085_angular_rate = 2000;

        private const int LSM6DSM_Acc_rang = 2;//g
        private const int LSM6DSM_angular_rate = 250;


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

        public DataProcessor()
        {
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

        public void addData(int Sensor_Id, int Data_type, int data)
        {
            int index = Sensor_Id;
            double returnValue = 0;
            if (
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
            else if (
                (enums.Data_type)Data_type == enums.Data_type.INCL_A ||
                (enums.Data_type)Data_type == enums.Data_type.INCL_B)
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
                sbyte BMI055Temp = (sbyte)temp;
                return 23 + 0.5 * BMI055Temp;

            }
            else if (ic_type == enums.IC_type.BMI085)
            {
                // Temperature Sensor slope
                // typ = 0.125
                // Units K/LSB

                byte msb = (byte)(temp >> 8);
                byte lsb = (byte)(temp);

                int BMI085Temp_int11 = (msb * 8) + (lsb / 32);

                if (BMI085Temp_int11 > 1023)
                {
                    BMI085Temp_int11 -= 2048;
                }
                return (BMI085Temp_int11 * 0.125) + 23;
            }
            else if (ic_type == enums.IC_type.LMS6DSO)
            {
                //16bit Resolution
                //Temperature sensitivity 256 LSB/°C
                //The output of the temperature sensor is 0 LSB (typ.) at 25 °C
                short LMS6DSOTemp_int16 = (short)temp;
                return 25 + (LMS6DSOTemp_int16 / 256);
            }
            else if (ic_type == enums.IC_type.MS5611_01BA03)
            {
                return (temp / 100);
            }

                return 1;
        }

        public void CalculateOffset()
        {
            int j = 0;
            for (int i = 0; i < amountBMI055; i++)
            {
                imus[i].calculateIMUOffset();
            }
            for (int i = 0; i < amountBMI085; i++)
            {
                imus[amountBMI055 + i].calculateIMUOffset();
            }
            for (int i = 0; i < amountLMS6DSO; i++)
            {
                imus[amountBMI055 + amountBMI085 + i].calculateIMUOffset();
            }
            for (int i = (int)enums.Sensor_Id.SCA103T_0; i < (int)enums.Sensor_Id.SCA103T_0 + amountInclino; i++)
            {
                inclinos[j].calculateInclinoOffset();
                j++;
            }
        }

        private double calculateLSM6DSM(int data, int Data_type)
        {
            double returnValue;
            if (
                (enums.Data_type)Data_type >= enums.Data_type.GYRO_X &&
                (enums.Data_type)Data_type <= enums.Data_type.GYRO_Z)
            {
                data = (Int16)data;
                returnValue = (LSM6DSM_angular_rate / 32767) * data;
            }
            else
            {
                data = (Int16)data;
                returnValue = (data / 32768) * 1000 * 2 ^ (LSM6DSM_Acc_rang + 1);
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
                returnValue = (BMI085_angular_rate / 32767) * data;
            }
            else
            {
                data = (Int16)data;
                returnValue = (data / 32768) * 1000 * 2 ^ (BMI085_Acc_rang + 1);
            }
            return returnValue;
        }

        private double calculateBMI055(int data, int Data_type)
        {
            double returnValue;
            data = (data >> 11) == 0 ? data : -1 ^ 0xFFF | data;
            if (
                (enums.Data_type)Data_type >= enums.Data_type.GYRO_X &&
                (enums.Data_type)Data_type <= enums.Data_type.GYRO_Z)
            {
                returnValue = (BMI055_angular_rate / 32767) * data;
            }
            else
            {
                returnValue = data * 0.98;
            }
            return returnValue;
        }
    }
}
