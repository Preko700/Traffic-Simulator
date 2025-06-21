using System;
using System.Windows;
using System.Windows.Controls;
using TrafficSimulator.Core.Models;
using System.Drawing;

namespace TrafficSimulator.UI.Controls
{
    public partial class PropertiesPanel : UserControl
    {
        private object _selectedElement;

        public object SelectedElement
        {
            get => _selectedElement;
            set
            {
                _selectedElement = value;
                UpdatePanelVisibility();
                UpdatePropertyValues();
            }
        }

        public event EventHandler<City> CityPropertiesChanged;
        public event EventHandler<Road> RoadPropertiesChanged;
        public event EventHandler<SimulationSettings> SimulationSettingsChanged;
        public event EventHandler DeleteElementRequested;

        public PropertiesPanel()
        {
            InitializeComponent();
            SetupEventHandlers();
        }

        private void SetupEventHandlers()
        {
            // Manejadores de eventos para las propiedades de la ciudad
            TxtCityName.LostFocus += (s, e) => UpdateCityProperty();
            TxtCityPositionX.LostFocus += (s, e) => UpdateCityProperty();
            TxtCityPositionY.LostFocus += (s, e) => UpdateCityProperty();
            SliderCityTrafficFactor.ValueChanged += (s, e) => UpdateCityProperty();

            // Manejadores de eventos para las propiedades de la carretera
            TxtRoadName.LostFocus += (s, e) => UpdateRoadProperty();
            SliderRoadTrafficLoad.ValueChanged += (s, e) => UpdateRoadProperty();
            ChkRoadBlocked.Checked += (s, e) => UpdateRoadProperty();
            ChkRoadBlocked.Unchecked += (s, e) => UpdateRoadProperty();
            TxtRoadBlockReason.LostFocus += (s, e) => UpdateRoadProperty();

            // Manejadores de eventos para los ajustes de simulación
            SliderSimulationSpeed.ValueChanged += (s, e) => UpdateSimulationSettings();
            SliderVehicleCount.ValueChanged += (s, e) => UpdateSimulationSettings();
            SliderTrafficFactor.ValueChanged += (s, e) => UpdateSimulationSettings();
        }

        private void UpdatePanelVisibility()
        {
            GeneralPropertiesPanel.Visibility = Visibility.Collapsed;
            CityPropertiesPanel.Visibility = Visibility.Collapsed;
            RoadPropertiesPanel.Visibility = Visibility.Collapsed;

            if (_selectedElement == null)
            {
                GeneralPropertiesPanel.Visibility = Visibility.Visible;
            }
            else if (_selectedElement is City)
            {
                CityPropertiesPanel.Visibility = Visibility.Visible;
            }
            else if (_selectedElement is Road)
            {
                RoadPropertiesPanel.Visibility = Visibility.Visible;
            }
        }

        private void UpdatePropertyValues()
        {
            if (_selectedElement is City city)
            {
                TxtCityName.Text = city.Name;
                TxtCityPositionX.Text = city.Position.X.ToString("F2");
                TxtCityPositionY.Text = city.Position.Y.ToString("F2");
                SliderCityTrafficFactor.Value = city.TrafficFactor;
            }
            else if (_selectedElement is Road road)
            {
                TxtRoadName.Text = road.Name;
                TxtRoadSource.Text = road.SourceCity.Name;
                TxtRoadDestination.Text = road.DestinationCity.Name;
                TxtRoadDistance.Text = road.Distance.ToString("F2");
                SliderRoadTrafficLoad.Value = road.TrafficLoad;
                ChkRoadBlocked.IsChecked = road.IsBlocked;
                TxtRoadBlockReason.Text = road.BlockReason;
            }
            else if (_selectedElement is SimulationSettings settings)
            {
                SliderSimulationSpeed.Value = settings.SimulationSpeed;
                SliderVehicleCount.Value = settings.VehicleCount;
                SliderTrafficFactor.Value = settings.GlobalTrafficFactor;
            }
        }

        private void UpdateCityProperty()
        {
            if (!(_selectedElement is City city)) return;

            // Actualizar nombre
            city.Name = TxtCityName.Text;

            // Actualizar posición si los valores son válidos
            if (double.TryParse(TxtCityPositionX.Text, out double posX) &&
                double.TryParse(TxtCityPositionY.Text, out double posY))
            {
                city.Position = new System.Drawing.Point((int)posX, (int)posY); // Explicit conversion
            }

            // Actualizar factor de tráfico
            city.TrafficFactor = SliderCityTrafficFactor.Value;

            // Notificar que las propiedades han cambiado
            CityPropertiesChanged?.Invoke(this, city);
        }


        private void UpdateRoadProperty()
        {
            if (!(_selectedElement is Road road)) return;

            // Actualizar nombre
            road.Name = TxtRoadName.Text;

            // Actualizar carga de tráfico
            road.TrafficLoad = SliderRoadTrafficLoad.Value;

            // Actualizar estado de bloqueo
            road.IsBlocked = ChkRoadBlocked.IsChecked ?? false;
            road.BlockReason = road.IsBlocked ? TxtRoadBlockReason.Text : string.Empty;

            // Notificar que las propiedades han cambiado
            RoadPropertiesChanged?.Invoke(this, road);
        }

        private void UpdateSimulationSettings()
        {
            if (!(_selectedElement is SimulationSettings settings)) return;

            settings.SimulationSpeed = SliderSimulationSpeed.Value;
            settings.VehicleCount = (int)SliderVehicleCount.Value;
            settings.GlobalTrafficFactor = SliderTrafficFactor.Value;

            // Notificar que las configuraciones han cambiado
            SimulationSettingsChanged?.Invoke(this, settings);
        }

        private void RequestDeleteElement()
        {
            DeleteElementRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}