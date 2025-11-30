using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace TimePass
{
    [HarmonyPatch(typeof(MissionState))]
    [HarmonyPatch("OpenNew")]
    public class MissionOpenNewPatches
    {
        static void Prefix(string missionName, ref MissionInitializerRecord rec, InitializeMissionBehaviorsDelegate handler, bool addDefaultMissionBehaviors, bool needsMemoryCleanup)
        {
            if (TimePassSettings.Instance.EnableSkyUpdate && TimePassSettings.Instance.EnableLightingHack)
            {
                rec.AtmosphereOnCampaign.TimeInfo.NightTimeFactor = 1;
                rec.AtmosphereOnCampaign.TimeInfo.TimeOfDay = 0;
            }
        }
    }
}