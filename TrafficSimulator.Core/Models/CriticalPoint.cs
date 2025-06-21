using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TrafficSimulator.Core.Models
{
    public class CriticalPoint : INotifyPropertyChanged
    {
        private City _location;
        private double _severity;
        private string _reason;
        private string _recommendation;

        public City Location
        {
            get => _location;
            set
            {
                if (_location != value)
                {
                    _location = value;
                    OnPropertyChanged();
                }
            }
        }

        public double Severity
        {
            get => _severity;
            set
            {
                if (_severity != value)
                {
                    _severity = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Reason
        {
            get => _reason;
            set
            {
                if (_reason != value)
                {
                    _reason = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Recommendation
        {
            get => _recommendation;
            set
            {
                if (_recommendation != value)
                {
                    _recommendation = value;
                    OnPropertyChanged();
                }
            }
        }

        public CriticalPoint()
        {
            Severity = 0.0;
        }

        public CriticalPoint(City location, double severity, string reason, string recommendation) : this()
        {
            Location = location;
            Severity = severity;
            Reason = reason;
            Recommendation = recommendation;
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