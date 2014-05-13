namespace DreamCheekyUSB {
	public class DreamCheekyBigRedBTN : DreamCheekyBTN {
		#region Constant and readonly values

		new public const int DEFAULT_VENDOR_ID = 0x1D34;
		new public const int DEFAULT_PRODUCT_ID = 0x000D;
		//Default for USB Big Red Button
		new public static class Messages {
			public const byte LID_CLOSED = 0x15;
			public const byte BUTTON_PRESSED = 0x16;
			public const byte LID_OPEN = 0x17;
		}

		public const string PID = "000d";

		#endregion

		#region Constructors

		public DreamCheekyBigRedBTN() {
			ActivatedMessage = Messages.BUTTON_PRESSED;
		}

		public DreamCheekyBigRedBTN(int deviceIndex = 0) : base(DEFAULT_VENDOR_ID, DEFAULT_PRODUCT_ID, deviceIndex) {
			ActivatedMessage = Messages.BUTTON_PRESSED;
		}

		public DreamCheekyBigRedBTN(int vendorID, int productID, int deviceIndex = 0) : base(vendorID, productID, deviceIndex) {
			ActivatedMessage = Messages.BUTTON_PRESSED;
		}

		public DreamCheekyBigRedBTN(string devicePath) : base(devicePath) {
			ActivatedMessage = Messages.BUTTON_PRESSED;
		}

		#endregion

		public bool LidIsOpen() {
			Write(CmdStatus);
			var data = Read();
			return data[0] == Messages.LID_OPEN;
		}

		public bool LidIsClosed() {
			Write(CmdStatus);
			var data = Read();
			return data[0] == Messages.LID_CLOSED;
		}
	}
}