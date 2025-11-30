using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;

namespace TimePass
{
    public class TimePassSettings : AttributeGlobalSettings<TimePassSettings>
    {
        public override string Id => "TimePass";
        public override string DisplayName => "Time Pass";
        public override string FolderName => "TimePass";
        public override string FormatType => "json2";

        // -------------------------
        // Combat Time Multipliers
        // -------------------------
        [SettingPropertyInteger("{=TP_Combat_Arena}Arena", -10, 10, "0", Order = 0, RequireRestart = false,
            HintText = "{=TP_Combat_Arena_Hint}How fast the time passed while inside Arena. 0 means disabled, negative means slower")]
        [SettingPropertyGroup("{=TP_Group_TimeMultiplier}Time Multiplier/{=TP_Group_Combat}Combat", GroupOrder = 20)]
        public int ArenaTimeMultiplier { get; set; } = 1;

        [SettingPropertyInteger("{=TP_Combat_Siege}Siege Battle", -10, 10, "0", Order = 0, RequireRestart = false,
            HintText = "{=TP_Combat_Siege_Hint}How fast the time passed during Siege Battle. 0 means disabled, negative means slower")]
        [SettingPropertyGroup("{=TP_Group_TimeMultiplier}Time Multiplier/{=TP_Group_Combat}Combat", GroupOrder = 20)]
        public int SiegeTimeMultiplier { get; set; } = 1;

        [SettingPropertyInteger("{=TP_Combat_SallyOut}Sally Out Battle", -10, 10, "0", Order = 0, RequireRestart = false,
            HintText = "{=TP_Combat_SallyOut_Hint}How fast the time passed during Sally Out Battle. 0 means disabled, negative means slower")]
        [SettingPropertyGroup("{=TP_Group_TimeMultiplier}Time Multiplier/{=TP_Group_Combat}Combat", GroupOrder = 20)]
        public int SallyOutTimeMultiplier { get; set; } = 1;

        [SettingPropertyInteger("{=TP_Combat_Outdoor}Outdoor Battle", -10, 10, "0", Order = 0, RequireRestart = false,
            HintText = "{=TP_Combat_Outdoor_Hint}How fast the time passed while in Battlefield. 0 means disabled, negative means slower")]
        [SettingPropertyGroup("{=TP_Group_TimeMultiplier}Time Multiplier/{=TP_Group_Combat}Combat", GroupOrder = 20)]
        public int BattlefieldDuration { get; set; } = 1;

        [SettingPropertyInteger("{=TP_Combat_Hideout}Hideout Battle", -10, 10, "0", Order = 0, RequireRestart = false,
            HintText = "{=TP_Combat_Hideout_Hint}How fast the time passed during Hideout Battle. 0 means disabled, negative means slower")]
        [SettingPropertyGroup("{=TP_Group_TimeMultiplier}Time Multiplier/{=TP_Group_Combat}Combat", GroupOrder = 20)]
        public int HideoutTimeMultiplier { get; set; } = 1;

        [SettingPropertyInteger("{=TP_Combat_Stealth}Stealth Mission", -10, 10, "0", Order = 0, RequireRestart = false,
            HintText = "{=TP_Combat_Stealth_Hint}How fast the time passed during Stealth Mission. 0 means disabled, negative means slower")]
        [SettingPropertyGroup("{=TP_Group_TimeMultiplier}Time Multiplier/{=TP_Group_Combat}Combat", GroupOrder = 20)]
        public int StealthTimeMultiplier { get; set; } = -3;

        [SettingPropertyInteger("{=TP_Combat_Naval}Naval Battle", -10, 10, "0", Order = 0, RequireRestart = false,
            HintText = "{=TP_Combat_Naval_Hint}How fast the time passed during Naval Battle. 0 means disabled, negative means slower")]
        [SettingPropertyGroup("{=TP_Group_TimeMultiplier}Time Multiplier/{=TP_Group_Combat}Combat", GroupOrder = 20)]
        public int NavalTimeMultiplier { get; set; } = 1;

        [SettingPropertyInteger("{=TP_Combat_Other}Other Battle", -10, 10, "0", Order = 10, RequireRestart = false,
            HintText = "{=TP_Combat_Other_Hint}How fast the time passed during uncategorized battle. 0 means disabled, negative means slower")]
        [SettingPropertyGroup("{=TP_Group_TimeMultiplier}Time Multiplier/{=TP_Group_Combat}Combat", GroupOrder = 20)]
        public int DefaultCombatTimeMultiplier { get; set; } = 1;

        // -------------------------
        // Exploration Time Multiplier
        // -------------------------
        [SettingPropertyInteger("{=TP_Explore_Town}Town", -10, 10, "0", Order = 0, RequireRestart = false,
            HintText = "{=TP_Explore_Town_Hint}How fast the time passed while inside town. 0 means disabled, negative means slower")]
        [SettingPropertyGroup("{=TP_Group_TimeMultiplier}Time Multiplier/{=TP_Group_Exploration}Exploration", GroupOrder = 20)]
        public int TownTimeMultiplier { get; set; } = 1;

        [SettingPropertyInteger("{=TP_Explore_Village}Village", -10, 10, "0", Order = 0, RequireRestart = false,
            HintText = "{=TP_Explore_Village_Hint}How fast the time passed while inside village. 0 means disabled, negative means slower")]
        [SettingPropertyGroup("{=TP_Group_TimeMultiplier}Time Multiplier/{=TP_Group_Exploration}Exploration", GroupOrder = 20)]
        public int VillageTimeMultiplier { get; set; } = 1;

        [SettingPropertyInteger("{=TP_Explore_Indoor}Indoor", -10, 10, "0", Order = 0, RequireRestart = false,
            HintText = "{=TP_Explore_Indoor_Hint}How fast the time passed while indoor. 0 means disabled, negative means slower")]
        [SettingPropertyGroup("{=TP_Group_TimeMultiplier}Time Multiplier/{=TP_Group_Exploration}Exploration", GroupOrder = 20)]
        public int IndoorTimeMultiplier { get; set; } = 1;

        [SettingPropertyInteger("{=TP_Explore_Other}Other", -10, 10, "0", Order = 10, RequireRestart = false,
            HintText = "{=TP_Explore_Other_Hint}How fast the time passed while indoor. 0 means disabled, negative means slower")]
        [SettingPropertyGroup("{=TP_Group_TimeMultiplier}Time Multiplier/{=TP_Group_Exploration}Exploration", GroupOrder = 20)]
        public int DefaultTimeMultiplier { get; set; } = 1;

        // -------------------------
        // UI
        // -------------------------
        [SettingPropertyBool("{=TP_UI_DisplayTime}Display Time", Order = 0, RequireRestart = false,
            HintText = "{=TP_UI_DisplayTime_Hint}Display Time Widget", IsToggle = true)]
        [SettingPropertyGroup("{=TP_Group_UI}UI", GroupOrder = 30)]
        public bool DisplayTime { get; set; } = true;

        [SettingPropertyBool("{=TP_UI_24H}Use 24 Hour Format", Order = 1, RequireRestart = false,
            HintText = "{=TP_UI_24H_Hint}Use 24 hours format (18:00) instead of (06:00 PM)")]
        [SettingPropertyGroup("{=TP_Group_UI}UI", GroupOrder = 30)]
        public bool Use24HourFormat { get; set; } = false;

        // -------------------------
        // VISUAL: SKY CHANGES
        // -------------------------
        [SettingPropertyBool("{=TP_Visual_SkyUpdate}Sky Visual Update", Order = 0, RequireRestart = false,
            HintText = "{=TP_Visual_SkyUpdate_Hint}Sky visual changes over time. Set to false if you have performance issue", IsToggle = true)]
        [SettingPropertyGroup("{=TP_Group_Visual}Visual Settings", GroupOrder = 40)]
        public bool EnableSkyUpdate { get; set; } = true;

        // -------------------------
        // VISUAL: ATMOSPHERE SOURCES
        // -------------------------
        [SettingPropertyBool("{=TP_Atmos_Interpolated}Include Culture Interpolated Atmosphere", Order = 0, RequireRestart = false,
            HintText = "{=TP_Atmos_Interpolated_Hint}Include Interpolated atmosphere (old atmosphere from pre 1.3.0)")]
        [SettingPropertyGroup("{=TP_Group_Visual}Visual Settings/{=TP_Group_Atmosphere}Atmosphere", GroupOrder = 40)]
        public bool IncludeCultureInterpolatedAtmosphere { get; set; } = true;

        [SettingPropertyBool("{=TP_Atmos_OldPreset}Include Old Preset Atmosphere", Order = 0, RequireRestart = false,
            HintText = "{=TP_Atmos_OldPreset_Hint}Include old preset atmosphere? (Modules\\Native\\Atmospheres\\TOD_xxx)")]
        [SettingPropertyGroup("{=TP_Group_Visual}Visual Settings/{=TP_Group_Atmosphere}Atmosphere", GroupOrder = 40)]
        public bool IncludeOldPresetAtmosphere { get; set; } = true;

        [SettingPropertyBool("{=TP_Atmos_NewPreset}Include New Preset Atmosphere", Order = 0, RequireRestart = false,
            HintText = "{=TP_Atmos_NewPreset_Hint}Include new preset atmosphere? (Modules\\Native\\Atmospheres\\TOD_photo_xxx)")]
        [SettingPropertyGroup("{=TP_Group_Visual}Visual Settings/{=TP_Group_Atmosphere}Atmosphere", GroupOrder = 40)]
        public bool IncludeNewPresetAtmosphere { get; set; } = true;
        
        [SettingPropertyBool("{=TP_Atmos_NavalPresetDefault}Include Naval Preset Atmosphere in Non Naval Scene", Order = 1, RequireRestart = false,
            HintText = "{=TP_Atmos_NavalPresetDefault_Hint}Include Naval preset atmosphere in non naval scene? (Modules\\NavalDLC\\Atmospheres\\TOD_naval_xxx)")]
        [SettingPropertyGroup("{=TP_Group_Visual}Visual Settings/{=TP_Group_Atmosphere}Atmosphere", GroupOrder = 40)]
        public bool IncludeNavalPresetAtmosphereInDefault { get; set; } = true;
        
        [SettingPropertyBool("{=TP_Atmos_DefaultPresetNaval}Include Non Naval Preset Atmosphere in Naval Scene", Order = 1, RequireRestart = false,
            HintText = "{=TP_Atmos_DefaultPresetNaval_Hint}Include Non Naval preset atmosphere in Naval scene? (Modules\\NavalDLC\\Atmospheres\\TOD_naval_xxx)")]
        [SettingPropertyGroup("{=TP_Group_Visual}Visual Settings/{=TP_Group_Atmosphere}Atmosphere", GroupOrder = 40)]
        public bool IncludeDefaultPresetAtmosphereInNaval { get; set; } = true;

        // -------------------------
        // VISUAL: ADVANCED
        // -------------------------
        [SettingPropertyBool("{=TP_Visual_LightingHack}Enable Lighting Hack", Order = 0, RequireRestart = false,
            HintText = "{=TP_Visual_LightingHack_Hint}Enable 'fix' for lighting too bright or too dark issue by initialize scene at night time.")]
        [SettingPropertyGroup("{=TP_Group_Visual}Visual Settings/{=TP_Group_Advanced}Advanced", GroupOrder = 50)]
        public bool EnableLightingHack { get; set; } = true;
        
        [SettingPropertyBool("{=TP_Adv_ColorGrade}Update Color Grade", Order = 1, RequireRestart = false,
            HintText = "{=TP_Adv_ColorGrade_Hint}Overall color feel inside scene. Disable for consistent color")]
        [SettingPropertyGroup("{=TP_Group_Visual}Visual Settings/{=TP_Group_Advanced}Advanced", GroupOrder = 50)]
        public bool UpdateColorGrade { get; set; } = true;

        [SettingPropertyBool("{=TP_Adv_EnvMult}Update Env Multiplier", Order = 1, RequireRestart = false)]
        [SettingPropertyGroup("{=TP_Group_Visual}Visual Settings/{=TP_Group_Advanced}Advanced", GroupOrder = 50)]
        public bool UpdateEnvMultiplier { get; set; } = true;

        [SettingPropertyBool("{=TP_Sky_UpdateBrightness}Update Sky Brightness", Order = 1, RequireRestart = false,
            HintText = "{=TP_Sky_UpdateBrightness_Hint}If turned off sky remains bright at night and vice versa")]
        [SettingPropertyGroup("{=TP_Group_Visual}Visual Settings/{=TP_Group_Sky}Sky", GroupOrder = 50)]
        public bool UpdateSkyBrightness { get; set; } = true;

        [SettingPropertyBool("{=TP_Sky_UpdateTexture}Update Sky Texture", Order = 1, RequireRestart = false,
            HintText = "{=TP_Sky_UpdateTexture_Hint}Update skybox texture. Might cause flickering!")]
        [SettingPropertyGroup("{=TP_Group_Visual}Visual Settings/{=TP_Group_Sky}Sky", GroupOrder = 50)]
        public bool UpdateSkybox { get; set; } = true;

        [SettingPropertyBool("{=TP_Sun_UpdateColor}Update Sun Color and Intensity", Order = 2, RequireRestart = false)]
        [SettingPropertyGroup("{=TP_Group_Visual}Visual Settings/{=TP_Group_Sun}Sun", GroupOrder = 50)]
        public bool UpdateSunColorAndIntensity { get; set; } = true;

        [SettingPropertyBool("{=TP_Sun_UpdateShaft}Update Sun Shaft", Order = 2, RequireRestart = false)]
        [SettingPropertyGroup("{=TP_Group_Visual}Visual Settings/{=TP_Group_Sun}Sun", GroupOrder = 50)]
        public bool UpdateSunShaft { get; set; } = true;

        [SettingPropertyBool("{=TP_PP_Brightpass}Update Brightpass Threshold", Order = 1, RequireRestart = false)]
        [SettingPropertyGroup("{=TP_Group_Visual}Visual Settings/{=TP_Group_PostPro}Post Pro", GroupOrder = 50)]
        public bool UpdateBrightpassThreshold { get; set; } = true;

        [SettingPropertyBool("{=TP_PP_MiddleGray}Update Middle Gray", Order = 1, RequireRestart = false)]
        [SettingPropertyGroup("{=TP_Group_Visual}Visual Settings/{=TP_Group_PostPro}Post Pro", GroupOrder = 50)]
        public bool UpdateMiddleGray { get; set; } = true;

        [SettingPropertyBool("{=TP_Fog_Ambient}Update Fog Ambient Color", Order = 1, RequireRestart = false,
            HintText = "{=TP_Fog_Ambient_Hint}Similar to color grade but for fog")]
        [SettingPropertyGroup("{=TP_Group_Visual}Visual Settings/{=TP_Group_Fog}Fog", GroupOrder = 50)]
        public bool UpdateAmbientColor { get; set; } = true;

        [SettingPropertyBool("{=TP_Fog_Main}Update Fog", Order = 1, RequireRestart = false,
            HintText = "{=TP_Fog_Main_Hint}Disabled if Realistic Weather installed")]
        [SettingPropertyGroup("{=TP_Group_Visual}Visual Settings/{=TP_Group_Fog}Fog", GroupOrder = 50)]
        public bool UpdateFog { get; set; } = true;

        [SettingPropertyBool("{=TP_Adv_Exposure}Update Exposure", Order = 2, RequireRestart = false)]
        [SettingPropertyGroup("{=TP_Group_Visual}Visual Settings/{=TP_Group_Advanced}Advanced", GroupOrder = 50)]
        public bool UpdateExposure { get; set; } = true;

        // -------------------------
        // DEBUG
        // -------------------------
        [SettingPropertyBool("{=TP_Debug_Enable}Enable Debug", Order = 0, RequireRestart = false,
            HintText = "{=TP_Debug_Enable_Hint}Show debug messages", IsToggle = true)]
        [SettingPropertyGroup("{=TP_Group_Debug}Debug", GroupOrder = 100)]
        public bool EnableDebug { get; set; } = false;

        // -------------------------
        // LOGIC
        // -------------------------
        public float GetTimePassMultiplier(TimePassLocationEnum location, bool isCombat)
        {
            switch (location)
            {
                case TimePassLocationEnum.Arena:
                    return ArenaTimeMultiplier >= 0 ? ArenaTimeMultiplier : -1 / (float)ArenaTimeMultiplier;
                case TimePassLocationEnum.Field:
                    return BattlefieldDuration >= 0 ? BattlefieldDuration : -1 / (float)BattlefieldDuration;
                case TimePassLocationEnum.Hideout:
                    return HideoutTimeMultiplier >= 0 ? HideoutTimeMultiplier : -1 / (float)HideoutTimeMultiplier;
                case TimePassLocationEnum.Village:
                    return VillageTimeMultiplier >= 0 ? VillageTimeMultiplier : -1 / (float)VillageTimeMultiplier;
                case TimePassLocationEnum.Town:
                    return TownTimeMultiplier >= 0 ? TownTimeMultiplier : -1 / (float)TownTimeMultiplier;
                case TimePassLocationEnum.Stealth:
                    return StealthTimeMultiplier >= 0 ? StealthTimeMultiplier : -1 / (float)StealthTimeMultiplier;
                case TimePassLocationEnum.Siege:
                    return SiegeTimeMultiplier >= 0 ? SiegeTimeMultiplier : -1 / (float)SiegeTimeMultiplier;
                case TimePassLocationEnum.SallyOut:
                    return SallyOutTimeMultiplier >= 0 ? SallyOutTimeMultiplier : -1 / (float)SallyOutTimeMultiplier;
                case TimePassLocationEnum.Indoor:
                    return IndoorTimeMultiplier >= 0 ? IndoorTimeMultiplier : -1 / (float)IndoorTimeMultiplier;
                case TimePassLocationEnum.Naval:
                    return NavalTimeMultiplier >= 0 ? NavalTimeMultiplier : -1 / (float)NavalTimeMultiplier;
                case TimePassLocationEnum.Other:
                default:
                    if (isCombat)
                        return DefaultCombatTimeMultiplier >= 0 ? DefaultCombatTimeMultiplier : -1 / (float)DefaultCombatTimeMultiplier;
                    else
                        return DefaultTimeMultiplier >= 0 ? DefaultTimeMultiplier : -1 / (float)DefaultTimeMultiplier;
            }
        }
    }
}
