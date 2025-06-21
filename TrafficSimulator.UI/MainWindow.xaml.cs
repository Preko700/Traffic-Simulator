using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using TrafficSimulator.Core.Models;
using TrafficSimulator.UI.Controls;
using TrafficSimulator.UI.ViewModels;

namespace TrafficSimulator.UI
{
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel = null!;
        private GraphEditorCanvas _graphEditor = null!;
        private PropertiesPanel _propertiesPanel = null!;
        private DispatcherTimer _updateTimer = null!;

        public MainWindow()
        {
            InitializeComponent();

            // Inicializar el ViewModel
            _viewModel = new MainViewModel();
            DataContext = _viewModel;

            // Inicializar los controles
            InitializeControls();

            // Configurar los manejadores de eventos
            SetupEventHandlers();

            // Configurar timer para actualizar la UI
            _updateTimer = new DispatcherTimer();
            _updateTimer.Interval = TimeSpan.FromMilliseconds(100);
            _updateTimer.Tick += UpdateTimer_Tick;
            _updateTimer.Start();
        }

        private void InitializeControls()
        {
            // Crear el editor de grafos
            _graphEditor = new GraphEditorCanvas
            {
                ViewModel = _viewModel
            };
            graphEditorContainer.Content = _graphEditor;

            // Crear el panel de propiedades
            _propertiesPanel = new PropertiesPanel();
            propertiesPanelContainer.Content = _propertiesPanel;
        }

        private void SetupEventHandlers()
        {
            // Suscribirse a cambios en el ViewModel
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            _viewModel.Vehicles.CollectionChanged += (s, e) => UpdateStatusBar();
            _viewModel.TrafficGraph.PropertyChanged += (s, e) => UpdateStatusBar();

            // Manejadores para eventos del panel de propiedades
            _propertiesPanel.CityPropertiesChanged += (s, city) => _graphEditor.UpdateCity(city);
            _propertiesPanel.RoadPropertiesChanged += (s, road) =>
            {
                _graphEditor.UpdateRoad(road);
                _viewModel.RecalculateRoutes();
            };
            _propertiesPanel.DeleteElementRequested += (s, e) => DeleteSelectedElement();
            _propertiesPanel.SimulationSettingsChanged += (s, settings) => _viewModel.UpdateTrafficLoad();
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.IsSimulationRunning))
            {
                UpdateSimulationButtonsState();
                UpdateSimulationStatus();
            }
            else if (e.PropertyName == nameof(MainViewModel.SelectedElement))
            {
                _propertiesPanel.SelectedElement = _viewModel.SelectedElement;
            }
            else if (e.PropertyName == nameof(MainViewModel.SelectedVehicle))
            {
                UpdateSelectedVehicleInfo();
            }
        }

        private void UpdateTimer_Tick(object? sender, EventArgs e)
        {
            UpdateStatusBar();
            UpdateStatistics();
        }

        // Manejadores de eventos del menú
        private void MenuNew_Click(object sender, RoutedEventArgs e) => NewGraph();
        private void MenuOpen_Click(object sender, RoutedEventArgs e) => OpenGraph();
        private void MenuSave_Click(object sender, RoutedEventArgs e) => SaveGraph();
        private void MenuSaveAs_Click(object sender, RoutedEventArgs e) => SaveGraphAs();
        private void MenuExit_Click(object sender, RoutedEventArgs e) => Close();
        private void MenuDelete_Click(object sender, RoutedEventArgs e) => DeleteSelectedElement();
        private void MenuClear_Click(object sender, RoutedEventArgs e) => ClearAll();
        private void MenuGenerateVehicles_Click(object sender, RoutedEventArgs e) => _viewModel.GenerateVehicles();
        private void MenuStartSimulation_Click(object sender, RoutedEventArgs e) => _viewModel.StartSimulation();
        private void MenuPauseSimulation_Click(object sender, RoutedEventArgs e) => _viewModel.PauseSimulation();
        private void MenuStopSimulation_Click(object sender, RoutedEventArgs e) => _viewModel.StopSimulation();
        private void MenuCriticalPoints_Click(object sender, RoutedEventArgs e) => ShowCriticalPoints();
        private void MenuRecommendations_Click(object sender, RoutedEventArgs e) => ShowRecommendations();
        private void MenuAbout_Click(object sender, RoutedEventArgs e) => ShowAbout();

        // Manejadores de eventos de la barra de herramientas
        private void BtnNew_Click(object sender, RoutedEventArgs e) => NewGraph();
        private void BtnOpen_Click(object sender, RoutedEventArgs e) => OpenGraph();
        private void BtnSave_Click(object sender, RoutedEventArgs e) => SaveGraph();
        private void BtnGenerateVehicles_Click(object sender, RoutedEventArgs e) => _viewModel.GenerateVehicles();
        private void BtnStartSimulation_Click(object sender, RoutedEventArgs e) => _viewModel.StartSimulation();
        private void BtnPauseSimulation_Click(object sender, RoutedEventArgs e) => _viewModel.PauseSimulation();
        private void BtnStopSimulation_Click(object sender, RoutedEventArgs e) => _viewModel.StopSimulation();

        private void TglSelect_Click(object sender, RoutedEventArgs e) => SetEditorMode(EditorMode.Select);
        private void TglAddCity_Click(object sender, RoutedEventArgs e) => SetEditorMode(EditorMode.AddCity);
        private void TglAddRoad_Click(object sender, RoutedEventArgs e) => SetEditorMode(EditorMode.AddRoad);

        private void BtnShowCriticalPoints_Click(object sender, RoutedEventArgs e) => ShowCriticalPoints();

        private void SetEditorMode(EditorMode mode)
        {
            // Actualizar los botones toggle
            tglAddCity.IsChecked = mode == EditorMode.AddCity;
            tglAddRoad.IsChecked = mode == EditorMode.AddRoad;
            tglSelect.IsChecked = mode == EditorMode.Select;

            // Actualizar el modo en el ViewModel
            _viewModel.CurrentEditorMode = mode;

            // Actualizar el mensaje de estado
            switch (mode)
            {
                case EditorMode.AddCity:
                    txtStatus.Text = "Haga clic en el lienzo para agregar una ciudad";
                    break;
                case EditorMode.AddRoad:
                    txtStatus.Text = "Haga clic en dos ciudades para conectarlas con una carretera";
                    break;
                case EditorMode.Select:
                    txtStatus.Text = "Haga clic en elementos para seleccionarlos o arrastrarlos";
                    break;
            }
        }

        private void NewGraph()
        {
            if (MessageBox.Show("¿Está seguro de que desea crear un nuevo grafo? Se perderán los cambios no guardados.",
                "Nuevo grafo", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                _viewModel.NewGraph();
                _graphEditor.UpdateCanvasFromViewModel();
                UpdateStatusBar();
                txtStatus.Text = "Nuevo grafo creado";
            }
        }

        private void OpenGraph()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Archivos de Traffic Simulator (*.ts)|*.ts|Todos los archivos (*.*)|*.*",
                Title = "Abrir grafo"
            };

            if (dialog.ShowDialog() == true)
            {
                _viewModel.LoadGraph(dialog.FileName);
                _graphEditor.UpdateCanvasFromViewModel();
                UpdateStatusBar();
            }
        }

        private void SaveGraph()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Archivos de Traffic Simulator (*.ts)|*.ts|Todos los archivos (*.*)|*.*",
                Title = "Guardar grafo",
                DefaultExt = "ts"
            };

            if (dialog.ShowDialog() == true)
            {
                _viewModel.SaveGraph(dialog.FileName);
                txtStatus.Text = "Grafo guardado";
            }
        }

        private void SaveGraphAs()
        {
            SaveGraph();
        }

        private void DeleteSelectedElement()
        {
            // Implementación pendiente
            MessageBox.Show("Función no implementada", "Eliminar elemento", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ClearAll()
        {
            // Implementación pendiente
            MessageBox.Show("Función no implementada", "Limpiar todo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ShowCriticalPoints()
        {
            // Implementación pendiente
            MessageBox.Show("Función no implementada", "Puntos críticos", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ShowRecommendations()
        {
            // Implementación pendiente
            MessageBox.Show("Función no implementada", "Recomendaciones", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ShowAbout()
        {
            MessageBox.Show("Traffic Simulator\nVersión 1.0\n© 2023", "Acerca de", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void UpdateStatusBar()
        {
            // Actualizar los contadores individuales en la barra de estado
            txtCityCount.Text = _viewModel.TrafficGraph.Cities.Count.ToString();
            txtRoadCount.Text = _viewModel.TrafficGraph.Roads.Count.ToString();
            txtVehicleCount.Text = _viewModel.Vehicles.Count.ToString();
        }

        private void UpdateStatistics()
        {
            // Implementación pendiente
        }

        private void UpdateSimulationButtonsState()
        {
            bool isRunning = _viewModel.IsSimulationRunning;

            // Actualizar estado de botones en la barra de herramientas
            btnStartSimulation.IsEnabled = !isRunning;
            btnPauseSimulation.IsEnabled = isRunning;
            btnStopSimulation.IsEnabled = isRunning;

            // Actualizar estado de opciones de menú
            menuStartSimulation.IsEnabled = !isRunning;
            menuPauseSimulation.IsEnabled = isRunning;
            menuStopSimulation.IsEnabled = isRunning;
        }

        private void UpdateSimulationStatus()
        {
            // Implementación pendiente
        }

        private void UpdateSelectedVehicleInfo()
        {
            // Implementación pendiente
        }
    }
}
