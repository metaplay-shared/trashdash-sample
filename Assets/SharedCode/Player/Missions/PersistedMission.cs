using Metaplay.Core.Model;

namespace Game.Logic.Missions
{
    [MetaSerializable]
    public class PersistedMission
    {
        [MetaMember(1)] public MissionType MissionType { get; }

        [MetaMember(2)] public float Progress { get; }
        [MetaMember(3)] public float Max { get; }
        [MetaMember(4)] public int Reward { get; }

        [MetaDeserializationConstructor]
        public PersistedMission(float progress, float max, int reward, MissionType missionType)
        {
            Progress = progress;
            Max = max;
            Reward = reward;
            MissionType = missionType;
        }
    }
}