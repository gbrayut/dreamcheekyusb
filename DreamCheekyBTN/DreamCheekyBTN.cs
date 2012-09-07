using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace DreamCheekyUSB
{
    public class DreamCheekyBTN 
    {
        #region Constant and readonly values
        public HidLibrary.HidDevice hidBTN;
        public const int DefaultVendorID = 0x1D34;  //Default Vendor ID for Dream Cheeky devices
        public const int DefaultProductID = 0x0008; //Default for Ironman USB stress button
        //Initialization values and test colors
        public static readonly byte[] cmd_status = new byte[9] { 0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02 };
        #endregion

        private AutoResetEvent WriteEvent = new AutoResetEvent(false);
        private System.Timers.Timer t = new System.Timers.Timer(100); //Timer for checking USB status every 100ms
        private Action Timer_Callback;
        private bool lastWriteResult = false;

        #region Constructors
        /// <summary>
        /// Default constructor. Will used VendorID=0x1D34 and ProductID=0x0008. Will throw exception if no device is found.
        /// </summary>
        /// <param name="DeviceIndex">Zero based device index if you have multiple devices plugged in.</param>
        public DreamCheekyBTN(int DeviceIndex = 0) : this(DefaultVendorID, DefaultProductID, DeviceIndex) { }

        /// <summary>
        /// Create object using VendorID and ProductID. Will throw exception if no USBLED is found.
        /// </summary>
        /// <param name="VendorID">Example to 0x1D34</param>
        /// <param name="ProductID">Example to 0x0008</param>
        /// <param name="DeviceIndex">Zero based device index if you have multiple devices plugged in.</param>
        public DreamCheekyBTN(int VendorID, int ProductID, int DeviceIndex=0)
        {
            var devices = HidLibrary.HidDevices.Enumerate(VendorID, ProductID);
            if (DeviceIndex >= devices.Count())
            {
                throw new ArgumentOutOfRangeException("DeviceIndex",String.Format("DeviceIndex={0} is invalid. There are only {1} devices connected.", DeviceIndex,devices.Count()));
            }
            hidBTN = devices.Skip(DeviceIndex).FirstOrDefault<HidLibrary.HidDevice>();
            if (!init())
            {
                throw new Exception(String.Format("Cannot find USB HID Device with VendorID=0x{0:X4} and ProductID=0x{1:X4}", VendorID, ProductID));
            }
        }

        /// <summary>
        /// Create object using Device path. Example: DreamCheekyBTN(@"\\?\hid#vid_1d34&pid_0008#6&1067c3dc&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}").
        /// </summary>
        /// <param name="DevicePath">Example: @"\\?\hid#vid_1d34&pid_0008#6&1067c3dc&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}"</param>
        public DreamCheekyBTN(string DevicePath)
        {
            hidBTN = HidLibrary.HidDevices.GetDevice(DevicePath);
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
            t.AutoReset = true;
            t.Elapsed += new System.Timers.ElapsedEventHandler(t_Elapsed);
            t.Enabled = false;
            t.Stop();
            if (hidBTN == default(HidLibrary.HidDevice))
            {
                return false; //Device not found, return false.
            }
            else //Device is valid
            {
                Trace.WriteLine("Init HID device: " + hidBTN.Description + "\r\n");
                return true;
            }
        }

        ~DreamCheekyBTN()
        {
            t.Dispose();
            Timer_Callback = null;

            if (hidBTN != null)
            {
                hidBTN.Dispose();
            }
        }
        #endregion

        public bool ButtonState { get; private set; }

        public bool Write(byte[] data)
        {   
            //Trace.WriteLine("\r\nWriteing Data=" + BitConverter.ToString(data));
            hidBTN.Write(data, SignalWrite);
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

        public HidLibrary.HidDeviceData Read()
        {
            return hidBTN.Read();
        }

        public bool GetStatus() {
            if (Write(cmd_status))
            {
                var data = Read();
                if (data.Data[1] == 0x1C)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                Debug.WriteLine("Status CMD failed...");
                return false;
            }
        }

        public void RegisterCallback(Action Callback)
        {
            Timer_Callback = Callback;
            t.Enabled = true;
            t.Start();
        }

        void t_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (GetStatus())
            {
                if (!ButtonState) //Only toggle if button not already set
                {
                    ButtonState = true;
                    Timer_Callback();
                }
            }
            else
            {
                ButtonState = false; //Reset state
            }
        }
    }
}

