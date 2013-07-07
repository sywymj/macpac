/****************************************************\
 * RegEdit.cs                                       *
 * Description: Registry manipulation tools         *
 * Constants: string RegRoot                        *
 * Methods: void Write, void Delete                 *
 *                                                  *
\****************************************************/

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
				if(e is System.Security.SecurityException || e is UnauthorizedAccessException) Console.WriteLine("Error {0:X8}: {1}", e.HResult, e.Message);
				else Console.WriteLine("Error {0:X8}: {1}", e.HResult, e.Message);
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
				if(e is System.Security.SecurityException || e is UnauthorizedAccessException) Console.WriteLine("Error {0:X8}: {1}", e.HResult, e.Message);
				else Console.WriteLine("Error {0:X8}: {1}", e.HResult, e.Message);
				throw e;
			}
		}
	}
}
