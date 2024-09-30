using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using HidSharp;

namespace MultitouchAnnotations
{
    public partial class MenuWindow : Window
    {
        private MainWindow mainWindow;
        private bool isMenuVisible = true;
        private ResourcesWindow resourcesWindow;
        private Border lastClickedBorder = null; // Armazena o último Border clicado

        // Constantes para SetWindowPos
        private const int HWND_TOPMOST = -1;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint SW_RESTORE = 0x0009;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, uint nCmdShow);

        [DllImport("user32.dll")]
        private static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        private const int VK_LWIN = 0x5B;
        private const int VK_NUMPAD9 = 0x69;
        private const uint KEYEVENTF_KEYUP = 0x0002;

        private DispatcherTimer topmostTimer;
        private bool isTrialVersion = false;

        public MenuWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;

            // Adiciona o evento de clique com botão direito para a caneta
            PenButton.MouseRightButtonDown += PenButton_MouseRightButtonDown;

            if (!IsHIDDeviceConnected(0x13FF, 0x0116)) // VID = 0x13FF, PID = 0x0116
            {
                MessageBoxResult result = MessageBox.Show(
                    "Hardware não encontrado. Iniciar o software na versão trial?",
                    "Hardware não encontrado",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    // Mostrar a etiqueta de versão trial
                    mainWindow.TrialVersionLabel.Visibility = Visibility.Visible;
                    ResourcesButton.IsEnabled = false;
                    isTrialVersion = true;
                }
                else
                {
                    // Fechar a aplicação
                    Application.Current.Shutdown();
                    Environment.Exit(0); // Garantir que o aplicativo encerre completamente
                }
            }

            this.Loaded += MenuWindow_Loaded;
        }

        private void MenuWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Centralizar na parte inferior da tela
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;
            var windowWidth = this.Width;
            var windowHeight = this.Height;

            // Centralizar horizontalmente e posicionar no inferior da tela
            this.Left = (screenWidth - windowWidth) / 2;
            this.Top = screenHeight - windowHeight - 50; // Ajustar altura a 50px da parte inferior

            // Forçar o MenuWindow a ficar no topo
            IntPtr hWnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);

            // Configurar um temporizador para manter o MenuWindow no topo
            topmostTimer = new DispatcherTimer();
            topmostTimer.Interval = TimeSpan.FromSeconds(1);
            topmostTimer.Tick += (s, args) =>
            {
                SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
            };
            topmostTimer.Start();

            // Minimizar a janela principal ao carregar a aplicação
            MinimizeButton_Click(sender, e);
        }

        private bool IsHIDDeviceConnected(int vendorId, int productId)
        {
            var devices = DeviceList.Local.GetHidDevices(vendorId, productId).ToList();
            foreach (var device in devices)
            {
                Debug.WriteLine($"Verificando dispositivo: VendorId={device.VendorID}, ProductId={device.ProductID}");
                if (device.VendorID == vendorId && device.ProductID == productId)
                {
                    return true;
                }
            }
            return false;
        }

        private void DesktopButton_Click(object sender, RoutedEventArgs e)
        {
            const string windowName = "Lousa Interativa Web";
            IntPtr hWnd = FindWindow(null, windowName);

            if (hWnd != IntPtr.Zero)
            {
                // Se a janela "Lousa Interativa Web" existir, simula o atalho Windows + Numpad9
                keybd_event(VK_LWIN, 0, 0, UIntPtr.Zero);
                keybd_event(VK_NUMPAD9, 0, 0, UIntPtr.Zero);
                keybd_event(VK_NUMPAD9, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
                keybd_event(VK_LWIN, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            }
            else
            {
                // Caso contrário, iniciar o script adequado
                string scriptPath;
                if (isTrialVersion)
                {
                    scriptPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Lousa Interativa minimum\WebViewToo-main\LousaWeb.ahk");
                }
                else
                {
                    scriptPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Lousa Interativa Web\WebViewToo-main\LousaWeb.ahk");
                }
                Process.Start(scriptPath);
            }

            // Fechar a janela principal e a janela do menu
            mainWindow.Close();
            this.Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            // Fechar a janela principal e a janela do menu
            mainWindow.Close();
            this.Close();

            // Fechar o processo "AutoHotkey64.exe" associado à janela "Lousa Interativa Web"
            const string windowName = "Lousa Interativa Web";
            IntPtr hWnd = FindWindow(null, windowName);
            if (hWnd != IntPtr.Zero)
            {
                GetWindowThreadProcessId(hWnd, out int processId);
                Process autoHotkeyProcess = Process.GetProcessById(processId);
                autoHotkeyProcess.Kill();
            }
        }

        public void PenButton_Click(object sender, RoutedEventArgs e)
        {
            MenuItem_Click(sender, e); // Marcação visual
            mainWindow.MaximizeAndSetPenMode();
        }

        private void PenButton_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Em vez de exibir o menu de contexto, abrir a janela de configurações da caneta
            mainWindow.OpenPenSettingsWindow();
        }




        private void EraserButton_Click(object sender, RoutedEventArgs e)
        {
            MenuItem_Click(sender, e); // Marcação visual
            mainWindow.MaximizeAndSetEraserMode();
        }

        private void ColorButton_Click(object sender, RoutedEventArgs e)
        {
            MenuItem_Click(sender, e); // Marcação visual
            mainWindow.ChooseColor();
        }

        private void ResourcesButton_Click(object sender, RoutedEventArgs e)
        {
            MenuItem_Click(sender, e); // Marcação visual
            if (ResourcesButton.IsEnabled)
            {
                if (resourcesWindow == null || !resourcesWindow.IsVisible)
                {
                    resourcesWindow = new ResourcesWindow(mainWindow);
                    resourcesWindow.Owner = this;
                    resourcesWindow.WindowStartupLocation = WindowStartupLocation.Manual;
                    resourcesWindow.Left = this.Left + (this.Width - resourcesWindow.Width) / 2;
                    resourcesWindow.Top = this.Top - resourcesWindow.Height;
                    resourcesWindow.Show();
                }
                else
                {
                    resourcesWindow.Close();
                }
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            MenuItem_Click(sender, e); // Marcação visual
            mainWindow.WindowState = WindowState.Minimized;
        }

        private void ToggleMenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (isMenuVisible)
            {
                ButtonsPanel.Visibility = Visibility.Collapsed; // Esconde os botões
                MenuGrid.Background = Brushes.Transparent; // Faz o fundo transparente
                ToggleMenuButton.Content = "▲";
                this.Width = ToggleMenuButton.Width + 20; // Ajusta a largura para o botão de toggle
            }
            else
            {
                ButtonsPanel.Visibility = Visibility.Visible; // Mostra os botões
                MenuGrid.Background = new SolidColorBrush(Color.FromRgb(243, 243, 243)); // Restaura o fundo
                ToggleMenuButton.Content = "▼";
                this.Width = 360; // Restaura a largura original
            }
            isMenuVisible = !isMenuVisible;
        }

        // Evento para clique e marcação do botão ativo
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var clickedButton = sender as Button;
            var clickedBorder = clickedButton?.Parent as Border;

            // Limpar a marcação visual do último botão clicado
            if (lastClickedBorder != null)
            {
                lastClickedBorder.Background = Brushes.Transparent;
            }

            // Aplicar o círculo ciano preenchido ao botão atual
            if (clickedBorder != null)
            {
                clickedBorder.Background = new SolidColorBrush(Color.FromArgb(128, 50, 232, 232)); // Ciano com transparência (50% opaco)
                lastClickedBorder = clickedBorder;
            }
        }
    }
}
