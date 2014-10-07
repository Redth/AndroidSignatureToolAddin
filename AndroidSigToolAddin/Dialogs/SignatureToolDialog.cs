using System;
using MonoDevelop.Projects;
using MonoDevelop.Core.ProgressMonitoring;
using System.Threading.Tasks;
using System.Collections.Generic;
using Gdk;
using AndroidSignatureTool.Core;
using System.IO;
using Gtk;

namespace AndroidSigToolAddin.Dialogs
{
	public partial class SignatureToolDialog : Gtk.Dialog
	{		
        AndroidSignatureTool.Core.Helper helper;

		public SignatureToolDialog () 			
		{
            Build ();

            helper = new Helper ();

            FieldEditable ();

            entryKeytool.Text = helper.FindKeytool ();
		}

        protected void buttonSha1Copy_Clicked (object sender, EventArgs e)
        {
            var clipboard = GetClipboard (Gdk.Selection.Clipboard);
            clipboard.Text = entrySha1.Text;
        }

        protected void buttonMd5Copy_Clicked (object sender, EventArgs e)
        {
            var clipboard = GetClipboard (Gdk.Selection.Clipboard);
            clipboard.Text = entryMd5.Text;
        }

        protected void buttonGenerate_Clicked (object sender, EventArgs e)
        {
            SignatureInfo sig = null;

            var keytool = helper.FindKeytool ();

            if (string.IsNullOrEmpty (keytool)) {
                MsgBox ("Unable to locate keytool", "Java Keytool is needed to generate signatures.  We were unable to automatically locate keytool.  Please enter the location manually.");
                return;
            }

            if (radioDefault.Active) {

                try { 
                    sig = helper.GetSignaturesFromKeystore ();
                } catch (Exception ex) {
                    MsgBox ("Error Generating Signatures", ex.ToString ());
                }

            } else {

                if (!File.Exists (entryCustomKeystore.Text)) {
                    MsgBox ("Invalid .Keystore File", "The .keystore file you selected was invalid or not found");
                    return;
                }

                var keystore = entryCustomKeystore.Text;
                var alias = entryCustomAlias.Text;
                var storepass = entryCustomStorePass.Text;
                var keypass = entryCustomKeyPass.Text;

                try {
                    sig = helper.GetSignaturesFromKeystore (keytool, keystore, alias, storepass, keypass);
                } catch (Exception ex) {
                    MsgBox ("Error Generating Signatures", ex.ToString ());
                }
            }

            if (sig != null) {
                entryMd5.Text = sig.MD5;
                entrySha1.Text = sig.SHA1;
            }
        }

        protected void buttonBrowseKeystore_Clicked (object sender, EventArgs e)
        {
            OpenFile ("Choose a .keystore file", ".keystore", "*.keystore");
        }

        protected void buttonBrowseKeytool_Clicked (object sender, EventArgs e)
        {
            OpenFile ("Locate Java keytool", "keytool", "keytool");
        }

        protected void buttonCancel_Clicked (object sender, EventArgs e)
        {
            this.Destroy ();
        }


        void OpenFile (string title, string filterName, string filter)
        {
            var fc = new Gtk.FileChooserDialog (title,
                this, Gtk.FileChooserAction.Open, "Cancel", Gtk.ResponseType.Cancel, 
                "Open", Gtk.ResponseType.Accept);

            var f = new Gtk.FileFilter ();
            f.Name = filterName;
            f.AddPattern (filter);

            fc.AddFilter (f);

            if (fc.Run() == (int)Gtk.ResponseType.Accept) 
            {
                entryCustomKeystore.Text = fc.Filename;
            }
            fc.Destroy();
        }

        void MsgBox (string title, string body)
        {
            var md = new MessageDialog (this, 
                DialogFlags.DestroyWithParent, MessageType.Info, 
                ButtonsType.Close, body);
            md.Title = Title;
            md.Run();
            md.Destroy();
        }

        protected void radioDefault_Toggled (object sender, EventArgs e)
        {
            FieldEditable ();
        }

        void FieldEditable ()
        {
            var enabled = !radioDefault.Active;

            entryCustomAlias.IsEditable = enabled;
            entryCustomKeyPass.IsEditable = enabled;
            entryCustomKeystore.IsEditable = enabled;
            entryCustomStorePass.IsEditable = enabled;             
        }
	}
}

