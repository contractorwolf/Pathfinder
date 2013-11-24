//-------------------------------------------------------------------------------------

//              GHI Electronics, LLC

//               Copyright (c) 2010

//               All rights reserved

//-------------------------------------------------------------------------------------

/*

 * You can use this file if you agree to the following:

 *

 * 1. This header can't be changed under any condition.

 *    

 * 2. This is a free software and therefore is provided with NO warranty.

 * 

 * 3. Feel free to modify the code but we ask you to provide us with

 *	  any bugs reports so we can keep the code up to date.

 *

 * 4. This code may ONLY be used with GHI Electronics, LLC products.

 *

 * THIS SOFTWARE IS PROVIDED BY GHI ELECTRONICS, LLC ``AS IS'' AND 

 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED 

 * TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR 

 * A PARTICULAR PURPOSE ARE DISCLAIMED.  IN NO EVENT SHALL 

 * GHI ELECTRONICS, LLC BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,

 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT 

 * LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,

 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON 

 * ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR ORT 

 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE 

 * OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.  

 *

 *	Specs are subject to change without any notice

 */





using System;

using System.IO.Ports;

using System.Threading;

using Microsoft.SPOT;

using Microsoft.SPOT.Hardware;



namespace GHIElectronics.NETMF.FEZ

{

    public static partial class FEZ_Extensions

    {

        static public class GPS

        {

            static char[] split_char = new char[] { ',' };

            static string[] sentence_fields;

            static SerialPort _port;

            static byte[] buffer = new byte[200];

            static bool _data_is_valid;

            private static GPSStateMachine gps_decoder_state;

            static char[] pree_buffer = new char[10];

            static char[] pree = { 'G', 'P', 'R', 'M', 'C' };

            static int pree_index = 0;

            static char[] sentence = new char[200];

            static char[] GPRMC_sentence_array = new char[200];

            static int GPRMC_sentence_array_length = 0;

            static int sentence_index = 0;
            static string sentence_string = "";


            static string _date;
            static string _time;
            static string _longitude;
            static string _latitude;
            static string _valid;
            static string _longitudeDir;
            static string _latitudeDir;
            static string _speed;
            static string _direction;


            private enum GPSStateMachine

            {

                FindPre,

                CopyingSentence,

            }

            static public void Initialize()
            {

                _port = new SerialPort("COM2", 19200, Parity.None, 8, StopBits.One);

                _port.ReadTimeout = 0;

                _port.ErrorReceived += new SerialErrorReceivedEventHandler(_port_ErrorReceived);

                _port.Open();// If you open the port after you set the event you will endup with problems

                _port.DataReceived += new SerialDataReceivedEventHandler(_port_DataReceived);

            }

            static void _port_DataReceived(object sender, SerialDataReceivedEventArgs e)
            {
                int buffer_index = 0;
                int data_received = _port.Read(buffer, 0, buffer.Length);

                while (buffer_index < data_received)
                {
                    switch (gps_decoder_state)
                    {
                        case GPSStateMachine.FindPre:

                            if (buffer[buffer_index] == pree[pree_index])

                            {

                                pree_index++;

                                if (pree_index >= pree.Length)

                                {

                                    gps_decoder_state = GPSStateMachine.CopyingSentence;

                                    pree_index = 0;

                                }

                            }

                            else

                                pree_index = 0;

                            break;

                        case GPSStateMachine.CopyingSentence:

                            sentence[sentence_index] = (char)buffer[buffer_index];

                            if (sentence[sentence_index] == '*' ||

                                sentence[sentence_index] == '\r' ||

                                sentence[sentence_index] == '\n')

                            {

                                //we have a full line

                                if (sentence_index > 10)

                                {

                                    if (sentence[12] == 'A')

                                    {

                                        lock (GPRMC_sentence_array)

                                        {

                                            Array.Copy(sentence, GPRMC_sentence_array, sentence_index);

                                            GPRMC_sentence_array_length = sentence_index;

                                            _data_is_valid = true;

                                        }

                                    }

                                    else

                                    {

                                        //invalid data
                                        _data_is_valid = false;

                                        Debug.Print("Searching for satellites...");

                                    }

                                }

                                sentence_index = 0;

                                gps_decoder_state = GPSStateMachine.FindPre;

                            }

                            else

                            {

                                sentence_index++;

                                if (sentence_index >= sentence.Length)

                                {

                                    Debug.Print("Sentence is too long!");

                                    sentence_index = 0;

                                    gps_decoder_state = GPSStateMachine.FindPre;

                                }

                            }

                            break;

                    }

                    buffer_index++;

                }

                //_port.Open();

            }



            static void _port_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)

            {

                Debug.Print("COM Error: " + e.EventType.ToString());

            }



            public static string GetSentence()

            {

                return sentence_string;

            }


            public static string GetLongitude()
            {
                return _longitude;
            }
            public static string GetLongitudeDirection()
            {
                return _longitudeDir;
            }
            public static string GetLatitude()
            {
                return _latitude;
            }

            public static string GetLatitudeDirection()
            {
                return _latitudeDir;
            }

            public static string GetDate()
            {
                return _date;

            }

            public static string GetTime()
            {

                return (_time.Substring(0,2) + ":" + _time.Substring(2,2) + ":" + _time.Substring(4,2));

            }
            
            public static string GetShortTime()
            {

                return (_time.Substring(2,2) + ":" + _time.Substring(4,2));

            }        

            public static string GetValid()
            {

                return (_valid);

            }  
 
            public static bool IsValid()
            {
                if(_valid=="A"){
                    return (true);

                }else{
                    return (false);
                }


            }  


            private static void ParseSentence()
            {
                if (sentence_fields.Length == 13)
                {
                    _date = sentence_fields[9];
                    _time = sentence_fields[1];
                    _longitude = sentence_fields[5];
                    _latitude = sentence_fields[3];
                    _valid = sentence_fields[2];
                    _longitudeDir = sentence_fields[6];
                    _latitudeDir = sentence_fields[4];
                    _speed = sentence_fields[7];
                    _direction = sentence_fields[8];
                }
                else
                {



                }
            }

            public static bool GetPosition()
            {

                lock (GPRMC_sentence_array)

                {

                    if (_data_is_valid == false)
                    {
                        return false;

                    }

                    sentence_string = new string(GPRMC_sentence_array, 0, GPRMC_sentence_array_length);

                    sentence_fields = sentence_string.Split(split_char);

                    ParseSentence();

                    _data_is_valid = false;

                    return true;



                }

            }
        }

    }

}