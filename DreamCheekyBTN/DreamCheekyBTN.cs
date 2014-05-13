using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using HidSharp;

namespace DreamCheekyUSB {
	public class DreamCheekyBTN : IDisposable {
		#region Constant and readonly values

		protected HidDeviceLoader Loader;
		protected HidStream Stream;
		public HidDevice HidBTN;
		public const int DEFAULT_VENDOR_ID = 0x1D34;
		//Default Vendor ID for Dream Cheeky devices
		public const int DEFAULT_PRODUCT_ID = 0x0008;
		//Default for Ironman USB stress button
		public static class Messages {
			public const byte BUTTON_PRESSED = 0x1C;
		}
		//Initialization values and test colors
		public static readonly byte[] CmdStatus = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02 };

		#endregion

		private AutoResetEvent WriteEvent = new AutoResetEvent(false);
		private System.Timers.Timer t = new System.Timers.Timer(100);
		//Timer for checking USB status every 100ms
		private Action Timer_Callback;
		private bool LidOpen;
		protected byte ActivatedMessage;

		#region Constructors

		/// <summary>
		/// Default constructor. Will used VendorID=0x1D34 and ProductID=0x0008. Will throw exception if no device is found.
		/// </summary>
		/// <param name="deviceIndex">Zero based device index if you have multiple devices plugged in.</param>
		public DreamCheekyBTN(int deviceIndex = 0) : this(DEFAULT_VENDOR_ID, DEFAULT_PRODUCT_ID, deviceIndex) {
		}

		/// <summary>
		/// Create object using VendorID and ProductID. Will throw exception if no USBLED is found.
		/// </summary>
		/// <param name="vendorID">Example to 0x1D34</param>
		/// <param name="productID">Example to 0x0008</param>
		/// <param name="deviceIndex">Zero based device index if you have multiple devices plugged in.</param>
		public DreamCheekyBTN(int vendorID, int productID, int deviceIndex = 0) {
			var loader = new HidDeviceLoader();
			var devices = new List<HidDevice>(loader.GetDevices(vendorID, productID));
			if (deviceIndex >= devices.Count) {
				throw new ArgumentOutOfRangeException("deviceIndex", String.Format("VID={0},PID={1},DeviceIndex={2} is invalid. There are only {3} devices connected.", vendorID, productID, deviceIndex, devices.Count));
			}
			HidBTN = devices[deviceIndex];
			if (!init()) {
				throw new Exception(String.Format("Cannot find USB HID Device with VendorID=0x{0:X4} and ProductID=0x{1:X4}", vendorID, productID));
			}
			ActivatedMessage = Messages.BUTTON_PRESSED;
		}

		/// <summary>
		/// Create object using Device path. Example: DreamCheekyBTN(@"\\?\hid#vid_1d34&pid_0008#6&1067c3dc&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}").
		/// </summary>
		/// <param name="devicePath">Example: @"\\?\hid#vid_1d34&pid_0008#6&1067c3dc&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}"</param>
		public DreamCheekyBTN(string devicePath) {
			var loader = new HidDeviceLoader();
			var devices = loader.GetDevices();
			foreach (var device in devices) {
				if (device.DevicePath == devicePath) {
					HidBTN = device;
				}
			}
			if (!init()) {
				throw new Exception(String.Format("Cannot find USB HID Device with DevicePath={0}", devicePath));
			}
			ActivatedMessage = Messages.BUTTON_PRESSED;
		}

		/// <summary>
		/// Private init function for constructors.
		/// </summary>
		/// <returns>True if success, false otherwise.</returns>
		private bool init() {
			WriteEvent.Reset();
			t.AutoReset = true;
			t.Elapsed += t_Elapsed;
			t.Enabled = false;
			t.Stop();
			if (HidBTN == default(HidDevice)) {
				return false; //Device not found, return false.
			}

			Stream = HidBTN.Open();
			//Device is valid
			Trace.WriteLine("Init HID device: " + HidBTN.ProductName + "\r\n");
			return true;
		}

		public void Dispose() {
			if (t != null) {
				t.Dispose();
				t = null;
			}
			Timer_Callback = null;

			if (Stream != default(HidStream)) {
				Stream.Close();
				Stream.Dispose();
				Stream = default(HidStream);
			}
		}

		~DreamCheekyBTN() {
			Dispose();
		}

		#endregion

		public bool ButtonState { get; private set; }

		private static bool IsUnix() {
			var platform = Environment.OSVersion.Platform;
			return (platform == PlatformID.MacOSX) || (platform == PlatformID.Unix);
		}

		public void Write(byte[] data) {
			//Trace.WriteLine("\r\nWriteing Data=" + BitConverter.ToString(data));
			if (IsUnix()) {
				Stream.Write(data);
			} else {
				var windowsData = new byte[9];
				data.CopyTo(windowsData, 1);
				Stream.Write(windowsData);
			}
		}

		public byte[] Read() {
			return Stream.Read();
		}

		public bool GetStatus() {
			Write(CmdStatus);
			var data = Read();

			var bigRedBtn = this as DreamCheekyBigRedBTN;
			if (bigRedBtn != null) {
				if (LidOpen && bigRedBtn.LidIsClosed()) {
					LidOpen = false;
					Console.WriteLine("Lid closed...");
				}
				if (!LidOpen && bigRedBtn.LidIsOpen()) {
					LidOpen = true;
					Console.WriteLine("Lid opened...");
				}
			}

			return data[0] == ActivatedMessage;
		}

		public void RegisterCallback(Action callback) {
			Timer_Callback = callback;
			t.Enabled = true;
			t.Start();
		}

		void t_Elapsed(object sender, System.Timers.ElapsedEventArgs e) {
			if (GetStatus()) {
				if (!ButtonState) { //Only toggle if button not already set
					ButtonState = true;
					Timer_Callback();
				}
			} else {
				ButtonState = false; //Reset state
			}
		}
	}
}

