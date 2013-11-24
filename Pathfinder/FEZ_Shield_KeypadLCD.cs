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

using System.Threading;

using Microsoft.SPOT;

using Microsoft.SPOT.Hardware;

using GHIElectronics.NETMF.Hardware;





namespace GHIElectronics.NETMF.FEZ

{

    public static partial class FEZ_Shields

    {

        static public class KeypadLCD

        {

            public enum Keys

            {

                Up,

                Down,

                Right,

                Left,

                Select,

                None,

            }



            static OutputPort LCD_RS;

            static OutputPort LCD_E;



            static OutputPort LCD_D4;

            static OutputPort LCD_D5;

            static OutputPort LCD_D6;

            static OutputPort LCD_D7;



            static AnalogIn AnKey;



            static OutputPort BackLight;



            const byte DISP_ON = 0xC;    //Turn visible LCD on

            const byte CLR_DISP = 1;      //Clear display

            const byte CUR_HOME = 2;      //Move cursor home and clear screen memory

            const byte SET_CURSOR = 0x80;   //SET_CURSOR + X : Sets cursor position to X

            

            public static void Initialize()

            {

                LCD_RS = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.Di8, false);

                LCD_E = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.Di9, false);



                LCD_D4 = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.Di4, false);

                LCD_D5 = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.Di5, false);

                LCD_D6 = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.Di6, false);

                LCD_D7 = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.Di7, false);

                

                AnKey = new AnalogIn((byte)FEZ_Pin.AnalogIn.An0);



                BackLight = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.Di10, true);



                LCD_RS.Write(false);



                // 4 bit data communication

                Thread.Sleep(50);



                LCD_D7.Write(false);

                LCD_D6.Write(false);

                LCD_D5.Write(true);

                LCD_D4.Write(true);



                LCD_E.Write(true);

                LCD_E.Write(false);



                Thread.Sleep(50);

                LCD_D7.Write(false);

                LCD_D6.Write(false);

                LCD_D5.Write(true);

                LCD_D4.Write(true);



                LCD_E.Write(true);

                LCD_E.Write(false);



                Thread.Sleep(50);

                LCD_D7.Write(false);

                LCD_D6.Write(false);

                LCD_D5.Write(true);

                LCD_D4.Write(true);



                LCD_E.Write(true);

                LCD_E.Write(false);



                Thread.Sleep(50);

                LCD_D7.Write(false);

                LCD_D6.Write(false);

                LCD_D5.Write(true);

                LCD_D4.Write(false);



                LCD_E.Write(true);

                LCD_E.Write(false);



                SendCmd(DISP_ON);

                SendCmd(CLR_DISP);

            }



            //Sends an ASCII character to the LCD

            static void Putc(byte c)

            {

                LCD_D7.Write((c & 0x80) != 0);

                LCD_D6.Write((c & 0x40) != 0);

                LCD_D5.Write((c & 0x20) != 0);

                LCD_D4.Write((c & 0x10) != 0);

                LCD_E.Write(true); LCD_E.Write(false); //Toggle the Enable Pin



                LCD_D7.Write((c & 0x08) != 0);

                LCD_D6.Write((c & 0x04) != 0);

                LCD_D5.Write((c & 0x02) != 0);

                LCD_D4.Write((c & 0x01) != 0);

                LCD_E.Write(true); LCD_E.Write(false); //Toggle the Enable Pin

                //Thread.Sleep(1);

            }



            //Sends an LCD command

            static void SendCmd(byte c)

            {

                LCD_RS.Write(false); //set LCD to data mode



                LCD_D7.Write((c & 0x80) != 0);

                LCD_D6.Write((c & 0x40) != 0);

                LCD_D5.Write((c & 0x20) != 0);

                LCD_D4.Write((c & 0x10) != 0);

                LCD_E.Write(true); LCD_E.Write(false); //Toggle the Enable Pin



                LCD_D7.Write((c & 0x08) != 0);

                LCD_D6.Write((c & 0x04) != 0);

                LCD_D5.Write((c & 0x02) != 0);

                LCD_D4.Write((c & 0x01) != 0);

                LCD_E.Write(true); LCD_E.Write(false); //Toggle the Enable Pin

                Thread.Sleep(1);

                LCD_RS.Write(true); //set LCD to data mode

            }



            public static void Print(string str)

            {

                for (int i = 0; i < str.Length; i++)

                    Putc((byte)str[i]);

            }



            public static void Clear()

            {

                SendCmd(CLR_DISP);

            }



            public static void CursorHome()

            {

                SendCmd(CUR_HOME);

            }

            public static void SetCursor(byte row, byte col)

            {

                SendCmd((byte)(SET_CURSOR | row << 6 | col));

            }

            public static Keys GetKey()

            {

                int i = AnKey.Read();

                // use this to read values to calibrate
            
                /*while (true)

                {

                    i = AnKey.Read();

                    Debug.Print(i.ToString());

                    Thread.Sleep(300);

                }*/

                const int ERROR = 15;



                if (i > 1024-ERROR)

                    return Keys.None;



                if (i < 0+ERROR)

                    return Keys.Right;



                if (i < 129 + ERROR && i > 129 - ERROR)

                    return Keys.Up;



                if (i < 304 + ERROR && i > 304 - ERROR)

                    return Keys.Down;



                if (i < 477 + ERROR && i > 477 - ERROR)

                    return Keys.Left;



                if (i < 720 + ERROR && i > 720 - ERROR)

                    return Keys.Select;



                return Keys.None;

            }

            public static void TurnBacklightOn()

            {

                BackLight.Write(true);

            }

            public static void ShutBacklightOff()

            {

                BackLight.Write(false);

            }

        }

    }

}

