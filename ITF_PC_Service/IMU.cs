using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITF_PC_Service
{
    public class IMU
    {
        
        private const int amountIMUAtributes = 6;
        public IMUData[] data = new IMUData[amountIMUAtributes];
        public int SensorId{ get; set; }
        public enums.IC_type icType;
        private int enumCorrection = 1;

        public IMU(enums.IC_type type)
        {
            icType = type;
            for(int i = 0; i < amountIMUAtributes; i++)
            {
                data[i] = new IMUData();
            }
        }

        public void addIMUData(int Data_type, double newData)
        {
            //Add data to an IMU dataArray
            data[Data_type - enumCorrection].addData(newData);
        }
        
        public void resetIMUData()
        {
            //Reset everything after data has been send
            for (int i = 0; i < amountIMUAtributes; i++)
            {
                data[i].reset();
            }
        }

        public void calculateIMUOffset()
        {
            for(int i = 0; i < amountIMUAtributes; i++)
            {
                data[i].calculateOffset();
            }
        }

        public void saveIMUOffsets()
        {
            string[] lines = new string[amountIMUAtributes+1];//1 for the sensor id

            enums.Sensor_Id write_sensor_Id = (enums.Sensor_Id)SensorId;

            lines[0] = write_sensor_Id.ToString();

            for(int i = 0; i < amountIMUAtributes; i++)
            {
                lines[i + 1] = data[i].offset.ToString();
                System.IO.File.WriteAllLines(@"C:\Users\Kees\Documents\Kraken\Inclino_Test_Facility_PC_Service\Offsets\BMI_Offsets.txt", lines);
            }
        }

    }
}

