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

namespace mybackuptoftp
{
    /// <summary>
    /// Lógica de interacción para WindowPassword.xaml
    /// </summary>
    public partial class WindowPassword : Window
    {
        private PageConfig pageConfig;

        public WindowPassword(PageConfig pg)
        {
            InitializeComponent();
            this.pageConfig = pg;
            Closed += WindowPassword_Closed;
            this.btAccept.Click += BtAccept_Click;
            this.btClose.Click += BtClose_Click;
        }

        private void BtClose_Click(object sender, RoutedEventArgs e)
        {
            this.pageConfig.IsEnabled = true;
            this.Close();
        }

        private void BtAccept_Click(object sender, RoutedEventArgs e)
        {
            if(!string.IsNullOrWhiteSpace(pbPassword.Password))
            {
                if (pbPassword.Password.Equals("myPassword"))
                {
                    PageConfig.continuePassword = true;
                }
                else
                {
                    PageConfig.continuePassword = false;
                }
                this.Close();
                PageConfig.pageConfig.BtInit_Click(null, null);
            }
        }

        private void WindowPassword_Closed(object sender, EventArgs e)
        {
            if (PageConfig.continuePassword)
            {
                PageConfig.showWindowPassword = false;
            }
            else
            {
                PageConfig.showWindowPassword = true;
            }
        }
    }
}
