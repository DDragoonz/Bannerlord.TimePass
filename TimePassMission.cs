using System;
using System.Linq;
using HarmonyLib;
using SandBox.Missions;
using SandBox.Missions.MissionLogics;
using SandBox.Missions.MissionLogics.Arena;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;
using Material = TaleWorlds.Engine.Material;
using Texture = TaleWorlds.Engine.Texture;

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
                InformationManager.DisplayMessage(new InformationMessage("Mission Initialize! CurrentTime : " + Mission.Scene.TimeOfDay));
            }
            
            Type realisticWeatherManagerType = AccessTools.TypeByName("RealisticWeather.RealisticWeatherManager");
            realisticWeatherInstalled = realisticWeatherManagerType != null;
            if (realisticWeatherInstalled)
            {
                Traverse realisticWeatherTraverse = Traverse.Create(realisticWeatherManagerType);
                hasDust = realisticWeatherTraverse.Field("_realisticWeatherManager").Method("get_HasDust").GetValue<bool>();
            }

            MissionInitializerRecord initializerRecord = Traverse.Create(Mission).Field("<InitializerRecord>k__BackingField").GetValue<MissionInitializerRecord>();
            interpAtmosphere = initializerRecord.AtmosphereOnCampaign.InterpolatedAtmosphereName; // filled during campaign, null during custom battle
            // presetAtmosphere = initializerRecord.AtmosphereOnCampaign.AtmosphereName; // null during campaign, filled during custom battle

            currentTime = Campaign.Current != null ? CampaignTime.Now.CurrentHourInDay : Mission.Scene.TimeOfDay;
            currentTimeSkyInfo = new TimePassSkyInfo
            {
                hour = -1,
            };
            expectedSkyTexture = "";

            InitUI();
        }

        public override void OnPreMissionTick(float dt)
        {
            base.OnPreMissionTick(dt);

            if (Mission == null || Mission.Scene == null || !Mission.Scene.IsLoadingFinished())
            {
                return;
            }
            
            bool isCombat = Mission.PlayerEnemyTeam != null && Mission.PlayerEnemyTeam.ActiveAgents.Any();
            float timePassMultiplier = TimePassSettings.Instance.GetTimePassMultiplier(GetCurrentLocation(Mission.Current), isCombat);
            if (Mission.Mode != MissionMode.Deployment && timePassMultiplier != 0)
            {
                float secondTick = dt * (60 * timePassMultiplier);
                // tick campagin time
                if (Campaign.Current != null)
                {
                    Traverse.Create(Campaign.Current).Field("<MapTimeTracker>k__BackingField")
                        .Method("Tick", secondTick).GetValue();
                    // sync time with campagin time
                    currentTime = CampaignTime.Now.CurrentHourInDay;
                }
                else
                {
                    // tick time
                    currentTime += secondTick / 3600;
                    currentTime %= 24;
                }
            }

            if (timePassVM != null)
            {
                timePassVM.TimeOfDay = currentTime;
                timePassVM.OnPropertyChanged("TimePassTimeOfDayText");
            }

            if (TimePassSettings.Instance.EnableSkyUpdate)
            {
                UpdateSky();
            }
        }
        
        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            if (Mission != null && Mission.Scene != null && Mission.Scene.IsLoadingFinished() && TimePassSettings.Instance.EnableSkyUpdate  && Mission.Mode != MissionMode.Deployment)
            {
                UpdateSkyTexture();
            }
        }

        private void UpdateSky()
        {

            if (Mission.Scene == null || Mission.Scene.IsAtmosphereIndoor || !Mission.Scene.IsLoadingFinished())
            {
                return;
            }

            bool isBadWeather = Mission.Scene.GetRainDensity() > 0.0f;
            
            if (!currentTimeSkyInfo.UpdateSkyInfo(currentTime, isBadWeather, interpAtmosphere, Mission.Current.IsNavalBattle))
            {
                return;
            }

            Mission.Scene.TimeOfDay = currentTime;
            expectedSkyTexture = hasDust ? "sky_photo_overcast_01" : currentTimeSkyInfo.skybox_texture;

            // realistic weather dust storm
            if (hasDust)
            {
                // calculation taken from realistic weather
                currentTimeSkyInfo.sky_brightness = currentTime < 12f
                    ? (MathF.Pow(2f, currentTime) - 1f) / 10f
                    : (MathF.Pow(2f, 24f - currentTime) - 1f) / 10f;
                Mission.Scene.SetSceneColorGradeIndex(23);
            }
            else if (TimePassSettings.Instance.UpdateColorGrade && currentTimeSkyInfo.color_grade_name != null)
            {
                Mission.Scene.SetSceneColorGrade(currentTimeSkyInfo.color_grade_name);
            }
            
            if (TimePassSettings.Instance.UpdateSunColorAndIntensity)
            {
                Mission.Scene.SetSun(ref currentTimeSkyInfo.sun_color, currentTimeSkyInfo.sun_altitude, 0, currentTimeSkyInfo.sun_intesity);
            }
            else
            {
                Mission.Scene.SetSunAngleAltitude(0, currentTimeSkyInfo.sun_altitude);
            }
            
            // force sun size to 0, because some atmosphere skybox already have sun, and some night atmosphere has crescent moon texture 
            Mission.Scene.SetSunSize(0);

            if (TimePassSettings.Instance.UpdateSunShaft) Mission.Scene.SetSunShaftStrength(currentTimeSkyInfo.sunshafts_strength);
            if (TimePassSettings.Instance.UpdateSkyBrightness) Mission.Scene.SetSkyBrightness(currentTimeSkyInfo.sky_brightness);

            if (TimePassSettings.Instance.UpdateExposure)
            {
                Mission.Scene.SetMaxExposure(currentTimeSkyInfo.max_exposure);
                Mission.Scene.SetMinExposure(currentTimeSkyInfo.min_exposure);
                Mission.Scene.SetTargetExposure(currentTimeSkyInfo.target_exposure);
            }

            if (TimePassSettings.Instance.UpdateBrightpassThreshold) Mission.Scene.SetBrightpassThreshold(currentTimeSkyInfo.brightpass_threshold);
            if (TimePassSettings.Instance.UpdateMiddleGray) Mission.Scene.SetMiddleGray(currentTimeSkyInfo.middle_gray);
            // Warning : it might causing color inconsistency over time
            if (TimePassSettings.Instance.UpdateAmbientColor) Mission.Scene.SetFogAmbientColor(ref currentTimeSkyInfo.fog_ambient_color);

            // realistic weather has it's own fog settings
            if (TimePassSettings.Instance.UpdateFog)
            {
                // fog settings from realistic weather is far more better than settings from atmosphere files. so I make it default behaviour
                if (realisticWeatherInstalled)
                {
                    // taken from realistic weather fog calculation
                    float fogFalloff = 0.5f * MathF.Sin(3.1415927f * currentTime / 24f);
                    Mission.Scene.SetFog(Mission.Scene.GetFog(), ref currentTimeSkyInfo.fog_color, fogFalloff);
                    Mission.Scene.SetFogAdvanced(0f, 0f, -40f);
                }
                else
                {
                    Mission.Scene.SetFog(currentTimeSkyInfo.fog_density, ref currentTimeSkyInfo.fog_color, currentTimeSkyInfo.fog_falloff);    
                }
            }

            if (TimePassSettings.Instance.UpdateEnvMultiplier)
            {
                Mission.Scene.SetEnvironmentMultiplier(Mission.Scene.IsDayTime, 1);
            }

            if (TimePassSettings.Instance.EnableDebug)
            {
                bool isCombat = Mission.PlayerEnemyTeam != null && Mission.PlayerEnemyTeam.ActiveAgents.Any();
                InformationManager.DisplayMessage(new InformationMessage("Location : " + GetCurrentLocation(Mission.Current)+ " combat : "+isCombat));
                InformationManager.DisplayMessage(new InformationMessage("Sky Info : " + currentTimeSkyInfo));
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
                                      || Mission.Scene.GetSkyboxMesh() == null
                                      || expectedSkyTexture == null
                                      || expectedSkyTexture == "")
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
        
        public static TimePassLocationEnum GetCurrentLocation(Mission mission)
        {
            if (mission != null)
            {
                if (mission.IsFieldBattle)
                {
                    return TimePassLocationEnum.Field;
                }
            
                // Arena
                if (mission.Mode == MissionMode.Tournament || mission.HasMissionBehavior<ArenaAgentStateDeciderLogic>())
                {
                    return TimePassLocationEnum.Arena;
                }

                if (mission.Mode == MissionMode.Stealth || mission.HasMissionBehavior<StealthFailCounterMissionLogic>())
                {
                    return TimePassLocationEnum.Stealth;
                } 
                
            
                // siege combat
                if (mission.IsSiegeBattle)
                {
                    return TimePassLocationEnum.Siege;
                }
                
                if (mission.IsSallyOutBattle)
                {
                    return TimePassLocationEnum.SallyOut;
                }

                if (mission.IsNavalBattle)
                {
                    return TimePassLocationEnum.Naval;
                }
            
                if (mission.Scene != null && mission.Scene.IsAtmosphereIndoor)
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

            GauntletLayer layer = new GauntletLayer("TimePassLayer", 100);
            timePassVM = new TimePassVM();
            timePassVM.TimeOfDay = currentTime;
            layer.LoadMovie("TimePassTimeWidget", timePassVM);
            ScreenManager.TopScreen.AddLayer(layer);
        }

        private float currentTime;
        private string expectedSkyTexture;
        private bool hasDust;
        private string interpAtmosphere;
        private bool realisticWeatherInstalled;
        private Material skyboxMaterialCache;
        private TimePassVM timePassVM;
        private TimePassSkyInfo currentTimeSkyInfo;
    }
}