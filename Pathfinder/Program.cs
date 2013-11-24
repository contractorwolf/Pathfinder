using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using GHIElectronics.NETMF.FEZ;



using System.IO.Ports;
using System.Text;
using Microsoft.SPOT.IO;
using GHIElectronics.NETMF.IO;
using GHIElectronics.NETMF.USBHost;
using System.IO;

namespace Pathfinder
{
    public class Program
    {


        static void DeviceConnectedEvent(USBH_Device device)
        {
            if (device.TYPE == USBH_DeviceType.MassStorage)
            {

                Debug.Print("USB Mass Storage detected...");

                log.Connect(device);

            }
        }


        static void DeviceDisconnectedEvent(USBH_Device device)
        {
            log.Connect(device);
            Debug.Print("USB Mass Storage Hardware disconnected...");
        }

        //static IMU_9DOF imu = new IMU_9DOF("COM1");

        static AttackAngle attack_angle = new AttackAngle();
        //static TurnAngle turn_angle = new TurnAngle(0, 0);



        static USBLog log = new USBLog();
        static string gpsLogLine = "";
        static int logInterval = 1000;
        static float heading;


        public static void Main()
        {
            // Blink board LED
            Debug.EnableGCMessages(false);
            bool ledState = false;

            OutputPort led = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.LED, ledState);

            USBHostController.DeviceConnectedEvent += DeviceConnectedEvent;
            USBHostController.DeviceDisconnectedEvent += DeviceDisconnectedEvent;

            // Blink board LED
            //bool ledState = false;
            //OutputPort led = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.LED, ledState);

            FEZ_Shields.KeypadLCD.Initialize();
            //FEZ_Shields.KeypadLCD.ShutBacklightOff();
            FEZ_Shields.KeypadLCD.TurnBacklightOn();

            int iterationIndex = 1;



            FEZ_Extensions.GPS.Initialize();
            FEZ_Shields.KeypadLCD.Clear();
            FEZ_Shields.KeypadLCD.CursorHome();

            Debug.Print("Pathfinder GPS started");
            FEZ_Shields.KeypadLCD.SetCursor(0, 0);
            FEZ_Shields.KeypadLCD.Print("Pathfinder v1.1");
            FEZ_Shields.KeypadLCD.SetCursor(1, 0);
            FEZ_Shields.KeypadLCD.Print("STARTED");


            Thread.Sleep(2000);

            Debug.Print("Waiting for valid GPS data...");
            FEZ_Shields.KeypadLCD.SetCursor(1, 0);
            FEZ_Shields.KeypadLCD.Print("Waiting for GPS");



            //attack_angle.setDestination("8050.3370", "3503.2423");//office?
            attack_angle.setDestination("8050.6108", "3513.8045");//home

            Compass c = new Compass();

            Thread.Sleep(1000);




            while (!FEZ_Extensions.GPS.GetPosition())
            {

                FEZ_Shields.KeypadLCD.SetCursor(0, 0);
                FEZ_Shields.KeypadLCD.Print("Waiting on GPS...");


                Debug.Print("Waiting for valid GPS data, HEADING: " + heading.ToString());

                Thread.Sleep(1000);
            }





            while (true)
            {

                if (FEZ_Extensions.GPS.GetPosition())
                {
                    FEZ_Shields.KeypadLCD.SetCursor(0, 10);
                    FEZ_Shields.KeypadLCD.Print("*");
                }
                else
                {
                    FEZ_Shields.KeypadLCD.SetCursor(0, 10);
                    FEZ_Shields.KeypadLCD.Print(" ");
                }
                 
                // attack_angle.set("8050.7678","3520.8833","8048.4875","3513.2123");
                attack_angle.setOrigin(FEZ_Extensions.GPS.GetLongitude(), FEZ_Extensions.GPS.GetLatitude());


                c.ReadBearings();
                heading = c.realHeading;


                //turn_angle.Set(,);

                Debug.Print(FEZ_Extensions.GPS.GetTime() + " " + FEZ_Extensions.GPS.GetSentence());

                gpsLogLine = FEZ_Extensions.GPS.GetLongitude() + FEZ_Extensions.GPS.GetLongitudeDirection() + "," + FEZ_Extensions.GPS.GetLatitude() + FEZ_Extensions.GPS.GetLatitudeDirection() + "," + FEZ_Extensions.GPS.GetValid() + "," + System.Math.Round(attack_angle.GetAngle()).ToString() + "," + attack_angle.GetDistance().ToString().Substring(0,6) + "," + heading.ToString();

                if (iterationIndex % logInterval == 0)
                { 
                    log.WriteLine(gpsLogLine, "Pathfinder" + FEZ_Extensions.GPS.GetDate() + ".txt", FEZ_Extensions.GPS.GetTime());
                }


                // LCD line 1
                FEZ_Shields.KeypadLCD.SetCursor(0, 0);
                //FEZ_Shields.KeypadLCD.Print(FEZ_Extensions.GPS.GetValid() + FEZ_Extensions.GPS.GetLatitude() + FEZ_Extensions.GPS.GetLatitudeDirection() + FEZ_Extensions.GPS.GetShortTime());
                FEZ_Shields.KeypadLCD.Print(attack_angle.GetAngle().ToString().Substring(0, 6) + " Deg");




                    
                FEZ_Shields.KeypadLCD.SetCursor(0, 11);                    
                FEZ_Shields.KeypadLCD.Print(FEZ_Extensions.GPS.GetShortTime());





                // LCD line 2
                FEZ_Shields.KeypadLCD.SetCursor(1, 0);
                //FEZ_Shields.KeypadLCD.Print(FEZ_Extensions.GPS.GetLongitude() + FEZ_Extensions.GPS.GetLongitudeDirection() + " " + System.Math.Round(attack_angle.GetAngle()).ToString() + "    ");//iterationIndex.ToString()
                //FEZ_Shields.KeypadLCD.Print(FEZ_Extensions.GPS.GetLongitude() + FEZ_Extensions.GPS.GetLongitudeDirection() + " " + attack_angle.GetDistance() + "    ");//iterationIndex.ToString()                   


                FEZ_Shields.KeypadLCD.Print(attack_angle.GetDistance().ToString().Substring(0, 6));
                FEZ_Shields.KeypadLCD.SetCursor(1, 6);
                FEZ_Shields.KeypadLCD.Print(" Mil ");

                //compass heading
                FEZ_Shields.KeypadLCD.SetCursor(1, 11);
                FEZ_Shields.KeypadLCD.Print(heading.ToString() + "    ");//.Substring(0, 6)


                      // FEZ_Shields.KeypadLCD.SetCursor(1, 5);                   


                  //  FEZ_Shields.KeypadLCD.Print(imu.GetAngle().ToString());


                //}
                //else
                //{

                //    //FEZ_Shields.KeypadLCD.SetCursor(0, 0);
                //    //FEZ_Shields.KeypadLCD.Print("Waiting on GPS...");


                //    Debug.Print("Waiting for valid GPS data, HEADING: " + heading.ToString());
                //}

                if (FEZ_Shields.KeypadLCD.GetKey() == FEZ_Shields.KeypadLCD.Keys.Select)
                {
                    FEZ_Shields.KeypadLCD.TurnBacklightOn();
                }
                else if(FEZ_Shields.KeypadLCD.GetKey() == FEZ_Shields.KeypadLCD.Keys.Left)
                {

                    FEZ_Shields.KeypadLCD.ShutBacklightOff();
                }
          


                Thread.Sleep(10);
                // toggle LED state
                //ledState = !ledState;
                //led.Write(ledState);
                iterationIndex++;

            }

        }

    }
}
