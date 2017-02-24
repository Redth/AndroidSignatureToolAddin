using System;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui.Content;
using MonoDevelop.Projects;
using AndroidKeystoreSignatureTool.Dialogs;

namespace AndroidKeystoreSignatureTool
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
                MessageService.ShowError ("Error", ex);
    		}
		}

		protected override void Update (CommandInfo info)
		{  
			info.Enabled = true; 
		}
	}
}

