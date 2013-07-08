using Microsoft.Win32;
using System;

namespace Macpac
{
	class RegEdit
	{
		const string RegRoot = @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002BE10318}\"; //base path
		public static void Write(string RegKey, string RegName = null, object RegVal = null)
		{
			try
			{
				if(RegName == null) Registry.LocalMachine.OpenSubKey(RegRoot, true).CreateSubKey(RegKey);
				else Registry.LocalMachine.OpenSubKey(RegRoot + RegKey, true).SetValue(RegName, RegVal, RegistryValueKind.String);
			}
			catch(Exception e)
			{
				throw e;
			}
		}
		public static void Delete(string RegKey, string RegVal = null)
		{
			try
			{
				if(RegVal == null) Registry.LocalMachine.OpenSubKey(RegRoot, true).DeleteSubKeyTree(RegKey);
				else Registry.LocalMachine.OpenSubKey(RegRoot + RegKey, true).DeleteValue(RegVal);
			}
			catch(Exception e)
			{
				throw e;
			}
		}
	}
}
