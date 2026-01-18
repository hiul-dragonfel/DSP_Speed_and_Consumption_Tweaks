using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static DSP_Speed_and_Consumption_Tweaks.DSP_Speed_and_Consumption_Tweaks_Plugin;

namespace DSP_Speed_and_Consumption_Tweaks
{
    using BepInEx;
    using BepInEx.Configuration;
    using BepInEx.Logging;



    public static class DSP_Config
    {
        private static readonly string MECHA_LAND_CONFIG = "ICARUS Land Configuration";
        private static readonly string MECHA_CRUISE_CONFIG = "ICARUS Cruise Configuration";
        private static readonly string MECHA_WARP_CONFIG = "ICARUS Warp Configuration";
        private static readonly string LOGISTIC_SHIP_CONFIG = "Logistic Ships Configuration";
        private static readonly string LOGISTIC_SHIP_WARP = "Logistic Ships Warp Configuration";
        private static readonly string LOGISTIC_SHIP_SAIL = "Logistic Ships Cruise Configuration";
        private static readonly string LOGISTIC_DRONE_CONFIG = "Logistic Drones Configuration";
        private static readonly string DARK_FOG_CONFIG = "Dark Fog Configuration";
        private static readonly string DEBUG_CONFIG = "Activate DEBUG messages";
        //private static readonly string UTILITY_SECTION = "Utility";

        public enum units{
            M = 1,
            AU = 40_000,
            LY = 2_400_000
        }

        public static class Mecha_LAND_CONFIG
        {
            // mecha settings
            // land
            public static ConfigEntry<double> WalkSpeedMultiplier;
            public static ConfigEntry<double> WalkSpeedUpgrageMultiplier;
            public static ConfigEntry<double> WalkEnergyCostMultiplier;
            public static ConfigEntry<double> JumpSpeedMultiplier;
            public static ConfigEntry<double> JumpEnergyMultiplier;
            public static ConfigEntry<double> FlightSpeedMultiplier;
            public static ConfigEntry<double> FlightTakeoffEnergyCostMultiplier;
            public static ConfigEntry<double> FlightEnergyCostMultiplier;
        }

        public static class Mecha_CRUISE_CONFIG
        {
            // mecha settings
            // cruise
            public static ConfigEntry<double> CruiseMaxSpeedMultiplier;
            public static ConfigEntry<double> CruiseAccelerationRateMultiplier;
            public static ConfigEntry<double> CruiseAccelerationEnergyCostMultiplier;

        }

        public static class Mecha_WARP_CONFIG
        {
            // warp
            public static ConfigEntry<double> maxWarpSpeedMultiplier;
            public static ConfigEntry<double> warpKeepingConsumptionMultiplier;
            public static ConfigEntry<double> warpStartPowerConsumptionMultiplier;
        }

        public static class Logistic_SHIP_CONFIG
        {
            //// ship settings
            public static ConfigEntry<double> taxiSpeed;
            public static ConfigEntry<double> approchSpeed;

            public static ConfigEntry<double> ShipCruiseSpeedMultiplier;
       
            public static ConfigEntry<double> ShipWarpSpeedMultiplier;
            
            public static ConfigEntry<double> ShipEneregyTransitCostMultiplier;
            public static ConfigEntry<double> ShipEnergyTakeOffMultiplier;
            public static ConfigEntry<double> ShipEnergyMaxSpeedMultiplier;
            
            public static ConfigEntry<double> ShipEnergyWarpMultiplier;
        }

        public static class Logistic_DRONE_CONFIG
        {
            //// drone settings
            public static ConfigEntry<double> DroneTravelSpeedMutliplier;
            public static ConfigEntry<double> DroneTaxiSpeed;
            public static ConfigEntry<double> DroneEnergyTransitMultiplier;
            public static ConfigEntry<double> DroneEnergyTakeOffMultiplier;
        }
        
        public static class Dark_Fog_CONFIG
        {
            public static ConfigEntry<double> maxEnemyAttackSpeedMultiplier;
            public static ConfigEntry<double> maxEnemySpeedMultiplier;
            public static ConfigEntry<double> maxCarrierSpeedMultiplier;
            public static ConfigEntry<double> maxRelaySpeedMultiplier;
            public static ConfigEntry<double> maxSeedSpeedMultiplier;
        }

        public static class Debug_CONFIG
        {
            public static ConfigEntry<bool> DEBUG;
        }

        internal static void Init(ConfigFile config)
        {
            ///////////////////////////////
            // ICARUS Land Configuration //
            ///////////////////////////////
            Mecha_LAND_CONFIG.WalkSpeedMultiplier = config.Bind(MECHA_LAND_CONFIG, "Walking speed Multiplier (Vanilla is 5m/s)", 1.0, 
                new ConfigDescription("Multiplies the base walking speed",
                new AcceptableValueRange<double>(.10, 10.0), null));
            Mecha_LAND_CONFIG.WalkSpeedUpgrageMultiplier = config.Bind(MECHA_LAND_CONFIG, "Walking speed Upgrade Multiplier", 1.0,
                new ConfigDescription("Amplifies the upgrades to the walking speed",
                new AcceptableValueRange<double>(.10, 10.0), null));
            Mecha_LAND_CONFIG.WalkEnergyCostMultiplier = config.Bind(MECHA_LAND_CONFIG, "Walking Energy Multiplier (Vanilla is 15000)", 1.0,
                new ConfigDescription("Multiplies the base walking energy consumption",
                new AcceptableValueRange<double>(0.0, 100.0), null));
            Mecha_LAND_CONFIG.JumpSpeedMultiplier = config.Bind(MECHA_LAND_CONFIG, "Jumping speed Multiplier (Vanilla is 32m/s)", 1.0,
                new ConfigDescription("Multiplies the base Jumping speed",
                new AcceptableValueRange<double>(.10, 10.0), null));
            Mecha_LAND_CONFIG.JumpEnergyMultiplier = config.Bind(MECHA_LAND_CONFIG, "Jumping Energy Multiplier (Vanilla is 25000)", 1.0,
                new ConfigDescription("Multiplies the base Jumping energy consumption",
                new AcceptableValueRange<double>(0.0, 100.0), null));
            Mecha_LAND_CONFIG.FlightSpeedMultiplier = config.Bind(MECHA_LAND_CONFIG, "Flight speed Multiplier (Vanilla is 250% WalkSpeed)", 1.0,
                new ConfigDescription("Changes the base Flight speed multiplyer",
                new AcceptableValueRange<double>(.10, 10.0), null));
            Mecha_LAND_CONFIG.FlightEnergyCostMultiplier = config.Bind(MECHA_LAND_CONFIG, "Flight energy Multiplier (Vanilla is 5m/s)", 1.0,
                new ConfigDescription("Multiplies the base Flight energy cost",
                new AcceptableValueRange<double>(0.0, 100.0), null));
            Mecha_LAND_CONFIG.FlightTakeoffEnergyCostMultiplier = config.Bind(MECHA_LAND_CONFIG, "Takeoff cost Multiplier (Vanilla is 5m/s)", 1.0,
                new ConfigDescription("Multiplies the base Takeoff cost",
                new AcceptableValueRange<double>(0.0, 100.0), null));

            //DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("Loading Config");
            /////////////////////////////////
            // ICARUS Cruise Configuration //
            /////////////////////////////////
            //DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("1");
            Mecha_CRUISE_CONFIG.CruiseMaxSpeedMultiplier = config.Bind(MECHA_CRUISE_CONFIG, "Maximum cruise speed (Vanilla is 1000m/s)", 1.0,
                new ConfigDescription("Multiplies the base max cruise speed for Icarus with this value (1.0 = vanilla speed). " +
                    "\nin game : " +
                    "\n40000 m is 1AU " +
                    "\n60 AU is 1 LY (2400000 m)"
                ,
                new AcceptableValueRange<double>(0.1, 100.0), null));
            //DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("1");
            Mecha_CRUISE_CONFIG.CruiseAccelerationRateMultiplier = config.Bind(MECHA_CRUISE_CONFIG, "Acceleration rate is 2% current speed / s", 1.0,
                new ConfigDescription("Max cruise acceleration multiplier rate for Icarus.",
                new AcceptableValueRange<double>(0.1, 100.0), null)
            );
            //DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("1");
            Mecha_CRUISE_CONFIG.CruiseAccelerationEnergyCostMultiplier = config.Bind(MECHA_CRUISE_CONFIG, "Energy used to maintain cruise acceleration (Vanilla is 24000 )", 1.0,
                new ConfigDescription("Icarus' Cruise Acceleration Energy Cost multiplier." +
                "\n24000 * speeddiff = finalcost",
                    new AcceptableValueRange<double>(0.0, 100.0), null));

            ///////////////////////////////
            // ICARUS Warp Configuration //
            ///////////////////////////////
            //DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("1");
            Mecha_WARP_CONFIG.maxWarpSpeedMultiplier = config.Bind(MECHA_WARP_CONFIG, "Maximum absolute warp speed (Vanilla is 1000000 M/s)", 1.0,
                new ConfigDescription("Base max warp speed Multiplier of Icarus.",
                new AcceptableValueRange<double>(0.1, 100.0), null));

            Mecha_WARP_CONFIG.warpStartPowerConsumptionMultiplier = config.Bind(MECHA_WARP_CONFIG, "Warp start power per speed (Vanilla is 1600)", 1.0,
                new ConfigDescription("Warp start power per speed Multiplier for Icarus.",
                    new AcceptableValueRange<double>(0.0, 100.0), null));
            //DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("4");
            Mecha_WARP_CONFIG.warpKeepingConsumptionMultiplier = config.Bind(MECHA_WARP_CONFIG, "Warp keeping power per speed (Vanilla is 100)", 1.0,
                new ConfigDescription("Warp keeping power per speed Multiplier for Icarus. ",
                    new AcceptableValueRange<double>(0.0,100.0), null));

            ///////////////////////////////////
            // Logistic Drones Configuration //
            ///////////////////////////////////
            //DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("5");
            Logistic_DRONE_CONFIG.DroneTravelSpeedMutliplier = config.Bind(LOGISTIC_DRONE_CONFIG, "Travel speed (Vanilla is 8 M/s)", 1.0,
                new ConfigDescription("Logistic Drones' base travel speed. ",
                new AcceptableValueRange<double>(.5, 10.0), null ));
            //DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("6");
            Logistic_DRONE_CONFIG.DroneTaxiSpeed = config.Bind(LOGISTIC_DRONE_CONFIG, "taxi speed (Vanilla is sqrt(droneMaxSpeed / 8)).", 1.0,
                new ConfigDescription("Taxi speed of Logistic Drones. (The taxi speed is takeoff/landing speed at the Station)"
                + "\nIt was just too unnatural to see the Drones or Ships docking at ligth speed when research was done."
                + "\nSo I made it fixed and adjustable.",
                new AcceptableValueRange<double>(.5, 32.0), null ));
            //DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("7");
            Logistic_DRONE_CONFIG.DroneEnergyTransitMultiplier = config.Bind(LOGISTIC_DRONE_CONFIG, "Energy consumption per meter (Vanilla is 20000)", 1.0,
                new ConfigDescription("Logistic Drones' energy consumption Multiplier per meter traveled. ",
                new AcceptableValueRange<double>(0.0, 10.0), null ));
            //DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("8");
            Logistic_DRONE_CONFIG.DroneEnergyTakeOffMultiplier = config.Bind(LOGISTIC_DRONE_CONFIG, "Energy consumption on take off (Vanilla is 800000)", 1.0,
                new ConfigDescription("Logistic Drones' energy consumption Multiplier on takeoff. ",
                new AcceptableValueRange<double>(0.0, 10.0), null ));

            //////////////////////////////////
            // Logistic Ships Configuration //
            //////////////////////////////////
            //DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("9");
            Logistic_SHIP_CONFIG.taxiSpeed = config.Bind(LOGISTIC_SHIP_SAIL, "Taxi speed (Vanilla is a fraction of the sailing speed)", 1.0,
                new ConfigDescription("This value will replace the taxi speed for landing and takeoff.",
                new AcceptableValueRange<double>(.5, 100.0), null));
            Logistic_SHIP_CONFIG.approchSpeed = config.Bind(LOGISTIC_SHIP_SAIL, "Maximum approch speed (Vanilla is a fraction of the sailing speed)", 400.0,
                new ConfigDescription("This value will replace the sailing speed for both ends of the trip.",
                new AcceptableValueRange<double>(0.5, 100000.0), null ));
            //DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("1");
            Logistic_SHIP_CONFIG.ShipCruiseSpeedMultiplier = config.Bind(LOGISTIC_SHIP_SAIL, "Base cruise speed (Vanilla is 400 M/s)", 1.0,
                new ConfigDescription("Base cruise speed Multiplier for Logistic Ships. (Base as in before research modifier)",
                new AcceptableValueRange<double>(.01, 100.0), null));
            Logistic_SHIP_CONFIG.ShipWarpSpeedMultiplier = config.Bind(LOGISTIC_SHIP_WARP, "Maximum warp speed (Vanilla is 1000000 M/s)", 1.0,
                new ConfigDescription("Base max warp speed Multiplier of Logistic Ships. (Base as in before research modifier)",
                new AcceptableValueRange<double>(.01, 100.0), null ));
            Logistic_SHIP_CONFIG.ShipEnergyWarpMultiplier = config.Bind(LOGISTIC_SHIP_WARP, "Energy consumption per warp", 1.0,
                new ConfigDescription("Energy consumption Multiplier per warp for Logistic Ships. (Vanilla is 100000000)",
                new AcceptableValueRange<double>(.0, 100.0), null));

            //DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("6");
            Logistic_SHIP_CONFIG.ShipEneregyTransitCostMultiplier = config.Bind(LOGISTIC_SHIP_CONFIG, "Energy consumption per Meter (Vanilla is 30 )", 1.0,
                new ConfigDescription("Logistic Ships' energy consumption Multiplier per meter traveled. ",
                new AcceptableValueRange<double>(0.0, 100.0), null ));
            //DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("7");
            Logistic_SHIP_CONFIG.ShipEnergyMaxSpeedMultiplier = config.Bind(LOGISTIC_SHIP_CONFIG, "Energy consumption For max speed (Vanilla is 200000)", 1.0,
                new ConfigDescription("Logistic Ships' energy consumption Multiplier for atteining max cruise speed."
                + "\nThis value is multiplied by the cruise speed of the ship during the trip (cruise speed value capped at 3000 for the calculation).",
                new AcceptableValueRange<double>(0.0, 100.0), null ));
            //DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("8");
            Logistic_SHIP_CONFIG.ShipEnergyTakeOffMultiplier = config.Bind(LOGISTIC_SHIP_CONFIG, "Energy consumption on takeoff", 1.0,
                new ConfigDescription("Logistic Ships' Energy consumption Multiplier on takeoff. (Vanilla is 6000000)",
                new AcceptableValueRange<double>(0.0, 100.0), null ));
            //DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("9");

            ////////////////////////////
            // Dark Fog Configuration //
            ////////////////////////////
            Dark_Fog_CONFIG.maxEnemyAttackSpeedMultiplier = config.Bind(DARK_FOG_CONFIG, "Battle Unit speed travel speed multiplier", 1.0,
                new ConfigDescription("Ground and Space War unit movement speed Multiplier when traveling for assault.",
                new AcceptableValueRange<double>(0.1, 10.0), null));
            Dark_Fog_CONFIG.maxEnemySpeedMultiplier = config.Bind(DARK_FOG_CONFIG, "Battle Unit speed Multiplier", 1.0,
                new ConfigDescription("Ground and Space War unit movement speed Multiplier.",
                new AcceptableValueRange<double>(0.1, 10.0), null));
            Dark_Fog_CONFIG.maxCarrierSpeedMultiplier = config.Bind(DARK_FOG_CONFIG, "Carrier speed (Vanilla is 1800 M/s)", 1.0,
                new ConfigDescription("Carrier's cruise speed Multiplier.",
                new AcceptableValueRange<double>(0.1, 10.0), null));
            Dark_Fog_CONFIG.maxRelaySpeedMultiplier = config.Bind(DARK_FOG_CONFIG, "Relay speed (Vanilla is 1000 M/s)", 1.0,
                new ConfigDescription("Relay's cruise speed Multiplier.",
                new AcceptableValueRange<double>(0.1, 10.0), null));
            Dark_Fog_CONFIG.maxSeedSpeedMultiplier = config.Bind(DARK_FOG_CONFIG, "Seed speed (Vanilla is 1200 M/s)", 1.0,
                new ConfigDescription("Seed's cruise speed Multiplier.",
                new AcceptableValueRange<double>(0.1, 10.0), null));

            //////////////////////////////////
            // DEBUG CONFIG                 //
            //////////////////////////////////
            //DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("1");
            Debug_CONFIG.DEBUG = config.Bind(DEBUG_CONFIG, "Debug messages :", false,
                new ConfigDescription("Enable log debug messages",
                new AcceptableValueList<bool>(true, false), null));
        }
    }
}