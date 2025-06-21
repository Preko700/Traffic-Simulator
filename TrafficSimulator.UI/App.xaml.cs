using System;
using System.Windows;

namespace TrafficSimulator.UI
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Configurar manejo global de excepciones
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Exception ex = (Exception)args.ExceptionObject;
                MessageBox.Show($"Se ha producido un error no controlado: {ex.Message}\n\n{ex.StackTrace}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            };

            // Crear y mostrar la ventana principal
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}