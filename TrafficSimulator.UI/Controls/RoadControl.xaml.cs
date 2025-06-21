using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using TrafficSimulator.Core.Models;

namespace TrafficSimulator.UI.Controls
{
    public partial class RoadControl : UserControl
    {
        private Road _road;
        private Line _roadLine;
        private TextBlock _roadNameText;
        private double _x1;
        private double _y1;
        private double _x2;
        private double _y2;

        public Road Road
        {
            get => _road;
            set
            {
                _road = value;
                UpdateVisual();
            }
        }

        public double X1
        {
            get => _x1;
            set
            {
                _x1 = value;
                UpdateLinePosition();
            }
        }

        public double Y1
        {
            get => _y1;
            set
            {
                _y1 = value;
                UpdateLinePosition();
            }
        }

        public double X2
        {
            get => _x2;
            set
            {
                _x2 = value;
                UpdateLinePosition();
            }
        }

        public double Y2
        {
            get => _y2;
            set
            {
                _y2 = value;
                UpdateLinePosition();
            }
        }

        public RoadControl()
        {
            InitializeComponent();
            CreateRoadElements();
        }

        public RoadControl(Road road) : this()
        {
            Road = road;
        }

        private void CreateRoadElements()
        {
            // Crear la línea que representa la carretera
            _roadLine = new Line
            {
                Stroke = Brushes.DarkGray,
                StrokeThickness = 3,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round
            };

            // Crear el texto para el nombre de la carretera
            _roadNameText = new TextBlock
            {
                Foreground = Brushes.Black,
                Background = Brushes.White,
                Padding = new Thickness(2),
                FontSize = 10,
                TextAlignment = TextAlignment.Center
            };

            // Agregar elementos al Canvas
            ((Canvas)Content).Children.Add(_roadLine);
            ((Canvas)Content).Children.Add(_roadNameText);
        }

        public void UpdateVisual()
        {
            if (_road == null)
                return;

            if (_road.IsBlocked)
            {
                _roadLine.Stroke = Brushes.Red;
                _roadLine.StrokeDashArray = new DoubleCollection { 5, 2 };
            }
            else
            {
                _roadLine.Stroke = Brushes.DarkGray;
                _roadLine.StrokeDashArray = null;
            }
            // ... resto de actualizaciones visuales

            // Actualizar el grosor según la carga de tráfico
            _roadLine.StrokeThickness = 3 + _road.TrafficLoad * 2;

            // Actualizar el texto
            _roadNameText.Text = _road.Name;

            UpdateLinePosition();
        }

        private void UpdateLinePosition()
        {
            // Actualizar la posición de la línea
            _roadLine.X1 = X1;
            _roadLine.Y1 = Y1;
            _roadLine.X2 = X2;
            _roadLine.Y2 = Y2;

            // Posicionar el texto en el medio de la línea
            double midX = (X1 + X2) / 2;
            double midY = (Y1 + Y2) / 2;

            Canvas.SetLeft(_roadNameText, midX - _roadNameText.ActualWidth / 2);
            Canvas.SetTop(_roadNameText, midY - _roadNameText.ActualHeight / 2);

            // Calcular el ángulo para el texto
            double angle = Math.Atan2(Y2 - Y1, X2 - X1) * 180 / Math.PI;

            // Si el ángulo hace que el texto se lea al revés, lo ajustamos
            if (angle < -90 || angle > 90)
            {
                angle += 180;
            }

            // Crear una transformación de rotación
            _roadNameText.RenderTransform = new RotateTransform(angle, _roadNameText.ActualWidth / 2, _roadNameText.ActualHeight / 2);
        }
    }
}