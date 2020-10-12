﻿using FlightStreamDeck.Logics;
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
                if (readLine.ToUpper().StartsWith("A0:"))
                {
                    int potVal = int.Parse(readLine.Split(":")[1]);
                    Debug.WriteLine(potVal);
                    string value = potVal.ToString();
                    uint data = unchecked((uint)potVal);
                    flightConnector.TrimSetValue(data);
                } else if (readLine.ToUpper().StartsWith("A1:"))
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

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failure in arduino message received...");
            }
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
