using System;
using Microsoft.SPOT;
using System.Threading;
using System.IO.Ports;
using Microsoft.SPOT.Hardware;
using GHIElectronics.NETMF.FEZ;
using System.Text;
using GHIElectronics.NETMF.System;

namespace Pathfinder
{
    class AttackAngle
    {
        double originX;
        double originY;

        double destinationX;
        double destinationY;

        double changeX;
        double changeY;

        double radians;
        double angle;
        double a;
        double distance;

        // change in x might need to be modified by @ 22% to account for the warp in this region     
        // the amount (22%) needs to be tested, its just a rough guess based on the graphing
        // of the x and y dot positions in this long/lat from the gps
        //double xWarp = .22;
        double xWarp = 1;

        // public methods
        public void setOrigin(string origX, string origY)
        {
            try
            {
                originX = -(double.Parse(origX));//negate because 8050.6108 is actually -8050.6108
                originY = double.Parse(origY);


                changeX = destinationX - originX;
                //change in x might need to be modified by @ 22% to account for the warp in this region

                changeX = changeX * xWarp;

                changeY = destinationY - originY;

                angle = ArcTangent(changeX, changeY);
                distance = PythagoreanTheorem(changeX, changeY);

            }
            catch{}

        }

        public void setDestination(string destX, string destY)
        {
            try
            {
                destinationX = -(double.Parse(destX));//negate because 8050.6108 is actually -8050.6108
                destinationY = double.Parse(destY);

            }
            catch{}

        }

        public double GetAngle()
        {
            return (angle);
        }

        public double GetDistance()
        {
            return (distance);
        }


        //private methods
        private double ArcTangent(double x, double y){
            radians = MathEx.Atan2(x, y);
            a = radians * (180 / MathEx.PI);

            if (a < 0)
            {
                a = 360 + a;
            }

            return(a);
        }

        private double PythagoreanTheorem(double x, double y){
            return(MathEx.Pow(MathEx.Pow(x, 2) + MathEx.Pow(y, 2), .5));
        }

    }
}
