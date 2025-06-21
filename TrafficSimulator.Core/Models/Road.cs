using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TrafficSimulator.Core.Models
{
    public class Road : INotifyPropertyChanged
    {
        private Guid _id;
        private string _name;
        private City _sourceCity;
        private City _destinationCity;
        private double _distance;
        private double _trafficLoad;
        private bool _isBlocked;
        private string _blockReason;

        public Guid Id
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        public City SourceCity
        {
            get => _sourceCity;
            set
            {
                if (_sourceCity != value)
                {
                    _sourceCity = value;
                    OnPropertyChanged();
                    // Recalcular distancia si cambian las ciudades
                    CalculateDistance();
                }
            }
        }

        public City DestinationCity
        {
            get => _destinationCity;
            set
            {
                if (_destinationCity != value)
                {
                    _destinationCity = value;
                    OnPropertyChanged();
                    // Recalcular distancia si cambian las ciudades
                    CalculateDistance();
                }
            }
        }

        public double Distance
        {
            get => _distance;
            set
            {
                if (_distance != value)
                {
                    _distance = value;
                    OnPropertyChanged();
                }
            }
        }

        public double TrafficLoad
        {
            get => _trafficLoad;
            set
            {
                if (_trafficLoad != value)
                {
                    _trafficLoad = value;
                    OnPropertyChanged();
                    // Notificar que el peso calculado podría haber cambiado
                    OnPropertyChanged(nameof(Weight));
                }
            }
        }

        public bool IsBlocked
        {
            get => _isBlocked;
            set
            {
                if (_isBlocked != value)
                {
                    _isBlocked = value;
                    OnPropertyChanged();
                    // Notificar que el peso calculado podría haber cambiado
                    OnPropertyChanged(nameof(Weight));
                }
            }
        }

        public string BlockReason
        {
            get => _blockReason;
            set
            {
                if (_blockReason != value)
                {
                    _blockReason = value;
                    OnPropertyChanged();
                }
            }
        }

        // Propiedad calculada para el peso de la arista en el grafo
        public double Weight
        {
            get
            {
                if (IsBlocked)
                    return double.PositiveInfinity; // Camino bloqueado

                // El peso se calcula en base a la distancia y la carga de tráfico
                return Distance * (1 + TrafficLoad);
            }
        }

        public Road()
        {
            Id = Guid.NewGuid();
            TrafficLoad = 0.0; // Sin carga de tráfico por defecto
            IsBlocked = false;
        }

        public Road(City source, City destination, string name = null) : this()
        {
            SourceCity = source;
            DestinationCity = destination;
            Name = name ?? $"Road {source.Name} to {destination.Name}";
            CalculateDistance();
        }

        // Calcula la distancia euclidiana entre las ciudades
        private void CalculateDistance()
        {
            if (SourceCity != null && DestinationCity != null)
            {
                double dx = DestinationCity.Position.X - SourceCity.Position.X;
                double dy = DestinationCity.Position.Y - SourceCity.Position.Y;
                Distance = Math.Sqrt(dx * dx + dy * dy);
            }
            else
            {
                Distance = 0;
            }
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