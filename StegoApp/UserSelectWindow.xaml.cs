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

    public partial class UserSelectWindow : Window
    {

        private User currentUser;

        public UserSelectWindow(User currentUser)
        {

            this.currentUser = currentUser;
            InitializeComponent();
            
        }

        public User SelectedUser
        {

            get;
            private set;

        }

        void FilterUsers(object sender, FilterEventArgs evArgs)
        {

            evArgs.Accepted = !currentUser.Equals(evArgs.Item);

        }

        void CancelClicked(object sender, RoutedEventArgs evArgs)
        {

            ExitDialog(false);

        }

        void OKClicked(object sender, RoutedEventArgs evArgs)
        {

            SelectedUser = (User)listBox.SelectedItem;
            ExitDialog(true);

        }

        void ExitDialog(bool result)
        {

            DialogResult = true;
            Close();

        }

    }
}
