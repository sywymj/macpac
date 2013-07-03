/***************************************************
MACPAC: A free command-line MAC address changing utility for Windows 7
Copyright (C) 2013 Ryan Miller fauxpark@gmail.com

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
***************************************************/

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;

namespace Macpac
{
	class Entry
	{
		public static int Main(string[] Args)
		{
			Console.WriteLine("\nMACPAC: MAC/Physical Address Changer\n");
			if (Args.Length > 0) //if we have args
			{
				if (Args.Contains("/?"))
				{
					ShowHelp();
					return 0; //OK
				}
				else if (Args.Contains("-gen"))
				{
					Console.WriteLine("Generated " + MacAddress.Generate((Args.Contains("-nofix")) ? true : false));
					return 0; //OK
				}
				if ((Args[0] == "-i" || Args[0] == "-id") && (Args.Length > 1)) //-i or -id must be the first argument and it must have an argument after it
				{
					if (Args.Length > 2)
					{
						ManagementObject NicObj = NetworkAdapter.Get(Args[1], (Args[0] == "-id") ? true : false);
						if (NicObj == null)
						{
							Console.WriteLine("Error: Invalid {0} specified.", (Args[0] == "-id") ? "adapter ID" : "connection name"); //get rid
							return 3; //Invalid adapter
						}
						for (int i = 2; i < Args.Length; i++) //iterate through the rest of the arguments - ignore -i <...>
						{
							switch(Args[i])
							{
								case "-s":
									if (Args.Length > i + 1) //check that -s is not the last argument
									{
										string Addr = Args[i + 1];
										if (new Regex(@"^([\da-fA-F]{12}|random)").IsMatch(Addr)) //valid mac address or "random" parameter
										{
											if (Addr == "random") Addr = MacAddress.Generate();
											if (!Args.Contains("-nofix")) Addr = MacAddress.Correct(Addr);
											Console.WriteLine("Setting {0}...", Addr);
											RegEdit.Write(NicObj["Index"].ToString().PadLeft(4, '0') + "\\", "NetworkAddress", Addr);
											if (!Args.Contains("-noreset")) NetworkAdapter.SetState(NicObj, 0);
										}
										else
										{
											Console.WriteLine("Error: Invalid MAC address specified.");
											return 5; //Invalid MAC
										}
									}
									else
									{
										Console.WriteLine("Error: No MAC Address specified.");
										return 4; //No MAC
									}
									break;
								case "-u":
									Console.WriteLine("Unsetting...");
									RegEdit.Delete(NicObj["Index"].ToString().PadLeft(4, '0') + "\\", "NetworkAddress");
									if (!Args.Contains("-noreset")) NetworkAdapter.SetState(NicObj, 0);
									break;
								case "-add":
									Console.WriteLine("Adding Network Address parameter...");
									string Param = NicObj["Index"].ToString().PadLeft(4, '0') + "\\Ndi\\Params\\NetworkAddress";
									RegEdit.Write(Param, "Default", "");
									RegEdit.Write(Param, "LimitText", "12");
									RegEdit.Write(Param, "Optional", "1");
									RegEdit.Write(Param, "ParamDesc", "NetworkAddress");
									RegEdit.Write(Param, "type", "edit");
									RegEdit.Write(Param, "UpperCase", "1");
									break;
								case "-del":
									Console.WriteLine("Deleting Network Address parameter...");
									RegEdit.Delete(NicObj["Index"].ToString().PadLeft(4, '0') + "\\Ndi\\Params\\NetworkAddress");
									break;
								case "-n":
									if (Args.Length > i + 1)
									{
										Console.WriteLine("Renaming \"{0}\" to \"{1}\"...", NicObj["NetConnectionID"], Args[i + 1]);
										NetworkAdapter.SetName(NicObj, Args[i + 1]);
									}
									else
									{
										Console.WriteLine("Error: No new NetConnectionID specified.");
										return 7; //No NetConnectionID
									}
									break;
								case "-r":
									Console.WriteLine("Resetting {0}...", NicObj["NetConnectionID"]);
									NetworkAdapter.SetState(NicObj, 0);
									break;
								case "-d":
									Console.WriteLine("Disabling {0}...", NicObj["NetConnectionID"]);
									NetworkAdapter.SetState(NicObj, 1);
									break;
								case "-e":
									Console.WriteLine("Enabling {0}...", NicObj["NetConnectionID"]);
									NetworkAdapter.SetState(NicObj, 2);
									break;
								case "-show":
									Console.WriteLine("Network Adapter: " + (NicObj["NetConnectionID"] ?? "n/a") + "\n");
									Console.WriteLine("Index:         " + NicObj["Index"]);
									Console.WriteLine("Manufacturer:  " + NicObj["Manufacturer"]);
									Console.WriteLine("Adapter Name:  " + NicObj["Name"]);
									Console.WriteLine("Status:        " + (Convert.ToBoolean(NicObj["NetEnabled"]) ? "Enabled" : "Disabled"));
									Console.WriteLine("MAC Address:   " + (NicObj["MACAddress"] ?? "n/a"));
									Console.WriteLine("Speed:         " + NetworkAdapter.FormatAdapterSpeed(Convert.ToUInt64(NicObj["Speed"])));
									Console.WriteLine("GUID:          " + (NicObj["GUID"] ?? "n/a"));
									Console.WriteLine("Last Reset:    " + ManagementDateTimeConverter.ToDateTime(NicObj["TimeOfLastReset"].ToString()).ToString("dd/MM/yyyy HH:mm:ss tt"));
									break;
							}
						}
					}
					else
					{
						Console.WriteLine("Error: No commands entered. Try \"macpac /?\" for help.");
						return 6; //No instructions for adapter
					}
				}
				else
				{
					Console.WriteLine("Error: No connection name or ID specified.\n");
					ShowHelp();
					return 2; //No adapter
				}
			}
			else
			{
				ShowHelp();
				return 1; //No arguments
			}
			if (Args.Contains("-keep")) Console.ReadKey();
			return 0; //OK
		}
		static void ShowHelp()
		{
			Console.WriteLine("Usage: macpac -i[d] <adapter> [[-s <MAC> | random] | -u] [-nofix] [-noreset]");
			Console.WriteLine("       [-add | -del] [-n <name>] [-r | -d | -e] [-show] [-gen] [/?]\n");
			Console.WriteLine("Options:");
			Console.WriteLine("   -i[d] <adapter>    The network adapter's Network Connections name or ID.");
			Console.WriteLine("   -s <MAC> | random  Set the adapter's MAC address.");
			Console.WriteLine("   -u                 Unset the custom MAC address (if any).");
			Console.WriteLine("   -gen               Generate a random MAC address.");
			Console.WriteLine("   -nofix             Don't force locally administered unicast address.");
			Console.WriteLine("   -noreset           Suppress adapter reset after successful MAC set/unset.");
			Console.WriteLine("   -add               Add Network Address property to adapter configuration");
			Console.WriteLine("                        in Advanced tab.");
			Console.WriteLine("   -del               Remove Network Address property.");
			Console.WriteLine("   -n <name>          Sets the connection name of the adapter.");
			Console.WriteLine("   -r                 Reset the specified network adapter.");
			Console.WriteLine("   -d                 Disable the specified network adapter.");
			Console.WriteLine("   -e                 Enable the specified network adapter.");
			Console.WriteLine("   -show              Show information on the adapter.");
			Console.WriteLine("   /?                 Display this help message.");
			Console.ReadKey();
		}
	}
	class RegEdit
	{
		const string RegRoot = @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002BE10318}\"; //base path
		public static void Write(string RegKey, string RegName = null, object RegVal = null)
		{
			try
			{
				if (RegName == null) Registry.LocalMachine.OpenSubKey(RegRoot, true).CreateSubKey(RegKey);
				else Registry.LocalMachine.OpenSubKey(RegRoot + RegKey, true).SetValue(RegName, RegVal, RegistryValueKind.String);
			}
			catch (Exception e)
			{
				if (e is System.Security.SecurityException || e is UnauthorizedAccessException) Console.WriteLine("RegWrite Error: Permission denied.\n{0}: {1}", e.HResult, e.Message);
				else Console.WriteLine("RegWrite Error {0}: {1}", e.HResult, e.Message);
			}
		}
		public static void Delete(string RegKey, string RegVal = null)
		{
			try
			{
				if (RegVal == null) Registry.LocalMachine.OpenSubKey(RegRoot, true).DeleteSubKeyTree(RegKey);
				else Registry.LocalMachine.OpenSubKey(RegRoot + RegKey, true).DeleteValue(RegVal);
			}
			catch (Exception e)
			{
				if (e is System.Security.SecurityException || e is UnauthorizedAccessException) Console.WriteLine("RegDelete Error: Permission denied.\n{0}: {1}", e.HResult, e.Message);
				else Console.WriteLine("RegDelete Error {0}: {1}", e.HResult, e.Message);
			}
		}
	}
	class NetworkAdapter //this is where all our network adapter-specific methods are kept
	{
		public static ManagementObject Get(string NicID, bool IsIndex) //returns a ManagementObject for the adapter which can then be used to get info and set its name or state
		{
			ManagementObject NicObj = null; //initialise to null, if the try fails we still have a value of sorts
			try
			{
				NicObj = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapter WHERE " + (IsIndex ? "Index" : "NetConnectionID") + "='" + NicID + "'").Get().Cast<ManagementObject>().FirstOrDefault();
			}
			catch (ManagementException e)
			{
				Console.WriteLine("Error {0}: {1}", e.ErrorCode, e.Message);
				return null;
			}
			return NicObj;
		}
		public static void SetName(ManagementObject NicObj, string NewName)
		{
			if ((NewName != null) && (NewName.Length > 0))
			{
				if (!new Regex(@"[\t\\/:\*\?\<\>\|""]|^-").IsMatch(NewName))
				{
					try
					{
						NicObj["NetConnectionID"] = NewName;
						NicObj.Put();
					}
					catch(Exception e)
					{
						switch(e.HResult.ToString("X"))
						{
							case "800702E4":
								Console.WriteLine("Error {0:X8}: You need administrative privileges to do this.", e.HResult);
								break;
							case "80070034":
								Console.WriteLine("Error {0:X8}: Another adapter already has this name.", e.HResult);
								break;
						}
					}
				}
				else Console.WriteLine("Error: Connection name cannot contain tabs or any of the following:\n\\ / : * ? < > | \"");
			}
			else Console.WriteLine("Error: Connection name cannot be null.");
		}
		public static void SetState(ManagementObject NicObj, int State)
		{
			if (State == 0 || State == 1) //reset or disable
			{
				short DisResult = Convert.ToInt16(NicObj.InvokeMethod("Disable", null));
				switch (DisResult)
				{
					case 0:
						Console.WriteLine("Disable succeeded.");
						break;
					case 5:
						Console.WriteLine("Error: Permission denied.");
						if (State == 0) return;
						break;
					default:
						Console.WriteLine("Disable Result: {0}", DisResult);
						if (State == 0) return;
						break;
				}
				if (State == 0) Thread.Sleep(1000);
			}
			if (State == 0 || State == 2) //reset or enable
			{
				short EnResult = Convert.ToInt16(NicObj.InvokeMethod("Enable", null));
				switch (EnResult)
				{
					case 0:
						Console.WriteLine("Enable succeeded.");
						break;
					case 5:
						Console.WriteLine("Error: Permission denied.");
						break;
					default:
						Console.WriteLine("Enable Result: {0}", EnResult);
						break;
				}
			}
		}
		public static string FormatAdapterSpeed(ulong Speed)
		{
			string[] Ords = { "", "K", "M", "G", "T", "P", "E" };
			decimal Rate = (decimal)Speed;
			int i = 0;
			for (; Rate >= 1000; i++) Rate /= 1000;
			return Rate.ToString("N2") + " " + Ords[i] + "bps";
		}
	}
	class MacAddress
	{
		public static string Generate(bool FixAddr = false)
		{
			string Addr = Convert.ToUInt64(new Random().NextDouble().ToString().Substring(2)).ToString("X2").PadLeft(12, '0').Substring(0, 12);
			if (FixAddr) return Correct(Addr);
			else return Addr;
		}
		public static string Correct(string Addr)
		{
			string[] HexDigits = { "2", "6", "A", "E" };
			if (HexDigits.Contains(Addr.Substring(1, 1))) return Addr.Substring(0, 1) + HexDigits[new Random().Next(0, 3)] + Addr.Substring(2);
			else return Addr;
		}
	}
}