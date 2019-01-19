using System.Threading;
using System.Windows;

namespace Updater
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private PipeServer server;

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
        }

        private void CenterWindowOnScreen(Window window)
        {
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            double windowWidth = window.Width;
            double windowHeight = window.Height;
            window.Left = (screenWidth / 2) - (windowWidth / 2);
            window.Top = (screenHeight / 2) - (windowHeight / 2);
        }

        public MainWindow()
        {
            InitializeComponent();
            CenterWindowOnScreen(this);
            server = new PipeServer();
            var thread = new Thread(server.Work);
            thread.Start();
        }
    }
}
