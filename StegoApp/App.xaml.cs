using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace StegoApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        void OnAppStart(object sender,StartupEventArgs args)
        {

            try
            {

                User.LoadUsers();
                LoginWindow win = new LoginWindow();
                win.Show();

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();

            }
            

        }
    }
}
