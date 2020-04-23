﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITF_PC_Service
{
    public class Inclino
    {
        private const int amountInclinoAtributes = 4;//InclinoA & InclinoB & InclinoTotaal
        public InclinoData[] data = new InclinoData[amountInclinoAtributes];
        public int SensorId { get; set; }
        public enums.IC_type icType;
        

        public Inclino(enums.IC_type type)
        {
            icType = type;
            for (int i = 0; i < amountInclinoAtributes; i++)
            {
                data[i] = new InclinoData();
            }
        }

        public void addInclinoData(int Data_type, int newData)
        {
            if((enums.Data_type)Data_type == enums.Data_type.INCL_A)
            {
                data[0].addData(newData);
            }
            else if ((enums.Data_type)Data_type == enums.Data_type.INCL_B)
            {
                data[1].addData(newData);
            }
            else if ((enums.Data_type)Data_type == enums.Data_type.VREF)
            {
                data[3].addData(newData);
            }
        }

        public void resetInclinoData()
        {
            for (int i = 0; i < amountInclinoAtributes; i++)
            {
                data[i].reset();
            }
        }

        public void calculateAllDifferential()
        {
            int smallestArraySize = 0;
            if (data[0].arraySize < data[1].arraySize)
            {
                smallestArraySize = data[0].arraySize;
            }
            else
            {
                smallestArraySize = data[1].arraySize;
            }

            for (int i = 0; i < smallestArraySize; i++)
            {
                data[2].AddCalculatedDifferential(data[0].dataArray[i], data[1].dataArray[i]);
            }
        }

        public void calculateInclinoOffset()
        {
            data[0].calculateOffset();
            data[1].calculateOffset();
        }


        public void saveInclinoOffsets()
        {
            string[] lines = new string[amountInclinoAtributes + 1];//1 for the sensor id

            lines[0] = SensorId.ToString();

            for (int i = 0; i < amountInclinoAtributes; i++)
            {
                lines[i + 1] = data[i].offset.ToString();
                System.IO.File.WriteAllLines(@"C:\Users\Kees\Documents\Kraken\Inclino_Test_Facility_PC_Service\Offsets\Inclino_Offsets.txt", lines);
            }
        }
    }


}
