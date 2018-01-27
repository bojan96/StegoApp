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
using static StegoApp.UnreadList;

namespace StegoApp
{

    public partial class MessageWindow : Window
    {

        public MessageWindow(User fromUser, string message, DateTime dateTime)
        {

            InitializeComponent();
            fromLabel.Content = fromUser.FullName;
            dateLabel.Content = dateTime.ToString();
            messageTextBlock.Text = message;

        }
        
    }

}