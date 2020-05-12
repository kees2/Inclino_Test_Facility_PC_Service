using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

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

        public void saveIMUOffsets(string path)
        {
            if (!File.Exists(path)){
                using (StreamWriter sw = File.CreateText(path)) { }
            }

            enums.Sensor_Id write_sensor_Id = (enums.Sensor_Id)SensorId;


            using (StreamWriter sw = File.AppendText(path))
            {
                sw.WriteLine(write_sensor_Id.ToString());
                for (int i = 0; i < amountIMUAtributes; i++)
                {
                    sw.WriteLine(data[i].offset);
                }    
            }
        }
        public void readIMUOffsets(string path)
        {
            using (StreamReader sr = File.OpenText(path))
            {
                string read_sensor_Id = ((enums.Sensor_Id)SensorId).ToString();
                string s = "";
                while ((sr.ReadLine()) != read_sensor_Id)
                {
                }
                for (int i = 0; i < amountIMUAtributes; i++)
                {
                    data[i].offset = Convert.ToDouble(sr.ReadLine());
                }
            }
        }

    }
}

