using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Security.Cryptography.X509Certificates;
using static StegoApp.UnreadList;
using System.Collections.ObjectModel;

namespace StegoApp
{

    public partial class MainWindow : Window
    {

        User currentUser;
        string currImagePath = "";
        UnreadList unreadList;
        ObservableCollection<Record> recordCollection;

        public MainWindow(User user)
        {

            InitializeComponent();
            currentUser = user;
            unreadList = new UnreadList(currentUser.UnreadFile);
            msgListView.ItemsSource = recordCollection =
                new ObservableCollection<Record>(unreadList.Messages.Values);

        }

        void SelectUserClicked(object sender, RoutedEventArgs evArgs)
        {

            var userSelectWin = new UserSelectWindow(currentUser);
            bool? result = userSelectWin.ShowDialog();

            if (result == true)
                toTextBox.Text = userSelectWin.SelectedUser.Nickname;

        }

        void SelectImageClicked(object sender, RoutedEventArgs evArgs)
        {

            var fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Image files|*.png;*.bmp";

            bool? result = fileDialog.ShowDialog();

            if (result == true)
                currImagePath = imageTextBox.Text = fileDialog.FileName;

        }

        void OnImageTextBoxTextChanged(object sender, TextChangedEventArgs evArgs)
        {

            if (imageTextBox.IsKeyboardFocused)
                return;

            ChangeImage();

        }

        // Keyboard lost focus
        void OnImageTextBoxLostFocus(object sender, RoutedEventArgs evArgs)
        {

            if (currImagePath != imageTextBox.Text)
                ChangeImage();

        }

        // Keyboard got focus
        void OnImageTextBoxGotFocus(object sender, RoutedEventArgs evArgs)
        {

            currImagePath = imageTextBox.Text;

        }

        void ChangeImage()
        {

            try
            {

                if (!Steganography.ValidImageFormat(imageTextBox.Text))
                {

                    SetPlaceholderImage();
                    ExclamationMsgBox("Format not supported", "Format not supported");

                    return;

                }

                var imageUri = new Uri(imageTextBox.Text);
                var image = new BitmapImage();
                image.BeginInit();
                image.UriSource = imageUri;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                imageControl.Source = image;

            }
            catch (IOException)
            {

                SetPlaceholderImage();
                MessageBox.Show("Could not open file", "Could not open file",
                    MessageBoxButton.OK, MessageBoxImage.Error);

            }

        }

        void SetPlaceholderImage() => 
            imageControl.Source = new BitmapImage(new Uri("Placeholder.bmp", UriKind.Relative));
    
        void OnPostMsgButtonClick(object sender, RoutedEventArgs e)
        {

            User toUser;
            bool queryResult = User.AllUsers.TryGetValue(toTextBox.Text, out toUser);

            if(!queryResult)
            {

                ExclamationMsgBox("User does not exist", "User does not exist");
                return;

            }

            using (X509Certificate2 toUserCert = CryptoService.FindCertificate(toUser))
            {

                if (toUserCert == null)
                {

                    ExclamationMsgBox($"Can not find certificate for user: \"{toUser.FullName}\"",
                        "Can not find certificate");
                    return;

                }

                using (RSA toUserPublicKey = toUserCert.GetRSAPublicKey())
                {

                    // No rsa public key or cert not valid
                    if (toUserPublicKey == null || !CryptoService.ValidateCertificate(toUserCert))
                    {

                        ExclamationMsgBox($"Certificate not valid for user: \"{toUser.FullName}\"",
                            "Certificate not valid");
                        return;

                    }

                    byte[] signature;
                    byte[] data;

                    try
                    {

                        using (RSA userPrivateKey = PrivateKeyDialog())
                        {
                            //Null means we cancel
                            if (userPrivateKey == null)
                                return;

                            string formattedMsg = Message.MakeXml(currentUser, msgTextBox.Text);
                            data = Encoding.UTF8.GetBytes(formattedMsg);
                            signature = CryptoService.SignData(data, userPrivateKey);

                        }

                    }
                    catch (FileFormatException)
                    {

                        ExclamationMsgBox("Invalid pem file", "Invalid pem file");
                        return;

                    }

                    byte[] symmKey = CryptoService.GenerateSymmetricKey();
                    byte[] iV = CryptoService.GenerateIV();
                    byte[] envelope = CryptoService.EncryptSymmetricData(symmKey, iV, toUserPublicKey);

                    byte[] dataAndSign = Utility.CombineByteArrays(data, signature);
                    byte[] encData = CryptoService.EncryptData(dataAndSign, symmKey, iV);
                    // Zero out the symmetric key
                    Array.Clear(symmKey, 0, symmKey.Length);
                    byte[] payload = Utility.CombineByteArrays(envelope, encData);
                    string imagePath = imageTextBox.Text;
                    try
                    {

                        Steganography.Embed(imagePath, imagePath, payload);

                    }
                    //TODO: Implement separate exception for small image
                    catch (FileFormatException)
                    {

                        MessageBox.Show("Image is too small for given message", "Small image",
                            MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        return;
                    }

                    string hash = CryptoService.HashFile(imagePath);
                    UnreadList list = new UnreadList(toUser.UnreadFile);
                    list.Add(imagePath, hash);
                    list.Write(toUser.UnreadFile);

                    MessageBox.Show("Message post succesful", "Success", MessageBoxButton.OK);

                }

            }
        }

        void OnMsgTxtBoxTextChanged(object sender, TextChangedEventArgs evArgs)
        {
            //EnablePostMsgButton();
        }

        void ExclamationMsgBox(string text, string caption)
        {
            MessageBox.Show(text, caption, MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        RSA PrivateKeyDialog()
        {

            var fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Pem file|*.pem";
            bool? result = fileDialog.ShowDialog();

            return result == true ? CryptoService.ReadPemFile(fileDialog.FileName) : null;

        }

        void OnItemDoubleClick(object sender, RoutedEventArgs evARgs)
        {

            var msgRecord = (Record)msgListView.SelectedItem;

            bool exist = File.Exists(msgRecord.Path);

            if (!exist)
            {

                ExclamationMsgBox("Message does not exist", "Message does not exist");
                RemoveMessage(msgRecord);
                return;

            }

            bool hashResult = CryptoService.HashValidation(msgRecord.Path, msgRecord.Hash);

            if (!hashResult)
            {

                ExclamationMsgBox("Message altered", "Message altered");
                RemoveMessage(msgRecord);
                return;

            }

            byte[] extractedData = Steganography.Extract(msgRecord.Path);
            (byte[] envelope, byte[] encData) = Utility.SplitArray(extractedData);
            byte[] symmKey;
            byte[] iV;

            try
            {

                using (RSA userPrivateKey = PrivateKeyDialog())
                {

                    if (userPrivateKey == null)
                        return;

                    (symmKey, iV) = CryptoService.DecryptSymmetricData(envelope, userPrivateKey);
                }

            }
            catch (FileFormatException)
            {

                // TODO: avoid code duplication
                // TODO: make exception class for this purpose
                ExclamationMsgBox("Invalid pem file", "Invalid pem file");
                return;

            }
            catch (CryptographicException)
            {

                ExclamationMsgBox("Decryption error", "Decryption error");
                return;

            }

            byte[] decData = CryptoService.DecryptData(encData, symmKey, iV);

            (byte[] msgBytes, byte[] signature) = Utility.SplitArray(decData);
            (string message, User fromUser, DateTime dateTime) =
                Message.ParseXml(Encoding.UTF8.GetString(msgBytes));

            RSA fromUserPublicKey = CryptoService.FindCertificate(fromUser).GetRSAPublicKey();
            bool verificationResult = CryptoService.VerifyData(msgBytes, signature, fromUserPublicKey);

            if (!verificationResult)
            {

                ExclamationMsgBox("Signature verification failed", "Signature verification failed");
                return;

            }

            RemoveMessage(msgRecord);
            ShowMessageWindow(message, fromUser, dateTime);  

        }

        void ShowMessageWindow(string message, User fromUser, DateTime dateTime)
        {

            var messageWin = new MessageWindow(fromUser, message, dateTime);
            messageWin.ShowDialog();

        }

        void RemoveMessage(Record msgRecord)
        {

            unreadList.Remove(msgRecord.Path);
            unreadList.Write(currentUser.UnreadFile);
            recordCollection.Remove(msgRecord);
            File.Delete(msgRecord.Path);

        }

        void EnablePostMsgButton()
        {

           /* postMsgButton.IsEnabled = msgTextBox.Text.Length > 0 && imageTextBox.Text.Length > 0 
                && toTextBox.Text.Length > 0;*/

        }

        void OnToUserTextBoxChange(object sender, TextChangedEventArgs evArgs)
        {

            //EnablePostMsgButton();

        }

    }

}