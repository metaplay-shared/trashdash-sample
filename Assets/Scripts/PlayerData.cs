using System;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Game.Logic;
using Game.Logic.Missions;
using Random = UnityEngine.Random;
#if UNITY_ANALYTICS
using UnityEngine.Analytics;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Save data for the game. This is stored locally in this case, but a "better" way to do it would be to store it on a server
/// somewhere to avoid player tampering with it. Here potentially a player could modify the binary file to add premium currency.
/// </summary>
public class PlayerData
{
    static protected PlayerData m_Instance;
    static public PlayerData instance { get { return m_Instance; } }

    protected string saveFile = "";
    private PlayerModel model => MetaplayClient.PlayerModel;


    // PlayerData fields ported to be part of the Metaplay PlayerModel. Readonly access is provided via get accessors
    // for minimizing callsite changes, mutations have to go through PlayerActions.
    public int premium => model.PlayerData.NumPremium;
    public int coins => model.PlayerData.NumCoins;
    public IReadOnlyDictionary<ConsumableType, int> consumables => model.PlayerData.Consumables; // Inventory of owned consumables and quantity.
    public IReadOnlyList<HighscoreEntry> highscores => model.PlayerData.Highscores;
    public int rank => model.PlayerData.Rank;
    public IReadOnlyList<string> themes => model.PlayerData.Themes;

    public List<string> characters => model.PlayerData.Characters;
    public int usedCharacter;                               // Currently equipped character.
    public int usedAccessory = -1;
    public List<string> characterAccessories => model.PlayerData.CharacterAccessories;  // List of owned accessories, in the form "charName:accessoryName".
    public string usedTheme;                                        // Currently used theme.
    public List<MissionBase> missions = new List<MissionBase>();
    
	public string previousName = "Trash Cat";

    public bool licenceAccepted;
    public bool tutorialDone;

    //ftue = First Time User Expeerience. This var is used to track thing a player do for the first time. It increment everytime the user do one of the step
    //e.g. it will increment to 1 when they click Start, to 2 when doing the first run, 3 when running at least 300m etc.
    public int ftueLevel = 0;

    // This will allow us to add data even after production, and so keep all existing save STILL valid. See loading & saving for how it work.
    // Note in a real production it would probably reset that to 1 before release (as all dev save don't have to be compatible w/ final product)
    // Then would increment again with every subsequent patches. We kept it to its dev value here for teaching purpose. 
    //static int s_Version = 14;
    
    // Mission management

    // Will add missions until we reach 2 missions.
    public void CheckMissionsCount()
    {
        while (missions.Count < 2)
            AddMission();
    }

    public void AddMission()
    {
        int val = Random.Range(0, (int)MissionType.MAX);
        
        MissionBase newMission = MissionBase.GetNewMissionFromType((MissionType)val);
        newMission.Created();

        missions.Add(newMission);
    }

    public void StartRunMissions(TrackManager manager)
    {
        for(int i = 0; i < missions.Count; ++i)
        {
            missions[i].RunStart(manager);
        }
    }

    public void UpdateMissions(TrackManager manager)
    {
        for(int i = 0; i < missions.Count; ++i)
        {
            missions[i].Update(manager);
        }
    }

    public bool AnyMissionComplete()
    {
        for (int i = 0; i < missions.Count; ++i)
        {
            if (missions[i].isComplete) return true;
        }

        return false;
    }

    public void ClaimMission(MissionBase mission)
    {
        // TODO: currently disabled
        //AddPremiumCurrency(mission.reward, Context.Mission);
        
#if UNITY_ANALYTICS // Using Analytics Standard Events v0.3.0
        AnalyticsEvent.ItemAcquired(
            AcquisitionType.Premium, // Currency type
            "mission",               // Context
            mission.reward,          // Amount
            "anchovies",             // Item ID
            premium,                 // Item balance
            "consumable",            // Item type
            rank.ToString()          // Level
        );
#endif
        
        missions.Remove(mission);

        CheckMissionsCount();

        Save();
    }

	// High Score management

	public int GetScorePlace(int score)
	{
		HighscoreEntry entry = new HighscoreEntry();
		entry.Score = score;
		entry.Name = "";

		int index = model.PlayerData.Highscores.BinarySearch(entry);

		return index < 0 ? (~index) : index;
	}

    // File management

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ClearOnLoad()
    {
        m_Instance = null;
    }

    static public void Create()
    {
        if (MetaplayClient.PlayerModel == null)
            throw new InvalidOperationException("MetaplayService not initialized");

        if (m_Instance != null)
            return;
        m_Instance = new PlayerData();

        // If we need to create the PlayerData, it's the very first call, so we use that to init the content databases
        // this allow to always init the database at the earlier we can, i.e. the start screen if started normally on device
        // or the Loadout screen if testing in editor
        CoroutineHandler.StartStaticCoroutine(CharacterDatabase.LoadDatabase());
        CoroutineHandler.StartStaticCoroutine(ThemeDatabase.LoadDatabase());

        m_Instance.saveFile = Application.persistentDataPath + "/save.bin";

#region migrate_state
        // Migrate offline state to PlayerModel as a once-off operation
        if (!MetaplayClient.PlayerModel.OfflineStateMigrated)
        {
            MigrateStateFromOfflineAction migrationAction = new MigrateStateFromOfflineAction();
            if (File.Exists(m_Instance.saveFile))
            {
                Debug.Log("Save file found on disk, migrating to cloud save.");
                migrationAction.OfflinePlayerData = MigrateFromSaveFile();
                // You might not want to delete the save file immediately, 
                // as there would be no way to recover it if the migration fails.
                File.Delete(m_Instance.saveFile);
            }
            // Canonical way to execute actions in Metaplay
            MetaplayClient.PlayerContext.ExecuteAction(migrationAction);
        }
#endregion migrate_state
        
        // Update the local state with Metaplay's player model.
        m_Instance.SyncFromModel(MetaplayClient.PlayerModel.PlayerData);
        
        m_Instance.CheckMissionsCount();
        
        m_Instance.Save();
    }
    
    static public void NewSave()
    {
        // TODO: implement by reset player state action
	}

    private static ClientPlayerData MigrateFromSaveFile()
    {
        ClientPlayerData playerData = new ClientPlayerData();
        BinaryReader r = new BinaryReader(new FileStream(m_Instance.saveFile, FileMode.Open));

        int ver = r.ReadInt32();

        if(ver < 6)
        {
            r.Close();
            r = new BinaryReader(new FileStream(m_Instance.saveFile, FileMode.Open));
            ver = r.ReadInt32();
        }

        playerData.NumCoins = r.ReadInt32();

        int consumableCount = r.ReadInt32();
        for (int i = 0; i < consumableCount; ++i)
        {
            playerData.Consumables.Add((ConsumableType)r.ReadInt32(), r.ReadInt32());
        }

        // Read character.
        int charCount = r.ReadInt32();
        for(int i = 0; i < charCount; ++i)
        {
            string charName = r.ReadString();

            if (charName.Contains("Raccoon") && ver < 11)
            {//in 11 version, we renamed Raccoon (fixing spelling) so we need to patch the save to give the character if player had it already
                charName = charName.Replace("Racoon", "Raccoon");
            }

            playerData.Characters.Add(charName);
        }

        playerData.UsedCharacter = r.ReadInt32();

        // Read character accesories.
        int accCount = r.ReadInt32();
        for (int i = 0; i < accCount; ++i)
        {
            playerData.CharacterAccessories.Add(r.ReadString());
        }

        // Read Themes.
        int themeCount = r.ReadInt32();
        for (int i = 0; i < themeCount; ++i)
        {
            playerData.Themes.Add(r.ReadString());
        }

        if (ver >= 14)
        {
            playerData.UsedTheme = r.ReadString();
        }
        else
        {
            int themeIdx = r.ReadInt32();
            playerData.UsedTheme = playerData.Themes[themeIdx];
        }

        // Save contains the version they were written with. If data are added bump the version & test for that version before loading that data.
        if(ver >= 2)
        {
            playerData.NumPremium = r.ReadInt32();
        }

        // Added highscores.
		if(ver >= 3)
        {
            int count = r.ReadInt32();
            for (int i = 0; i < count; ++i)
            {
                HighscoreEntry entry = new HighscoreEntry();
                entry.Name = r.ReadString();
                entry.Score = r.ReadInt32();

                playerData.Highscores.Add(entry);
            }
        }

        // Added missions.
        if(ver >= 4)
        {
            playerData.Missions.Clear();

            int count = r.ReadInt32();
            for(int i = 0; i < count; ++i)
            {
                MissionType type = (MissionType)r.ReadInt32();
                MissionBase tempMission = MissionBase.GetNewMissionFromType(type);

                tempMission.Deserialize(r);

                playerData.Missions.Add(new PersistedMission(tempMission.progress, tempMission.max, tempMission.reward, type));
            }
        }

        // Added highscore previous name used.
        if(ver >= 7)
        {
            playerData.PreviousName = r.ReadString();
        }

        if(ver >= 8)
        {
            playerData.LicenseAccepted = r.ReadBoolean();
        }

        if (ver >= 9) 
        {
            // Read and discard old audio settings
            r.ReadSingle();
            r.ReadSingle();
            r.ReadSingle();
        }

        if(ver >= 10)
        {
            playerData.FTUELevel = r.ReadInt32();
            playerData.Rank = r.ReadInt32();
        }

        if (ver >= 12)
        {
            playerData.TutorialDone = r.ReadBoolean();
        }

        if (ver >= 13)
        {
            int numCompletedRuns = r.ReadInt32();
            for (int i = 0; i < numCompletedRuns; ++i)
            {
                CompletedRun completedRun = new CompletedRun();
                completedRun.Deserialize(r);
            }
        }

        r.Close();

        return playerData;
    }

    public void SyncFromModel(ClientPlayerData playerData)
    {
        licenceAccepted = playerData.LicenseAccepted;
        tutorialDone = playerData.TutorialDone;
        usedTheme = playerData.UsedTheme;
        usedCharacter = playerData.UsedCharacter;
        usedAccessory = playerData.UsedAccessory;
        ftueLevel = playerData.FTUELevel;
        previousName = playerData.PreviousName;

        missions.Clear();
        foreach (var mission in playerData.Missions)
        {
            MissionBase newMission = null;
            switch (mission.MissionType)
            {
                case MissionType.SINGLE_RUN:
                    newMission = new SingleRunMission();
                    break;
                case MissionType.PICKUP:
                    newMission = new PickupMission();
                    break;
                case MissionType.OBSTACLE_JUMP:
                    newMission = new BarrierJumpMission();
                    break;
                case MissionType.SLIDING:
                    newMission = new SlidingMission();
                    break;
                case MissionType.MULTIPLIER:
                    newMission = new MultiplierMission();
                    break;
                case MissionType.MAX:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (newMission != null)
            {
                newMission.progress = mission.Progress;
                newMission.max = mission.Max;
                newMission.reward = mission.Reward;
                missions.Add(newMission);
            }
        }
    }
    
    #region save_state
    public void Save()
    {
        UpdateClientAuthoritativeStateAction syncAction = new UpdateClientAuthoritativeStateAction();
        syncAction.LicenseAccepted = licenceAccepted;
        syncAction.TutorialDone = tutorialDone;
        syncAction.UsedTheme = usedTheme;
        syncAction.UsedCharacter = usedCharacter;
        syncAction.UsedAccessory = usedAccessory;
        syncAction.FTUELevel = ftueLevel;
        syncAction.PreviousName = previousName;
        syncAction.Missions = missions.Select(x => x.ToPersisted()).ToList();
        MetaplayClient.PlayerContext.ExecuteAction(syncAction);
    }
    #endregion save_state
}

// Helper class to cheat in the editor for test purpose
#if UNITY_EDITOR
public class PlayerDataEditor : Editor
{
	[MenuItem("Trash Dash Debug/Clear Save")]
    static public void ClearSave()
    {
        File.Delete(Application.persistentDataPath + "/save.bin");
    } 

    [MenuItem("Trash Dash Debug/Give 1000000 fishbones and 1000 premium")]
    static public void GiveCoins()
    {
        MetaplayClient.PlayerContext.ExecuteAction(new DebugAddCurrencyAction(1000000, 1000));
    }

    [MenuItem("Trash Dash Debug/Give 10 Consumables of each types")]
    static public void AddConsumables()
    {
       
        for(int i = 0; i < ShopItemList.s_ConsumablesTypes.Length; ++i)
        {
            Consumable c = ConsumableDatabase.GetConsumbale(ShopItemList.s_ConsumablesTypes[i]);
            if(c != null)
            {
                MetaplayClient.PlayerContext.ExecuteAction(new DebugAddConsumableAction((ConsumableType)c.GetConsumableType(), 10));
            }
        }
    }
}
#endif