using System;
using System.Linq;
using System.Management;
using System.Text.RegularExpressions;
using System.Threading;

namespace Macpac
{
	class NetworkAdapter
	{
		public static ManagementObject Get(string NicID, bool IsIndex)
		{
			ManagementObject NicObj = null; //initialise to null, if the try fails we still have a value of sorts
			string Query = String.Format("SELECT * FROM Win32_NetworkAdapter WHERE {0}='{1}'", (IsIndex ? "Index" : "NetConnectionID"), NicID);
			try
			{
				NicObj = new ManagementObjectSearcher(Query).Get().Cast<ManagementObject>().FirstOrDefault();
			}
			catch(ManagementException e)
			{
				Console.WriteLine("Error {0:X8}: {1}", e.HResult, e.Message);
				return null;
			}
			return NicObj;
		}
		public static bool SetName(ManagementObject NicObj, string NewName)
		{
			if((NewName != null) && (NewName.Length > 0))
			{
				if(!new Regex(@"[\t\\/:\*\?\<\>\|""]|^-").IsMatch(NewName))
				{
					try
					{
						NicObj["NetConnectionID"] = NewName;
						NicObj.Put();
					}
					catch(Exception e)
					{
						switch((uint)e.HResult)
						{
							case 2147943140:
								Console.WriteLine("Error {0:X8}: You need administrative privileges to do this.", e.HResult);
								break;
							case 2147942452:
								Console.WriteLine("Error {0:X8}: Another adapter already has this name.", e.HResult);
								break;
							default:
								Console.WriteLine("Error {0:X8}: An unknown error occurred.", e.HResult);
								break;
						}
						return false;
					}
				}
				else
				{
					Console.WriteLine("Error: Connection name cannot contain tabs or any of the following:\n\\ / : * ? < > | \"");
					return false;
				}
			}
			else
			{
				Console.WriteLine("Error: Connection name cannot be null.");
				return false;
			}
			return true;
		}
		public static bool SetState(ManagementObject NicObj, int State)
		{
			if(State == 0 || State == 1) //reset or disable
			{
				short DisResult = Convert.ToInt16(NicObj.InvokeMethod("Disable", null));
				switch(DisResult)
				{
					case 0:
						Console.WriteLine("Disable succeeded.");
						break;
					case 5:
						Console.WriteLine("Error: Permission denied.");
						if(State == 0) return false;
						break;
					default:
						Console.WriteLine("Error: Disable result: {0}", DisResult);
						if(State == 0) return false;
						break;
				}
				if(State == 0) Thread.Sleep(1000);
			}
			if(State == 0 || State == 2) //reset or enable
			{
				short EnResult = Convert.ToInt16(NicObj.InvokeMethod("Enable", null));
				switch(EnResult)
				{
					case 0:
						Console.WriteLine("Enable succeeded.");
						break;
					case 5:
						Console.WriteLine("Error: Permission denied.");
						return false;
					default:
						Console.WriteLine("Error: Enable result: {0}", EnResult);
						return false;
				}
			}
			return true;
		}
		public static string FormatSpeed(ulong Speed)
		{
			string[] Ords = { "", "K", "M", "G", "T", "P", "E" };
			decimal Rate = (decimal)Speed;
			int i = 0;
			for(; Rate >= 1000; i++) Rate /= 1000;
			return Rate.ToString("N2") + " " + Ords[i] + "bps";
		}
	}
}
