using System;
using Microsoft.SPOT;

namespace Pathfinder
{
    class MicroTimer
    {

        DateTime start;
        DateTime end;
        TimeSpan diff;



        public MicroTimer()
        {



        }

        public void StartTimer(){
            start = DateTime.Now;
        }

        public void StopTimer()
        {
            end = DateTime.Now;
        }


        public int GetMilliseconds()
        {
            diff = end - start;
            return(diff.Milliseconds);
        }

    }
}
