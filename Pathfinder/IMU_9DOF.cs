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
    class IMU_9DOF
    {
        static SerialPort port;
        static byte[] rx_data = new byte[256];
        static byte[] tx_data = new byte[1];


        static double radians = 0;
        static double angle = 0;




        static int index = 0;
        static string sentence_buffer = "";
        static string last_reading = "";



        static string[] sentence_array;
        static string[] last_reading_split;
        static char[] char_array;

        static MicroTimer mt = new MicroTimer();

        static bool valid;

        string command = "4";//send full sensor sentence (2 is just mag data)


        static double x_total = 0;
        static double y_total = 0;



        static int valid_count = 0;

        static int sentence_index = 0;




        public IMU_9DOF(string port_name)
        {

            port = new SerialPort(port_name, 38400);

            port.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);


            tx_data = new System.Text.UTF8Encoding().GetBytes(command);
            port.Open();
            port.Write(tx_data, 0, tx_data.Length);

        }

        public string GetSentence(){
            return (last_reading);
        }


        public double GetAngle(){
            if (valid == true)
            {
                //ConvertXYToAngle(last_reading_split[8], last_reading_split[7]);
                return (angle);
            }
            else
            {
                return (0);
            }
        }


        public string GetMagX()
        {
            if (valid == true)
            {
                return (last_reading_split[8]);
            }
            else
            {
                return ("");
            }
        }

        public string GetMagY(){
            if (valid == true)
            {
                return (last_reading_split[7]);
            }
            else
            {
                return ("");
            }
        }

        public string GetMagZ(){
            if (valid == true)
            {
                return (last_reading_split[9]);
            }
            else
            {
                return ("");
            }
        }

        static private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
 


            try
            {

                // read data of BytesToRead length
                port.Read(rx_data, 0, port.BytesToRead);
                port.DiscardInBuffer();
                port.Flush();

                char_array = Encoding.UTF8.GetChars(rx_data);


                sentence_buffer = "";

                index = 0;
                while (index < char_array.Length)
                {
                    //add char(s) together to get the full 256 (amount on port.BytesToRead) serial blob as one sentence
                    sentence_buffer = sentence_buffer + char_array[index];

                    index++;
                }



                mt.StartTimer();

                ////START NEW AVG AREA
                //GetLastSentence();
                GetAvgSentence();

                ////END AVG AREA

                mt.StopTimer();

            }
            catch
            {
                mt.StopTimer();
                valid = false;
                Debug.Print("rx_data error, ignored");
            }



            


            Debug.Print(mt.GetMilliseconds().ToString());
        
            

        }




        private static void GetLastSentence()
        {




            //START NEW AVG AREA
            //split the sentence buffer into sentence chunks 
            sentence_array = sentence_buffer.Split('#');// looks like this: \n\n\r$,8,-14,241,373,371,373,22,119,-486,#

            //if its less than 2 sentences its prob not even a full senetence so ignore it
            if (sentence_array.Length > 2)
            {
                //sentence has at least 3 blocks of imu sentence, second to last is most likely full and more recent
                //this may need to be explored later, maybe average the full sentences to have a more stabilized reading
                //thererby less prone to small fluctuations from the 3 magnetometers(10/11/10)
                last_reading = sentence_array[sentence_array.Length - 2];

                //split the most recent full sentence into words
                last_reading_split = last_reading.Split(',');


                //if the sentence has 11 words it has all the necessary data for a reading
                if (last_reading_split.Length == 11)
                {

                    valid = true;


                    //do all extra data conversion here, because its being done under its own thread
                    ConvertXYToAngle(last_reading_split[8], last_reading_split[7]);



                    Debug.Print("valid sentence received: " + last_reading);
                    //Debug.Print("valid sentence received: " + sentence_buffer);
                    //Debug.Print(sentence_buffer);


                }
                else
                {
                    valid = false;
                    Debug.Print("last_reading_split.length too short");
                }

            }
            else
            {
                valid = false;
                Debug.Print("sentence_array.Length too short: " + sentence_array.Length.ToString());
            }


            //END AVG AREA


        }


        private static void GetAvgSentence()
        {
            //the variation is around 10 degrees which is about the same as using the last sentence
            // i am guesing this is because of the inherent error or fluctuation in the data coming
            // out of the 3 mags, i thing avg sentence is better than last, keep it in the code for now


            //
            //START NEW AVG AREA

            sentence_array = sentence_buffer.Split('#');// looks like this: \n\n\r$,8,-14,241,373,371,373,22,119,-486,#

            //Debug.Print(sentence_buffer);

            valid_count = 0;
            sentence_index = 0;
            x_total = 0;
            y_total = 0;


            //~3 millisecs, now totals ~4 millisecs
            while (sentence_index < sentence_array.Length)
            {
                last_reading = sentence_array[sentence_index];


                //split the most recent full sentence into words
                last_reading_split = last_reading.Split(','); //~3 millisecs

                //if the sentence has 11 words it has all the necessary data for a reading
                if (last_reading_split.Length == 11)
                {
                    //~8 millisecs, now totals ~12 millisecs

                    try
                    {
                        x_total = x_total + int.Parse(last_reading_split[7]);
                        y_total = y_total + int.Parse(last_reading_split[8]);

                        valid_count++;
                    }
                    catch
                    {

                    }

                }else{
                    //Debug.Print("last_reading_split.length too short");
                }



                sentence_index++;
            }

            if (valid_count > 0)
            {
                valid = true;
                ConvertXYToAngle(y_total / valid_count, x_total / valid_count);

            }

            //END AVG AREA


        }



        private static void ConvertXYToAngle(string Y, string X)
        {
            try
            {

                radians = MathEx.Atan2(double.Parse(Y), double.Parse(X));
                angle = radians * (180 / MathEx.PI);

                if (angle < 0)
                {
                    angle = angle + 360;
                }

            }
            catch
            {
                Debug.Print("conversion failed: (" + X + "," + Y + ")");


            }

        }

        private static void ConvertXYToAngle(double Y, double X)
        {
            try
            {

                radians = MathEx.Atan2(Y, X);
                angle = radians * (180 / MathEx.PI);

                if (angle < 0)
                {
                    angle = angle + 360;
                }

            }
            catch
            {
                Debug.Print("conversion failed: (" + X.ToString() + "," + Y.ToString() + ")");


            }

        }



    }
}
