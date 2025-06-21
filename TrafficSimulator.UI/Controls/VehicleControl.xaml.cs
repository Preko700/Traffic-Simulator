using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using TrafficSimulator.Core.Models;

namespace TrafficSimulator.UI.Controls
{
    public partial class VehicleControl : UserControl
    {
        private Vehicle? _vehicle;
        private Ellipse _vehicleShape;
        private bool _isSelected;

        public VehicleControl()
        {
            _vehicleShape = new Ellipse
            {
                Width = 8,
                Height = 8,
                Fill = Brushes.Red,
                Stroke = Brushes.DarkRed,
                StrokeThickness = 1
            };

            Content = _vehicleShape;
            Width = 8;  // Match the shape size  
            Height = 8; // Match the shape size  
        }

        public void SetVehicle(Vehicle vehicle)
        {
            _vehicle = vehicle;
            UpdateVisual();
        }

        public Vehicle? GetVehicle()
        {
            return _vehicle;
        }

        public void SetIsSelected(bool isSelected)
        {
            _isSelected = isSelected;
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            if (_isSelected)
            {
                _vehicleShape.Fill = Brushes.Yellow;
                _vehicleShape.Stroke = Brushes.Orange;
                _vehicleShape.StrokeThickness = 2;
                _vehicleShape.Width = 12;
                _vehicleShape.Height = 12;
                Width = 12;
                Height = 12;
            }
            else
            {
                _vehicleShape.Fill = Brushes.Red;
                _vehicleShape.Stroke = Brushes.DarkRed;
                _vehicleShape.StrokeThickness = 1;
                _vehicleShape.Width = 8;
                _vehicleShape.Height = 8;
                Width = 8;
                Height = 8;
            }
        }
    }
}
