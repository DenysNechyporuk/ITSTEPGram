using System.Windows;
using System.Windows.Threading;

namespace ITSTEPGram
{
    public partial class App : Application
    {
        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Exception error = e.Exception;
            while (error.InnerException != null)
            {
                error = error.InnerException;
            }

            MessageBox.Show("Помилка: " + error.Message, "ITSTEPGram", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }
    }
}
