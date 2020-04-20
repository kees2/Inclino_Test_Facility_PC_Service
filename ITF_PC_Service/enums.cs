using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITF_PC_Service
{
    public class enums
    {

        public enum IC_type
        {
            BMI55 = 0,
            SCA103T = 1,
            BMI085 = 2,
            LMS6DSO = 3,
            MS5611_01BA03 = 4,
            undefined = 5
        };

        public enum Data_type
        {
            EMPTY = 0,
            GYRO_X = 1,
            GYRO_Y = 2,
            GYRO_Z = 3,
            ACC_X = 4,
            ACC_Y = 5,
            ACC_Z = 6,
            TEMP = 7,
            INCL_A = 8,
            INCL_B = 9,
            BARO = 10,
            VREF = 11,
            INCL = 12,
        };

        public enum Atribute_type
        {
            Raw = 0,
            Min_value = 1,
            Max_value = 2,
            AVG = 3,
            ERROR_AVG = 4
        };

        public enum Sensor_Id
        {
            EMPTY = 0,
            BMI055_0 = 1,
            BMI055_1 = 2,
            BMI055_2 = 3,
            BMI055_3 = 4,
            BMI055_4 = 5,
            BMI055_5 = 6,
            BMI055_6 = 7,
            BMI055_7 = 8,
            SCA103T_0 = 9,
            SCA103T_1 = 10,
            SCA103T_2 = 11,
            SCA103T_3 = 12,
            SCA103T_4 = 13,
            SCA103T_5 = 14,
            SCA103T_6 = 15,
            SCA103T_7 = 16,
            BMI085 = 17,
            LMS6DSO = 18,
            MS5611_01BA03 = 19
        };


    }
}
