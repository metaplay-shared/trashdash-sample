using System.IO;
using Metaplay.Core;
using Metaplay.Core.Math;
using Metaplay.Core.Model;

namespace Game.Logic
{
    [MetaSerializable]
    public class CompletedRun
    {
        [MetaMember(1)] public MetaTime Timestamp;
        [MetaMember(2)] public string Character;
        [MetaMember(3)] public string ObstacleTypeHit;
        [MetaMember(4)] public string ThemeUsed;
        [MetaMember(5)] public int CoinsGained;
        [MetaMember(6)] public int PremiumGained;
        [MetaMember(7)] public int Score;
        [MetaMember(8)] public F64 WorldDistance;

        public void Serialize(BinaryWriter w)
        {
            w.Write(Timestamp.MillisecondsSinceEpoch);
            w.Write(Character);
            w.Write(ObstacleTypeHit);
            w.Write(ThemeUsed);
            w.Write(CoinsGained);
            w.Write(PremiumGained);
            w.Write(Score);
            w.Write(WorldDistance.Raw);
        }

        public void Deserialize(BinaryReader r)
        {
            Timestamp = MetaTime.FromMillisecondsSinceEpoch(r.ReadInt64());
            Character = r.ReadString();
            ObstacleTypeHit = r.ReadString();
            ThemeUsed = r.ReadString();
            CoinsGained = r.ReadInt32();
            PremiumGained = r.ReadInt32();
            Score = r.ReadInt32();
            WorldDistance = F64.FromRaw(r.ReadInt64());
        }
    }
}