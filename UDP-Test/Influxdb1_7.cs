using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfluxDB.LineProtocol.Client;
using InfluxDB.LineProtocol.Payload;

namespace UDP_Test
{
    class Influxdb1_7
    {
        
        const int amount_IMU_datatypes = 6;
        const int amount_Inclino_datatypes = 2;

        string databaseAddress = "http://127.0.0.1:8086";
        string databaseName = "Bridgescout";

        private LineProtocolClient client;
        //LineProtocolPayload payload;

        public void initDB()
        {
            client = new LineProtocolClient(new Uri(databaseAddress), databaseName);
            //payload = new LineProtocolPayload();
        }

        public void sendData()
        {
            //client.WriteAsync(payload);
            //payload = new LineProtocolPayload();
        }

        //Send a single datapackage to the database
        public int addData(int sensor_id, enums.Data_type data_type, double data, enums.IC_type ic_type)
        {
            LineProtocolPayload payload = new LineProtocolPayload();
            var fields = new Dictionary<string, object>();

            fields.Add("Raw", data);

            var tags = new Dictionary<string, string>
            {
                    {"id", ((enums.Sensor_Id)sensor_id).ToString()},
                    {"ic_type", ic_type.ToString()},
                    {"data_type", data_type.ToString()},
            };
            payload.Add(new LineProtocolPoint("sensor_measurement", new ReadOnlyDictionary<string, object>(fields), tags));
            client.WriteAsync(payload);
            return 0;
        }

        //Send all IMU data to the influxdb database
        public int addIMUs(IMU[] source, int amountIMUs)
        {
            LineProtocolPayload payload = new LineProtocolPayload();
            for (int i = 0; i < amountIMUs; i++)
            {
                for (int j = 0; j < amount_IMU_datatypes; j++)
                {
                    if(source[i].data[j].arraySize > 0)
                    {
                        LineProtocolPoint point = convertToPoint(source[i].SensorId, (enums.Data_type)(j+1), source[i].data[j], source[i].icType, "IMU_measurement");
                        payload.Add(point);
                    }
                    else
                    {
                        //Console.WriteLine("Error Sensor {0} doesn't have data of type {1}", (enums.Sensor_Id)source[i].SensorId, (enums.Data_type)(j+1));
                    }
                }
            }
            client.WriteAsync(payload);
            return 0;
        }


        public int addInclinos(Inclino[] source, int amountInclinos)
        {
            LineProtocolPayload payload = new LineProtocolPayload();
            LineProtocolPoint point;
            for (int i = 0; i < amountInclinos; i++)
            {
                if ((source[i].data[0].arraySize != 0) & (source[i].data[1].arraySize != 0)){//This should always be like this
                    point = convertToPoint(source[i].SensorId, enums.Data_type.INCL_A, source[i].data[0], source[i].icType, "Inclino_measurement");
                    payload.Add(point);
                    point = convertToPoint(source[i].SensorId, enums.Data_type.INCL_B, source[i].data[1], source[i].icType, "Inclino_measurement");
                    payload.Add(point);
                    source[i].calculateAllDifferential();
                    point = convertToPoint(source[i].SensorId, enums.Data_type.INCL, source[i].data[2], source[i].icType, "Inclino_measurement");
                    payload.Add(point);
                }
            }
            var result  = client.WriteAsync(payload);

            return 0;
        }

        //Convert packagedata to min, max, AVG, AVG_Error data which are puten in a influxdb LineProtocolPoint format
        public static LineProtocolPoint convertToPoint(int sensor_id, enums.Data_type data_type, Data data, enums.IC_type ic_type, string measurement)//naam aanpassen naar iets algemeneererdere iets niet alleen IMU
        {
            var fields = new Dictionary<string, object>();

            fields.Add("Min_value", data.determineMin());
            fields.Add("Max_value", data.determineMax());
            fields.Add("AVG_value", data.calcAverage());
            fields.Add("ERROR_AVG_value", data.calcAverageError());

            if (fields.Count == 0)
            {
                return null;
            }

            var tags = new Dictionary<string, string>
            {
                    {"id", ((enums.Sensor_Id)sensor_id).ToString()},
                    {"ic_type", ic_type.ToString()},
                    {"data_type", data_type.ToString()},
            };
            //Console.WriteLine("Sensor_id {0}, data_type {1}, Min Value {2}, Max value {3}, AVG_value {4}, ERROR_AVG_Value {5}", sensor_id, data_type,data.determineMin(),data.determineMax(),data.calcAverage(),data.calcAverageError());
            return new LineProtocolPoint(measurement, new ReadOnlyDictionary<string, object>(fields), tags);
        }



       




    }
}
