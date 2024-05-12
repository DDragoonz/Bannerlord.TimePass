using System;
using System.Linq;
using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;
using SandBox.Missions.MissionLogics.Arena;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.MountAndBlade;

namespace TimePass
{

    public class TimePassSettings : AttributeGlobalSettings<TimePassSettings>
    {

        public override string Id => "TimePass";
        public override string DisplayName => "Time Pass";
        public override string FolderName => "TimePass";
        public override string FormatType => "json2";

        //settings
        [SettingPropertyBool("Enable Debug", Order = 0, RequireRestart = false, HintText = "Show debug Message", IsToggle = true)]
        [SettingPropertyGroup("Debug", GroupOrder = 100)]
        public bool EnableDebug{ get; set; } = false;

        [SettingPropertyFloatingInteger("Arena", 1f, 60, "0.0 IRL Minute", Order = 0, RequireRestart = false, HintText = "How long 1 hour in game while inside Arena should last (in minute)")]
        [SettingPropertyGroup("Time Settings/Combat", GroupOrder = 20)]
        public float ArenaHourDuration{ get; set; } = 1;

        [SettingPropertyFloatingInteger("Siege Battle", 1f, 60, "0.0 IRL Minute", Order = 0, RequireRestart = false, HintText = "How long 1 hour in game while in Siege Battle should last (in minute)")]
        [SettingPropertyGroup("Time Settings/Combat", GroupOrder = 20)]
        public float SiegeHourDuration{ get; set; } = 1;

        [SettingPropertyFloatingInteger("Outdoor Battle", 1f, 60, "0.0 IRL Minute", Order = 0, RequireRestart = false, HintText = "How long 1 hour in game while in Battlefield should last (in minute)")]
        [SettingPropertyGroup("Time Settings/Combat", GroupOrder = 20)]
        public float BattlefieldDuration{ get; set; } = 1;

        [SettingPropertyFloatingInteger("Hideout Battle", 1f, 60, "0.0 IRL Minute", Order = 0, RequireRestart = false, HintText = "How long 1 hour in game while in Hideout Battle should last (in minute)")]
        [SettingPropertyGroup("Time Settings/Combat", GroupOrder = 20)]
        public float HideoutDuration{ get; set; } = 1;

        [SettingPropertyFloatingInteger("Other Battle", 1f, 60, "0.0 IRL Minute", Order = 10, RequireRestart = false, HintText = "How long 1 hour in game during uncategorized battle should last (in minute)")]
        [SettingPropertyGroup("Time Settings/Combat", GroupOrder = 20)]
        public float DefaultCombatHourDuration{ get; set; } = 1;

        [SettingPropertyFloatingInteger("Town", 1f, 60, "0.0 IRL Minute", Order = 0, RequireRestart = false, HintText = "How long 1 hour in game while inside town should last (in minute)")]
        [SettingPropertyGroup("Time Settings/Exploration", GroupOrder = 20)]
        public float TownHourDuration{ get; set; } = 1;

        [SettingPropertyFloatingInteger("Village", 1f, 60, "0.0 IRL Minute", Order = 0, RequireRestart = false, HintText = "How long 1 hour in game while inside village should last (in minute)")]
        [SettingPropertyGroup("Time Settings/Exploration", GroupOrder = 20)]
        public float VillageDuration{ get; set; } = 1;

        [SettingPropertyFloatingInteger("Indoor", 1f, 60, "0.0 IRL Minute", Order = 0, RequireRestart = false, HintText = "How long 1 hour in game while indoor should last (in minute)")]
        [SettingPropertyGroup("Time Settings/Exploration", GroupOrder = 20)]
        public float IndoorHourDuration{ get; set; } = 1;

        [SettingPropertyFloatingInteger("Other", 1f, 60, "0.0 IRL Minute", Order = 10, RequireRestart = false, HintText = "How long 1 hour in game while indoor should last (in minute)")]
        [SettingPropertyGroup("Time Settings/Exploration", GroupOrder = 20)]
        public float DefaultHourDuration{ get; set; } = 1;

        // [SettingPropertyFloatingInteger("Real Second to World Second Ratio", 1f, 3600f, "0.00", Order = 0, RequireRestart = false, HintText = "how many in game second passed for every one second in real time. bigger value means faster time")]
        // [SettingPropertyGroup("Time Settings", GroupOrder = 20)]
        // public float RealSecondToWorldSecondRatio{ get; set; } = 60;

        [SettingPropertyBool("Sky Visual Update", Order = 0, RequireRestart = false, HintText = "sky visual changes over time. set it to false if you have bad performance issue", IsToggle = true)]
        [SettingPropertyGroup("Visual Settings", GroupOrder = 30)]
        public bool EnableSkyUpdate{ get; set; } = false;

        [SettingPropertyFloatingInteger("Update Interval", 1f, 3600f, "0.0 Second", Order = 1, RequireRestart = false, HintText = "how often sky updates happen (in seconds), set it to bigger value if you have performance issue")]
        [SettingPropertyGroup("Visual Settings", GroupOrder = 20)]
        public float SkyTickTimeInterval{ get; set; } = 5f;

        [SettingPropertyFloatingInteger("Bad Weather Update Interval", 1f, 3600f, "0.0 Second",Order = 2, RequireRestart = false, HintText = "sky tick interval during bad weather")]
        [SettingPropertyGroup("Visual Settings", GroupOrder = 20)]
        public float RainSkyTickTimeInterval{ get; set; } = 5f;

        // UI settings
        [SettingPropertyBool("Display Time", Order = 0, RequireRestart = false, HintText = "Display Time Widget", IsToggle = true)]
        [SettingPropertyGroup("UI", GroupOrder = 40)]
        public bool DisplayTime{ get; set; } = true;

        [SettingPropertyBool("Use 24 Hour Format", Order = 1, RequireRestart = false, HintText = "Use 24 hours format (18:00) instead of (06:00 PM)")]
        [SettingPropertyGroup("UI", GroupOrder = 40)]
        public bool Use24HourFormat{ get; set; } = false;

        // advanced settings
        [SettingPropertyBool("Use Culture Color Scheme", Order = 0, RequireRestart = false, HintText = "Change ambient color based on culture")]
        [SettingPropertyGroup("Visual Settings/Advanced", GroupOrder = 50)]
        public bool UsePerCultureAtmosphereSettings{ get; set; } = true;

        [SettingPropertyBool("Update Color Grade", Order = 1, RequireRestart = false, HintText = "overall color feel inside scene. turn it off if you prefer more consistent color")]
        [SettingPropertyGroup("Visual Settings/Advanced", GroupOrder = 50)]
        public bool UpdateColorGrade{ get; set; } = true;

        [SettingPropertyBool("Update Sky Brightness", Order = 1, RequireRestart = false, HintText = "if turned off sky will remain bright during night, and vice versa", IsToggle = true)]
        [SettingPropertyGroup("Visual Settings/Advanced/Sky", GroupOrder = 50)]
        public bool UpdateSkyBrightness{ get; set; } = true;

        [SettingPropertyFloatingInteger("Minimum Sky Brightness", 0.3f, 1000f, Order = 1, RequireRestart = false, HintText = "Minimum Sky Brightness won't be higher than initial scene brightness")]
        [SettingPropertyGroup("Visual Settings/Advanced/Sky", GroupOrder = 50)]
        public float MinimumSkyBrightness{ get; set; } = 100f;

        [SettingPropertyBool("Update Sky Texture", Order = 0, RequireRestart = false, HintText = "Update Sky box Texture. Might cause sky flickering!")]
        [SettingPropertyGroup("Visual Settings/Advanced/Sky", GroupOrder = 50)]
        public bool UpdateSkybox{ get; set; } = true;

        [SettingPropertyBool("Update Sun Color and Intensity", Order = 2, RequireRestart = false, HintText = "")]
        [SettingPropertyGroup("Visual Settings/Advanced/Sun", GroupOrder = 50)]
        public bool UpdateSunColorAndIntensity{ get; set; } = true;

        [SettingPropertyBool("Update Sun Shaft", Order = 2, RequireRestart = false, HintText = "")]
        [SettingPropertyGroup("Visual Settings/Advanced/Sun", GroupOrder = 50)]
        public bool UpdateSunShaft{ get; set; } = true;

        [SettingPropertyBool("Update Sun Size", Order = 2, RequireRestart = false, HintText = "disabled by default, because it sometimes looks weird to see sun size grow and shrink overtime")]
        [SettingPropertyGroup("Visual Settings/Advanced/Sun", GroupOrder = 50)]
        public bool UpdateSunSize{ get; set; } = false;

        [SettingPropertyBool("Update Brightpass Threshold", Order = 0, RequireRestart = false, HintText = "")]
        [SettingPropertyGroup("Visual Settings/Advanced/Post Pro", GroupOrder = 50)]
        public bool UpdateBrightpassThreshold{ get; set; } = true;

        [SettingPropertyBool("Update Middle Gray", Order = 0, RequireRestart = false, HintText = "")]
        [SettingPropertyGroup("Visual Settings/Advanced/Post Pro", GroupOrder = 50)]
        public bool UpdateMiddleGray{ get; set; } = true;

        [SettingPropertyBool("Update Fog Ambient Color", Order = 0, RequireRestart = false, HintText = "similar with color grade but for fog")]
        [SettingPropertyGroup("Visual Settings/Advanced/Fog", GroupOrder = 50)]
        public bool UpdateAmbientColor{ get; set; } = false;

        [SettingPropertyBool("Update Fog", Order = 0, RequireRestart = false, HintText = "if realistic weather installed, fog won't be updated anyway, since it already has it's own fog settings")]
        [SettingPropertyGroup("Visual Settings/Advanced/Fog", GroupOrder = 50)]
        public bool UpdateFog{ get; set; } = true;

        [SettingPropertyBool("Update Exposure", Order = 2, RequireRestart = false, HintText = "")]
        [SettingPropertyGroup("Visual Settings/Advanced", GroupOrder = 50)]
        public bool UpdateExposure{ get; set; } = true;

        public float GetTimePassDuration(TimePassLocationEnum location, bool isCombat)
        {
            switch (location)
            {
                case TimePassLocationEnum.Arena:
                    return ArenaHourDuration;
                case TimePassLocationEnum.Field:
                    return BattlefieldDuration;
                case TimePassLocationEnum.Hideout:
                    return HideoutDuration;
                case TimePassLocationEnum.Village:
                    return VillageDuration;
                case TimePassLocationEnum.Town:
                    return TownHourDuration;
                case TimePassLocationEnum.Siege:
                    return SiegeHourDuration;
                case TimePassLocationEnum.Indoor:
                    return IndoorHourDuration;
                default:
                    break;
            }

            return isCombat ? DefaultCombatHourDuration : DefaultHourDuration;
        }


    }
}