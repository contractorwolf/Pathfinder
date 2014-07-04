using System;
using Microsoft.SPOT;
using System.IO.Ports;
using System.Threading;

namespace Pathfinder
{
    public class Servo
    {
        private SerialPort _servo;
        private byte[] command = new byte[8] { (byte)'!', (byte)'S', (byte)'C', 0, 0, 0, 0, 0x0D };
        private int sleep_time = 2;
        private int ramping = 0;
        private string port = "COM1";

        public Servo()
        {
            _servo = new SerialPort(port, 2400, Parity.None, 8, StopBits.One);
            _servo.Open();
        }

        public void SendAngle(int angle, int channel)
        {
            if (angle > 180) { angle = 180; }
            if (angle < 0) { angle = 0; }

            // convert to 250 - 1100 (should be 1250)
            int position = 82 * angle / 18 + 250;

            //Debug.Print(angle.ToString());
            //Debug.Print(channel.ToString());

            SendRawPosition(position, channel);
        }

        private void SendRawPosition(int position, int channel)
        {
            command[3] = (byte)channel;
            command[4] = (byte)ramping;
            command[5] = (byte)position;
            command[6] = (byte)(position >> 8);

            _servo.Write(command, 0, command.Length);

            //Debug.Print(position.ToString());
            Thread.Sleep(sleep_time);
        }
    }
}
