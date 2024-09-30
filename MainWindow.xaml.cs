using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MultitouchAnnotations
{
    public partial class MainWindow : Window
    {
        public MenuWindow menuWindow;
        private DrawingAttributes penAttributes;
        private PathFigure currentPathFigure;
        private PolyLineSegment currentPolyLineSegment;

        public MainWindow()
        {
            InitializeComponent();

            // Configuração inicial da caneta
            penAttributes = new DrawingAttributes
            {
                Color = Colors.Black,
                Width = 5,
                Height = 5,
                StylusTip = StylusTip.Ellipse
            };

            DrawingCanvas.EditingMode = InkCanvasEditingMode.None; // Desabilita o modo de edição InkCanvas para usarmos o Path
            DrawingCanvas.MouseMove += DrawingCanvas_MouseMove;
            DrawingCanvas.MouseLeftButtonDown += DrawingCanvas_MouseLeftButtonDown;
            DrawingCanvas.MouseLeftButtonUp += DrawingCanvas_MouseLeftButtonUp;

            // Inicializar e mostrar a janela do menu após o MainWindow ser carregado
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            menuWindow = new MenuWindow(this);
            menuWindow.Show();
        }

        // Método para definir a cor da caneta
        public void ChooseColor(Color color)
        {
            penAttributes.Color = color;
        }

        // Método para escolher o tamanho do traço
        public void ChooseThickness(double thickness)
        {
            penAttributes.Width = thickness;
            penAttributes.Height = thickness;
        }

        // Método para escolher o tipo de traço (sólido, tracejado, pontilhado, tracejado com ponto)
        public void ChooseLineStyle(string lineStyle)
        {
            DoubleCollection dashArray = null; // Coleção para definir o padrão de traço

            switch (lineStyle)
            {
                case "Sólido":
                    dashArray = null; // Linha sólida (sem padrão de traço)
                    break;

                case "Tracejado":
                    dashArray = new DoubleCollection { 4, 2 }; // Padrão tracejado: traço de 4 unidades, espaço de 2 unidades
                    break;

                case "Pontilhado":
                    dashArray = new DoubleCollection { 1, 2 }; // Padrão pontilhado: ponto de 1 unidade, espaço de 2 unidades
                    break;

                case "Tracejado com Ponto":
                    dashArray = new DoubleCollection { 4, 2, 1, 2 }; // Padrão de traço e ponto
                    break;
            }

            DrawingPath.StrokeDashArray = dashArray;
        }

        private void DrawingCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // Cria uma nova figura de caminho para o traçado
                Point startPoint = e.GetPosition(DrawingCanvas);
                currentPathFigure = new PathFigure { StartPoint = startPoint };
                currentPolyLineSegment = new PolyLineSegment();
                currentPathFigure.Segments.Add(currentPolyLineSegment);

                PathGeometry pathGeometry = new PathGeometry();
                pathGeometry.Figures.Add(currentPathFigure);

                Path path = new Path
                {
                    Stroke = new SolidColorBrush(penAttributes.Color),
                    StrokeThickness = penAttributes.Width,
                    Data = pathGeometry,

                    // Arredondar os traços
                    StrokeLineJoin = PenLineJoin.Round, // Arredonda as junções de linhas
                    StrokeStartLineCap = PenLineCap.Round, // Arredonda o início do traço
                    StrokeEndLineCap = PenLineCap.Round   // Arredonda o final do traço
                };

                // Aplicar o DashArray ao Path, se houver
                path.StrokeDashArray = DrawingPath.StrokeDashArray;

                DrawingCanvas.Children.Add(path);
            }
        }

        private void DrawingCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && currentPolyLineSegment != null)
            {
                // Adiciona o ponto atual ao PolyLineSegment para desenhar o traço
                Point currentPoint = e.GetPosition(DrawingCanvas);
                currentPolyLineSegment.Points.Add(currentPoint);
            }
        }

        private void DrawingCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Finaliza o traçado
            currentPolyLineSegment = null;
            currentPathFigure = null;
        }

        // Evento de clique no botão da caneta
        private void PenButton_Click(object sender, RoutedEventArgs e)
        {
            MaximizeAndSetPenMode();
        }

        // Evento de clique no botão da borracha
        private void EraserButton_Click(object sender, RoutedEventArgs e)
        {
            MaximizeAndSetEraserMode();
        }

        // Método para maximizar a janela e ativar o modo caneta
        public void MaximizeAndSetPenMode()
        {
            this.WindowState = WindowState.Maximized;
            SetPenMode();
        }

        // Método para maximizar a janela e ativar o modo borracha
        public void MaximizeAndSetEraserMode()
        {
            this.WindowState = WindowState.Maximized;
            SetEraserMode();
        }

        // Definir o modo de desenho (caneta)
        public void SetPenMode()
        {
            DrawingCanvas.EditingMode = InkCanvasEditingMode.None;

            // Remover eventos de borracha
            DrawingCanvas.MouseMove -= Erase_MouseMove;
            DrawingCanvas.MouseLeftButtonDown -= Erase_MouseLeftButtonDown;

            // Vincula novamente os eventos de desenho
            DrawingCanvas.MouseMove += DrawingCanvas_MouseMove;
            DrawingCanvas.MouseLeftButtonDown += DrawingCanvas_MouseLeftButtonDown;
            DrawingCanvas.MouseLeftButtonUp += DrawingCanvas_MouseLeftButtonUp;
        }

        // Definir o modo de borracha
        public void SetEraserMode()
        {
            DrawingCanvas.EditingMode = InkCanvasEditingMode.None;

            // Remover eventos de desenho
            DrawingCanvas.MouseMove -= DrawingCanvas_MouseMove;
            DrawingCanvas.MouseLeftButtonDown -= DrawingCanvas_MouseLeftButtonDown;

            // Vincula os eventos de apagar
            DrawingCanvas.MouseMove += Erase_MouseMove;
            DrawingCanvas.MouseLeftButtonDown += Erase_MouseLeftButtonDown;
        }

        // Função para apagar os traços (modo borracha)
        private void Erase_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point erasePoint = e.GetPosition(DrawingCanvas);
            HitTestResult hitTestResult = VisualTreeHelper.HitTest(DrawingCanvas, erasePoint);

            if (hitTestResult != null && hitTestResult.VisualHit is Path path)
            {
                // Remove o traço do InkCanvas
                DrawingCanvas.Children.Remove(path);
            }
        }

        // Evento de apagar durante o movimento do mouse
        private void Erase_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point erasePoint = e.GetPosition(DrawingCanvas);

                // Cria uma área ao redor do ponto de apagar para verificar traços maiores de uma só vez
                Rect hitTestArea = new Rect(erasePoint.X - 5, erasePoint.Y - 5, 10, 10);

                // Executa o HitTest apenas na área de interseção
                VisualTreeHelper.HitTest(DrawingCanvas, null, result =>
                {
                    if (result.VisualHit is Path path)
                    {
                        DrawingCanvas.Children.Remove(path); // Remove o traço
                    }

                    // Continue o hit testing
                    return HitTestResultBehavior.Continue;
                }, new GeometryHitTestParameters(new RectangleGeometry(hitTestArea)));
            }
        }


        // Evento de clique no botão de escolha de cor
        public void ChooseColor()
        {
            var colorDialog = new System.Windows.Forms.ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var selectedColor = colorDialog.Color;
                penAttributes.Color = Color.FromArgb(
                    selectedColor.A,
                    selectedColor.R,
                    selectedColor.G,
                    selectedColor.B);
            }
        }

        // Evento para minimizar a janela
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        // Evento para manipulação de atalhos de teclado (P: Caneta, E: Borracha, C: Cor)
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.P:
                    MaximizeAndSetPenMode();
                    break;
                case Key.E:
                    MaximizeAndSetEraserMode();
                    break;
                case Key.C:
                    ChooseColor(Colors.Black); // Cor padrão ao pressionar C
                    break;
            }
        }

        // Método para abrir a janela de configurações da caneta
        public void OpenPenSettingsWindow()
        {
            PenSettingsWindow penSettingsWindow = new PenSettingsWindow();

            // Inscreve-se para ouvir as alterações de configurações
            penSettingsWindow.PenSettingsChanged += (color, thickness, dashStyle) =>
            {
                ApplyPenSettings(color, thickness, dashStyle);
            };

            penSettingsWindow.ShowDialog();
        }


        // Aplicar configurações de caneta com cor, espessura e estilo de traço
        public void ApplyPenSettings(SolidColorBrush color, double thickness, DashStyle dashStyle)
        {
            penAttributes.Color = color.Color;
            penAttributes.Width = thickness;
            penAttributes.Height = thickness;

            // Ajusta o estilo de traço
            DoubleCollection dashArray = null;
            if (dashStyle == DashStyles.Dash)
                dashArray = new DoubleCollection { 4, 2 };
            else if (dashStyle == DashStyles.Dot)
                dashArray = new DoubleCollection { 1, 2 };
            else if (dashStyle == DashStyles.DashDot)
                dashArray = new DoubleCollection { 4, 2, 1, 2 };

            DrawingPath.StrokeDashArray = dashArray;

            DrawingCanvas.DefaultDrawingAttributes = penAttributes;
        }

    }
}
