using System;
using Microsoft.SPOT;

namespace Pathfinder
{
    class PathwayItem
    {
        public double X { get; set; }
        public double Y { get; set; }

        public PathwayItem(double x, double y){
            X = x;
            Y = y;
        }

    }
}
