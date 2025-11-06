using Metaplay.Core.Model;

namespace Game.Logic.Missions
{
    [MetaSerializable]
    public enum MissionType
    {
        SINGLE_RUN,
        PICKUP,
        OBSTACLE_JUMP,
        SLIDING,
        MULTIPLIER,
        MAX
    }
}