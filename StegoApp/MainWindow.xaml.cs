using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
                imageTextBox.Text = fileDialog.FileName;


        }

    }
}
