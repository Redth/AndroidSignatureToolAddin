using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace AndroidKeystoreSignatureGenerator
{
	public class LocationHelperWindows : ILocations
	{
		const string MDREG_KEY = @"SOFTWARE\Novell\Mono for Android";
		const string MDREG_ANDROID = "AndroidSdkDirectory";
		const string MDREG_JAVA = "JavaSdkDirectory";
		const string MDREG_MONODROID = "InstallDirectory";
		const string ANDROID_INSTALLER_PATH = @"SOFTWARE\Android SDK Tools";
		const string ANDROID_INSTALLER_KEY = "Path";

		#region ILocations Members
		public string GetJavaSdkDirectory()
		{
			string subkey = @"SOFTWARE\JavaSoft\Java Development Kit";

			LogMessage("Looking for Java SDK..");

			// Look for the registry keys written by the Java SDK installer
			foreach (var wow64 in new[] { RegistryEx.Wow64.Key32, RegistryEx.Wow64.Key64 })
			{
				string key_name = string.Format(@"{0}\{1}\{2}", "HKLM", subkey, "CurrentVersion");
				var currentVersion = RegistryEx.GetValueString(RegistryEx.LocalMachine, subkey, "CurrentVersion", wow64);

				if (!string.IsNullOrEmpty(currentVersion))
				{
					LogMessage("  Key {0} found: {1}.", key_name, currentVersion);

					if (CheckRegistryKeyForExecutable(RegistryEx.LocalMachine, subkey + "\\" + currentVersion, "JavaHome", wow64, "bin", "jarsigner.exe"))
						return RegistryEx.GetValueString(RegistryEx.LocalMachine, subkey + "\\" + currentVersion, "JavaHome", wow64);
				}

				LogMessage("  Key {0} not found.", key_name);
			}

			// We ran out of things to check..
			return null;
		}

		public string GetAndroidSdkDirectory()
		{
			var roots = new[] { RegistryEx.CurrentUser, RegistryEx.LocalMachine };
			var wow = RegistryEx.Wow64.Key32;

			LogMessage("Looking for Android SDK..");

			// Check for the key written by the Android SDK installer first
			foreach (var root in roots)
				if (CheckRegistryKeyForExecutable(root, ANDROID_INSTALLER_PATH, ANDROID_INSTALLER_KEY, wow, "platform-tools", "adb.exe"))
					return RegistryEx.GetValueString(root, ANDROID_INSTALLER_PATH, ANDROID_INSTALLER_KEY, wow);

			// Check for the key the user gave us in the VS options page
			foreach (var root in roots)
				if (CheckRegistryKeyForExecutable(root, MDREG_KEY, MDREG_ANDROID, wow, "platform-tools", "adb.exe"))
					return RegistryEx.GetValueString(root, MDREG_KEY, MDREG_ANDROID, wow);

			// Check 2 default locations
			var program_files = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
			var installerLoc = Path.Combine(program_files, @"\Android\android-sdk-windows");
			var unzipLoc = @"C:\android-sdk-windows";

			if (ValidateAndroidSdkLocation(installerLoc))
			{
				LogMessage("  adb.exe found in {0}", installerLoc);
				return installerLoc;
			}

			if (ValidateAndroidSdkLocation(unzipLoc))
			{
				LogMessage("  adb.exe found in {0}", unzipLoc);
				return unzipLoc;
			}

			// We ran out of things to check..
			return null;
		}

		public string GetXamarinSdkDirectory()
		{
			// Find user's \Program Files (x86)
			var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

			// We keep our tools in:
			// <program files>\MSBuild\Xamarin
			return Path.Combine(programFilesX86, "MSBuild", "Xamarin");
		}

		public string GetJavaKeytoolPath()
		{
			return Path.Combine(GetJavaSdkDirectory(), "bin", "keytool.exe");
		}

		public string GetXamarinDebugKeystorePath()
		{
			return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), @"AppData\Local\Xamarin\Mono for Android\debug.keystore");
		}
		#endregion

		private static void LogMessage(string message)
		{
			Console.WriteLine(message);
		}
		private static void LogMessage(string message, params object[] args)
		{
			Console.WriteLine(message, args);
		}
		private static bool CheckRegistryKeyForExecutable(UIntPtr key, string subkey, string valueName, RegistryEx.Wow64 wow64, string subdir, string exe)
		{
			string key_name = string.Format(@"{0}\{1}\{2}", key == RegistryEx.CurrentUser ? "HKCU" : "HKLM", subkey, valueName);

			var path = NullIfEmpty(RegistryEx.GetValueString(key, subkey, valueName, wow64));

			if (path == null)
			{
				LogMessage("  Key {0} not found.", key_name);
				return false;
			}

			if (!File.Exists(Path.Combine(path, subdir, exe)))
			{
				LogMessage("  Key {0} found:\n    Path does not contain {1} in \\{2} ({3}).", key_name, exe, subdir, path);
				return false;
			}

			LogMessage("  Key {0} found:\n    Path contains {1} in \\{2} ({3}).", key_name, exe, subdir, path);

			return true;
		}

		static string NullIfEmpty(string s)
		{
			if (s == null || s.Length != 0)
				return s;
			return null;
		}

		public static bool ValidateAndroidSdkLocation(string loc)
		{
			return !string.IsNullOrEmpty(loc) && File.Exists(Path.Combine(Path.Combine(loc, "platform-tools"), "adb.exe"));
		}

		class RegistryEx
		{
			const string ADVAPI = "advapi32.dll";

			public static UIntPtr CurrentUser = (UIntPtr)0x80000001;
			public static UIntPtr LocalMachine = (UIntPtr)0x80000002;

			[DllImport(ADVAPI, CharSet = CharSet.Unicode, SetLastError = true)]
			static extern int RegOpenKeyEx(UIntPtr hKey, string subKey, uint reserved, uint sam, out UIntPtr phkResult);

			[DllImport(ADVAPI, CharSet = CharSet.Unicode, SetLastError = true)]
			static extern int RegQueryValueExW(UIntPtr hKey, string lpValueName, int lpReserved, out uint lpType,
			  StringBuilder lpData, ref uint lpcbData);

			[DllImport(ADVAPI, CharSet = CharSet.Unicode, SetLastError = true)]
			static extern int RegSetValueExW(UIntPtr hKey, string lpValueName, int lpReserved,
				uint dwType, string data, uint cbData);

			[DllImport(ADVAPI, CharSet = CharSet.Unicode, SetLastError = true)]
			static extern int RegSetValueExW(UIntPtr hKey, string lpValueName, int lpReserved,
				uint dwType, IntPtr data, uint cbData);

			[DllImport(ADVAPI, CharSet = CharSet.Unicode, SetLastError = true)]
			static extern int RegCreateKeyEx(UIntPtr hKey, string subKey, uint reserved, string @class, uint options,
				uint samDesired, IntPtr lpSecurityAttributes, out UIntPtr phkResult, out Disposition lpdwDisposition);

			[DllImport("advapi32.dll", SetLastError = true)]
			static extern int RegCloseKey(UIntPtr hKey);

			public static string GetValueString(UIntPtr key, string subkey, string valueName, Wow64 wow64)
			{
				UIntPtr regKeyHandle;
				uint sam = (uint)Rights.QueryValue + (uint)wow64;
				if (RegOpenKeyEx(key, subkey, 0, sam, out regKeyHandle) != 0)
					return null;

				try
				{
					uint type;
					var sb = new StringBuilder(2048);
					uint cbData = (uint)sb.Capacity;
					if (RegQueryValueExW(regKeyHandle, valueName, 0, out type, sb, ref cbData) == 0)
					{
						return sb.ToString();
					}
					return null;
				}
				finally
				{
					RegCloseKey(regKeyHandle);
				}
			}

			public static void SetValueString(UIntPtr key, string subkey, string valueName, string value, Wow64 wow64)
			{
				UIntPtr regKeyHandle;
				uint sam = (uint)(Rights.CreateSubKey | Rights.SetValue) + (uint)wow64;
				uint options = (uint)Options.NonVolatile;
				Disposition disposition;
				if (RegCreateKeyEx(key, subkey, 0, null, options, sam, IntPtr.Zero, out regKeyHandle, out disposition) != 0)
				{
					throw new Exception("Could not open or craete key");
				}

				try
				{
					uint type = (uint)ValueType.String;
					uint lenBytesPlusNull = ((uint)value.Length + 1) * 2;
					var result = RegSetValueExW(regKeyHandle, valueName, 0, type, value, lenBytesPlusNull);
					if (result != 0)
						throw new Exception(string.Format("Error {0} setting registry key '{1}{2}@{3}'='{4}'",
							result, key, subkey, valueName, value));
				}
				finally
				{
					RegCloseKey(regKeyHandle);
				}
			}

			[Flags]
			enum Rights : uint
			{
				None = 0,
				QueryValue = 0x0001,
				SetValue = 0x0002,
				CreateSubKey = 0x0004,
				EnumerateSubKey = 0x0008,
			}

			enum Options
			{
				BackupRestore = 0x00000004,
				CreateLink = 0x00000002,
				NonVolatile = 0x00000000,
				Volatile = 0x00000001,
			}

			public enum Wow64 : uint
			{
				Key64 = 0x0100,
				Key32 = 0x0200,
			}

			enum ValueType : uint
			{
				None = 0, //REG_NONE
				String = 1, //REG_SZ
				UnexpandedString = 2, //REG_EXPAND_SZ
				Binary = 3, //REG_BINARY
				DWord = 4, //REG_DWORD
				DWordLittleEndian = 4, //REG_DWORD_LITTLE_ENDIAN
				DWordBigEndian = 5, //REG_DWORD_BIG_ENDIAN
				Link = 6, //REG_LINK
				MultiString = 7, //REG_MULTI_SZ
				ResourceList = 8, //REG_RESOURCE_LIST
				FullResourceDescriptor = 9, //REG_FULL_RESOURCE_DESCRIPTOR
				ResourceRequirementsList = 10, //REG_RESOURCE_REQUIREMENTS_LIST
				QWord = 11, //REG_QWORD
				QWordLittleEndian = 11, //REG_QWORD_LITTLE_ENDIAN
			}

			enum Disposition : uint
			{
				CreatedNewKey = 0x00000001,
				OpenedExistingKey = 0x00000002,
			}
		}

	}
}
