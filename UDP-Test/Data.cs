using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDP_Test
{
    public class Data
    {

        private const int bufferSize = 6000;//We only get 1000 messages, but we set it to 2000 to have a buffer
        public double[] dataArray { get; set; }
        public int arraySize { get; set; }
        public double[] DataArray { get; set; }
        public double offset { get; set; }

        public Data()
        {
            arraySize = 0;
            dataArray = new double[bufferSize];
        }

        public void reset()
        {
            arraySize = 0;
            Array.Clear(dataArray, 0, dataArray.Length);
        }

        public void addData(double data)
        {
            dataArray[arraySize] = (data - offset);
            arraySize++;
        }

        //als arraysize 0 is afvangen
        public double calcAverage()
        {
            double total = 0;
            for(int i = 0; i < arraySize; i++)
            {
                total += dataArray[i];
            }
            if(arraySize != 0)
            {
                return total / arraySize;
            }
            else
            {
                return 0;
            }
        }

        public double calcAverageError()
        {
            double total = 0;
            for (int i = 0; i < arraySize; i++)
            {
                total += Math.Abs(dataArray[i]);
            }
            if (arraySize != 0)
            {
                return total / arraySize;
            }
            else
            {
                return 0;
            }
        }

        public double determineMin()
        {
            if(arraySize != 0)
            {
                double minValue = double.MaxValue;
                for (int i = 0; i < arraySize; i++)
                {
                    if (dataArray[i] < minValue)
                    {
                        minValue = dataArray[i];
                    }
                }
                if(minValue > 600)
                {

                }
                return minValue;

            }
            else
            {
                return 0;//aanpassen
            }
          
        }

        public double determineMax()
        {
            double maxValue = double.MinValue;
            for (int i = 0; i < arraySize; i++)
            {
                if(dataArray[i] > maxValue)
                {
                    maxValue = dataArray[i];
                }
            }
            return maxValue;
        }

        public void calculateOffset()
        {
            offset = calcAverage();
        }
    }
}
