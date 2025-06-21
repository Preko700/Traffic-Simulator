using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows;

namespace TrafficSimulator.Core.Models
{
    public class City : INotifyPropertyChanged
    {
        private Guid _id;
        private string _name;
        private Point _position;
        private double _trafficFactor;

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

        public Point Position
        {
            get => _position;
            set
            {
                if (_position != value)
                {
                    _position = value;
                    OnPropertyChanged();
                }
            }
        }

        public double TrafficFactor
        {
            get => _trafficFactor;
            set
            {
                if (_trafficFactor != value)
                {
                    _trafficFactor = value;
                    OnPropertyChanged();
                }
            }
        }

        public City()
        {
            Id = Guid.NewGuid();
            TrafficFactor = 1.0; // Factor neutral por defecto
        }

        public City(string name, Point position) : this()
        {
            Name = name;
            Position = position;
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