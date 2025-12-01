using System;
using System.Collections.Generic;
using Game.Logic.Missions;
using Metaplay.Core;
using Metaplay.Core.Model;

namespace Game.Logic
{
    [MetaSerializable]
    public enum ConsumableType
    {
        NONE,
        COIN_MAG,
        SCORE_MULTIPLAYER,
        INVINCIBILITY,
        EXTRALIFE,
        MAX_COUNT
    }

    [MetaSerializable]
    public struct HighscoreEntry : IComparable<HighscoreEntry>
    {
        [MetaMember(1)] public string Name;
        [MetaMember(2)] public int Score;
        
        public int CompareTo(HighscoreEntry other)
        {
            // We want to sort from highest to lowest, so inverse the comparison.
            return other.Score.CompareTo(Score);
        }        
    }

    #region client_player_data
    // MetaSerializable and MetaMember attributes are required for serialization.
    [MetaSerializable]
    public partial class ClientPlayerData
    {
        [MetaMember(1)] public int NumCoins { get; set; }
        [MetaMember(2)] public int NumPremium { get; set; }
        
        // More player data can be added here.
    }
    #endregion client_player_data
    
    public partial class ClientPlayerData
    {
        [MetaMember(3)] public MetaDictionary<ConsumableType, int> Consumables { get; set; } = new MetaDictionary<ConsumableType, int>();
        [MetaMember(4)] public List<string> Characters { get; set; } = new List<string>();
        [MetaMember(5)] public List<string> CharacterAccessories { get; set; } = new List<string>();
        [MetaMember(6)] public List<string> Themes { get; set; } = new List<string>();
        [MetaMember(7)] public List<HighscoreEntry> Highscores { get; set; } = new List<HighscoreEntry>();
        [MetaMember(8)] public bool LicenseAccepted { get; set; }
        [MetaMember(9)] public bool TutorialDone { get; set; }
        [MetaMember(10)] public int Rank { get; set; }
        
        [MetaMember(11)] public string UsedTheme { get; set; }
        [MetaMember(12)] public int UsedCharacter { get; set; }
        [MetaMember(13)] public int UsedAccessory { get; set; }
        [MetaMember(14)] public int FTUELevel { get; set; }
        [MetaMember(15)] public string PreviousName { get; set; }
        [MetaMember(16)] public List<PersistedMission> Missions { get; set; } = new List<PersistedMission>();
    }
}