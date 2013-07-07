using System;
using System.Linq;
using System.Management;

namespace Macpac
{
	class Entry
	{
		public static int Main(string[] Args)
		{
			Console.WriteLine("\nMACPAC: MAC/Physical Address Changer\n");
			if(Args.Length > 0) //if we have args
			{
				if(Args.Contains("/?"))
				{
					ShowHelp();
					return 0; //OK
				}
				else if(Args.Contains("-g"))
				{
					Console.WriteLine("Generated " + MacAddress.Generate((Args.Contains("-nofix"))));
					return 0; //OK
				}
				if((Args[0] == "-i" || Args[0] == "-id") && (Args.Length > 1)) //-i or -id must be the first argument and it must have an argument after it
				{
					if(Args.Length > 2)
					{
						ManagementObject NicObj = NetworkAdapter.Get(Args[1], (Args[0] == "-id"));
						if(NicObj == null)
						{
							Console.WriteLine("Error: Invalid adapter name or ID specified.");
							return 3; //Invalid adapter
						}
						for(int i = 2; i < Args.Length; i++) //iterate through the rest of the arguments - ignore -i <...>
						{
							switch(Args[i])
							{
								case "-s":
									if(Args.Length > i + 1) //check that -s is not the last argument
									{
										string Addr = Args[i + 1];
										if(MacAddress.Validate(Addr) || Addr == "random") //valid mac address or "random" parameter
										{
											if(Addr == "random") Addr = MacAddress.Generate();
											if(!Args.Contains("-nofix")) Addr = MacAddress.Correct(Addr);
											Console.WriteLine("Setting {0} for {1}...", Addr, NicObj["NetConnectionID"]);
											try
											{
												RegEdit.Write(NicObj["Index"].ToString().PadLeft(4, '0') + "\\", "NetworkAddress", Addr);
												Console.WriteLine("Successfully set MAC address.");
											}
											catch
											{
												return 12; //Set MAC failed
											}
											if(!Args.Contains("-noreset"))
											{
												Console.WriteLine("Resetting {0}...", NicObj["NetConnectionID"]);
												NetworkAdapter.SetState(NicObj, 0);
											}
										}
										else
										{
											Console.WriteLine("Error: Invalid MAC address specified.");
											return 5; //Invalid MAC
										}
									}
									else
									{
										Console.WriteLine("Error: No MAC address specified.");
										return 4; //No MAC
									}
									break;
								case "-u":
									Console.WriteLine("Unsetting MAC address of {0}...", NicObj["NetConnectionID"]);
									try
									{
										RegEdit.Delete(NicObj["Index"].ToString().PadLeft(4, '0') + "\\", "NetworkAddress");
										Console.WriteLine("Successfully unset MAC address.");
									}
									catch
									{
										return 11; //Unset MAC failed
									}
									if (!Args.Contains("-noreset")) NetworkAdapter.SetState(NicObj, 0);
									break;
								case "-add":
									Console.WriteLine("Adding Network Address parameter for {0}...", NicObj["NetConnectionID"]);
									string Param = NicObj["Index"].ToString().PadLeft(4, '0') + "\\Ndi\\Params\\NetworkAddress";
									try
									{
										RegEdit.Write(Param, "Default", "");
										RegEdit.Write(Param, "LimitText", "12");
										RegEdit.Write(Param, "Optional", "1");
										RegEdit.Write(Param, "ParamDesc", "NetworkAddress");
										RegEdit.Write(Param, "type", "edit");
										RegEdit.Write(Param, "UpperCase", "1");
										Console.WriteLine("Successfully added Network Address parameter.");
									}
									catch
									{
										return 10; //Add Network Address parameter failed
									}
									break;
								case "-del":
									Console.WriteLine("Deleting Network Address parameter for {0}...", NicObj["NetConnectionID"]);
									try
									{
										RegEdit.Delete(NicObj["Index"].ToString().PadLeft(4, '0') + "\\Ndi\\Params\\NetworkAddress");
										Console.WriteLine("Successfully deleted Network Address parameter.");
									}
									catch
									{
										return 9; //Delete Network Address parameter failed
									}
									break;
								case "-n":
									if(Args.Length > i + 1)
									{
										Console.WriteLine("Renaming '{0}' to '{1}'...", NicObj["NetConnectionID"], Args[i + 1]);
										if(NetworkAdapter.SetName(NicObj, Args[i + 1])) Console.WriteLine("Successfully renamed the network adapter.");
										else return 8; //Adapter name change failed
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
									Console.WriteLine("Speed:         " + NetworkAdapter.FormatSpeed(Convert.ToUInt64(NicObj["Speed"])));
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
			if(Args.Contains("-keep")) Console.ReadKey();
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
			Console.WriteLine("   -g                 Generate a random MAC address.");
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
		}
	}
}
