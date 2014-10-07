using System;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using Mono.TextEditor;
using System.Diagnostics;
using AndroidSigToolAddin.Dialogs;

namespace AndroidSigToolAddin
{
	public enum SignatureToolCommands
	{
		ShowDialog,
	}

	public class SignatureToolCommandHandler : CommandHandler
	{
		protected override void Run ()
		{
    		try
    		{
    			var dialog = new SignatureToolDialog ();
    			MessageService.ShowCustomDialog (dialog);
    		}
    		catch (Exception ex)
    		{
    			MessageService.ShowException (ex);
    		}
		}

		protected override void Update (CommandInfo info)
		{  
			info.Enabled = true; 
		}
	}
}

