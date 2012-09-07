using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace DreamCheekyUSB
{
    public class DreamCheekyLED : DreamCheekyUSB.LEDBase
    {
        #region Constant and readonly values
        public const int DefaultVendorID = 0x1D34;
        public const int DefaultProductID = 0x0004;
        public const byte maxColorValue = 64; //Colors are capped at 60, so scale accordingly
        //Initialization values and test colors
        public static readonly byte[] init01 = new byte[9] { 0, 0x1F, 0x02, 0x00, 0x5F, 0x00, 0x00, 0x1F, 0x03 };
        public static readonly byte[] init02 = new byte[9] { 0, 0x00, 0x02, 0x00, 0x5F, 0x00, 0x00, 0x1F, 0x04 };
        public static readonly byte[] init03 = new byte[9] { 0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1F, 0x05 };
        public static readonly byte[] init04 = new byte[9] { 0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 };
        public static readonly byte[] cmd_Red = new byte[9] { 0, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1F, 0x05 };
        public static readonly byte[] cmd_Green = new byte[9] { 0, 0x00, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x1F, 0x05 };
        public static readonly byte[] cmd_Blue = new byte[9] { 0, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0x1F, 0x05 };
        public static readonly byte[] cmd_Off = new byte[9] { 0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1F, 0x05 }; 
        #endregion

        private AutoResetEvent WriteEvent = new AutoResetEvent(false);
        
        private bool lastWriteResult = false;

        #region Constructors
        /// <summary>
        /// Default constructor. Will used VendorID=0x1D34 and ProductID=0x0004. Will throw exception if no USBLED is found.
        /// </summary>
        /// <param name="DeviceIndex">Zero based device index if you have multiple devices plugged in.</param>
        public DreamCheekyLED(int DeviceIndex = 0) : this(DefaultVendorID, DefaultProductID, DeviceIndex) { }

        /// <summary>
        /// Create object using VendorID and ProductID. Will throw exception if no USBLED is found.
        /// </summary>
        /// <param name="VendorID">Example to 0x1D34</param>
        /// <param name="ProductID">Example to 0x0004</param>
        /// <param name="DeviceIndex">Zero based device index if you have multiple devices plugged in.</param>
        public DreamCheekyLED(int VendorID, int ProductID, int DeviceIndex=0)
        {
            var devices = HidLibrary.HidDevices.Enumerate(VendorID, ProductID);
            if (DeviceIndex >= devices.Count())
            {
                throw new ArgumentOutOfRangeException("DeviceIndex",String.Format("DeviceIndex={0} is invalid. There are only {1} devices connected.", DeviceIndex,devices.Count()));
            }
            hidLED = devices.Skip(DeviceIndex).FirstOrDefault<HidLibrary.HidDevice>();
            if (!init())
            {
                throw new Exception(String.Format("Cannot find USB HID Device with VendorID=0x{0:X4} and ProductID=0x{1:X4}", VendorID, ProductID));
            }
        }

        /// <summary>
        /// Create object using Device path. Example: DreamCheekyLED(@"\\?\hid#vid_1d34&pid_0004#6&1067c3dc&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}").
        /// </summary>
        /// <param name="DevicePath">Example: @"\\?\hid#vid_1d34&pid_0004#6&1067c3dc&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}"</param>
        public DreamCheekyLED(string DevicePath)
        {
            hidLED = HidLibrary.HidDevices.GetDevice(DevicePath);
            if (!init())
            {
                throw new Exception(String.Format("Cannot find USB HID Device with DevicePath={0}", DevicePath));
            }
        }

        /// <summary>
        /// Private init function for constructors.
        /// </summary>
        /// <returns>True if success, false otherwise.</returns>
        private bool init()
        {
            this.WriteEvent.Reset();
            if (hidLED == default(HidLibrary.HidDevice))
            {
                return false; //Device not found, return false.
            }
            else //Device is valid
            {
                Trace.WriteLine("Init HID device: " + hidLED.Description + "\r\n");
                return true;
            }
        } 
        #endregion

        /// <summary>
        /// Sends initialization commands to USB LED device so it is ready to change colors. LED will be turned off after calling Initialize.
        /// NOTE: Write command will automatically call Initialize.
        /// </summary>
        /// <returns>True if successfull, false otherwise</returns>
        internal override bool Initialize()
        {
            bool bReturn = base.Initialize();
            if (bReturn)
            {
                //Note: Write will automatically open the device if needed.
                bReturn &= Write(init01);
                bReturn &= Write(init02);
                bReturn &= Write(init03);
                bReturn &= Write(init04);
            }
            return bReturn;
        }

        public override bool Write(byte[] data)
        {
            if (!initialized) { Initialize(); }
            //Trace.WriteLine("\r\nWriteing Data=" + BitConverter.ToString(data));
            hidLED.Write(data, SignalWrite);
            this.WriteEvent.WaitOne();
            return lastWriteResult;
        }

        /// <summary>
        /// Used by Write method to set WriteEvent when finished
        /// </summary>
        /// <param name="result"></param>
        private void SignalWrite(bool result)
        {
            //Trace.WriteLine("Writeing Data result=" + result);
            lastWriteResult = result;
            this.WriteEvent.Set();
        }


        public override bool SetColor(System.Drawing.Color Color)
        {
            //Trace.WriteLine("\r\nSetColor: " + Color.Name);
            return SetColor(Color.R, Color.G, Color.B);
        }

        /// <summary>
        /// Set color using RGB. Values should range 0-255 and will be scaled to match USBLED output.
        /// </summary>
        /// <param name="Red">0-255</param>
        /// <param name="Green">0-255</param>
        /// <param name="Blue">0-255</param>
        /// <returns>True if sucessfull</returns>
        public override bool SetColor(byte Red, byte Green, byte Blue)
        {
            byte[] data = cmd_Off.Clone() as byte[];
            var t = (byte)(((float)Red / 255F)*60F);
            //Scale color values
            data[1] = ((byte)(((float)Red / 255F) * maxColorValue));
            data[2] = ((byte)(((float)Green / 255F) * maxColorValue)); ;
            data[3] = ((byte)(((float)Blue / 255F) * maxColorValue)); ;
            return Write(data);
        }

        
    }
}

