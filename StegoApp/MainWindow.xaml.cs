﻿using Microsoft.Win32;
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

namespace StegoApp
{

    public partial class MainWindow : Window
    {

        User currentUser;
        string currImagePath = "";

        public MainWindow(User user)
        {

            InitializeComponent();
            currentUser = user;

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
                    MessageBox.Show("Format not supported", "Format not supported",
                        MessageBoxButton.OK, MessageBoxImage.Exclamation);

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

        void SetPlaceholderImage()
        {
            imageControl.Source = new BitmapImage(new Uri("Placeholder.bmp", UriKind.Relative));
        }

        void OnPostMsgButtonClick(object sender, RoutedEventArgs e)
        {
            

            User toUser = User.AllUsers[toTextBox.Text];

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

                        MessageBox.Show("Invalid pem file", "Invalid pem file",
                            MessageBoxButton.OK, MessageBoxImage.Exclamation);

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
                    catch(FileFormatException)
                    {

                        MessageBox.Show("Image is too small for given message", "Small image", 
                            MessageBoxButton.OK, MessageBoxImage.Exclamation);

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
            postMsgButton.IsEnabled = msgTextBox.Text.Length > 0;
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

    }

}