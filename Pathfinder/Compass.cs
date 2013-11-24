// HMC6343 Digital Tilt-Compensated Driver
// Written by Greg Oberfield
// Feel free to use, modify or distribute this code for non-commercial use
// This header must remain intact - if you make changes, please add your name
//
// 2010-08-30: Greg Oberfield: Initial Build
 
using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace Pathfinder
{
    public class Compass : IDisposable
    {
        const ushort HMC6343_ADDRESS = 0x32 >> 1;
        //const ushort HMC6343_ADDRESS = 0x33;

        const int CLOCK_FREQ = 400;
        const int DELAY = 100;
 
        //Write buffer
        byte[] headingCommand = new byte[] { 0x50 }; // Read bearing command
 
        //Read buffer
        private byte[] inBuffer = new byte[6]; // Six bytes, MSB followed by LSB for each heading, pitch and roll
 
        public float realHeading = 0.0f;
        public float realPitch = 0.0f;
        public float realRoll = 0.0f;
 
        //Create Read & Write transactions
        I2CDevice compass;
        I2CDevice.I2CTransaction[] bearingTrans;
        I2CDevice.I2CTransaction[] readTrans;
 
        /// <summary>
        /// Initializes a new instance of the <see cref="HMC6343"/> class.
        /// </summary>
        public Compass()
        {
            Thread.Sleep(500); // As per spec sheet, give the compass 500ms to warm up - doing it here guarantees it
                               // is done only once on object creation
            compass = new I2CDevice(new I2CDevice.Configuration((ushort)HMC6343_ADDRESS, CLOCK_FREQ));
            bearingTrans = new I2CDevice.I2CTransaction[] { I2CDevice.CreateWriteTransaction(headingCommand) };
            readTrans = new I2CDevice.I2CTransaction[] { I2CDevice.CreateReadTransaction(inBuffer) };
        }
 
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        public void Dispose()
        {
            compass.Dispose();
            bearingTrans = null;
            readTrans = null;
        }
 
        /// <summary>
        /// Reads heading, pitch and roll and converts to usable degrees.
        /// </summary>
        public void ReadBearings()
        {
            compass.Execute(bearingTrans, DELAY);
            Thread.Sleep(100);  // Give the compass the requested 1ms delay before reading
            compass.Execute(readTrans, DELAY);
 
            int heading = (int)(((ushort)inBuffer[0]) << 8 | (ushort)inBuffer[1]);
            int pitch = (int)(((ushort)inBuffer[2]) << 8 | (ushort)inBuffer[3]);
            int roll = (int)(((ushort)inBuffer[4]) << 8 | (ushort)inBuffer[5]);
            realHeading = heading / 10.0f;




            realPitch = (float)pitch / 10.0f;
            if (realPitch > 6400)
                realPitch = (realPitch - 6400 - 155);
            realRoll = (float)roll / 10.0f;
            if (realRoll > 6400)
                realRoll = (realRoll - 6400 - 155);
        }
 
    }
}

