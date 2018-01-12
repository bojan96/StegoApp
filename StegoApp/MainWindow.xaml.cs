using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            var result = userSelectWin.ShowDialog();

            if (result == true)
                toTextBox.Text = userSelectWin.SelectedUser.Nickname;

        }

        void SelectImageClicked(object sender, RoutedEventArgs evArgs)
        {

            var fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Image files|*.png;*.bmp";

            var result = fileDialog.ShowDialog();

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

            if(currImagePath != imageTextBox.Text)
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

    }
}
