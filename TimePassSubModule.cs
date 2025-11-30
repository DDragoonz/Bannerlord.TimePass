using HarmonyLib;
using TaleWorlds.MountAndBlade;

namespace TimePass
{
    public class TimePassSubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            new Harmony("mod.bannerlord.timepass").PatchAll();
        }

        public override void OnBeforeMissionBehaviorInitialize(Mission mission)
        {
            base.OnBeforeMissionBehaviorInitialize(mission);
            mission.AddMissionBehavior(new TimePassMission());
        }
    }
}