using System;
using MonoDevelop.Projects;
using MonoDevelop.Core.ProgressMonitoring;
using System.Threading.Tasks;
using System.Collections.Generic;
using Gdk;
using AndroidKeystoreSignatureGenerator;
using System.IO;
using Gtk;

namespace AndroidKeystoreSignatureTool.Dialogs
{
	public partial class SignatureToolDialog : Gtk.Dialog
	{
		public SignatureToolDialog()
		{
			Build();

			FieldEditable();

			entryKeytool.Text = LocationHelper.GetJavaKeytoolPath();
		}

		protected void buttonSha1Copy_Clicked(object sender, EventArgs e)
		{
			var clipboard = GetClipboard(Gdk.Selection.Clipboard);
			clipboard.Text = entrySha1.Text;
		}

		protected void buttonMd5Copy_Clicked(object sender, EventArgs e)
		{
			var clipboard = GetClipboard(Gdk.Selection.Clipboard);
			clipboard.Text = entryMd5.Text;
		}

		protected void buttonCopySha256_Clicked(System.Object sender, System.EventArgs e)
		{
			var clipboard = GetClipboard(Gdk.Selection.Clipboard);
			clipboard.Text = entrySha256.Text;
		}

		protected void buttonCopyFacebookSHA1_Clicked(System.Object sender, System.EventArgs e)
		{
			var clipboard = GetClipboard(Gdk.Selection.Clipboard);
			clipboard.Text = entryFacebookSHA1.Text;
		}

		protected void buttonGenerate_Clicked(object sender, EventArgs e)
		{
			var keytoolPath = entryKeytool.Text;

			if (string.IsNullOrEmpty(keytoolPath))
			{
				MsgBox("Unable to locate keytool", "Java Keytool is needed to generate signatures.  We were unable to automatically locate keytool.  Please enter the location manually.");
				return;
			}

			IAndroidKeystoreSignatureGenerator generator;

			if (radioDefault.Active)
			{

				generator = KeystoreSignatureGeneratorFactory.CreateForXamarinDebugKeystore(keytoolPath);

			}
			else
			{

				if (!File.Exists(entryCustomKeystore.Text))
				{
					MsgBox("Invalid .Keystore File", "The .keystore file you selected was invalid or not found");
					return;
				}

				var keystore = entryCustomKeystore.Text;
				var alias = entryCustomAlias.Text;
				var storepass = entryCustomStorePass.Text;
				var keypass = entryCustomKeyPass.Text;

				generator = KeystoreSignatureGeneratorFactory.Create(keytoolPath, keystore, alias, storepass, keypass);
			}

			try
			{
				var sigs = generator.GenerateSignatures();
				entryMd5.Text = sigs.Md5;
				entrySha1.Text = sigs.Sha1;
				entrySha256.Text = sigs.Sha256;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				MsgBox("Error Generating Signatures", "Please make sure you have the correct Keystore alias, store pass, and keypass values!");
			}

			try
			{
				var fbSigs = generator.GenerateFacebookSignatures();
				entryFacebookSHA1.Text = fbSigs.Sha1;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				MsgBox("Error Generating Signatures", "Please make sure you have the correct Keystore alias, store pass, and keypass values!");
			}
		}

		protected void buttonBrowseKeystore_Clicked(object sender, EventArgs e)
		{
			OpenFile("Choose a .keystore file", ".keystore", "*.keystore");
		}

		protected void buttonBrowseKeytool_Clicked(object sender, EventArgs e)
		{
			var pattern = "keytool.exe";
			if (PlatformDetection.IsMac || PlatformDetection.IsLinux)
				pattern = "keytool";

			OpenFile("Java keytool Location", "keytool", pattern);
		}

		protected void buttonCancel_Clicked(object sender, EventArgs e)
		{
			this.Destroy();
		}


		void OpenFile(string title, string filterName, string filter)
		{
			var fc = new Gtk.FileChooserDialog(title,
				this, Gtk.FileChooserAction.Open, "Cancel", Gtk.ResponseType.Cancel,
				"Open", Gtk.ResponseType.Accept);

			var f = new Gtk.FileFilter();
			f.Name = filterName;
			f.AddPattern(filter);

			fc.AddFilter(f);

			if (fc.Run() == (int)Gtk.ResponseType.Accept)
			{
				if (filterName == ".keystore")
				{
					entryCustomKeystore.Text = fc.Filename;
				}
				else if (filterName == "keytool")
				{
					entryKeytool.Text = fc.Filename;
				}
			}
			fc.Destroy();
		}

		void MsgBox(string title, string body)
		{
			var md = new MessageDialog(this,
				DialogFlags.DestroyWithParent, MessageType.Info,
				ButtonsType.Close, body);
			md.Title = Title;
			md.Run();
			md.Destroy();
		}

		protected void radioDefault_Toggled(object sender, EventArgs e)
		{
			FieldEditable();
		}

		void FieldEditable()
		{
			var enabled = !radioDefault.Active;

			entryCustomAlias.IsEditable = enabled;
			entryCustomKeyPass.IsEditable = enabled;
			entryCustomKeystore.IsEditable = enabled;
			entryCustomStorePass.IsEditable = enabled;
		}
	}
}

