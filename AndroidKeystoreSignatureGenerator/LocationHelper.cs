using System;

namespace AndroidKeystoreSignatureGenerator
{
	public static class LocationHelper
	{
		private static ILocations locations;

		static LocationHelper()
		{
			if (PlatformDetection.IsWindows)
				locations = new LocationHelperWindows();
			else
				locations = new LocationHelperUnix();
		}

		public static string GetAndroidSdkDirectory()
		{
			return locations.GetAndroidSdkDirectory();
		}

		public static string GetJavaSdkDirectory()
		{
			return locations.GetJavaSdkDirectory();
		}

		public static string GetXamarinSdkDirectory()
		{
			return locations.GetXamarinSdkDirectory();
		}

		public static string GetJavaKeytoolPath()
		{
			return locations.GetJavaKeytoolPath();
		}

		public static string GetXamarinDebugKeystorePath()
		{
			return locations.GetXamarinDebugKeystorePath();
		}
	}

	public interface ILocations
	{
		string GetXamarinDebugKeystorePath();
		string GetJavaKeytoolPath();
		string GetAndroidSdkDirectory();
		string GetJavaSdkDirectory();
		string GetXamarinSdkDirectory();
	}
}
