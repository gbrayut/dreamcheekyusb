using System;
using System.Diagnostics;
using HidSharp;

namespace DreamCheekyUSB {
	public abstract class LEDBase : IDisposable {
		/// <summary>
		/// HidLibray.HidDevice for this USBLED instance
		/// </summary>
		public HidDevice HidLED;
		internal HidStream Stream;
		internal HidDeviceLoader Loader;
		internal bool initialized = false;

		internal virtual bool Initialize() { //Should be extended by base classes
			if (initialized) {
				return true;
			}
			if (HidLED == null) {
				throw new NullReferenceException("hidLED not initialized");
			}
			Stream = HidLED.Open();
			initialized = true;
			return true;
		}

		public void Dispose() {
			Trace.WriteLine("Releasing control of USB device...");
			if (Stream != null) {
				Stream.Close();
				Stream.Dispose();
				Stream = null;
			}
		}

		public abstract bool Write(byte[] data);

		public abstract bool SetColor(System.Drawing.Color color);

		public abstract bool SetColor(byte red, byte green, byte blue);

		/// <summary>
		/// Accepts rgb value as a string in xxx,xxx,xxx format. Will convert to byte values.
		/// </summary>
		/// <param name="rgb">Example: 255,255,0 for Yellow</param>
		/// <returns>True if success, False otherwise.</returns>
		public bool SetColor(string rgb) {
			try {
				var rxRGB = System.Text.RegularExpressions.Regex.Match(rgb, "^([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5]),([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5]),([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])$");
				if (rxRGB.Success) {
					byte red = byte.Parse(rxRGB.Groups[1].Value);
					byte green = byte.Parse(rxRGB.Groups[2].Value);
					byte blue = byte.Parse(rxRGB.Groups[3].Value);
					return SetColor(red, green, blue);
				} 
					
				System.Diagnostics.Trace.WriteLine("Invalid RGB string: " + rgb);
				return false;
			} catch (Exception ex) {
				System.Diagnostics.Trace.WriteLine("Error in SetColor: " + ex.Message);
				return false;
			}
		}

		public bool Off() {
			return SetColor(0, 0, 0);
		}

		/// <summary>
		/// Cycle Red, Green, Blue, then fade Red, Green Blue
		/// </summary>
		/// <returns></returns>
		public virtual bool Test() {
			bool bReturn = true;
			bReturn &= SetColor(255, 0, 0);
			System.Threading.Thread.Sleep(250);
			bReturn &= SetColor(0, 255, 0);
			System.Threading.Thread.Sleep(250);
			bReturn &= SetColor(0, 0, 255);
			System.Threading.Thread.Sleep(250);
			bReturn &= Off();
			System.Threading.Thread.Sleep(250);
			bReturn &= FadeInOut(System.Drawing.Color.Red, 1000);
			System.Threading.Thread.Sleep(100);
			bReturn &= FadeInOut(System.Drawing.Color.Green, 1000);
			System.Threading.Thread.Sleep(100);
			bReturn &= FadeInOut(System.Drawing.Color.Blue, 1000);
			return bReturn;
		}

		public virtual bool TestBlink() {
			bool bReturn = true;
			bReturn &= Blink(System.Drawing.Color.Red);
			bReturn &= Blink(System.Drawing.Color.Orange);
			bReturn &= Blink(System.Drawing.Color.Yellow);
			bReturn &= Blink(System.Drawing.Color.Green);
			bReturn &= Blink(System.Drawing.Color.Blue);
			bReturn &= Blink(System.Drawing.Color.Indigo);
			bReturn &= Blink(System.Drawing.Color.Violet);
			return Off() && bReturn;
		}

		public bool FadeInOut(System.Drawing.Color toColor, int totalMs = 2000) {
			if (totalMs <= 0) {
				throw new ArgumentOutOfRangeException("totalMs", "must be greater than zero");
			}
			FadeIn(toColor, totalMs / 2);
			return FadeOut(toColor, totalMs / 2);
		}

		public void FadeIn(System.Drawing.Color toColor, int totalMs = 1000) {
			if (totalMs <= 0) {
				throw new ArgumentOutOfRangeException("totalMs", "must be greater than zero");
			}
			int t = 0;
			const int step = 35;
			float ratio;
			while (t < totalMs) {
				System.Threading.Thread.Sleep(step);
				ratio = (float)t / (float)totalMs;
				byte red = (byte)(toColor.R * ratio);
				byte green = (byte)(toColor.G * ratio);
				byte blue = (byte)(toColor.B * ratio);
				SetColor(red, green, blue);
				t += step;
			}
		}

		public bool FadeOut(System.Drawing.Color fromColor, int totalMs = 1000) {
			if (totalMs <= 0) {
				throw new ArgumentOutOfRangeException("totalMs", "must be greater than zero");
			}
			var t = 0;
			const int step = 35;
			float ratio;
			while (t < totalMs) {
				System.Threading.Thread.Sleep(step);
				ratio = (float)t / (float)totalMs;
				byte red = (byte)(fromColor.R - (fromColor.R * ratio));
				byte green = (byte)(fromColor.G - (fromColor.G * ratio));
				byte blue = (byte)(fromColor.B - (fromColor.B * ratio));
				SetColor(red, green, blue);
				t += step;
			}
			return Off();
		}

		public bool Blink(System.Drawing.Color color, int count = 1, int blinkMs = 500) {
			if (count <= 0) {
				throw new ArgumentOutOfRangeException("count", "Count cannot be less than zero");
			}
			if (blinkMs <= 0) {
				throw new ArgumentOutOfRangeException("blinkMs", "BlinkMs cannot be less than zero");
			}

			int i = 0;
			bool bReturn = true;
			while (i < count) {
				bReturn &= FadeInOut(color, blinkMs);
				i++;
			}
			return bReturn;
		}

		public bool TryParseNametoColor(string name, out System.Drawing.Color result) {
			result = System.Drawing.Color.FromName(name);
			if (result.IsKnownColor || result.IsSystemColor) {
				return true;
			}

			return false;
		}
	}
}

