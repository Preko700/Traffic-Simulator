using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TrafficSimulator.Core.Models
{
    public class SimulationSettings : INotifyPropertyChanged
    {
        private double _simulationSpeed;
        private int _vehicleCount;
        private TimeSpan _timeOfDay;
        private double _globalTrafficFactor;

        public double SimulationSpeed
        {
            get => _simulationSpeed;
            set
            {
                if (_simulationSpeed != value)
                {
                    _simulationSpeed = value;
                    OnPropertyChanged();
                }
            }
        }

        public int VehicleCount
        {
            get => _vehicleCount;
            set
            {
                if (_vehicleCount != value)
                {
                    _vehicleCount = value;
                    OnPropertyChanged();
                }
            }
        }

        public TimeSpan TimeOfDay
        {
            get => _timeOfDay;
            set
            {
                if (_timeOfDay != value)
                {
                    _timeOfDay = value;
                    OnPropertyChanged();
                }
            }
        }

        public double GlobalTrafficFactor
        {
            get => _globalTrafficFactor;
            set
            {
                if (_globalTrafficFactor != value)
                {
                    _globalTrafficFactor = value;
                    OnPropertyChanged();
                }
            }
        }

        public SimulationSettings()
        {
            SimulationSpeed = 1.0; // Velocidad normal
            VehicleCount = 50; // Número de vehículos por defecto
            TimeOfDay = new TimeSpan(12, 0, 0); // Mediodía por defecto
            GlobalTrafficFactor = 1.0; // Factor de tráfico global neutral
        }

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}