using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace StegoApp
{

    public partial class App : Application
    {

        void OnAppStart(object sender, StartupEventArgs args)
        {

            AppDomain.CurrentDomain.UnhandledException +=
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

        void OnUnhandledException(object sender, UnhandledExceptionEventArgs evArgs)
        {

            Exception ex = (Exception)evArgs.ExceptionObject;
            MessageBox.Show(ex.Message, "Unhandled exception", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();

        }

    }

}
