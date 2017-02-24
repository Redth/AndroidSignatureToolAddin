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

        ILocations locationHelper;

        public void GenerateSignatures ()
        {
            IAndroidKeystoreSignatureGenerator generator;
            if (KeystoreMode == "xamarin" || string.IsNullOrEmpty(KeystorePath))
                generator = KeystoreSignatureGeneratorFactory.CreateForXamarinDebugKeystore(KeytoolPath);
            else
                generator = KeystoreSignatureGeneratorFactory.Create(KeytoolPath, KeystorePath, Alias, Storepass, Keypass);

            var signatures = generator.GenerateSignatures();
            Md5 = signatures?.Md5 ?? "";
            Sha1 = signatures?.Sha1 ?? "";
            Sha256 = signatures?.Sha256 ?? "";
            NotifyPropertyChanged(nameof(Md5));
            NotifyPropertyChanged(nameof(Sha1));
            NotifyPropertyChanged(nameof(Sha256));

            var fbSignatures = generator.GenerateFacebookSignatures();
            FacebookSha1 = fbSignatures?.Sha1;
            NotifyPropertyChanged(nameof(FacebookSha1));
        }
        
        public string Md5 { get; private set; }
        public string Sha1 { get; private set; }
        public string Sha256 { get; private set; }
        public string FacebookSha1 { get; private set; }

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

        public string KeystoreMode = "xamarin";

        public void NotifyPropertyChanged (string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
