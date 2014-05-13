using System;
using System.Diagnostics;
using System.Threading;
using HidSharp;
using System.Collections.Generic;

namespace DreamCheekyUSB {
	public class DreamCheekyLED : LEDBase {
		#region Constant and readonly values

		public const int DefaultVendorID = 0x1D34;
		public const int DefaultProductID = 0x0004;
		//Colors are capped at 60, so scale accordingly
		//Initialization values and test colors
		public const byte MaxColorValue = 60;
		public static readonly byte[] Init01 = { 0x1F, 0x02, 0x00, 0x5F, 0x00, 0x00, 0x1F, 0x03 };
		public static readonly byte[] Init02 = { 0x00, 0x02, 0x00, 0x5F, 0x00, 0x00, 0x1F, 0x04 };
		public static readonly byte[] Init03 = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1F, 0x05 };
		public static readonly byte[] Init04 = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 };
		public static readonly byte[] CmdRed = { 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1F, 0x05 };
		public static readonly byte[] CmdGreen = { 0x00, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x1F, 0x05 };
		public static readonly byte[] CmdBlue = { 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0x1F, 0x05 };
		public static readonly byte[] CmdOff = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1F, 0x05 };

		#endregion

		private AutoResetEvent WriteEvent = new AutoResetEvent(false);
		private bool IsUnix;

		#region Constructors

		/// <summary>
		/// Default constructor. Will used VendorID=0x1D34 and ProductID=0x0004. Will throw exception if no USBLED is found.
		/// </summary>
		/// <param name="deviceIndex">Zero based device index if you have multiple devices plugged in.</param>
		public DreamCheekyLED(int deviceIndex = 0) : this(DefaultVendorID, DefaultProductID, deviceIndex) {
		}

		/// <summary>
		/// Create object using VendorID and ProductID. Will throw exception if no USBLED is found.
		/// </summary>
		/// <param name="vendorID">Example to 0x1D34</param>
		/// <param name="productID">Example to 0x0004</param>
		/// <param name="deviceIndex">Zero based device index if you have multiple devices plugged in.</param>
		public DreamCheekyLED(int vendorID, int productID, int deviceIndex = 0) {
			Trace.WriteLine("Instantiating HidDeviceLoader...");
			Loader = new HidDeviceLoader();

			Trace.WriteLine(String.Format("Enumerating HID USB Devices (VendorID {0}, ProductID {1})...", vendorID, productID));
			var devices = new List<HidDevice>(Loader.GetDevices(vendorID, productID));
			if (deviceIndex >= devices.Count) {

				throw new ArgumentOutOfRangeException("deviceIndex", String.Format("deviceIndex={0} is invalid. There are only {1} devices connected.", deviceIndex, devices.Count));
			}

			HidLED = devices[deviceIndex];
			if (!init()) {
				throw new Exception(String.Format("Cannot find USB HID Device with VendorID=0x{0:X4} and ProductID=0x{1:X4}", vendorID, productID));
			}
		}

		/// <summary>
		/// Create object using Device path. Example: DreamCheekyLED(@"\\?\hid#vid_1d34&pid_0004#6&1067c3dc&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}").
		/// </summary>
		/// <param name="devicePath">Example: @"\\?\hid#vid_1d34&pid_0004#6&1067c3dc&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}"</param>
		public DreamCheekyLED(string devicePath) {
			Trace.WriteLine("Instantiating HidDeviceLoader...");
			Loader = new HidDeviceLoader();

			Trace.WriteLine("Enumerating all USB Devices...");
			var devices = Loader.GetDevices();

			Trace.WriteLine("Searching for device at {0}...", devicePath);
			foreach (var device in devices) {
				if (device.DevicePath == devicePath) {
					HidLED = device;
				}
			}

			if (!init()) {
				throw new Exception(String.Format("Cannot find USB HID Device with devicePath={0}", devicePath));
			}
		}

		/// <summary>
		/// Private init function for constructors.
		/// </summary>
		/// <returns>True if success, false otherwise.</returns>
		private bool init() {
			WriteEvent.Reset();
			if (HidLED == default(HidDevice)) {
				return false; //Device not found, return false.
			}

			//Device is valid
			Trace.WriteLine("Init HID device: " + HidLED.ProductName + "\r\n");
			return true;
		}

		#endregion

		/// <summary>
		/// Sends initialization commands to USB LED device so it is ready to change colors. LED will be turned off after calling Initialize.
		/// NOTE: Write command will automatically call Initialize.
		/// </summary>
		/// <returns>True if successfull, false otherwise</returns>
		internal override bool Initialize() {
			DeterminePlatform();

			bool bReturn = base.Initialize();
			if (bReturn) {
				//Note: Write will automatically open the device if needed.
				bReturn &= Write(Init01);
				bReturn &= Write(Init02);
				bReturn &= Write(Init03);
				bReturn &= Write(Init04);
			}
			return bReturn;
		}

		private void DeterminePlatform() {
			var platform = Environment.OSVersion.Platform;
			IsUnix = ((platform == PlatformID.MacOSX) || (platform == PlatformID.Unix));
			Trace.WriteLineIf(IsUnix, "Running on UNIX-like OS...");
			Trace.WriteLineIf(!IsUnix, "Running on Windows...");
		}

		public override bool Write(byte[] data) {
			if (!initialized) {
				Initialize();
			}

			Trace.WriteLine("\r\nWriting Data: {0} ", BitConverter.ToString(data));
			if (IsUnix) {
				Stream.Write(data);
			} else {
				var windowsData = new byte[9];
				data.CopyTo(windowsData, 1);
				Stream.Write(windowsData);
			}
			return true;
		}

		public override bool SetColor(System.Drawing.Color color) {
			Trace.WriteLine("\r\nSetColor: {0}", color.Name);
			return SetColor(color.R, color.G, color.B);
		}

		/// <summary>
		/// Set color using RGB. Values should range 0-255 and will be scaled to match USBLED output.
		/// </summary>
		/// <param name="red">0-255</param>
		/// <param name="green">0-255</param>
		/// <param name="blue">0-255</param>
		/// <returns>True if sucessfull</returns>
		public override bool SetColor(byte red, byte green, byte blue) {
			Trace.WriteLine(String.Format("\r\nSetColor (RGB): ({0},{1},{2})", red, green, blue));
			byte[] data = CmdOff.Clone() as byte[];
			// var t = (byte)(((float)Red / 255F)*60F);
			//Scale color values
			data[0] = ((byte)(((float)red / 255F) * MaxColorValue));
			data[1] = ((byte)(((float)green / 255F) * MaxColorValue));
			data[2] = ((byte)(((float)blue / 255F) * MaxColorValue));
			return Write(data);
		}
	}
}

