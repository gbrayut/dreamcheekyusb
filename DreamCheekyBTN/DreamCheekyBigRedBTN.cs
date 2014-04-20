namespace DreamCheekyUSB
{
	public class DreamCheekyBigRedBTN : DreamCheekyBTN
	{
		#region Constant and readonly values
        new public const int DefaultProductID = 0x000D; //Default for USB Big Red Button
		new public class Messages
		{
			public const byte LidClosed = 0x15;
			public const byte ButtonPressed = 0x16;
			public const byte LidOpen = 0x17;
		}
		
		public const string PID = "000d";
		#endregion

		#region Constructors
		public DreamCheekyBigRedBTN() : base() {
			activatedMessage = Messages.ButtonPressed;
		}
		
		public DreamCheekyBigRedBTN(int DeviceIndex = 0) : base(DefaultVendorID, DefaultProductID, DeviceIndex) {
			activatedMessage = Messages.ButtonPressed;
		}
		
		public DreamCheekyBigRedBTN(int VendorID, int ProductID, int DeviceIndex=0) : base(VendorID, ProductID, DeviceIndex) {
			activatedMessage = Messages.ButtonPressed;
		}
		
		public DreamCheekyBigRedBTN(string DevicePath) : base(DevicePath) {
			activatedMessage = Messages.ButtonPressed;
		}
		#endregion
	}
}