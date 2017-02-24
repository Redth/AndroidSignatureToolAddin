using System;
using System.IO;

namespace AndroidKeystoreSignatureGenerator
{
	public class LocationHelperUnix : ILocations
	{
		#region ILocations Members
		public string GetJavaSdkDirectory()
		{
			// Check each directory in $PATH
			var path = Environment.GetEnvironmentVariable("PATH");
			var pathDirs = path.Split(new char[] { Path.PathSeparator }, StringSplitOptions.RemoveEmptyEntries);

			foreach (var dir in pathDirs)
				if (ValidateJavaSdkLocation(Path.GetDirectoryName(dir)))
					return Path.GetDirectoryName(dir);

			// We ran out of things to check..
			return null;
		}

		public string GetAndroidSdkDirectory()
		{
			// Check the environment variable override first
			var location = Environment.GetEnvironmentVariable("ANDROID_SDK_PATH");

			if (ValidateAndroidSdkLocation(location))
				return location;

			// Check each directory in $PATH
			var path = Environment.GetEnvironmentVariable("PATH");
			var pathDirs = path.Split(new char[] { Path.PathSeparator }, StringSplitOptions.RemoveEmptyEntries);

			foreach (var dir in pathDirs)
				if (ValidateAndroidSdkLocation(dir))
					return dir;

			// We ran out of things to check..
			return null;
		}

		public string GetXamarinSdkDirectory()
		{
			// Right now all we use this for is finding libxoid.so
			return Environment.GetEnvironmentVariable("MONO_ANDROID_PATH");
		}

		public string GetJavaKeytoolPath()
		{
			return Path.Combine(GetJavaSdkDirectory(), "bin", "keytool");
		}

		public string GetXamarinDebugKeystorePath()
		{
			return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local/share/Xamarin/Mono for Android/debug.keystore");
		}
		#endregion

		public static bool ValidateAndroidSdkLocation(string loc)
		{
			return !string.IsNullOrEmpty(loc) && File.Exists(Path.Combine(Path.Combine(loc, "platform-tools"), "adb"));
		}

		public static bool ValidateJavaSdkLocation(string loc)
		{
			return !string.IsNullOrEmpty(loc) && File.Exists(Path.Combine(Path.Combine(loc, "bin"), "jarsigner"));
		}
	}
}
