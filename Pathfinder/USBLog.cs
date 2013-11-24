using System;
using Microsoft.SPOT;
using System.Threading;
using System.IO.Ports;
using System.Text;
using Microsoft.SPOT.IO;
using GHIElectronics.NETMF.FEZ;
using GHIElectronics.NETMF.IO;
using GHIElectronics.NETMF.USBHost;
using System.IO;


namespace Pathfinder
{
    class USBLog
    {

        private PersistentStorage ps;
        private bool isConnected = false;
        private FileStream outStream;
        private int lineCount = 0;
        private int byte_index;



        public void Connect(USBH_Device device)
        {
            try
            {
                ps = new PersistentStorage(device);
                ps.MountFileSystem();
                isConnected = true;
            }
            catch
            {



            }
            
        }


        public void Disconnect()
        {
            isConnected = false;

        }



        public bool WriteLine(string line)
        {
            if (isConnected)
            {
                try
                {
                    outStream = new FileStream(VolumeInfo.GetVolumes()[0].RootDirectory + @"\log.txt", FileMode.Append, FileAccess.Write);

                    Debug.Print("writing line: " + line);

                    byte[] b = new UTF8Encoding().GetBytes(lineCount.ToString() + " " + line + "\r\n");


                    byte_index = 0;
                    //while (byte_index < b.Length)
                    //{
                    //    outStream.WriteByte(b[byte_index]);
                    outStream.Write(b, 0, b.Length);
                    //    byte_index++;
                    //}

                    outStream.Flush();
                    outStream.Close();

                    lineCount++;

                    return (true);

                }
                catch
                {
                    Debug.Print("error writing line: " + line);
                    return (false);
                }

            }
            else
            {
                Debug.Print("not connected writing line: " + line);
                return (false);
            }
        }

        public bool WriteLine(string line, string logfile)
        {
            if (isConnected)
            {
                try
                {
                    outStream = new FileStream(VolumeInfo.GetVolumes()[0].RootDirectory + @"\" + logfile, FileMode.Append, FileAccess.Write);

                    Debug.Print("writing line: " + line + " [ " + lineCount.ToString());

                    byte[] b = new UTF8Encoding().GetBytes(lineCount.ToString() + " " + line + "\r\n");

                    byte_index = 0;
                    //while (byte_index < b.Length)
                    //{
                    //    outStream.WriteByte(b[byte_index]);
                    outStream.Write(b, 0, b.Length);
                    //    byte_index++;
                    //}

                    outStream.Flush();
                    outStream.Close();

                    lineCount++;

                    return (true);

                }
                catch
                {
                    isConnected = false;
                    Debug.Print("error writing line: " + line + " [ "  + lineCount.ToString());
                    lineCount++;
                    return (false);
                    
                }

            }
            else
            {
                isConnected = false;
                Debug.Print("not connected writing line: " + line + " [ "  + lineCount.ToString());
                lineCount++;                
                return (false);
            }
        }

        public bool WriteLine(string line, string logfile, string time)
        {
            if (isConnected)
            {
                try
                {
                    outStream = new FileStream(VolumeInfo.GetVolumes()[0].RootDirectory + @"\" + logfile, FileMode.Append, FileAccess.Write);

                    Debug.Print("writing line: " + line + " [ " + lineCount.ToString());

                    byte[] b = new UTF8Encoding().GetBytes(time + " " + line + "    " + lineCount.ToString() + "\r\n");

                    byte_index = 0;
                    //while (byte_index < b.Length)
                    //{
                    //    outStream.WriteByte(b[byte_index]);
                    outStream.Write(b, 0, b.Length);
                    //    byte_index++;
                    //}

                    outStream.Flush();
                    outStream.Close();

                    lineCount++;

                    return (true);

                }
                catch
                {
                    isConnected = false;
                    Debug.Print("error writing line: " + line + " [ "  + lineCount.ToString());
                    lineCount++;
                    return (false);
                    
                }

            }
            else
            {
                isConnected = false;
                Debug.Print("not connected writing line: " + line + " [ "  + lineCount.ToString());
                lineCount++;                
                return (false);
            }
        }

    }
}
