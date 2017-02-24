using System;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

[assembly: System.Reflection.AssemblyKeyFileAttribute("androidkeystoresignaturegenerator.snk")]

namespace AndroidKeystoreSignatureGenerator
{
	public interface IAndroidKeystoreSignatureGenerator
	{
		string KeytoolPath { get; }
		string KeystorePath { get; }
		string KeystoreAlias { get; }
		string KeystoreStorepass { get; }
		string KeystoreKeypass { get; }

		KeystoreSignatures GenerateSignatures();
		FacebookKeystoreSignatures GenerateFacebookSignatures();
	}

	public static class KeystoreSignatureGeneratorFactory
	{
		const string XAMARIN_DEFAULT_KEYSTORE_DEBUG_ALIAS = "androiddebugkey";
		const string XAMARIN_DEFAULT_KEYSTORE_DEBUG_STOREPASS = "android";
		const string XAMARIN_DEFAULT_KEYSTORE_DEBUG_KEYPASS = "android";


		public static IAndroidKeystoreSignatureGenerator CreateForXamarinDebugKeystore(string keytoolPath)
		{
			if (string.IsNullOrEmpty(keytoolPath))
				keytoolPath = LocationHelper.GetJavaKeytoolPath();
			
			if (!File.Exists(keytoolPath))
				throw new FileNotFoundException("Failed to locate keytool", keytoolPath);

			var debugKeystore = LocationHelper.GetXamarinDebugKeystorePath();

			if (!File.Exists(debugKeystore))
				throw new FileNotFoundException("Failed to locate debug.keystore", debugKeystore);

			return new KeystoreSignatureGenerator(
				keytoolPath,
				debugKeystore,
				XAMARIN_DEFAULT_KEYSTORE_DEBUG_ALIAS,
				XAMARIN_DEFAULT_KEYSTORE_DEBUG_STOREPASS,
				XAMARIN_DEFAULT_KEYSTORE_DEBUG_KEYPASS);
		}

		public static IAndroidKeystoreSignatureGenerator Create(string keytoolPath, string keystorePath, string keystoreAlias, string keystoreStorepass = "", string keystoreKeypass = "")
		{
            if (string.IsNullOrEmpty(keytoolPath))
				keytoolPath = LocationHelper.GetJavaKeytoolPath();

			if (!File.Exists(keytoolPath))
				throw new FileNotFoundException("Failed to locate keytool", keytoolPath);

			if (!File.Exists(keystorePath))
				throw new FileNotFoundException("Failed to locate keystore", keystorePath);

			return new KeystoreSignatureGenerator(keytoolPath, keystorePath, keystoreAlias, keystoreStorepass, keystoreKeypass);
		}
	}

	class KeystoreSignatureGenerator : IAndroidKeystoreSignatureGenerator
	{
		public KeystoreSignatureGenerator(string keytoolPath, string keystorePath, string keystoreAlias, string keystoreStorepass, string keystoreKeypass)
		{
			KeytoolPath = keytoolPath;
			KeystorePath = keystorePath;
			KeystoreAlias = keystoreAlias;
			KeystoreStorepass = keystoreStorepass;
			KeystoreKeypass = keystoreKeypass;
		}

		public string KeytoolPath { get; private set; }
		public string KeystorePath { get; private set; }
		public string KeystoreAlias { get; private set; }
		public string KeystoreStorepass { get; private set; }
		public string KeystoreKeypass { get; private set; }

		public KeystoreSignatures GenerateSignatures ()
		{
			var result = new KeystoreSignatures();

			var output = RunProcess(KeytoolPath, string.Format("-list -v -keystore \"{0}\" -alias {1} -storepass {2} -keypass {3}", KeystorePath, KeystoreAlias, KeystoreStorepass, KeystoreKeypass));

			var rxMd5 = "MD5:\\s+(?<sig>[A-Za-z0-9:]+)";
			var rxSha1 = "SHA1:\\s+(?<sig>[A-Za-z0-9:]+)";
			var rxSha256 = "SHA256:\\s+(?<sig>[A-Za-z0-9:]+)";

			var md5 = Regex.Match(output, rxMd5, RegexOptions.Singleline | RegexOptions.IgnoreCase);
			var sha1 = Regex.Match(output, rxSha1, RegexOptions.Singleline | RegexOptions.IgnoreCase);
			var sha256 = Regex.Match(output, rxSha256, RegexOptions.Singleline | RegexOptions.IgnoreCase);


			if (md5 != null && md5.Success)
			{
				if (md5.Groups["sig"] != null && md5.Groups["sig"] != null && md5.Groups["sig"].Success)
					result.Md5 = md5.Groups["sig"].Value;
			}

			if (sha1 != null && sha1.Success)
			{
				if (sha1.Groups["sig"] != null && sha1.Groups["sig"] != null && sha1.Groups["sig"].Success)
					result.Sha1 = sha1.Groups["sig"].Value;
			}

			if (sha256 != null && sha256.Success)
			{
				if (sha256.Groups["sig"] != null && sha256.Groups["sig"] != null && sha256.Groups["sig"].Success)
					result.Sha256 = sha256.Groups["sig"].Value;
			}

			return result;
		}

		public FacebookKeystoreSignatures GenerateFacebookSignatures()
		{
			//keytool -exportcert -alias <RELEASE_KEY_ALIAS> -keystore <RELEASE_KEY_PATH> | openssl sha1 -binary | openssl base64

			// keytool -exportcert -alias my_key -keystore my.keystore -storepass PASSWORD > mycert.bin
			// openssl sha1 - binary mycert.bin > sha1.bin
			// openssl base64 -in sha1.bin -out base64.txt
			var result = new FacebookKeystoreSignatures();

			using (var certStream = new MemoryStream())
			using (var sha1Stream = new MemoryStream())
			{
				var args = string.Format("-exportcert -alias {0} -keystore \"{1}\" -storepass {2}", KeystoreAlias, KeystorePath, KeystoreStorepass);
				RunProcessBinary(KeytoolPath, args, null, certStream);
				certStream.Seek(0, SeekOrigin.Begin);

				if (certStream.Length > 0)
				{
					try
					{
						var sha1 = System.Security.Cryptography.SHA1.Create();
						var sha1Bytes = sha1.ComputeHash(certStream);
						result.Sha1 = Convert.ToBase64String(sha1Bytes);
					}
					catch { }

					try
					{
						var sha256 = System.Security.Cryptography.SHA256.Create();
						var sha256Bytes = sha256.ComputeHash(certStream);
						result.Sha256 = Convert.ToBase64String(sha256Bytes);
					}
					catch { }
				}
			}

			return result;
		}

		string RunProcess(string file, string args, Stream stdinStream = null)
		{
			var sbOut = new StringBuilder();

			var p = new System.Diagnostics.Process();
			p.StartInfo = new System.Diagnostics.ProcessStartInfo(file, args);
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.RedirectStandardOutput = true;
			if (stdinStream != null)
				p.StartInfo.RedirectStandardInput = true;
			p.StartInfo.CreateNoWindow = true;
			p.OutputDataReceived += (sender, e) =>
			{
				sbOut.Append(e.Data);
			};

			p.Start();
			p.BeginOutputReadLine();

			if (stdinStream != null)
			{
				stdinStream.CopyTo(p.StandardInput.BaseStream);
				p.StandardInput.Close();
			}

			p.WaitForExit();

			return sbOut.ToString();
		}

		void RunProcessBinary(string file, string args, Stream stdinStream, Stream stdoutStream)
		{
			var p = new System.Diagnostics.Process();
			p.StartInfo = new System.Diagnostics.ProcessStartInfo(file, args);
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.RedirectStandardOutput = true;
			if (stdinStream != null)
				p.StartInfo.RedirectStandardInput = true;
			p.StartInfo.CreateNoWindow = true;
			p.Start();

			if (stdinStream != null)
			{
				stdinStream.CopyTo(p.StandardInput.BaseStream);
				p.StandardInput.Close();
			}

			p.StandardOutput.BaseStream.CopyTo(stdoutStream);
			p.WaitForExit();
		}
	}

	public class KeystoreSignatures
	{
		public string Md5 { get; set; }
		public string Sha1 { get; set; }
		public string Sha256 { get; set; }
	}

	public class FacebookKeystoreSignatures
	{
		public string Sha1 { get; set; }
		public string Sha256 { get; set; }
	}
}
