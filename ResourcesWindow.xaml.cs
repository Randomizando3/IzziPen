using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace MultitouchAnnotations
{
    public partial class ResourcesWindow : Window
    {
        private readonly string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        private MainWindow mainWindow;

        public ResourcesWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
        }

        private void InteractiveActivities_Click(object sender, RoutedEventArgs e)
        {
            MinimizeCloseAndStartProcess(@"Neuron\Examples\Janela Atividades\Atividades.exe");
        }

        private void ThreeDResources_Click(object sender, RoutedEventArgs e)
        {
            MinimizeCloseAndStartProcess(@"Neuron\Examples\Janela Software 3D\Módulo 3D.exe");
        }

        private void Encyclopedia_Click(object sender, RoutedEventArgs e)
        {
            MinimizeCloseAndStartProcess(@"Neuron\Examples\Enciclopedia\Enciclopedia.exe");
        }

        private void GoogleEarth_Click(object sender, RoutedEventArgs e)
        {
            MinimizeCloseAndStartProcess(@"Neuron\Examples\GoogleEarth\googleearth.exe");
        }

        private void Translator_Click(object sender, RoutedEventArgs e)
        {
            MinimizeCloseAndStartProcess(@"Neuron\Examples\tradutor\tradutor.exe");
        }

        private void Browser_Click(object sender, RoutedEventArgs e)
        {
            MinimizeCloseAndStartProcess(@"Neuron\Examples\navegador\navegador.exe");
        }

        private void SolarSystem_Click(object sender, RoutedEventArgs e)
        {
            MinimizeCloseAndStartProcess(@"Neuron\Examples\SistemaSolar\sistemasolar.exe");
        }

        private void HumanAnatomyAtlas_Click(object sender, RoutedEventArgs e)
        {
            MinimizeCloseAndStartProcess(@"Neuron\Examples\Atlas de Anatomia Humana\AtlasdeAnatomiaHumana.exe");
        }

        private void Calculator_Click(object sender, RoutedEventArgs e)
        {
            MinimizeCloseAndStartProcess(@"Neuron\Examples\calculadora\calculadora.exe");
        }

        private void VirtualKeyboard_Click(object sender, RoutedEventArgs e)
        {
            MinimizeCloseAndStartProcess(@"Neuron\Examples\teclado\teclado.exe");
        }

        private void Stopwatch_Click(object sender, RoutedEventArgs e)
        {
            MinimizeCloseAndStartProcess(@"Neuron\Examples\stopwatch.exe");
        }

        private void ScreenBlock_Click(object sender, RoutedEventArgs e)
        {
            MinimizeCloseAndStartProcess(@"Neuron\Examples\screenblock.exe");
        }

        private void AudioRecorder_Click(object sender, RoutedEventArgs e)
        {
            MinimizeCloseAndStartProcess(@"Neuron\Examples\recorder\gravador.exe");
        }

        private void Camera_Click(object sender, RoutedEventArgs e)
        {
            MinimizeCloseAndStartProcess(@"Neuron\Examples\camera\camera.exe");
        }

        private void Screenshot_Click(object sender, RoutedEventArgs e)
        {
            MinimizeCloseAndStartProcess(@"Neuron\Examples\capturadetela\LightscreenPortable.exe");
        }

        private void PowerPoint_Click(object sender, RoutedEventArgs e)
        {
            MinimizeCloseAndStartProcess(@"Neuron\Examples\Office\Presentations.exe");
        }

        private void Word_Click(object sender, RoutedEventArgs e)
        {
            MinimizeCloseAndStartProcess(@"Neuron\Examples\Office\TextMaker.exe");
        }

        private void Excel_Click(object sender, RoutedEventArgs e)
        {
            MinimizeCloseAndStartProcess(@"Neuron\Examples\Office\PlanMaker.exe");
        }

        private void MinimizeCloseAndStartProcess(string relativePath)
        {
            mainWindow.WindowState = WindowState.Minimized;
            this.Close();
            StartProcess(relativePath);
        }

        private void StartProcess(string relativePath)
        {
            string fullPath = Path.Combine(baseDir, relativePath);
            if (File.Exists(fullPath))
            {
                Process.Start(fullPath);
            }
            else
            {
                MessageBox.Show($"O arquivo {fullPath} não foi encontrado.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Função para fechar a janela ao clicar no botão "X"
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
