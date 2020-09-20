using FlightStreamDeck.Core;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SharpDeck;
using SharpDeck.Events.Received;
using SharpDeck.Manifest;
using System;
using System.Threading.Tasks;

namespace FlightStreamDeck.Logics.Actions
{
    public class GenericGaugeSettings
    {
        public string Header { get; set; }
        public float MinValue { get; set; }
        public float MaxValue { get; set; }
        public string ToggleValue { get; set; }
        public string DisplayValue { get; set; }
        public bool ArduinoConnectedInd { get; set; }
    }

    [StreamDeckAction("tech.flighttracker.streamdeck.generic.gauge")]
    public class GenericGaugeAction : StreamDeckAction<GenericGaugeSettings>
    {
        private readonly ILogger<ApToggleAction> logger;
        private readonly IFlightConnector flightConnector;
        private readonly IImageLogic imageLogic;
        private readonly EnumConverter enumConverter;

        private TOGGLE_EVENT? toggleEvent = null;
        private TOGGLE_VALUE? displayValue = null;
        private TOGGLE_VALUE? subDisplayValue = null;

        private float currentValue = 0;
        private float currentSubValue = float.MinValue;

        private GenericGaugeSettings settings;

        public GenericGaugeAction(ILogger<ApToggleAction> logger, IFlightConnector flightConnector, IImageLogic imageLogic,
            EnumConverter enumConverter)
        {
            this.logger = logger;
            this.flightConnector = flightConnector;
            this.imageLogic = imageLogic;
            this.enumConverter = enumConverter;
        }

        protected override async Task OnWillAppear(ActionEventArgs<AppearancePayload> args)
        {
            InitializeSettings(args.Payload.GetSettings<GenericGaugeSettings>());

            flightConnector.GenericValuesUpdated += FlightConnector_GenericValuesUpdated;

            RegisterValues();

            await UpdateImage();
        }

        protected override Task OnWillDisappear(ActionEventArgs<AppearancePayload> args)
        {
            flightConnector.GenericValuesUpdated -= FlightConnector_GenericValuesUpdated;
            DeRegisterValues();
            return Task.CompletedTask;
        }

        protected override Task OnKeyDown(ActionEventArgs<KeyPayload> args)
        {
            if (toggleEvent.HasValue) flightConnector.Toggle(toggleEvent.Value);
            return Task.CompletedTask;
        }

        protected override async Task OnSendToPlugin(ActionEventArgs<JObject> args)
        {
            try
            {
                InitializeSettings(args.Payload.ToObject<GenericGaugeSettings>());
                await UpdateImage();
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
            }
        }

        private void InitializeSettings(GenericGaugeSettings settings)
        {
            this.settings = settings;

            TOGGLE_EVENT? newToggleEvent = enumConverter.GetEventEnum(settings.ToggleValue);
            TOGGLE_VALUE? newDisplayValue = enumConverter.GetVariableEnum(settings.DisplayValue);
            TOGGLE_VALUE? newSubDisplayValue = null;

            if (newDisplayValue == TOGGLE_VALUE.INDICATED_ALTITUDE)
            {
                newSubDisplayValue = TOGGLE_VALUE.KOHLSMAN_SETTING_MB;
            }

            if (newDisplayValue != displayValue || newSubDisplayValue != subDisplayValue)
            {
                DeRegisterValues();
            }

            toggleEvent = newToggleEvent;
            displayValue = newDisplayValue;
            subDisplayValue = newSubDisplayValue;

            RegisterValues();
        }

        private async void FlightConnector_GenericValuesUpdated(object sender, ToggleValueUpdatedEventArgs e)
        {
            if (StreamDeck == null) return;

            bool isUpdated = false;

            if (displayValue.HasValue && e.GenericValueStatus.ContainsKey(displayValue.Value))
            {
                float.TryParse(e.GenericValueStatus[displayValue.Value], out float newValue);
                isUpdated |= currentValue != newValue;
                currentValue = newValue;
            }

            if (subDisplayValue.HasValue && e.GenericValueStatus.ContainsKey(subDisplayValue.Value))
            {
                float.TryParse(e.GenericValueStatus[subDisplayValue.Value], out float newValue);
                if(subDisplayValue.Value == TOGGLE_VALUE.KOHLSMAN_SETTING_MB)
                {
                    newValue = (float)(newValue / 33.864);
                }
                isUpdated |= currentSubValue != newValue;
                currentSubValue = newValue;
            }

            if (isUpdated)
            {
                await UpdateImage();
            }
        }

        private void RegisterValues()
        {
            if (toggleEvent.HasValue) flightConnector.RegisterToggleEvent(toggleEvent.Value);
            if (displayValue.HasValue) flightConnector.RegisterSimValue(displayValue.Value);
            if (subDisplayValue.HasValue) flightConnector.RegisterSimValue(subDisplayValue.Value);
        }

        private void DeRegisterValues()
        {
            if (displayValue.HasValue) flightConnector.DeRegisterSimValue(displayValue.Value);
            if (subDisplayValue.HasValue) flightConnector.DeRegisterSimValue(subDisplayValue.Value);
            currentValue = 0;
            currentValue = float.MinValue;
        }

        private async Task UpdateImage()
        {
            if (settings != null)
            {
                await SetImageAsync(imageLogic.GetGenericGaugeImage($"{settings.Header}{(settings.ArduinoConnectedInd && DeckLogic.arudinoConnected ? "*" : "")}", currentValue, currentSubValue, settings.MinValue, settings.MaxValue));
            }
        }
    }
}
