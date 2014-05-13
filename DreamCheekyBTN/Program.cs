using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace DreamCheekyUSB {
	class Program {
		static string strCMD = "";
		static string strCMDARGs = "";
		static string strMacro = "";
		static int count = 0;

		static int Main(string[] args) {
			int actions = 0;
			DreamCheekyBTN btn = null;
			try {
				if (args.ContainsInsensitive("debug")) {
					Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
				}

				string devicearg = args.StartsWith("device=").FirstOrDefault();
				if (string.IsNullOrEmpty(devicearg)) {
					Trace.WriteLine("\r\nConnecting to DreamCheekyBTN using default values...");
					try {
						btn = new DreamCheekyBTN();
					} catch (Exception ignored) {
						Trace.WriteLine(ignored.Message);
						Trace.WriteLine("No Iron Man USB Stress Buttons found.\r\nSearching for Big Red Buttons...");
						btn = new DreamCheekyBigRedBTN(DreamCheekyBigRedBTN.DEFAULT_VENDOR_ID, DreamCheekyBigRedBTN.DEFAULT_PRODUCT_ID);
					}
				} else {
					Trace.WriteLine("\r\nConnecting to DreamCheekyBTN using specified device {0}...", devicearg);
					string[] deviceSplit = devicearg.Substring(7).Split(',');
					if (deviceSplit.Length == 1) {
						//One argument = device path
						var searchString = string.Format("pid_{0}", DreamCheekyBigRedBTN.PID);
						if (deviceSplit[0].Contains(searchString)) {
							Trace.WriteLine("Device is a Big Red Button...");
							btn = new DreamCheekyBigRedBTN(deviceSplit[0]);
						} else {
							Trace.WriteLine("Device is a Iron Man USB Stress Button...");
							btn = new DreamCheekyBTN(deviceSplit[0]);
						}
					} else {
						//Two or Three arguments = VID,PID,Count=0
						int devicecount = 0;
						if (deviceSplit.Length > 2) {
							devicecount = int.Parse(deviceSplit[2]);
						}

						int VID = int.Parse(deviceSplit[0].Substring(2), System.Globalization.NumberStyles.HexNumber);
						int PID = int.Parse(deviceSplit[1].Substring(2), System.Globalization.NumberStyles.HexNumber);
												
						if (PID == DreamCheekyBTN.DEFAULT_PRODUCT_ID) {
							btn = new DreamCheekyBTN(VID, PID, devicecount);
						} else {
							btn = new DreamCheekyBigRedBTN(VID, PID, devicecount);
						}
					}
				}

				string cmdarg = args.StartsWith("CMD=").FirstOrDefault();
				if (!string.IsNullOrEmpty(cmdarg)) {
					actions++;
					strCMD = cmdarg.Substring(4);
					Console.WriteLine("Setting command to: " + strCMD);
				}

				string argarg = args.StartsWith("ARG=").FirstOrDefault();
				if (!string.IsNullOrEmpty(argarg)) {
					strCMDARGs = argarg.Substring(4);
					Console.WriteLine("Setting command arguments to: " + strCMDARGs);
				}

				string macroarg = args.StartsWith("MACRO=").FirstOrDefault();
				if (!string.IsNullOrEmpty(macroarg)) {
					actions++;
					string[] macosplit = macroarg.Split('=');
					strMacro = macosplit[1];
					Console.WriteLine("Setting Macro to: " + strMacro);
				}

				if (actions > 0) {
					btn.RegisterCallback(DoAction);
					Console.WriteLine("Listening for button press events. Press any key to escape...");
					Console.ReadKey(true);
				}
			} catch (Exception ex) {
				Trace.TraceError("\r\n\r\nError: " + ex.Message + "\r\n\r\n");
			}

			//Pause on exit or display usage syntax
			if (actions > 0) {
				Trace.WriteLine("Finished\r\n");
			} else { //No actions specified, show help
				Console.WriteLine("  DreamCheekyBTN.exe [device=...] [options]");

				Console.WriteLine("\r\nExamples:");

				Console.WriteLine("  DreamCheekyBTN.exe debug MACRO=ASDF~  (ASDF then Enter)");
				Console.WriteLine("  DreamCheekyBTN.exe MACRO=%+{F1}       (ALT+SHIFT+F1)");
				Console.WriteLine("  DreamCheekyBTN.exe CMD=c:\\temp\\test.bat");
				Console.WriteLine(@"  DreamCheekyBTN.exe CMD=powershell ARG=""-noexit -executionpolicy unrestricted -File c:\test.ps1""");

				Console.WriteLine("\r\n\r\nDevice Path:");
				Console.WriteLine("  Optional, Defaults to first USB device with VID=0x1D34 and PID=0x0008");
				Console.WriteLine("  Example (VID,PID,Index): device=\"0x1D34,0x0008,0\"");
				Console.WriteLine("  Example (Path): device=" + @"""\\?\hid#vid_1d34&pid_0008#6&1067c3dc&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}""");

				Console.WriteLine("\r\nOptions:");
				Console.WriteLine("  debug = Print Console statements to Console.Out");

				Console.WriteLine("\r\nCMD: will run specified command when button is pressed");
				Console.WriteLine("ARG: can be used to specified command arguments");
				Console.WriteLine("  Example (open calculator): CMD=calc");
				Console.WriteLine("  Example (run Powershell commands): ");
				Console.WriteLine("     CMD=\"%SystemRoot%\\system32\\WindowsPowerShell\\v1.0\\powershell.exe\"");
				Console.WriteLine(@"     ARG=""-Command \""& {write-host 'BEEP!'; [console]::beep(440,1000);}\""""");
				Console.WriteLine("  NOTE: use ^& instead of & if running from command prompt as & is special character");

				Console.WriteLine("\r\nMACRO: will send specified key sequense to active window via C# Sendkeys");
				Console.WriteLine("NOTE: +=Shift, ^=CTRL, %=ALT, ~=Return, use () to group characters.");
				Console.WriteLine("  Example: MACRO=\"%^g\"        (ALT + CTRL + g)");
				Console.WriteLine("  Example: MACRO=\"%(asdf)\"    (ALT + asdf)");
				Console.WriteLine("\r\n");

			}

			if (args.ContainsInsensitive("pause")) {
				Console.WriteLine("\r\nPress enter to exit...");
				Console.ReadLine();
			}

			if (btn != null) {
				btn.Dispose();
			}
			return 0;
		}

		static void DoAction() {
			count++;
			Console.WriteLine(String.Format("\r\n{0}: Detected button press event. Count={1}", DateTime.Now.ToLongTimeString(), count));
			if (!string.IsNullOrEmpty(strCMD)) {
				try {
					Process.Start(strCMD, strCMDARGs);
				} catch (Exception ex) {
					Trace.TraceError("Error: " + ex.Message);
				}
			}
			if (!string.IsNullOrEmpty(strMacro)) {
				try {
					Console.WriteLine("Sending keys: " + strMacro);
					System.Windows.Forms.SendKeys.SendWait(strMacro);
				} catch (Exception ex) {
					Trace.TraceError("Error: " + ex.Message);
				}
			}
		}
	}

	/// <summary>
	/// Extenstions for working with string arrays
	/// </summary>
	public static class StringArrayExtenstions {
		public static bool ContainsInsensitive(this string[] args, string name) {
			return args.Contains(name, StringComparer.CurrentCultureIgnoreCase);
		}

		public static IEnumerable<string> StartsWith(this string[] args, string value, StringComparison options = StringComparison.CurrentCultureIgnoreCase) {
			var q = from a in args
			        where a.StartsWith(value, options)
			        select a;
			return q;
		}
	}
}
