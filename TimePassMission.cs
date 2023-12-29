using System;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

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

            skyTickTime = 0.0f;
            lastTickHour = Campaign.Current == null ? 0 : (int)CampaignTime.Now.CurrentHourInDay;
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
            }

            if (!TimePassSettings.Instance.disableSkyUpdate)
            {
                UpdateSky(dt);
            }
        }

        private void UpdateSky(float dt)
        {
            if (Mission.Current == null || Mission.Scene == null || Mission.Scene.IsAtmosphereIndoor
                || !Mission.Scene.IsLoadingFinished())
            {
                return;
            }
            
            bool isBadWeather = Mission.Scene.GetRainDensity() > 0.0f;

            // update sky tick counter
            if (!UpdateSkyTickCounter(dt))
            {
                return;
            }

            
            int currentHour = (int)TimePassSkyInfo.GetCurrentTimeOfDay();
            int unclampedAtmosphereHour = TimePassSkyInfo.GetUnclampedAtmosphereHour(currentHour,isBadWeather);
            int atmosphereHour = TimePassSkyInfo.ClampAtmosphereHour(unclampedAtmosphereHour);

            // update whole atmosphere once in an hour
            string atmosphereName = TimePassSkyInfo.GetAtmosphereName(atmosphereHour, isBadWeather);
            TimePassSkyInfo skyInfo = TimePassSkyInfo.GetOrReadSkyInfo(currentHour, atmosphereName);
            
            if (currentHour != lastTickHour)
            {
                // runtime atmosphere changing is currently disabled. 
                // reason 1 : changing atmosphere in runtime will causing brief flicker / flash, which might causing discomfort
                // reason 2 : for some reason, changing atmosphere in runtime might delete some scene objects (road, tree)
                // downside : skybox will remain same as player enter the scene, if player enter scene during day and time passed until night, 
                // it will still show sun instead moon, and vice versa

                // Mission.Scene.SetAtmosphereWithName(atmosphereName);
                // if(TimePassSettings.Instance.enableDebug)InformationManager.DisplayMessage(new InformationMessage("Current Atmosphere : " + atmosphereName));

                lastTickHour = currentHour;
            }


            // lerp sun position every tick
            int unclampedNextAtmosphereHour = TimePassSkyInfo.GetUnclampedNextAtmosphereHour(currentHour,isBadWeather);
            int nextAtmosphereHour = TimePassSkyInfo.ClampAtmosphereHour(unclampedNextAtmosphereHour);
            int timeDiff = Math.Abs(unclampedNextAtmosphereHour - unclampedAtmosphereHour);
            float hourProgress = TimePassSkyInfo.GetCurrentTimeOfDay() - unclampedAtmosphereHour;
            // if atmosphere changed
            if (timeDiff > 0)
            {
                hourProgress /= timeDiff;
            }


            string nextAtmosphereName = TimePassSkyInfo.GetAtmosphereName(nextAtmosphereHour, isBadWeather);
            TimePassSkyInfo nextSkyInfo = TimePassSkyInfo.GetOrReadSkyInfo(unclampedNextAtmosphereHour, nextAtmosphereName);
            
            TimePassSkyInfo lerpSkyInfo = TimePassSkyInfo.Lerp(skyInfo, nextSkyInfo, hourProgress);
            // Mission.Scene.SetSkyRotation(lerpSkyInfo.skybox_rotation);

            float normalizedHour = (TimePassSkyInfo.GetCurrentTimeOfDay() % 24) / 24;
            bool isSunMoon;
            float timeFactorForSnow = 1, timeFactorForRain = 1;
            if (Campaign.Current != null)
            {
                TimePassDefaultSkyParamCalculator.GetSeasonTimeFactorOfCampaignTime(CampaignTime.Now,
                    out timeFactorForSnow,
                    out timeFactorForRain);
            }


            // get sun position from this function is smoother than lerping between sky info
            TimePassDefaultSkyParamCalculator.GetSunPosition(normalizedHour, timeFactorForSnow,
                out lerpSkyInfo.sun_altitude, out lerpSkyInfo.sun_angle, out
                isSunMoon);


            // realistic weather dust storm
            // lerpSkyInfo.sky_brightness = (TimePassSkyInfo.GetCurrentTimeOfDay() < 12f)
            //     ? ((MathF.Pow(2f, TimePassSkyInfo.GetCurrentTimeOfDay()) - 1f) / 10f)
            //     : ((MathF.Pow(2f, 24f - TimePassSkyInfo.GetCurrentTimeOfDay()) - 1f) / 10f);
            // Mission.Scene.SetSceneColorGradeIndex(23);
            

            Mission.Scene.SetSun(ref lerpSkyInfo.sun_color, lerpSkyInfo.sun_altitude
                , lerpSkyInfo.sun_angle, lerpSkyInfo.sun_intesity);
            // Mission.Scene.SetSunAngleAltitude(lerpSkyInfo.sun_angle,lerpSkyInfo.sun_altitude);
            // Mission.Scene.SetSunSize(lerpSkyInfo.sun_size);
            Mission.Scene.SetSunShaftStrength(lerpSkyInfo.sunshafts_strength);
            Mission.Scene.SetSkyBrightness(lerpSkyInfo.sky_brightness);
            Mission.Scene.SetMaxExposure(lerpSkyInfo.max_exposure);
            Mission.Scene.SetMinExposure(lerpSkyInfo.min_exposure);
            Mission.Scene.SetTargetExposure(lerpSkyInfo.target_exposure);
            Mission.Scene.SetBrightpassThreshold(lerpSkyInfo.brightpass_threshold);
            Mission.Scene.SetFogAmbientColor(ref lerpSkyInfo.fog_ambient_color);
            Mission.Scene.SetColorGradeBlend(skyInfo.color_grade_name,nextSkyInfo.color_grade_name,hourProgress);
            
            


            if (TimePassSettings.Instance.enableDebug)
            {
                InformationManager.DisplayMessage(new InformationMessage("Sky Info : " + lerpSkyInfo));
                InformationManager.DisplayMessage(new InformationMessage("rain density : " + Mission.Scene
                    .GetRainDensity() + " snow density : " + Mission.Scene.GetSnowDensity()));
            }
        }


        // return true if should tick sky
        private bool UpdateSkyTickCounter(float dt)
        {
            skyTickTime += dt;
            
            float tickInterval = TimePassSettings.Instance.skyTickTimeInterval;

            GameEntity rainPrefab = Mission.Scene.GetFirstEntityWithName("rain_prefab_entity") ?? Mission.Scene.GetFirstEntityWithName("snow_prefab_entity");
            bool hasRainParticle = rainPrefab != null && rainPrefab.ChildCount > 0;
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
    }
}