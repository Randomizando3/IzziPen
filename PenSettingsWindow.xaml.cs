using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MultitouchAnnotations
{
    public partial class PenSettingsWindow : Window
    {
        private SolidColorBrush selectedColor = Brushes.Black; // Cor padrão
        private double lineThickness = 5; // Espessura padrão
        private DashStyle selectedDashStyle = DashStyles.Solid; // Tipo de traço padrão
        private Ellipse lastSelectedEllipse; // Guarda o último Ellipse selecionado para feedback visual

        // Evento para notificar quando as configurações da caneta forem alteradas
        public event Action<SolidColorBrush, double, DashStyle> PenSettingsChanged;

        public PenSettingsWindow()
        {
            InitializeComponent();
            this.Topmost = true;

            // Posicionar a janela a 15% da margem inferior da tela
            var screenHeight = SystemParameters.PrimaryScreenHeight;
            var screenWidth = SystemParameters.PrimaryScreenWidth;

            // Definir o Left para centralizar horizontalmente
            this.Left = (screenWidth - this.Width) / 2;

            // Definir o Top para posicionar a janela a 15% da margem inferior
            this.Top = screenHeight * 0.90 - this.Height;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            // Fecha a janela de configurações
            this.Close();

            // Ativar a caneta e marcar visualmente no MenuWindow
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                // Ativa a caneta
                mainWindow.MaximizeAndSetPenMode();

                // Acessa o MenuWindow e simula um clique no botão da caneta para ativar a marcação visual
                if (mainWindow.menuWindow != null)
                {
                    mainWindow.menuWindow.PenButton_Click(mainWindow.menuWindow.PenButton, null);
                }
            }
        }

        // Método para selecionar cor padrão e adicionar feedback visual (contorno ciano)
        private void ColorSelected_Click(object sender, MouseButtonEventArgs e)
        {
            // Identifica o Ellipse clicado
            var ellipse = sender as Ellipse;

            if (ellipse != null)
            {
                // Obtém a cor diretamente da propriedade Fill
                var brush = ellipse.Fill as SolidColorBrush;

                if (brush != null)
                {
                    // Armazena a cor selecionada
                    selectedColor = brush;

                    // Aplique a cor selecionada, chamando o método que altera a cor da caneta
                    ApplySelectedColor(brush.Color);

                    // Atualiza o feedback visual
                    UpdateEllipseSelection(ellipse);
                }
            }
        }

        private void UpdateEllipseSelection(Ellipse selectedEllipse)
        {
            // Se já houver um Ellipse selecionado, restaura o contorno original (cinza)
            if (lastSelectedEllipse != null)
            {
                lastSelectedEllipse.Stroke = Brushes.Gray;
                lastSelectedEllipse.StrokeThickness = 1;
            }

            // Aplica o contorno ciano ao Ellipse recém-selecionado
            selectedEllipse.Stroke = Brushes.Cyan;
            selectedEllipse.StrokeThickness = 3; // Define um contorno mais espesso

            // Guarda a referência ao Ellipse selecionado
            lastSelectedEllipse = selectedEllipse;
        }

        private void ApplySelectedColor(Color color)
        {
            // Atualiza o selectedColor com a nova cor
            selectedColor = new SolidColorBrush(color);

            // Aplica as configurações da caneta com a cor atualizada
            ApplyPenSettings();
        }

        // Método para abrir o seletor de cor personalizado
        private void OpenColorPicker_Click(object sender, RoutedEventArgs e)
        {
            var colorDialog = new System.Windows.Forms.ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var color = System.Drawing.ColorTranslator.ToHtml(colorDialog.Color);
                selectedColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
                ApplyPenSettings();
            }
        }

        // Método para ajustar a espessura do traço
        private void LineThicknessChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            lineThickness = e.NewValue;
            ApplyPenSettings();
        }

        // Método para selecionar o tipo de traço
        private void LineTypeChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            var selectedItem = comboBox.SelectedItem as ComboBoxItem;
            switch (selectedItem.Content.ToString())
            {
                case "Sólida":
                    selectedDashStyle = DashStyles.Solid;
                    break;
                case "Tracejada":
                    selectedDashStyle = DashStyles.Dash;
                    break;
                case "Traço e Ponto":
                    selectedDashStyle = DashStyles.DashDot;
                    break;
                case "Pontilhada":
                    selectedDashStyle = DashStyles.Dot;
                    break;
            }
            ApplyPenSettings();
        }

        // Método para aplicar as configurações da caneta
        private void ApplyPenSettings()
        {
            // Notifica a MainWindow sobre as configurações da caneta com a cor, espessura e estilo de traço atuais
            PenSettingsChanged?.Invoke(selectedColor, lineThickness, selectedDashStyle);
        }
    }
}
