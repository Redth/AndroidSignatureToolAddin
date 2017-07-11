using AndroidKeystoreSignatureGenerator;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AndroidKeystoreSignatureTool.VSIX
{
    public class ToolWindowViewModel : INotifyPropertyChanged
    {
        public ToolWindowViewModel()
        {
            try
            {
                keytoolPath = LocationHelper.GetJavaKeytoolPath();
            } catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }
        
        public Task GenerateSignaturesAsync ()
        {
            return Task.Run(() => {
               try
               {
                   IAndroidKeystoreSignatureGenerator generator;
                   if (KeystoreMode == "xamarin") // || string.IsNullOrEmpty(KeystorePath))
                       generator = KeystoreSignatureGeneratorFactory.CreateForXamarinDebugKeystore(KeytoolPath);
                   else
                       generator = KeystoreSignatureGeneratorFactory.Create(KeytoolPath, KeystorePath, Alias, Storepass, Keypass);

                   var signatures = generator.GenerateSignatures();
                   Md5 = signatures?.Md5 ?? string.Empty;
                   Sha1 = signatures?.Sha1 ?? string.Empty;
                   Sha256 = signatures?.Sha256 ?? string.Empty;

                   var fbSignatures = generator.GenerateFacebookSignatures();
                   FacebookSha1 = fbSignatures?.Sha1 ?? string.Empty;
               }
               catch (Exception ex)
               {
                   Console.WriteLine("Failed to generate signature: {0}", ex);
               }
            });
        }

        string md5 = string.Empty;
        public string Md5
        {
            get { return md5; }
            set { md5 = value; NotifyPropertyChanged(nameof(Md5)); }
        }

        string sha1 = string.Empty;
        public string Sha1
        {
            get { return sha1; }
            set { sha1 = value; NotifyPropertyChanged(nameof(Sha1)); }
        }

        string sha256 = string.Empty;
        public string Sha256
        {
            get { return sha256; }
            set { sha256 = value; NotifyPropertyChanged(nameof(Sha256)); }
        }

        string facebookSha1 = string.Empty;
        public string FacebookSha1
        {
            get { return facebookSha1; }
            set { facebookSha1 = value; NotifyPropertyChanged(nameof(FacebookSha1)); }
        }

        bool customKeystore = false;
        public bool CustomKeystore
        {
            get { return customKeystore; }
            set { customKeystore = value;  NotifyPropertyChanged(nameof(CustomKeystore)); }
        }

        string alias = null;
        public string Alias
        {
            get { return alias; }
            set { alias = value; NotifyPropertyChanged(nameof(Alias)); }
        }

        string storepass = null;
        public string Storepass
        {
            get { return storepass; }
            set { storepass = value; NotifyPropertyChanged(nameof(Storepass)); }
        }

        string keypass = null;
        public string Keypass
        {
            get { return keypass; }
            set { keypass = value;  NotifyPropertyChanged(nameof(Keypass)); }
        }

        string keytoolPath = null;
        public string KeytoolPath
        {
            get { return keytoolPath; }
            set { keytoolPath = value; NotifyPropertyChanged(nameof(KeytoolPath)); }
        }

        string keystorePath = null;
        public string KeystorePath
        {
            get { return keystorePath; }
            set { keystorePath = value; NotifyPropertyChanged(nameof(KeystorePath)); }
        }

        string keystoreMode = "xamarin";
        public string KeystoreMode
        {
            get { return keystoreMode; }
            set { keystoreMode = value; NotifyPropertyChanged(nameof(KeystoreMode)); }
        }

        public void NotifyPropertyChanged (string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
