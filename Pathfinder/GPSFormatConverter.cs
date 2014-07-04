using System;
using Microsoft.SPOT;

namespace Pathfinder
{
    public class GPSFormatConverter
    {

        public double DecimalGPS { get; set; }
        private int polarity = 1;
        private int degrees = 0;
        private double minutes = 0;
        private double degreesDecimal = 0;
        string[] gpsSplit = new string[2];



        public double Convert(string stringGPS)
        {//08053.4579 OR 3503.4430
            polarity = 1;
            degrees = 0;

            
            gpsSplit = stringGPS.Split('.');


            if (gpsSplit[0].Length == 5)
            {//denotes leading zero for negative, example: 08053.4579 
                polarity = -1;
                degrees = int.Parse(stringGPS.Substring(1, 2));//80
                minutes = double.Parse(stringGPS.Substring(3, 7));//53.4579 
                degreesDecimal = minutes / 60;
            }
            else
            {// 4 and 4 not negative, example: 3503.4430

                polarity = 1;
                degrees = int.Parse(stringGPS.Substring(0, 2));//35
                minutes = double.Parse(stringGPS.Substring(2, 7));//03.4430
                degreesDecimal = minutes / 60;
            }

            DecimalGPS = polarity * (degrees + degreesDecimal);


            return (DecimalGPS);

        }





    }
}
