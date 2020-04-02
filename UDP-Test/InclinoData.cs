using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDP_Test
{
    public class InclinoData : Data
    {

        public const double INCLINOMAXVOLTAGE = 3.3;
        public const int ADCBITS = 12;
        public double ADCRES = 2 ^ ADCBITS;


        double calculateInclinoValue(int inclinoDataA, int inclinoDataB)
        {
            double differential;
            differential = ConvertToVoltage(inclinoDataA) - ConvertToVoltage(inclinoDataB);
            return CalculateAngle(differential);
        }

        double ConvertToVoltage(int measuredValue)
        {
            return INCLINOMAXVOLTAGE / ADCRES * measuredValue;
        }

        public void AddCalculatedDifferential(double inclinoDataA, double inclinoDataB)
        {
                dataArray[arraySize] = inclinoDataA - inclinoDataB;
                arraySize++;
        }

        public double CalculateAngle(double voltage)
        {
            double offset = 0;
            double sensitivity = 280 / 15;
            double test = (voltage - offset) / sensitivity;
            return Math.Asin(test) * (180 / Math.PI);
        }

    }
}
