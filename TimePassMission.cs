using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace TimePass
{
    public class TimePassMission : MissionLogic
    {
        public override void OnAfterMissionCreated()
        {
            base.OnAfterMissionCreated();
            if (TimePassSettings.Instance.enableDebug)
                InformationManager.DisplayMessage(
                    new InformationMessage("Mission Initialize! CurrentTime : " +
                                           TimePassSkyInfo.GetCurrentTimeOfDay()));

            skyTickTime = 0.0f;
            lastTickHour = Campaign.Current == null ? 0 : (int)CampaignTime.Now.CurrentHourInDay;
        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);

            if (Mission.Current != null && Mission.Current.Scene != null && Mission.Current
                    .Scene.IsLoadingFinished())
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
            if (Mission.Current == null || Mission.Current.Scene == null || Mission.Current.Scene.IsAtmosphereIndoor
                || !Mission.Current.Scene.IsLoadingFinished())
            {
                return;
            }

            // update sky tick counter
            if (!UpdateSkyTickCounter(dt))
            {
                return;
            }

            int currentHour = (int)TimePassSkyInfo.GetCurrentTimeOfDay();

            // update whole atmosphere once in an hour
            string atmosphereName =
                TimePassSkyInfo.GetAtmosphereName(currentHour, Mission.Current.Scene.GetRainDensity());
            if (currentHour != lastTickHour)
            {
                // runtime atmosphere changing is currently disabled. 
                // reason 1 : changing atmosphere in runtime will causing brief flicker / flash, which might causing discomfort
                // reason 2 : for some reason, changing atmosphere in runtime might delete some scene objects (road, tree)
                // downside : skybox will remain same as player enter the scene, if player enter scene during day and time passed until night, 
                // it will still show sun instead moon, and vice versa

                // Mission.Current.Scene.SetAtmosphereWithName(atmosphereName);
                // if(TimePassSettings.Instance.enableDebug)InformationManager.DisplayMessage(new InformationMessage("Current Atmosphere : " + atmosphereName));

                lastTickHour = currentHour;
            }


            // lerp sun position every tick
            int nextHour = currentHour + 1;
            float hourProgress = TimePassSkyInfo.GetCurrentTimeOfDay() - currentHour;
            // since there is no 00:00 atmosphere data, we lerp for 2 hours during 23:00 to 1:00
            if (currentHour == 0)
            {
                hourProgress = (hourProgress + 1) / 2;
            }

            TimePassSkyInfo skyInfo = TimePassSkyInfo.GetOrReadSkyInfo(currentHour, atmosphereName);
            string nextAtmosphereName =
                TimePassSkyInfo.GetAtmosphereName(nextHour, Mission.Current.Scene.GetRainDensity());
            TimePassSkyInfo nextSkyInfo = TimePassSkyInfo.GetOrReadSkyInfo(nextHour, nextAtmosphereName);
            TimePassSkyInfo lerpSkyInfo = TimePassSkyInfo.Lerp(skyInfo, nextSkyInfo, hourProgress);
            // Mission.Current.Scene.SetSkyRotation(lerpSkyInfo.skybox_rotation);

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

            Mission.Current.Scene.SetSun(ref lerpSkyInfo.sun_color, lerpSkyInfo.sun_altitude
                , lerpSkyInfo.sun_angle, lerpSkyInfo.sun_intesity);
            // Mission.Current.Scene.SetSunAngleAltitude(lerpSkyInfo.sun_angle,sunAltitude);
            // Mission.Current.Scene.SetSunSize(lerpSkyInfo.sun_size);
            Mission.Current.Scene.SetSunShaftStrength(lerpSkyInfo.sunshafts_strength);
            Mission.Current.Scene.SetSkyBrightness(lerpSkyInfo.sky_brightness);
            Mission.Current.Scene.SetMaxExposure(lerpSkyInfo.max_exposure);
            Mission.Current.Scene.SetMinExposure(lerpSkyInfo.min_exposure);
            Mission.Current.Scene.SetTargetExposure(lerpSkyInfo.target_exposure);
            Mission.Current.Scene.SetBrightpassThreshold(lerpSkyInfo.brightpass_threshold);
            Mission.Current.Scene.SetFogAmbientColor(ref lerpSkyInfo.fog_ambient_color);
            // TODO : update fog?

            if (TimePassSettings.Instance.enableDebug)
            {
                InformationManager.DisplayMessage(new InformationMessage("Sky Info : " + lerpSkyInfo));
                InformationManager.DisplayMessage(new InformationMessage("rain density : " + Mission.Current.Scene
                    .GetRainDensity() + " snow density : " + Mission.Current.Scene.GetSnowDensity()));
            }
        }


        // return true if should tick sky
        private bool UpdateSkyTickCounter(float dt)
        {
            skyTickTime += dt;

            float rainDensity = Mission.Current.Scene.GetRainDensity();
            float snowDensity = Mission.Current.Scene.GetSnowDensity();
            float tickInterval = TimePassSettings.Instance.skyTickTimeInterval;

            // override tick interval to rainSkyTickTimeInterval during rain.
            // because it will make rain particle goes crazy if updating too much in short period
            if (rainDensity > 0.5f && tickInterval < TimePassSettings.Instance.rainSkyTickTimeInterval)
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