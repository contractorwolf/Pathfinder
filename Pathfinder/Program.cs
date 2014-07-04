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

        static string gpsLogLine = "";
        static int logInterval = 50;
        static float heading;

        static PathwayItem[] Pathway = new PathwayItem[2];
        static int currentPathItem = 0;
        static bool ledState = false;
        static int iterationIndex = 1;


        //additional objects
        static Servo servo = new Servo();
        static Compass c = new Compass();
        static AttackAngle attack_angle = new AttackAngle();
        static TurnAngle turn_angle = new TurnAngle();
        static USBLog log = new USBLog();
        static GPSFormatConverter gpsconverter = new GPSFormatConverter();

        //static MicroTimer timer = new MicroTimer();


        static double currentX;
        static double currentY;
        static double currentDistance;






        public static void Main()
        {
            //REMOVE ADDITIONAL GC DATA FROM OUTPUT
            Debug.EnableGCMessages(false);





            //pin setup
            OutputPort led = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.LED, ledState);


            //attach USB methods
            USBHostController.DeviceConnectedEvent += DeviceConnectedEvent;
            USBHostController.DeviceDisconnectedEvent += DeviceDisconnectedEvent;


            //initializeobjects
            FEZ_Shields.KeypadLCD.Initialize();
            FEZ_Extensions.GPS.Initialize();



            //FEZ_Shields.KeypadLCD.ShutBacklightOff();
            FEZ_Shields.KeypadLCD.TurnBacklightOn();
            FEZ_Shields.KeypadLCD.Clear();
            FEZ_Shields.KeypadLCD.CursorHome();

            Debug.Print("Pathfinder GPS started");
            FEZ_Shields.KeypadLCD.SetCursor(0, 0);
            FEZ_Shields.KeypadLCD.Print("Pathfinder v1.2");
            FEZ_Shields.KeypadLCD.SetCursor(1, 0);
            FEZ_Shields.KeypadLCD.Print("STARTED");

            Thread.Sleep(2000);

            Debug.Print("Waiting for valid GPS data...");
            FEZ_Shields.KeypadLCD.SetCursor(1, 0);
            FEZ_Shields.KeypadLCD.Print("Waiting for GPS");


            //*******************************************************
            //ALL GPS COORDINATES MUST BE CONVERTED
            //08053.4536 is actuall NOT DECIMAL FORMAT
            //MUST TAKE DIGITS AFTER 080[xx.xxxx] AND CONVERT BY DIVIDING BY 60 BECAUSE IT IS IN MINUTES WITH A DECIMAL FORMAT
            //08053.4536 = 080(53.4536/60) = -80.890893
            //**********************************************************


            Pathway[0] = new PathwayItem(-80.838949999999997, 35.054038333333331);//office
            Pathway[1] = new PathwayItem(-80.843513333333334, 35.230074999999999);//uptown





            //set initial destination
            attack_angle.setDestination(Pathway[currentPathItem].X, Pathway[currentPathItem].Y);



            Thread.Sleep(1000);

           // timer.StartTimer();
            //wait for initial gps position
            while (!FEZ_Extensions.GPS.GetPosition())
            {
                FEZ_Shields.KeypadLCD.SetCursor(0, 0);
                FEZ_Shields.KeypadLCD.Print("Waiting on GPS...");

                c.ReadBearings();
                heading = c.realHeading;

                Debug.Print("Waiting for valid GPS data, HEADING: " + heading.ToString());

                Thread.Sleep(1000);
            }

           // timer.StopTimer();
           // Debug.Print("startgps time: " + timer.GetMilliseconds().ToString()); 

            //get initial
            currentX = gpsconverter.Convert(FEZ_Extensions.GPS.GetLongitude());
            currentY = gpsconverter.Convert(FEZ_Extensions.GPS.GetLatitude());
            attack_angle.setOrigin(currentX, currentY);
            currentDistance = attack_angle.GetDistance();



            while (true)
            {
               // timer.StartTimer();


                if (FEZ_Extensions.GPS.GetPosition())
                {
                    FEZ_Shields.KeypadLCD.SetCursor(0, 10);
                    FEZ_Shields.KeypadLCD.Print("*");

                    //UPDATE
                    currentX =  gpsconverter.Convert(FEZ_Extensions.GPS.GetLongitude());
                    currentY = gpsconverter.Convert(FEZ_Extensions.GPS.GetLatitude());


                    attack_angle.setOrigin(currentX, currentY);

                    currentDistance = attack_angle.GetDistance();

                }
                else
                {
                    FEZ_Shields.KeypadLCD.SetCursor(0, 10);
                    FEZ_Shields.KeypadLCD.Print(" ");
                }
                 



                c.ReadBearings();
                heading = c.realHeading;


                turn_angle.Set(attack_angle.GetAngle(),heading);
                servo.SendAngle(turn_angle.CurrentServoAngle, 0);//0 is channel 0, attached to the steering on rc car





                gpsLogLine = currentX + "," 
                            + currentY + "," 
                            + Pathway[currentPathItem].X + "," 
                            + Pathway[currentPathItem].Y + "," 
                            + System.Math.Round(attack_angle.GetAngle()) +  "," 
                            + heading + "," 
                            + turn_angle.CurrentServoAngle + ","  
                            + currentDistance.ToString().Substring(0, 6);

                if (iterationIndex % 5 == 0)
                {
                    Debug.Print(FEZ_Extensions.GPS.GetTime() + " " + gpsLogLine);
                }



                if (iterationIndex % logInterval == 0)
                {
                    log.WriteLine(gpsLogLine, "Pathfinder" + FEZ_Extensions.GPS.GetDate() + ".txt", FEZ_Extensions.GPS.GetTime());
                }


                // LCD line 1
                FEZ_Shields.KeypadLCD.SetCursor(0, 0);
                FEZ_Shields.KeypadLCD.Print(attack_angle.GetAngle().ToString().Substring(0, 6) + " Deg");
                FEZ_Shields.KeypadLCD.SetCursor(0, 11);                    
                FEZ_Shields.KeypadLCD.Print(FEZ_Extensions.GPS.GetShortTime());

                // LCD line 2
                FEZ_Shields.KeypadLCD.SetCursor(1, 0);
                FEZ_Shields.KeypadLCD.Print(attack_angle.GetDistance().ToString().Substring(0, 6));
                FEZ_Shields.KeypadLCD.SetCursor(1, 6);
                FEZ_Shields.KeypadLCD.Print(" Mil ");

                //compass heading
                FEZ_Shields.KeypadLCD.SetCursor(1, 11);
                FEZ_Shields.KeypadLCD.Print(heading.ToString() + "    ");//.Substring(0, 6)



                //test for button presses
                if (FEZ_Shields.KeypadLCD.GetKey() == FEZ_Shields.KeypadLCD.Keys.Select)
                {
                    FEZ_Shields.KeypadLCD.TurnBacklightOn();
                }
                else if(FEZ_Shields.KeypadLCD.GetKey() == FEZ_Shields.KeypadLCD.Keys.Left)
                {
                    FEZ_Shields.KeypadLCD.ShutBacklightOff();
                }
                else if (FEZ_Shields.KeypadLCD.GetKey() == FEZ_Shields.KeypadLCD.Keys.Up)
                {
                    IncrementPathway();
                    attack_angle.setDestination(Pathway[currentPathItem].X, Pathway[currentPathItem].Y);
                    Debug.Print("DESTINATION CHANGE: " + Pathway[currentPathItem].X + "," + Pathway[currentPathItem].Y);
                    Thread.Sleep(200);
                }
                else if (FEZ_Shields.KeypadLCD.GetKey() == FEZ_Shields.KeypadLCD.Keys.Down)
                {
                    DecrementPathway();
                    attack_angle.setDestination(Pathway[currentPathItem].X, Pathway[currentPathItem].Y);
                    Debug.Print("DESTINATION CHANGE: " + Pathway[currentPathItem].X + "," + Pathway[currentPathItem].Y);
                    Thread.Sleep(200);
                }

                //REMOVE AFTER ALL ISSUES ARE SOLVED
                //allows for easier deploying of test code
                Thread.Sleep(20);

                //timer.StopTimer();
                //Debug.Print("loop time: " + timer.GetMilliseconds().ToString()); 
                iterationIndex++;


            }

        }




        //ADDITIONAL METHODS
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

        static private void IncrementPathway()
        {
            int nextItem = currentPathItem + 1;

            if (nextItem > Pathway.Length - 1)
            {
                currentPathItem = 0;
            }
            else
            {
                currentPathItem = nextItem;
            }
        }

        static private void DecrementPathway()
        {
            int prevItem = currentPathItem - 1;

            if (prevItem >= 0)
            {
                currentPathItem = prevItem;
            }
            else
            {
                currentPathItem = Pathway.Length - 1;
            }
        }

    }



}
