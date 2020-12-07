using FlightStreamDeck.Logics;
using FlightStreamDeck.SimConnectFSX;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace FlightStreamDeck.AddOn
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DeckLogic deckLogic;
        private readonly IFlightConnector flightConnector;
        private readonly ILogger<MainWindow> logger;
        private IntPtr Handle;
        private SerialPort arduinoPort = new SerialPort("COM3", 9600);

        public MainWindow(DeckLogic deckLogic, IFlightConnector flightConnector, ILogger<MainWindow> logger)
        {
            InitializeComponent();
            this.deckLogic = deckLogic;
            this.flightConnector = flightConnector;
            this.logger = logger;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Hide();
            deckLogic.Initialize();

            // Initialize SimConnect
            if (flightConnector is SimConnectFlightConnector simConnect)
            {
                simConnect.Closed += SimConnect_Closed;

                // Create an event handle for the WPF window to listen for SimConnect events
                Handle = new WindowInteropHelper(sender as Window).Handle; // Get handle of main WPF Window
                var HandleSource = HwndSource.FromHwnd(Handle); // Get source of handle in order to add event handlers to it
                HandleSource.AddHook(simConnect.HandleSimConnectEvents);

                //var viewModel = ServiceProvider.GetService<MainViewModel>();

                try
                {
                    await InitializeSimConnectAsync(simConnect);
                }
                catch (BadImageFormatException ex)
                {
                    logger.LogError(ex, "Cannot find SimConnect!");

                    var result = MessageBox.Show(this, "SimConnect not found. This component is needed to connect to Flight Simulator.\n" +
                        "Please download SimConnect from\n\nhttps://events-storage.flighttracker.tech/downloads/SimConnect.zip\n\n" +
                        "follow the ReadMe.txt in the zip file and try to start again.\n\nThis program will now exit.\n\nDo you want to open the SimConnect link above?",
                        "Needed component is missing",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Error);

                    if (result == MessageBoxResult.Yes)
                    {
                        try
                        {
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = "https://events-storage.flighttracker.tech/downloads/SimConnect.zip",
                                UseShellExecute = true
                            });
                        }
                        catch { }
                    }

                    App.Current.Shutdown(-1);
                }
            }
        }

        private async Task InitializeSimConnectAsync(SimConnectFlightConnector simConnect)
        {
            myNotifyIcon.Icon = new Icon("Images/button@2x.ico");
            while (true)
            {
                try
                {
                    simConnect.Initialize(Handle);
                    setupArduino();
                    myNotifyIcon.Icon = new Icon("Images/button_active@2x.ico");
                    simConnect.Send("Connected to Stream Deck plugin");
                    break;
                }
                catch (COMException)
                {
                    await Task.Delay(5000).ConfigureAwait(true);
                }
            }
        }

        private void setupArduino()
        {
            try
            {
                logger.LogDebug($"connecting arduino: {arduinoPort.PortName}");
                arduinoPortOpen();
                DeckLogic.arudinoConnected = true;
                arduinoPort.DataReceived += arduinoPort_DataReceived;
            }
            catch (Exception) { }
        }

        public void arduinoPortOpen()
        {
            if (arduinoPort.IsOpen)
            {
                arduinoPort.Close();
            }

            arduinoPort.Open();
        }

        void arduinoPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string readLine = arduinoPort.ReadLine();
                logger.LogDebug($"arduino DataReceived: {readLine}");
                string pin = readLine.Split(":")[0].ToUpper();
                switch (pin)
                {
                    case "A0":
                        handleA0PinUpdate(readLine);
                        break;
                    case "A1":
                        handleA1PinUpdate(readLine);
                        break;
                    case "A2":
                        handleA2PinUpdate(readLine.Replace("\r",""));
                        break;
                    case "A3":
                        handleA3PinUpdate(readLine.Replace("\r", ""));
                        break;
                    case "A4":
                        handleA4PinUpdate(readLine.Replace("\r", ""));
                        break;
                    case "A5":
                        handleA5PinUpdate(readLine.Replace("\r", ""));
                        break;
                    case "A6":
                        handleA6PinUpdate(readLine.Replace("\r", ""));
                        break;
                    case "A7":
                        handleA7PinUpdate(readLine.Replace("\r", ""));
                        break;
                    case "A8":
                        handleA8PinUpdate(readLine.Replace("\r", ""));
                        break;
                    case "A9":
                        handleA9PinUpdate(readLine.Replace("\r", ""));
                        break;
                    case "A10":
                        handleA10PinUpdate(readLine.Replace("\r", ""));
                        break;
                    case "A11":
                    case "A12":
                        handleA11PinUpdate(readLine.Replace("\r", ""));
                        break;
                }

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failure in arduino message received...");
            }
        }

        private void handleA0PinUpdate(string readLine)
        {
            int potVal = int.Parse(readLine.Split(":")[1])*-1;
            Debug.WriteLine(potVal);
            string value = potVal.ToString();
            uint data = unchecked((uint)potVal);
            flightConnector.TrimSetValue(data);
        }

        private void handleA1PinUpdate(string readLine)
        {
            int potVal = int.Parse(readLine.Split(":")[1]);
            switch (potVal)
            {
                case 0:
                    flightConnector.MagnetoOff();
                    break;
                case 1:
                    flightConnector.MagnetoRight();
                    break;
                case 2:
                    flightConnector.MagnetoLeft();
                    break;
                case 3:
                    flightConnector.MagnetoBoth();
                    break;
                case 4:
                    flightConnector.MagnetoStart();
                    break;

            }
            Debug.WriteLine(potVal);
        }

        private void handleA2PinUpdate(string readLine)
        {
            flightConnector.ToggleMasterAlternator(readLine.Split(":")[2] =="1");
        }

        private void handleA3PinUpdate(string readLine)
        {
            flightConnector.ToggleMasterBattery(readLine.Split(":")[2] =="1");
        }

        private void handleA4PinUpdate(string readLine)
        {
            flightConnector.ToggleFuelPump(readLine.Split(":")[2] =="1");
        }

        private void handleA5PinUpdate(string readLine)
        {
            flightConnector.ToggleBeacon(readLine.Split(":")[2] =="1");
        }

        private void handleA6PinUpdate(string readLine)
        {
            flightConnector.ToggleLanding(readLine.Split(":")[2] =="1");
        }

        private void handleA7PinUpdate(string readLine)
        {
            flightConnector.ToggleTaxi(readLine.Split(":")[2] =="1");
        }

        private void handleA8PinUpdate(string readLine)
        {
            flightConnector.ToggleNav(readLine.Split(":")[2] =="1");
        }

        private void handleA9PinUpdate(string readLine)
        {
            flightConnector.ToggleStrobe(readLine.Split(":")[2] =="1");
        }

        private void handleA10PinUpdate(string readLine)
        {
            flightConnector.TogglePitot(readLine.Split(":")[2] =="1");
        }

        private void handleA11PinUpdate(string readLine)
        {
            int potVal = int.Parse(readLine.Split(":")[1]);
            flightConnector.AvMasterToggle((uint)potVal);
        }

        private async void SimConnect_Closed(object sender, EventArgs e)
        {
            var simConnect = sender as SimConnectFlightConnector;
            await InitializeSimConnectAsync(simConnect);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            myNotifyIcon.Dispose();
        }
    }
}
