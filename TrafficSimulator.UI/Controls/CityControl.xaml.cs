using System.Windows.Controls;
using TrafficSimulator.Core.Models;

namespace TrafficSimulator.UI.Controls
{
    public partial class CityControl : UserControl
    {
        private City _city;

        public City City
        {
            get => _city;
            set
            {
                _city = value;
                UpdateVisual();
            }
        }

        public CityControl()
        {
            InitializeComponent();
        }

        public CityControl(City city) : this()
        {
            City = city;
        }

        private void UpdateVisual()
        {
            if (_city == null)
                return;

            // Actualizar el nombre
            CityLabel.Text = _city.Name.Length > 3 ? _city.Name.Substring(0, 3) : _city.Name;
            CityName.Text = _city.Name;

            // Aquí podemos personalizar más el aspecto según propiedades de la ciudad
            // como el factor de tráfico, etc.
        }
    }
}