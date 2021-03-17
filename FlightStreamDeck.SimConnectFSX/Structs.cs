﻿using Microsoft.FlightSimulator.SimConnect;
using System;
using System.Runtime.InteropServices;

namespace FlightStreamDeck.SimConnectFSX
{
    enum GROUPID
    {
        FLAG = 2000000000,
        MAX = 1,
    };

    enum DEFINITIONS
    {
        AircraftData,
        FlightStatus,
        GenericData
    }

    internal enum DATA_REQUESTS
    {
        NONE,
        SUBSCRIBE_GENERIC,
        AIRCRAFT_DATA,
        FLIGHT_STATUS,
        ENVIRONMENT_DATA,
        FLIGHT_PLAN,
        TOGGLE_VALUE_DATA
    }

    public enum EVENTS
    {
        MESSAGE_RECEIVED,
        AUTOPILOT_ON,
        AUTOPILOT_OFF,
        AUTOPILOT_TOGGLE,
        AP_HDG_TOGGLE,
        AP_NAV_TOGGLE,
        AP_APR_TOGGLE,
        AP_ALT_TOGGLE,
        AP_VS_TOGGLE,
        AP_FLC_ON,
        AP_FLC_OFF,
        AP_HDG_SET,
        AP_HDG_INC,
        AP_HDG_DEC,
        AP_ALT_SET,
        AP_ALT_INC,
        AP_ALT_DEC,
        AP_VS_SET,
        AP_VS_INC,
        AP_VS_DEC,
        AP_AIRSPEED_SET,
        AP_AIRSPEED_INC,
        AP_AIRSPEED_DEC,
        QNH_SET,
        QNH_INC,
        QNH_DEC,
        AVIONICS_TOGGLE,
        ELEV_TRIM_SET,
        MAG_OFF,
        MAG_LEFT,
        MAG_RIGHT,
        MAG_BOTH,
        MAG_START,
        MASTER_BATTERY_TOGGLE,
        MASTER_ALTERNATOR_TOGGLE,
        FUEL_PUMP,
        PITOT_HEAT_TOGGLE,
        TOGGLE_BEACON_LIGHTS,
        LANDING_LIGHTS_TOGGLE,
        TOGGLE_TAXI_LIGHTS,
        TOGGLE_NAV_LIGHTS,
        STROBES_TOGGLE
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct AircraftDataStruct
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string Type;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string Model;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Title;
        public double EstimatedCruiseSpeed;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct FlightStatusStruct
    {
        public int SimRate;

        public double Latitude;
        public double Longitude;
        public double Altitude;
        public double AltitudeAboveGround;
        public double Pitch;
        public double Bank;
        public double TrueHeading;
        public double MagneticHeading;
        public double GroundAltitude;
        public double GroundSpeed;
        public double IndicatedAirSpeed;
        public double VerticalSpeed;

        public double FuelTotalQuantity;

        public double WindVelocity;
        public double WindDirection;

        public int IsOnGround;
        public int StallWarning;
        public int OverspeedWarning;

        public int IsAutopilotOn;
        public int IsApHdgOn;
        public int ApHdg;
        public int IsApNavOn;
        public int IsApAprOn;
        public int IsApAltOn;
        public int ApAlt;
        public int IsApVsOn;
        public int ApVs;
        public int IsApFlcOn;
        public int ApAirspeed;

        public int QNHmbar;

        public int Transponder;
        public int Com1;
        public int Com2;
        public int AvMasterOn;
        public double Nav1OBS;
        public double Nav2OBS;
        public double ADFCard;
        public int MasterBattOn;
        public int MasterAltOn;
        public int FuelPumpOn;
        public int BeaconOn;
        public int LandingLightsOn;
        public int TaxiLightsOn;
        public int NavLightsOn;
        public int StrobeLightsOn;
        public int PitotHeatOn;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct GenericValuesStruct
    {
        unsafe public fixed double Data[64];

        unsafe public double Get(int index)
        {
            return Data[index];
        }
    }
}
