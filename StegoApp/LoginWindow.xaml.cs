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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StegoApp
{

    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        void OnLoginButtonClick(object sender, RoutedEventArgs evArgs)
        {

            Login();

        }

        bool CheckCredentials(string username,string password)
        {

            if (username == "")
            {

                MessageBox.Show("Username field is empty");
                return false;

            }

            if (password == "")
            {

                MessageBox.Show("Password field is empty");
                return false;

            }

            return true;
        }

        void OnEnterPressed(object sender,KeyEventArgs evArgs)
        {

            if (evArgs.Key == Key.Enter)
                Login();

        }

        void Login()
        {

            bool validCred = CheckCredentials(usernameTextBox.Text, passwordTextBox.Password);

            if (!validCred)
                return;

            User user;
            var status = User.AuthenticateUser(usernameTextBox.Text, passwordTextBox.Password, out user);

            bool openMainWindow = (status == User.AuthStatus.Succesful);

            if (status == User.AuthStatus.InvalidUsername)
                MessageBox.Show("Invalid username", "Invalid username", MessageBoxButton.OK);
            else if (status == User.AuthStatus.InvalidPassword)
                MessageBox.Show("Invalid password", "Invalid password", MessageBoxButton.OK);


            if (openMainWindow)
            {

                MainWindow mainWin = new MainWindow(user);
                mainWin.Show();
                Close();

            }

        }

    }
}
