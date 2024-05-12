using System;
using System.Linq;
using HarmonyLib;
using SandBox.Missions.MissionLogics.Arena;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
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
        
        public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;

        public override void AfterStart()
        {
            base.AfterStart();
            if (TimePassSettings.Instance.EnableDebug)
            {
                InformationManager.DisplayMessage(new InformationMessage("Mission Initialize! CurrentTime : " + TimePassSkyInfo.TimeOfDay));
            }

            // allow first tick to always update sky
            skyTickTime = Math.Max(TimePassSettings.Instance.SkyTickTimeInterval, TimePassSettings.Instance.RainSkyTickTimeInterval);
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
            presetAtmosphere = initializerRecord.AtmosphereOnCampaign.AtmosphereName; // null during campaign, filled during custom battle

            // init custom time of day

            TimePassSkyInfo.Init(Mission.Scene.TimeOfDay, Mission.Scene.GetRainDensity() > 0, interpAtmosphere);

            expectedSkyTexture = "";

            currentLocation = GetCurrentLocation();

            InitUI();
        }

        public override void OnPreMissionTick(float dt)
        {
            base.OnPreMissionTick(dt);

            if (Mission != null && Mission.Scene != null && Mission.Scene.IsLoadingFinished())
            {
                bool isCombat = Mission.PlayerEnemyTeam != null && Mission.PlayerEnemyTeam.ActiveAgents.Any();
                float secondTick = dt * (60 / TimePassSettings.Instance.GetTimePassDuration(currentLocation,isCombat)); //TimePassSettings.Instance.RealSecondToWorldSecondRatio;

                // tick campagin time
                if (Campaign.Current != null)
                {
                    Traverse.Create(Campaign.Current).Field("<MapTimeTracker>k__BackingField")
                        .Method("Tick", secondTick).GetValue();
                    // sync time with campagin time
                    TimePassSkyInfo.TimeOfDay = CampaignTime.Now.CurrentHourInDay;
                }
                else
                {
                    // tick time
                    TimePassSkyInfo.TimeOfDay += secondTick / 3600;
                    TimePassSkyInfo.TimeOfDay %= 24;
                }


                if (timePassVM != null)
                {
                    timePassVM.OnPropertyChanged("TimePassTimeOfDayText");
                }

                if (TimePassSettings.Instance.EnableSkyUpdate)
                {
                    UpdateSky(dt);
                }
            }
        }
        
        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            if (Mission != null && Mission.Scene != null && Mission.Scene.IsLoadingFinished() && TimePassSettings.Instance.EnableSkyUpdate)
            {
                UpdateSkyTexture();
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

            bool isBadWeather = Mission.Scene.GetRainDensity() > 0.0f;

            TimePassSkyInfo targetSkyInfo = TimePassSkyInfo.GetTargetSkyInfo(TimePassSkyInfo.TimeOfDay, isBadWeather, interpAtmosphere);
            targetSkyInfo.ClampToInitialSkyInfo();
            int currentHour = (int)TimePassSkyInfo.TimeOfDay;

            if (currentHour != lastTickHour)
            {
                // runtime atmosphere changing is currently disabled. 
                // reason 1 : changing atmosphere in runtime will causing brief flicker / flash, which might causing discomfort
                // reason 2 : for some reason, changing atmosphere in runtime might delete some scene objects (road, tree)
                // downside : skybox will remain same as player enter the scene, if player enter scene during day and time passed until night, 
                // it will still show sun instead moon, and vice versa

                // string atmosphereName = TimePassSkyInfo.GetAtmosphereName(currentHour, isBadWeather);
                // Mission.Scene.SetAtmosphereWithName(atmosphereName);
                // Mission.Scene.ForceLoadResources();

                // if(TimePassSettings.Instance.enableDebug)InformationManager.DisplayMessage(new InformationMessage("Current Atmosphere : " + atmosphereName));

                lastTickHour = currentHour;

            }


            expectedSkyTexture = hasDust ? "sky_photo_overcast_01" : targetSkyInfo.skybox_texture;

            // realistic weather dust storm
            if (hasDust)
            {
                // calculation taken from realistic weather
                targetSkyInfo.sky_brightness = TimePassSkyInfo.TimeOfDay < 12f
                    ? (MathF.Pow(2f, TimePassSkyInfo.TimeOfDay) - 1f) / 10f
                    : (MathF.Pow(2f, 24f - TimePassSkyInfo.TimeOfDay) - 1f) / 10f;
                Mission.Scene.SetSceneColorGradeIndex(23);
            }
            else if (TimePassSettings.Instance.UpdateColorGrade)
            {
                try
                {
                    Mission.Scene.SetColorGradeBlend(targetSkyInfo.color_grade_name, targetSkyInfo.color_grade_name2,
                        targetSkyInfo.color_grade_alpha);
                }
                catch (Exception e)
                {
                    if (TimePassSettings.Instance.EnableDebug)
                    {
                        InformationManager.DisplayMessage(new InformationMessage(e.Message, Colors.Red));
                    }
                }

            }


            if (TimePassSettings.Instance.UpdateSunColorAndIntensity)
            {
                Mission.Scene.SetSun(ref targetSkyInfo.sun_color, targetSkyInfo
                    .sun_altitude, targetSkyInfo.sun_angle, targetSkyInfo.sun_intesity);
            }
            else
            {
                Mission.Scene.SetSunAngleAltitude(targetSkyInfo.sun_angle, targetSkyInfo.sun_altitude);
            }

            // Warning : changing sun size overtime is somehow ugly.. because the size is not really consistent
            if (TimePassSettings.Instance.UpdateSunSize) Mission.Scene.SetSunSize(targetSkyInfo.sun_size);
            if (TimePassSettings.Instance.UpdateSunShaft) Mission.Scene.SetSunShaftStrength(targetSkyInfo.sunshafts_strength);
            if (TimePassSettings.Instance.UpdateSkyBrightness) Mission.Scene.SetSkyBrightness(targetSkyInfo.sky_brightness);


            if (TimePassSettings.Instance.UpdateExposure)
            {
                Mission.Scene.SetMaxExposure(targetSkyInfo.max_exposure);
                Mission.Scene.SetMinExposure(targetSkyInfo.min_exposure);
                Mission.Scene.SetTargetExposure(targetSkyInfo.target_exposure);
            }

            if (TimePassSettings.Instance.UpdateBrightpassThreshold) Mission.Scene.SetBrightpassThreshold(targetSkyInfo.brightpass_threshold);
            if (TimePassSettings.Instance.UpdateMiddleGray) Mission.Scene.SetMiddleGray(targetSkyInfo.middle_gray);
            // Warning : it might causing color inconsistency over time
            if (TimePassSettings.Instance.UpdateAmbientColor) Mission.Scene.SetFogAmbientColor(ref targetSkyInfo.fog_ambient_color);

            // realistic weather has it's own fog settings
            if (TimePassSettings.Instance.UpdateFog)
            {
                // fog settings from realistic weather is far more better than settings from atmosphere files lol. so I make it default behaviour
                // if (realisticWeatherInstalled)
                // {
                // taken from realistic weather fog calculation
                float fogFalloff = 0.5f * MathF.Sin(3.1415927f * TimePassSkyInfo.TimeOfDay / 24f);
                Mission.Scene.SetFog(Mission.Scene.GetFog(), ref targetSkyInfo.fog_color, fogFalloff);
                Mission.Scene.SetFogAdvanced(0f, 0f, -40f);
                // }
                // else
                // {
                //     Mission.Scene.SetFog(targetSkyInfo.fog_density, ref targetSkyInfo.fog_color, targetSkyInfo.fog_falloff);    
                // }

            }

            if (TimePassSettings.Instance.EnableDebug)
            {
                bool isCombat = Mission.PlayerEnemyTeam != null && Mission.PlayerEnemyTeam.ActiveAgents.Any();
                InformationManager.DisplayMessage(new InformationMessage("Location : " + currentLocation+ " combat : "+isCombat));
                InformationManager.DisplayMessage(new InformationMessage("Sky Info : " + targetSkyInfo));
                InformationManager.DisplayMessage(new InformationMessage("rain density : " + Mission.Scene
                    .GetRainDensity() + " snow density : " + Mission.Scene.GetSnowDensity() + " fog density : " + Mission.Scene.GetFog()));
            }


        }

        private void UpdateSkyTexture()
        {
            if (!TimePassSettings.Instance.UpdateSkybox)
            {
                return;
            }
            
            if (Mission.Scene == null || Mission.Scene.IsAtmosphereIndoor
                                      || !Mission.Scene.IsLoadingFinished()
                                      || Mission.Scene.GetSkyboxMesh() == null)
            {
                return;
            }

            if (skyboxMaterialCache == null)
            {
                skyboxMaterialCache = Mission.Scene.GetSkyboxMesh().GetMaterial().CreateCopy();
            }

            if (skyboxMaterialCache == null)
            {
                return;
            }

            if (skyboxMaterialCache.GetTexture(Material.MBTextureType.DiffuseMap).Name != expectedSkyTexture)
            {
                Texture texture = Texture.GetFromResource(expectedSkyTexture);
                skyboxMaterialCache.SetTexture(Material.MBTextureType.DiffuseMap, texture);
            }

            Mesh skyboxMesh = Mission.Scene.GetSkyboxMesh();
            if (skyboxMesh.GetMaterial().GetTexture(Material.MBTextureType.DiffuseMap).Name != expectedSkyTexture)
            {
                skyboxMesh.SetMaterial(skyboxMaterialCache);
            }

        }
        
        // return true if should tick sky
        private bool UpdateSkyTickCounter(float dt)
        {
            skyTickTime += dt;

            float tickInterval = TimePassSettings.Instance.SkyTickTimeInterval;

            bool isBadWeather = Mission.Scene.GetRainDensity() > 0;
            // override tick interval to rainSkyTickTimeInterval during bad weather (snowy or rain).
            // because it will make rain / snow particle goes crazy if updating too much in short period
            if (isBadWeather && tickInterval < TimePassSettings.Instance.RainSkyTickTimeInterval)
            {
                tickInterval = TimePassSettings.Instance.RainSkyTickTimeInterval;
            }

            if (skyTickTime < tickInterval)
            {
                return false;
            }

            skyTickTime = 0;

            return true;
        }
        
        private TimePassLocationEnum GetCurrentLocation()
        {
            if (Mission.Current != null)
            {
                if (Mission.Current.IsFieldBattle)
                {
                    return TimePassLocationEnum.Field;
                }
            
                // Arena
                if (Mission.Current.HasMissionBehavior<ArenaAgentStateDeciderLogic>())
                {
                    return TimePassLocationEnum.Arena;
                }
            
                // siege combat
                if (Mission.Current.IsSiegeBattle)
                {
                    return TimePassLocationEnum.Siege;
                }
            
                if (Mission.Current.Scene.IsAtmosphereIndoor)
                {
                    return TimePassLocationEnum.Indoor;
                }
            }
            
            
            if (Settlement.CurrentSettlement != null)
            {
                if (Settlement.CurrentSettlement.IsHideout)
                {
                    return TimePassLocationEnum.Hideout;
                }
                if (Settlement.CurrentSettlement.IsTown)
                {
                    return TimePassLocationEnum.Town;
                }
                if (Settlement.CurrentSettlement.IsVillage)
                {
                    return TimePassLocationEnum.Village;
                }
            }

            return TimePassLocationEnum.Other;
            
        }

        // ReSharper disable once InconsistentNaming
        private void InitUI()
        {
            if (!TimePassSettings.Instance.DisplayTime)
            {
                return;
            }

            GauntletLayer layer = new GauntletLayer(100);
            timePassVM = new TimePassVM();
            layer.LoadMovie("TimePassTimeWidget", timePassVM);
            ScreenManager.TopScreen.AddLayer(layer);
        }

        private string expectedSkyTexture;
        private bool hasDust;

        private string interpAtmosphere;
        private int lastTickHour = -1;
        private string presetAtmosphere;
        private bool realisticWeatherInstalled;

        private Material skyboxMaterialCache;


        // cache
        private float skyTickTime;

        private TimePassVM timePassVM;
        private TimePassLocationEnum currentLocation;
    }
}