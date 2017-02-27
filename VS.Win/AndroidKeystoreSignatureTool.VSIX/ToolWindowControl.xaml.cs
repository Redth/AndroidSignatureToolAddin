//------------------------------------------------------------------------------
// <copyright file="ToolWindowControl.xaml.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace AndroidKeystoreSignatureTool.VSIX
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for ToolWindowControl.
    /// </summary>
    public partial class ToolWindowControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToolWindowControl"/> class.
        /// </summary>
        public ToolWindowControl()
        {
            viewModel = new ToolWindowViewModel();
            DataContext = viewModel;

            try
            {
                this.InitializeComponent();
            } catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        ToolWindowViewModel viewModel;
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            var radio = sender as RadioButton;
            var tag = radio.Tag?.ToString();

            var mode = "xamarin";
            if (tag == "0")
                mode = "custom";

            viewModel.CustomKeystore = mode == "custom";
            viewModel.KeystoreMode = mode;
        }
        
        private async void GenerateSignaturesButton_Click(object sender, RoutedEventArgs e)
        {
            buttonGenerate.IsEnabled = false;
            textGenerate.Visibility = Visibility.Collapsed;
            textGenerating.Visibility = Visibility.Visible;

            await viewModel.GenerateSignaturesAsync();

            textGenerating.Visibility = Visibility.Collapsed;
            textGenerate.Visibility = Visibility.Visible;
            buttonGenerate.IsEnabled = true;
        }
        
        private void BrowseKeytoolButton_Click(object sender, RoutedEventArgs e)
        {
            var file = OpenFile("keytool.exe", "Keytool.exe (keytool.exe)");

            if (!string.IsNullOrEmpty(file))
                viewModel.KeytoolPath = file;
        }

        private void BrowseKeystoreButton_Click(object sender, RoutedEventArgs e)
        {
            var file = OpenFile(".keystore", "Keystore Files (*.keystore)");

            if (!string.IsNullOrEmpty(file))
                viewModel.KeystorePath = file;
        }

        string OpenFile (string ext, string filter)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ext; // ".png";
            dlg.Filter = filter; // "JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif";

            var result = dlg.ShowDialog();

            if (result.HasValue && result.Value)
                return dlg.FileName;

            return null;
        }

        private void CopyFacebookSha1Button_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(textFbSha1.Text, TextDataFormat.Text);
        }

        private void CopySha1Button_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(textSha1.Text, TextDataFormat.Text);
        }

        private void CopySha256Button_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(textSha256.Text, TextDataFormat.Text);
        }

        private void CopyMd5Button_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(textMd5.Text, TextDataFormat.Text);
        }
    }
}