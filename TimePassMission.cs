using System;
using System.Collections.Generic;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ScreenSystem;

namespace TimePass
{
    public class TimePassMission : MissionLogic
    {
        public override void AfterStart()
        {
            base.AfterStart();
            if (TimePassSettings.Instance.enableDebug)
                InformationManager.DisplayMessage(
                    new InformationMessage("Mission Initialize! CurrentTime : " +
                                           TimePassSkyInfo.GetCurrentTimeOfDay()));

            // allow first tick to always update sky
            skyTickTime = Math.Max(TimePassSettings.Instance.realSecondToWorldSecondRatio, TimePassSettings.Instance.rainSkyTickTimeInterval);
            lastTickHour = Campaign.Current == null ? 0 : (int)CampaignTime.Now.CurrentHourInDay;

            Type realisticWeatherManagerType = AccessTools.TypeByName("RealisticWeather.RealisticWeatherManager");
            realisticWeatherInstalled = realisticWeatherManagerType != null;
            if (realisticWeatherInstalled)
            {
                Traverse realisticWeatherTraverse = Traverse.Create(realisticWeatherManagerType);
                hasDust = realisticWeatherTraverse.Field("_realisticWeatherManager").Method("get_HasDust").GetValue<bool>();
            }

            MissionInitializerRecord initializerRecord = Traverse.Create(Mission).Field("<InitializerRecord>k__BackingField").GetValue<MissionInitializerRecord>();
            interpAtmosphere = initializerRecord.AtmosphereOnCampaign.InterpolatedAtmosphereName; // filled during campaign, null during custom battle
            presetAtmosphere = initializerRecord.AtmosphereOnCampaign.AtmosphereName; // null during campagin, filled during custom battle

            InitUI();
        }

        private void InitUI()
        {
            if (!TimePassSettings.Instance.displayTime)
            {
                return;
            }
            
            GauntletLayer layer = new GauntletLayer(100);
            timePassVM = new TimePassVM();
            layer.LoadMovie("TimePassTimeWidget", timePassVM);
            ScreenManager.TopScreen.AddLayer(layer);
        }

        public override void OnPreDisplayMissionTick(float dt)
        {
            base.OnPreDisplayMissionTick(dt);

            if (Mission.Current != null && Mission.Scene != null && Mission.Scene.IsLoadingFinished())
            {
                float secondTick = dt * TimePassSettings.Instance.realSecondToWorldSecondRatio;

                // tick campagin time
                Traverse.Create(Campaign.Current).Field("<MapTimeTracker>k__BackingField")
                    .Method("Tick", secondTick).GetValue();

                if (timePassVM != null)
                {
                    timePassVM.OnPropertyChanged("TimePassTimeOfDayText");
                }
            }

            if (!TimePassSettings.Instance.disableSkyUpdate)
            {
                UpdateSky(dt);
            }
        }

        private void UpdateSky(float dt)
        {
            if (Mission.Scene == null || Mission.Scene.IsAtmosphereIndoor
                                      || !Mission.Scene.IsLoadingFinished())
            {
                return;
            }


            // update sky tick counter
            if (!UpdateSkyTickCounter(dt))
            {
                return;
            }


            // if (currentHour != lastTickHour)
            // {
            // runtime atmosphere changing is currently disabled. 
            // reason 1 : changing atmosphere in runtime will causing brief flicker / flash, which might causing discomfort
            // reason 2 : for some reason, changing atmosphere in runtime might delete some scene objects (road, tree)
            // downside : skybox will remain same as player enter the scene, if player enter scene during day and time passed until night, 
            // it will still show sun instead moon, and vice versa

            // Mission.Scene.SetAtmosphereWithName(atmosphereName);
            // if(TimePassSettings.Instance.enableDebug)InformationManager.DisplayMessage(new InformationMessage("Current Atmosphere : " + atmosphereName));

            //     lastTickHour = currentHour;
            // }


            bool isBadWeather = Mission.Current.Scene.GetRainDensity() > 0.0f;

            TimePassSkyInfo targetSkyInfo = TimePassSkyInfo.GetTargetSkyInfo(TimePassSkyInfo.GetCurrentTimeOfDay(), isBadWeather, interpAtmosphere);


            // realistic weather dust storm
            if (hasDust)
            {
                // calculation taken from realistic weather
                targetSkyInfo.sky_brightness = (TimePassSkyInfo.GetCurrentTimeOfDay() < 12f)
                    ? ((MathF.Pow(2f, TimePassSkyInfo.GetCurrentTimeOfDay()) - 1f) / 10f)
                    : ((MathF.Pow(2f, 24f - TimePassSkyInfo.GetCurrentTimeOfDay()) - 1f) / 10f);
                Mission.Scene.SetSceneColorGradeIndex(23);
            }
            else if (TimePassSettings.Instance.updateColorGrade)
            {
                Mission.Scene.SetColorGradeBlend(targetSkyInfo.color_grade_name, targetSkyInfo.color_grade_name2,
                    targetSkyInfo.color_grade_alpha);
            }


            if (TimePassSettings.Instance.updateSunColorAndIntensity)
            {
                Mission.Scene.SetSun(ref targetSkyInfo.sun_color, targetSkyInfo
                    .sun_altitude, targetSkyInfo.sun_angle, targetSkyInfo.sun_intesity);
            }
            else
            {
                Mission.Scene.SetSunAngleAltitude(targetSkyInfo.sun_angle, targetSkyInfo.sun_altitude);
            }

            // Warning : changing sun size overtime is somehow ugly.. because the size is not really consistent
            if (TimePassSettings.Instance.updateSunSize) Mission.Scene.SetSunSize(targetSkyInfo.sun_size);
            if (TimePassSettings.Instance.updateSunShaft) Mission.Scene.SetSunShaftStrength(targetSkyInfo.sunshafts_strength);
            if (TimePassSettings.Instance.updateSkyBrightness) Mission.Scene.SetSkyBrightness(targetSkyInfo.sky_brightness);
            if (TimePassSettings.Instance.updateExposure)
            {
                Mission.Scene.SetMaxExposure(targetSkyInfo.max_exposure);
                Mission.Scene.SetMinExposure(targetSkyInfo.min_exposure);
                Mission.Scene.SetTargetExposure(targetSkyInfo.target_exposure);
            }

            if (TimePassSettings.Instance.updateBrightpassThreshold) Mission.Scene.SetBrightpassThreshold(targetSkyInfo.brightpass_threshold);
            if (TimePassSettings.Instance.updateMiddleGray) Mission.Scene.SetMiddleGray(targetSkyInfo.middle_gray);
            // Warning : it might causing color inconsistency over time
            if (TimePassSettings.Instance.updateAmbientColor) Mission.Scene.SetFogAmbientColor(ref targetSkyInfo.fog_ambient_color);

            // realistic weather has it's own fog settings
            if (!realisticWeatherInstalled && TimePassSettings.Instance.updateFog)
            {
                Mission.Scene.SetFog(targetSkyInfo.fog_density, ref targetSkyInfo.fog_color, targetSkyInfo.fog_falloff);
            }


            if (TimePassSettings.Instance.enableDebug)
            {
                InformationManager.DisplayMessage(new InformationMessage("Sky Info : " + targetSkyInfo));
                InformationManager.DisplayMessage(new InformationMessage("rain density : " + Mission.Scene
                    .GetRainDensity() + " snow density : " + Mission.Scene.GetSnowDensity()));
            }
        }


        // return true if should tick sky
        private bool UpdateSkyTickCounter(float dt)
        {
            skyTickTime += dt;

            float tickInterval = TimePassSettings.Instance.skyTickTimeInterval;

            // unfortunately dependency on rain particle prefab is not reliable, there still a case where rain prefab is null but rain particle still exist
            // GameEntity rainPrefab = Mission.Scene.GetFirstEntityWithName("rain_prefab_entity") ?? Mission.Scene.GetFirstEntityWithName("snow_prefab_entity");
            // bool hasRainParticle = rainPrefab != null && rainPrefab.ChildCount > 0;
            bool hasRainParticle = Mission.Current.Scene.GetRainDensity() > 0;
            // override tick interval to rainSkyTickTimeInterval during bad weather (snowy or rain).
            // because it will make rain / snow particle goes crazy if updating too much in short period
            if (hasRainParticle && tickInterval < TimePassSettings.Instance.rainSkyTickTimeInterval)
            {
                tickInterval = TimePassSettings.Instance.rainSkyTickTimeInterval;
            }

            if (skyTickTime < tickInterval)
            {
                return false;
            }

            skyTickTime = 0;

            return true;
        }


        // cache
        private float skyTickTime = 0;
        private int lastTickHour = -1;
        private bool realisticWeatherInstalled = false;
        private bool hasDust = false;

        private string interpAtmosphere;
        private string presetAtmosphere;

        private TimePassVM timePassVM;
    }
}