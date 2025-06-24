using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.IO;
using DS2DEngine;

namespace RogueCastle
{
    public class SaveGameManager
    {
        private string m_storagePath;
        private string m_fileNamePlayer = "RogueLegacyPlayer.rcdat";
        private string m_fileNameUpgrades = "RogueLegacyBP.rcdat";
        private string m_fileNameMap = "RogueLegacyMap.rcdat";
        private string m_fileNameMapData = "RogueLegacyMapDat.rcdat";
        private string m_fileNameLineage = "RogueLegacyLineage.rcdat";
        private Game m_game;

        private int m_saveFailCounter;
        private bool m_autosaveLoaded;

        public SaveGameManager(Game game)
        {
            m_saveFailCounter = 0;
            m_autosaveLoaded = false;
            m_game = game;
        }

        public void Initialize()
        {
            if (LevelEV.RUN_DEMO_VERSION == true)
            {
                m_fileNamePlayer = "RogueLegacyDemoPlayer.rcdat";
                m_fileNameUpgrades = "RogueLegacyDemoBP.rcdat";
                m_fileNameMap = "RogueLegacyDemoMap.rcdat";
                m_fileNameMapData = "RogueLegacyDemoMapDat.rcdat";
                m_fileNameLineage = "RogueLegacyDemoLineage.rcdat";
            }
            
            PerformDirectoryCheck();
        }

        private void GetStoragePath()
        {
            if (string.IsNullOrEmpty(m_storagePath))
            {
                m_storagePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RogueLegacy");
            }
            if (!Directory.Exists(m_storagePath))
            {
                Directory.CreateDirectory(m_storagePath);
            }
        }

        private void PerformDirectoryCheck()
        {
            GetStoragePath();

            if (!Directory.Exists(Path.Combine(m_storagePath, "Profile1")))
            {
                Directory.CreateDirectory(Path.Combine(m_storagePath, "Profile1"));
                CopyFile(m_fileNamePlayer, "Profile1");
                CopyFile("AutoSave_" + m_fileNamePlayer, "Profile1");
                CopyFile(m_fileNameUpgrades, "Profile1");
                CopyFile("AutoSave_" + m_fileNameUpgrades, "Profile1");
                CopyFile(m_fileNameMap, "Profile1");
                CopyFile("AutoSave_" + m_fileNameMap, "Profile1");
                CopyFile(m_fileNameMapData, "Profile1");
                CopyFile("AutoSave_" + m_fileNameMapData, "Profile1");
                CopyFile(m_fileNameLineage, "Profile1");
                CopyFile("AutoSave_" + m_fileNameLineage, "Profile1");
            }
            if (!Directory.Exists(Path.Combine(m_storagePath, "Profile2")))
            {
                Directory.CreateDirectory(Path.Combine(m_storagePath, "Profile2"));
            }
            if (!Directory.Exists(Path.Combine(m_storagePath, "Profile3")))
            {
                Directory.CreateDirectory(Path.Combine(m_storagePath, "Profile3"));
            }
        }

        private void CopyFile(string fileName, string profileName)
        {
            string sourcePath = Path.Combine(m_storagePath, fileName);
            string destPath = Path.Combine(m_storagePath, profileName, fileName);
            if (File.Exists(sourcePath))
            {
                using (Stream fileToCopy = File.OpenRead(sourcePath))
                using (Stream copiedFile = File.Create(destPath))
                {
                    fileToCopy.CopyTo(copiedFile);
                }
            }
        }

        public void SaveFiles(params SaveType[] saveList)
        {
            if (LevelEV.DISABLE_SAVING == false)
            {
                GetStoragePath();
                try
                {
                    foreach (SaveType saveType in saveList)
                        SaveData(saveType, false);
                    m_saveFailCounter = 0;
                }
                catch (IOException)
                {
                    if (m_saveFailCounter > 2)
                    {
                        RCScreenManager manager = Game.ScreenManager;
                        manager.DialogueScreen.SetDialogue("Save File Error Antivirus");
                        Tweener.Tween.RunFunction(0.25f, manager, "DisplayScreen", ScreenType.Dialogue, true, typeof(List<object>));
                        m_saveFailCounter = 0;
                    }
                    else
                    {
                        m_saveFailCounter++;
                    }
                }
            }
        }

        public void SaveBackupFiles(params SaveType[] saveList)
        {
            if (LevelEV.DISABLE_SAVING == false)
            {
                GetStoragePath();
                foreach (SaveType saveType in saveList)
                    SaveData(saveType, true);
            }
        }

        public void SaveAllFileTypes(bool saveBackup)
        {
            if (saveBackup == false)
                SaveFiles(SaveType.PlayerData, SaveType.UpgradeData, SaveType.Map, SaveType.MapData, SaveType.Lineage);
            else
                SaveBackupFiles(SaveType.PlayerData, SaveType.UpgradeData, SaveType.Map, SaveType.MapData, SaveType.Lineage);
        }

        public void LoadFiles(ProceduralLevelScreen level, params SaveType[] loadList)
        {
            if (LevelEV.ENABLE_BACKUP_SAVING == true)
            {
                GetStoragePath();
                SaveType currentType = SaveType.None;
                try
                {
                    if (LevelEV.DISABLE_SAVING == false)
                    {
                        foreach (SaveType loadType in loadList)
                        {
                            currentType = loadType;
                            LoadData(loadType, level);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Save File Error: " + e.Message);
                    if (currentType != SaveType.Map && currentType != SaveType.MapData && currentType != SaveType.None)
                    {
                        if (m_autosaveLoaded == false)
                        {
                            RCScreenManager manager = Game.ScreenManager;
                            manager.DialogueScreen.SetDialogue("Save File Error");
                            Game.gameIsCorrupt = true;
                            manager.DialogueScreen.SetConfirmEndHandler(this, "LoadAutosave");
                            manager.DisplayScreen(ScreenType.Dialogue, false, null);
                            Game.PlayerStats.HeadPiece = 0;
                        }
                        else
                        {
                            m_autosaveLoaded = false;
                            RCScreenManager manager = Game.ScreenManager;
                            manager.DialogueScreen.SetDialogue("Save File Error 2");
                            Game.gameIsCorrupt = true;
                            manager.DialogueScreen.SetConfirmEndHandler(m_game, "Exit");
                            manager.DisplayScreen(ScreenType.Dialogue, false, null);
                            Game.PlayerStats.HeadPiece = 0;
                        }
                    }
                    else throw new Exception();
                }
            }
            else
            {
                if (LevelEV.DISABLE_SAVING == false)
                {
                    GetStoragePath();
                    foreach (SaveType loadType in loadList)
                        LoadData(loadType, level);
                }
            }
        }

        public void ForceBackup()
        {
            RCScreenManager manager = Game.ScreenManager;
            manager.DialogueScreen.SetDialogue("Save File Error");
            manager.DialogueScreen.SetConfirmEndHandler(this, "LoadAutosave");
            manager.DisplayScreen(ScreenType.Dialogue, false, null);
        }

        public void LoadAutosave()
        {
            Console.WriteLine("Save file corrupted");
            SkillSystem.ResetAllTraits();
            Game.PlayerStats.Dispose();
            Game.PlayerStats = new PlayerStats();
            Game.ScreenManager.Player.Reset();
            LoadBackups();
            Game.ScreenManager.DisplayScreen(ScreenType.Title, true);
        }

        public void StartNewGame()
        {
            this.ClearAllFileTypes(false);
            this.ClearAllFileTypes(true);
            SkillSystem.ResetAllTraits();
            Game.PlayerStats.Dispose();
            Game.PlayerStats = new PlayerStats();
            Game.ScreenManager.Player.Reset();
            Game.ScreenManager.DisplayScreen(ScreenType.TutorialRoom, true);
        }

        public void ResetAutosave()
        {
            m_autosaveLoaded = false;
        }

        public void LoadAllFileTypes(ProceduralLevelScreen level)
        {
            LoadFiles(level, SaveType.PlayerData, SaveType.UpgradeData, SaveType.Map, SaveType.MapData, SaveType.Lineage);
        }

        public void ClearFiles(params SaveType[] deleteList)
        {
            GetStoragePath();
            foreach (SaveType deleteType in deleteList)
                DeleteData(deleteType, false);
        }

        public void ClearBackupFiles(params SaveType[] deleteList)
        {
            GetStoragePath();
            foreach (SaveType deleteType in deleteList)
                DeleteData(deleteType, true);
        }

        public void ClearAllFileTypes(bool deleteBackups)
        {
            if (deleteBackups == false)
                ClearFiles(SaveType.PlayerData, SaveType.UpgradeData, SaveType.Map, SaveType.MapData, SaveType.Lineage);
            else
                ClearBackupFiles(SaveType.PlayerData, SaveType.UpgradeData, SaveType.Map, SaveType.MapData, SaveType.Lineage);
        }

        private void DeleteData(SaveType deleteType, bool isBackup)
        {
            string fileName;
            switch (deleteType)
            {
                case SaveType.PlayerData: fileName = m_fileNamePlayer; break;
                case SaveType.UpgradeData: fileName = m_fileNameUpgrades; break;
                case SaveType.Map: fileName = m_fileNameMap; break;
                case SaveType.MapData: fileName = m_fileNameMapData; break;
                case SaveType.Lineage: fileName = m_fileNameLineage; break;
                default: return;
            }

            if (isBackup)
            {
                fileName = "AutoSave_" + fileName;
            }

            string fullPath = Path.Combine(m_storagePath, "Profile" + Game.GameConfig.ProfileSlot, fileName);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
            Console.WriteLine("Save file type " + deleteType + (isBackup ? " (Backup)" : "") + " deleted.");
        }
        
        private void LoadBackups()
        {
            Console.WriteLine("Replacing save file with back up saves");
            GetStoragePath();
            string profileDir = Path.Combine(m_storagePath, "Profile" + Game.GameConfig.ProfileSlot);

            LoadBackupForFile(profileDir, m_fileNamePlayer);
            LoadBackupForFile(profileDir, m_fileNameUpgrades);
            LoadBackupForFile(profileDir, m_fileNameMap);
            LoadBackupForFile(profileDir, m_fileNameMapData);
            LoadBackupForFile(profileDir, m_fileNameLineage);
            
            m_autosaveLoaded = true;
        }

        private void LoadBackupForFile(string profileDir, string fileName)
        {
            string autoSavePath = Path.Combine(profileDir, "AutoSave_" + fileName);
            string mainSavePath = Path.Combine(profileDir, fileName);
            string corruptionBackupPath = Path.Combine(profileDir, "AutoSaveBACKUP_" + fileName);

            if (File.Exists(autoSavePath) && File.Exists(mainSavePath))
            {
                File.Copy(autoSavePath, corruptionBackupPath, true);
                File.Copy(autoSavePath, mainSavePath, true);
            }
        }


        private void SaveData(SaveType saveType, bool saveBackup)
        {
            switch (saveType)
            {
                case (SaveType.PlayerData):
                    SavePlayerData(saveBackup);
                    break;
                case (SaveType.UpgradeData):
                    SaveUpgradeData(saveBackup);
                    break;
                case (SaveType.Map):
                    SaveMap(saveBackup);
                    break;
                case (SaveType.MapData):
                    SaveMapData(saveBackup);
                    break;
                case (SaveType.Lineage):
                    SaveLineageData(saveBackup);
                    break;
            }
            Console.WriteLine("\nData type " + saveType + " saved!");
        }

        private string GetFullSavePath(string baseFileName, bool isBackup)
        {
            string fileName = isBackup ? "AutoSave_" + baseFileName : baseFileName;
            return Path.Combine(m_storagePath, "Profile" + Game.GameConfig.ProfileSlot, fileName);
        }

        private void SavePlayerData(bool saveBackup)
        {
            string fullPath = GetFullSavePath(m_fileNamePlayer, saveBackup);

            if (Game.PlayerStats.RevisionNumber <= 0)
            {
                string playerName = Game.PlayerStats.PlayerName;
                string romanNumeral = "";
                Game.ConvertPlayerNameFormat(ref playerName, ref romanNumeral);
                Game.PlayerStats.PlayerName = playerName;
                Game.PlayerStats.RomanNumeral = romanNumeral;
            }

            using (Stream stream = File.Create(fullPath))
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(Game.PlayerStats.Gold);
                Game.PlayerStats.CurrentHealth = Game.ScreenManager.Player.CurrentHealth;
                writer.Write(Game.PlayerStats.CurrentHealth);
                Game.PlayerStats.CurrentMana = (int)Game.ScreenManager.Player.CurrentMana;
                writer.Write(Game.PlayerStats.CurrentMana);
                writer.Write(Game.PlayerStats.Age);
                writer.Write(Game.PlayerStats.ChildAge);
                writer.Write(Game.PlayerStats.Spell);
                writer.Write(Game.PlayerStats.Class);
                writer.Write(Game.PlayerStats.SpecialItem);
                writer.Write((byte)Game.PlayerStats.Traits.X);
                writer.Write((byte)Game.PlayerStats.Traits.Y);
                writer.Write(Game.PlayerStats.PlayerName);
                writer.Write(Game.PlayerStats.HeadPiece);
                writer.Write(Game.PlayerStats.ShoulderPiece);
                writer.Write(Game.PlayerStats.ChestPiece);
                writer.Write(Game.PlayerStats.DiaryEntry);
                writer.Write(Game.PlayerStats.BonusHealth);
                writer.Write(Game.PlayerStats.BonusStrength);
                writer.Write(Game.PlayerStats.BonusMana);
                writer.Write(Game.PlayerStats.BonusDefense);
                writer.Write(Game.PlayerStats.BonusWeight);
                writer.Write(Game.PlayerStats.BonusMagic);
                writer.Write(Game.PlayerStats.LichHealth);
                writer.Write(Game.PlayerStats.LichMana);
                writer.Write(Game.PlayerStats.LichHealthMod);
                writer.Write(Game.PlayerStats.NewBossBeaten);
                writer.Write(Game.PlayerStats.EyeballBossBeaten);
                writer.Write(Game.PlayerStats.FairyBossBeaten);
                writer.Write(Game.PlayerStats.FireballBossBeaten);
                writer.Write(Game.PlayerStats.BlobBossBeaten);
                writer.Write(Game.PlayerStats.LastbossBeaten);
                writer.Write(Game.PlayerStats.TimesCastleBeaten);
                writer.Write(Game.PlayerStats.NumEnemiesBeaten);
                writer.Write(Game.PlayerStats.TutorialComplete);
                writer.Write(Game.PlayerStats.CharacterFound);
                writer.Write(Game.PlayerStats.LoadStartingRoom);
                writer.Write(Game.PlayerStats.LockCastle);
                writer.Write(Game.PlayerStats.SpokeToBlacksmith);
                writer.Write(Game.PlayerStats.SpokeToEnchantress);
                writer.Write(Game.PlayerStats.SpokeToArchitect);
                writer.Write(Game.PlayerStats.SpokeToTollCollector);
                writer.Write(Game.PlayerStats.IsDead);
                writer.Write(Game.PlayerStats.FinalDoorOpened);
                writer.Write(Game.PlayerStats.RerolledChildren);
                writer.Write(Game.PlayerStats.IsFemale);
                writer.Write(Game.PlayerStats.TimesDead);
                writer.Write(Game.PlayerStats.HasArchitectFee);
                writer.Write(Game.PlayerStats.ReadLastDiary);
                writer.Write(Game.PlayerStats.SpokenToLastBoss);
                writer.Write(Game.PlayerStats.HardcoreMode);
                Game.PlayerStats.TotalHoursPlayed += Game.HoursPlayedSinceLastSave;
                Game.HoursPlayedSinceLastSave = 0;
                writer.Write(Game.PlayerStats.TotalHoursPlayed);
                writer.Write((byte)Game.PlayerStats.WizardSpellList.X);
                writer.Write((byte)Game.PlayerStats.WizardSpellList.Y);
                writer.Write((byte)Game.PlayerStats.WizardSpellList.Z);

                List<Vector4> enemyList = Game.PlayerStats.EnemiesKilledList;
                foreach (Vector4 enemy in enemyList)
                {
                    writer.Write((byte)enemy.X);
                    writer.Write((byte)enemy.Y);
                    writer.Write((byte)enemy.Z);
                    writer.Write((byte)enemy.W);
                }

                int numKilledEnemiesInRun = Game.PlayerStats.EnemiesKilledInRun.Count;
                List<Vector2> enemiesKilledInRun = Game.PlayerStats.EnemiesKilledInRun;
                writer.Write(numKilledEnemiesInRun);
                foreach (Vector2 enemyData in enemiesKilledInRun)
                {
                    writer.Write((int)enemyData.X);
                    writer.Write((int)enemyData.Y);
                }

                writer.Write(Game.PlayerStats.ChallengeEyeballUnlocked);
                writer.Write(Game.PlayerStats.ChallengeSkullUnlocked);
                writer.Write(Game.PlayerStats.ChallengeFireballUnlocked);
                writer.Write(Game.PlayerStats.ChallengeBlobUnlocked);
                writer.Write(Game.PlayerStats.ChallengeLastBossUnlocked);
                writer.Write(Game.PlayerStats.ChallengeEyeballBeaten);
                writer.Write(Game.PlayerStats.ChallengeSkullBeaten);
                writer.Write(Game.PlayerStats.ChallengeFireballBeaten);
                writer.Write(Game.PlayerStats.ChallengeBlobBeaten);
                writer.Write(Game.PlayerStats.ChallengeLastBossBeaten);
                writer.Write(Game.PlayerStats.ChallengeEyeballTimesUpgraded);
                writer.Write(Game.PlayerStats.ChallengeSkullTimesUpgraded);
                writer.Write(Game.PlayerStats.ChallengeFireballTimesUpgraded);
                writer.Write(Game.PlayerStats.ChallengeBlobTimesUpgraded);
                writer.Write(Game.PlayerStats.ChallengeLastBossTimesUpgraded);
                writer.Write(Game.PlayerStats.RomanNumeral);
                writer.Write(Game.PlayerStats.HasProsopagnosia);
                writer.Write(LevelEV.SAVEFILE_REVISION_NUMBER);
                writer.Write(Game.PlayerStats.ArchitectUsed);
            }
        }

        private void SaveUpgradeData(bool saveBackup)
        {
            string fullPath = GetFullSavePath(m_fileNameUpgrades, saveBackup);

            using (Stream stream = File.Create(fullPath))
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                List<byte[]> blueprintArray = Game.PlayerStats.GetBlueprintArray;
                foreach (byte[] categoryType in blueprintArray)
                foreach (byte equipmentState in categoryType)
                    writer.Write(equipmentState);

                List<byte[]> abilityBPArray = Game.PlayerStats.GetRuneArray;
                foreach (byte[] categoryType in abilityBPArray)
                foreach (byte equipmentState in categoryType)
                    writer.Write(equipmentState);
                
                sbyte[] equippedArray = Game.PlayerStats.GetEquippedArray;
                foreach (sbyte equipmentState in equippedArray)
                    writer.Write(equipmentState);

                sbyte[] equippedAbilityArray = Game.PlayerStats.GetEquippedRuneArray;
                foreach (sbyte equipmentState in equippedAbilityArray)
                    writer.Write(equipmentState);

                SkillObj[] skillArray = SkillSystem.GetSkillArray();
                foreach (SkillObj skill in skillArray)
                    writer.Write(skill.CurrentLevel);
            }
        }

        private void SaveMap(bool saveBackup)
        {
            string fullPath = GetFullSavePath(m_fileNameMap, saveBackup);
            
            using (Stream stream = File.Create(fullPath))
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                ProceduralLevelScreen levelToSave = Game.ScreenManager.GetLevelScreen();
                if (levelToSave != null)
                {
                    if (LevelEV.RUN_DEMO_VERSION == true)
                        writer.Write(levelToSave.RoomList.Count - 4);
                    else
                        writer.Write(levelToSave.RoomList.Count - 12);

                    List<byte> enemyTypeList = new List<byte>();
                    List<byte> enemyDifficultyList = new List<byte>();

                    foreach (RoomObj room in levelToSave.RoomList)
                    {
                        if (room.Name != "Boss" && room.Name != "Tutorial" && room.Name != "Ending" && room.Name != "Compass" && room.Name != "ChallengeBoss")
                        {
                            writer.Write((int)room.PoolIndex);
                            writer.Write((byte)room.LevelType);
                            writer.Write((int)room.X);
                            writer.Write((int)room.Y);
                            writer.Write((byte)room.TextureColor.R);
                            writer.Write((byte)room.TextureColor.G);
                            writer.Write((byte)room.TextureColor.B);

                            foreach (EnemyObj enemy in room.EnemyList)
                            {
                                if (enemy.IsProcedural == true)
                                {
                                    enemyTypeList.Add(enemy.Type);
                                    enemyDifficultyList.Add((byte)enemy.Difficulty);
                                }
                            }
                        }
                    }
                    writer.Write(enemyTypeList.Count);
                    foreach (byte enemyType in enemyTypeList)
                        writer.Write(enemyType);
                    foreach (byte enemyDifficulty in enemyDifficultyList)
                        writer.Write(enemyDifficulty);
                }
                else
                    Console.WriteLine("WARNING: Attempting to save LEVEL screen but it was null.");
            }
        }

        private void SaveMapData(bool saveBackup)
        {
            string fullPath = GetFullSavePath(m_fileNameMapData, saveBackup);
            
            using (Stream stream = File.Create(fullPath))
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                ProceduralLevelScreen levelToSave = Game.ScreenManager.GetLevelScreen();
                if (levelToSave != null)
                {
                    List<RoomObj> levelMapList = levelToSave.MapRoomsAdded;
                    List<bool> roomVisited = new List<bool>();
                    List<bool> bonusRoomCompleted = new List<bool>();
                    List<int> bonusRoomData = new List<int>();
                    List<bool> chestOpened = new List<bool>();
                    List<byte> chestTypes = new List<byte>();
                    List<bool> fairyChestFailed = new List<bool>();
                    List<bool> enemyDead = new List<bool>();
                    List<bool> breakablesOpened = new List<bool>();

                    foreach (RoomObj room in levelToSave.RoomList)
                    {
                        roomVisited.Add(levelMapList.Contains(room));
                        if (room is BonusRoomObj bonusRoom)
                        {
                            bonusRoomCompleted.Add(bonusRoom.RoomCompleted);
                            bonusRoomData.Add(bonusRoom.ID);
                        }
                        if (room.Name != "Boss" && room.Name != "ChallengeBoss")
                            foreach (EnemyObj enemy in room.EnemyList)
                                enemyDead.Add(enemy.IsKilled);
                        
                        if (room.Name != "Bonus" && room.Name != "Boss" && room.Name != "Compass" && room.Name != "ChallengeBoss")
                        {
                            foreach (GameObj obj in room.GameObjList)
                            {
                                if (obj is BreakableObj breakable)
                                    breakablesOpened.Add(breakable.Broken);
                                else if (obj is ChestObj chest)
                                {
                                    chestTypes.Add(chest.ChestType);
                                    chestOpened.Add(chest.IsOpen);
                                    if (chest is FairyChestObj fairyChest)
                                        fairyChestFailed.Add(fairyChest.State == ChestConditionChecker.STATE_FAILED);
                                }
                            }
                        }
                    }
                    writer.Write(roomVisited.Count);
                    foreach (bool state in roomVisited) writer.Write(state);
                    writer.Write(bonusRoomCompleted.Count);
                    foreach (bool state in bonusRoomCompleted) writer.Write(state);
                    foreach (int data in bonusRoomData) writer.Write(data);
                    writer.Write(chestTypes.Count);
                    foreach (byte type in chestTypes) writer.Write(type);
                    writer.Write(chestOpened.Count);
                    foreach (bool state in chestOpened) writer.Write(state);
                    writer.Write(fairyChestFailed.Count);
                    foreach (bool state in fairyChestFailed) writer.Write(state);
                    writer.Write(enemyDead.Count);
                    foreach (bool state in enemyDead) writer.Write(state);
                    writer.Write(breakablesOpened.Count);
                    foreach (bool state in breakablesOpened) writer.Write(state);
                }
                else
                    Console.WriteLine("WARNING: Attempting to save level screen MAP data but level was null.");
            }
        }

        private void SaveLineageData(bool saveBackup)
        {
            string fullPath = GetFullSavePath(m_fileNameLineage, saveBackup);

            using (Stream stream = File.Create(fullPath))
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                List<PlayerLineageData> currentBranches = Game.PlayerStats.CurrentBranches;
                int numChildren = currentBranches?.Count ?? 0;
                writer.Write(numChildren);

                for (int i = 0; i < numChildren; i++)
                {
                    PlayerLineageData data = currentBranches[i];
                    if (Game.PlayerStats.RevisionNumber <= 0)
                    {
                        string playerName = data.Name;
                        string romanNumeral = "";
                        Game.ConvertPlayerNameFormat(ref playerName, ref romanNumeral);
                        data.Name = playerName;
                        data.RomanNumeral = romanNumeral;
                        currentBranches[i] = data;
                    }
                    writer.Write(data.Name);
                    writer.Write(data.Spell);
                    writer.Write(data.Class);
                    writer.Write(data.HeadPiece);
                    writer.Write(data.ChestPiece);
                    writer.Write(data.ShoulderPiece);
                    writer.Write(data.Age);
                    writer.Write(data.ChildAge);
                    writer.Write((byte)data.Traits.X);
                    writer.Write((byte)data.Traits.Y);
                    writer.Write(data.IsFemale);
                    writer.Write(data.RomanNumeral);
                }

                List<FamilyTreeNode> familyTreeArray = Game.PlayerStats.FamilyTreeArray;
                int numBranches = familyTreeArray?.Count ?? 0;
                writer.Write(numBranches);
                
                for (int i = 0; i < numBranches; i++)
                {
                    FamilyTreeNode data = familyTreeArray[i];
                    if (Game.PlayerStats.RevisionNumber <= 0)
                    {
                        string playerName = data.Name;
                        string romanNumeral = "";
                        Game.ConvertPlayerNameFormat(ref playerName, ref romanNumeral);
                        data.Name = playerName;
                        data.RomanNumeral = romanNumeral;
                        familyTreeArray[i] = data;
                    }
                    writer.Write(data.Name);
                    writer.Write(data.Age);
                    writer.Write(data.Class);
                    writer.Write(data.HeadPiece);
                    writer.Write(data.ChestPiece);
                    writer.Write(data.ShoulderPiece);
                    writer.Write(data.NumEnemiesBeaten);
                    writer.Write(data.BeatenABoss);
                    writer.Write((byte)data.Traits.X);
                    writer.Write((byte)data.Traits.Y);
                    writer.Write(data.IsFemale);
                    writer.Write(data.RomanNumeral);
                }
            }
        }

        private void LoadData(SaveType loadType, ProceduralLevelScreen level)
        {
            if (FileExists(loadType))
            {
                switch (loadType)
                {
                    case (SaveType.PlayerData): LoadPlayerData(); break;
                    case (SaveType.UpgradeData): LoadUpgradeData(); break;
                    case (SaveType.Map): Console.WriteLine("Cannot load Map directly from LoadData. Call LoadMap() instead."); break;
                    case (SaveType.MapData): if (level != null) LoadMapData(level); else Console.WriteLine("Could not load Map data."); break;
                    case (SaveType.Lineage): LoadLineageData(); break;
                }
                Console.WriteLine("\nData of type " + loadType + " Loaded.");
            }
            else
                Console.WriteLine("Could not load data of type " + loadType + " because data did not exist.");
        }

        private void LoadPlayerData()
        {
            string fullPath = GetFullSavePath(m_fileNamePlayer, false);
            using (Stream stream = File.OpenRead(fullPath))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                Game.PlayerStats.Gold = reader.ReadInt32();
                Game.PlayerStats.CurrentHealth = reader.ReadInt32();
                Game.PlayerStats.CurrentMana = reader.ReadInt32();
                Game.PlayerStats.Age = reader.ReadByte();
                Game.PlayerStats.ChildAge = reader.ReadByte();
                Game.PlayerStats.Spell = reader.ReadByte();
                Game.PlayerStats.Class = reader.ReadByte();
                Game.PlayerStats.SpecialItem = reader.ReadByte();
                Game.PlayerStats.Traits = new Vector2(reader.ReadByte(), reader.ReadByte());
                Game.PlayerStats.PlayerName = reader.ReadString();
                Game.PlayerStats.HeadPiece = reader.ReadByte();
                Game.PlayerStats.ShoulderPiece = reader.ReadByte();
                Game.PlayerStats.ChestPiece = reader.ReadByte();

                if (Game.PlayerStats.HeadPiece == 0 && Game.PlayerStats.ShoulderPiece == 0 && Game.PlayerStats.ChestPiece == 0)
                   throw new Exception("Corrupted Save File: All equipment pieces are 0.");

                Game.PlayerStats.DiaryEntry = reader.ReadByte();
                Game.PlayerStats.BonusHealth = reader.ReadInt32();
                Game.PlayerStats.BonusStrength = reader.ReadInt32();
                Game.PlayerStats.BonusMana = reader.ReadInt32();
                Game.PlayerStats.BonusDefense = reader.ReadInt32();
                Game.PlayerStats.BonusWeight = reader.ReadInt32();
                Game.PlayerStats.BonusMagic = reader.ReadInt32();
                Game.PlayerStats.LichHealth = reader.ReadInt32();
                Game.PlayerStats.LichMana = reader.ReadInt32();
                Game.PlayerStats.LichHealthMod = reader.ReadSingle();
                Game.PlayerStats.NewBossBeaten = reader.ReadBoolean();
                Game.PlayerStats.EyeballBossBeaten = reader.ReadBoolean();
                Game.PlayerStats.FairyBossBeaten = reader.ReadBoolean();
                Game.PlayerStats.FireballBossBeaten = reader.ReadBoolean();
                Game.PlayerStats.BlobBossBeaten = reader.ReadBoolean();
                Game.PlayerStats.LastbossBeaten = reader.ReadBoolean();
                Game.PlayerStats.TimesCastleBeaten = reader.ReadInt32();
                Game.PlayerStats.NumEnemiesBeaten = reader.ReadInt32();
                Game.PlayerStats.TutorialComplete = reader.ReadBoolean();
                Game.PlayerStats.CharacterFound = reader.ReadBoolean();
                Game.PlayerStats.LoadStartingRoom = reader.ReadBoolean();
                Game.PlayerStats.LockCastle = reader.ReadBoolean();
                Game.PlayerStats.SpokeToBlacksmith = reader.ReadBoolean();
                Game.PlayerStats.SpokeToEnchantress = reader.ReadBoolean();
                Game.PlayerStats.SpokeToArchitect = reader.ReadBoolean();
                Game.PlayerStats.SpokeToTollCollector = reader.ReadBoolean();
                Game.PlayerStats.IsDead = reader.ReadBoolean();
                Game.PlayerStats.FinalDoorOpened = reader.ReadBoolean();
                Game.PlayerStats.RerolledChildren = reader.ReadBoolean();
                Game.PlayerStats.IsFemale = reader.ReadBoolean();
                Game.PlayerStats.TimesDead = reader.ReadInt32();
                Game.PlayerStats.HasArchitectFee = reader.ReadBoolean();
                Game.PlayerStats.ReadLastDiary = reader.ReadBoolean();
                Game.PlayerStats.SpokenToLastBoss = reader.ReadBoolean();
                Game.PlayerStats.HardcoreMode = reader.ReadBoolean();
                Game.PlayerStats.TotalHoursPlayed = reader.ReadSingle();
                Game.PlayerStats.WizardSpellList = new Vector3(reader.ReadByte(), reader.ReadByte(), reader.ReadByte());

                for (int i = 0; i < EnemyType.Total; i++)
                    Game.PlayerStats.EnemiesKilledList[i] = new Vector4(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                
                int numKilledEnemiesInRun = reader.ReadInt32();
                for (int i = 0; i < numKilledEnemiesInRun; i++)
                    Game.PlayerStats.EnemiesKilledInRun.Add(new Vector2(reader.ReadInt32(), reader.ReadInt32()));

                if (reader.BaseStream.Position != reader.BaseStream.Length)
                {
                    Game.PlayerStats.ChallengeEyeballUnlocked = reader.ReadBoolean();
                    Game.PlayerStats.ChallengeSkullUnlocked = reader.ReadBoolean();
                    Game.PlayerStats.ChallengeFireballUnlocked = reader.ReadBoolean();
                    Game.PlayerStats.ChallengeBlobUnlocked = reader.ReadBoolean();
                    Game.PlayerStats.ChallengeLastBossUnlocked = reader.ReadBoolean();
                    Game.PlayerStats.ChallengeEyeballBeaten = reader.ReadBoolean();
                    Game.PlayerStats.ChallengeSkullBeaten = reader.ReadBoolean();
                    Game.PlayerStats.ChallengeFireballBeaten = reader.ReadBoolean();
                    Game.PlayerStats.ChallengeBlobBeaten = reader.ReadBoolean();
                    Game.PlayerStats.ChallengeLastBossBeaten = reader.ReadBoolean();
                    if (reader.BaseStream.Position != reader.BaseStream.Length)
                    {
                        Game.PlayerStats.ChallengeEyeballTimesUpgraded = reader.ReadSByte();
                        Game.PlayerStats.ChallengeSkullTimesUpgraded = reader.ReadSByte();
                        Game.PlayerStats.ChallengeFireballTimesUpgraded = reader.ReadSByte();
                        Game.PlayerStats.ChallengeBlobTimesUpgraded = reader.ReadSByte();
                        Game.PlayerStats.ChallengeLastBossTimesUpgraded = reader.ReadSByte();
                        Game.PlayerStats.RomanNumeral = reader.ReadString();
                        Game.PlayerStats.HasProsopagnosia = reader.ReadBoolean();
                        Game.PlayerStats.RevisionNumber = reader.ReadInt32();
                        if (reader.BaseStream.Position != reader.BaseStream.Length)
                        {
                            Game.PlayerStats.ArchitectUsed = reader.ReadBoolean();
                        }
                    }
                }
            }
        }

        private void LoadUpgradeData()
        {
            string fullPath = GetFullSavePath(m_fileNameUpgrades, false);
            using (Stream stream = File.OpenRead(fullPath))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                List<byte[]> blueprintArray = Game.PlayerStats.GetBlueprintArray;
                for (int i = 0; i < EquipmentCategoryType.Total; i++)
                for (int k = 0; k < EquipmentBaseType.Total; k++)
                    blueprintArray[i][k] = reader.ReadByte();

                List<byte[]> abilityBPArray = Game.PlayerStats.GetRuneArray;
                for (int i = 0; i < EquipmentCategoryType.Total; i++)
                for (int k = 0; k < EquipmentAbilityType.Total; k++)
                    abilityBPArray[i][k] = reader.ReadByte();

                sbyte[] equippedArray = Game.PlayerStats.GetEquippedArray;
                for (int i = 0; i < EquipmentCategoryType.Total; i++)
                    equippedArray[i] = reader.ReadSByte();

                sbyte[] equippedAbilityArray = Game.PlayerStats.GetEquippedRuneArray;
                for (int i = 0; i < EquipmentCategoryType.Total; i++)
                    equippedAbilityArray[i] = reader.ReadSByte();

                SkillObj[] traitArray = SkillSystem.GetSkillArray();
                SkillSystem.ResetAllTraits();
                Game.PlayerStats.CurrentLevel = 0;
                for (int i = 0; i < (int)SkillType.DIVIDER - 2; i++)
                {
                    int traitLevel = reader.ReadInt32();
                    for (int k = 0; k < traitLevel; k++)
                        SkillSystem.LevelUpTrait(traitArray[i], false);
                }
                Game.ScreenManager.Player.UpdateEquipmentColours();
            }
        }

        public ProceduralLevelScreen LoadMap()
        {
            GetStoragePath();
            ProceduralLevelScreen loadedLevel = null;
            string fullPath = GetFullSavePath(m_fileNameMap, false);
            using (Stream stream = File.OpenRead(fullPath))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                int roomSize = reader.ReadInt32();
                Vector4[] roomList = new Vector4[roomSize];
                Vector3[] roomColorList = new Vector3[roomSize];

                for (int i = 0; i < roomSize; i++)
                {
                    roomList[i].W = reader.ReadInt32();
                    roomList[i].X = reader.ReadByte();
                    roomList[i].Y = reader.ReadInt32();
                    roomList[i].Z = reader.ReadInt32();
                    roomColorList[i].X = reader.ReadByte();
                    roomColorList[i].Y = reader.ReadByte();
                    roomColorList[i].Z = reader.ReadByte();
                }
                loadedLevel = LevelBuilder2.CreateLevel(roomList, roomColorList);

                int numEnemies = reader.ReadInt32();
                List<byte> enemyList = new List<byte>();
                for (int i = 0; i < numEnemies; i++)
                    enemyList.Add(reader.ReadByte());

                List<byte> enemyDifficultyList = new List<byte>();
                for (int i = 0; i < numEnemies; i++)
                    enemyDifficultyList.Add(reader.ReadByte());

                LevelBuilder2.OverrideProceduralEnemies(loadedLevel, enemyList.ToArray(), enemyDifficultyList.ToArray());
            }
            return loadedLevel;
        }

        private void LoadMapData(ProceduralLevelScreen createdLevel)
        {
            string fullPath = GetFullSavePath(m_fileNameMapData, false);
            using (Stream stream = File.OpenRead(fullPath))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                int numRooms = reader.ReadInt32();
                List<bool> roomsVisited = new List<bool>();
                for (int i = 0; i < numRooms; i++) roomsVisited.Add(reader.ReadBoolean());
                
                int numBonusRooms = reader.ReadInt32();
                List<bool> bonusRoomStates = new List<bool>();
                for (int i = 0; i < numBonusRooms; i++) bonusRoomStates.Add(reader.ReadBoolean());

                List<int> bonusRoomData = new List<int>();
                for (int i = 0; i < numBonusRooms; i++) bonusRoomData.Add(reader.ReadInt32());

                int numChests = reader.ReadInt32();
                List<byte> chestTypes = new List<byte>();
                for (int i = 0; i < numChests; i++) chestTypes.Add(reader.ReadByte());

                numChests = reader.ReadInt32();
                List<bool> chestStates = new List<bool>();
                for (int i = 0; i < numChests; i++) chestStates.Add(reader.ReadBoolean());

                numChests = reader.ReadInt32();
                List<bool> fairyChestStates = new List<bool>();
                for (int i = 0; i < numChests; i++) fairyChestStates.Add(reader.ReadBoolean());

                int numEnemies = reader.ReadInt32();
                List<bool> enemyStates = new List<bool>();
                for (int i = 0; i < numEnemies; i++) enemyStates.Add(reader.ReadBoolean());
                
                int numBreakables = reader.ReadInt32();
                List<bool> breakableStates = new List<bool>();
                for (int i = 0; i < numBreakables; i++) breakableStates.Add(reader.ReadBoolean());

                int bonusRoomCounter = 0, chestTypeCounter = 0, chestCounter = 0, fairyChestCounter = 0, enemyCounter = 0, breakablesCounter = 0;

                foreach (RoomObj room in createdLevel.RoomList)
                {
                    if (room is BonusRoomObj bonusRoom && bonusRoomCounter < bonusRoomStates.Count)
                    {
                        if (bonusRoomStates[bonusRoomCounter])
                            bonusRoom.RoomCompleted = true;
                        bonusRoom.ID = bonusRoomData[bonusRoomCounter];
                        bonusRoomCounter++;
                    }

                    if (Game.PlayerStats.LockCastle == false && room.Name != "Boss" && room.Name != "ChallengeBoss")
                        foreach (EnemyObj enemy in room.EnemyList)
                            if (enemyStates[enemyCounter++])
                                enemy.KillSilently();

                    if (room.Name != "Bonus" && room.Name != "Boss" && room.Name != "Compass" && room.Name != "ChallengeBoss")
                    {
                        foreach (GameObj obj in room.GameObjList)
                        {
                            if (Game.PlayerStats.LockCastle == false && obj is BreakableObj breakable && breakablesCounter < breakableStates.Count && breakableStates[breakablesCounter++])
                                breakable.ForceBreak();
                            else if (obj is ChestObj chest && chestCounter < chestStates.Count)
                            {
                                chest.IsProcedural = false;
                                chest.ChestType = chestTypes[chestTypeCounter++];
                                if (chestStates[chestCounter++])
                                    chest.ForceOpen();
                                if (Game.PlayerStats.LockCastle == false && chest is FairyChestObj fairyChest && fairyChestCounter < fairyChestStates.Count && fairyChestStates[fairyChestCounter++])
                                    fairyChest.SetChestFailed(true);
                            }
                        }
                    }
                }
                if (numRooms > 0)
                {
                    List<RoomObj> roomsVisitedList = new List<RoomObj>();
                    for (int i = 0; i < roomsVisited.Count; i++)
                        if (roomsVisited[i]) roomsVisitedList.Add(createdLevel.RoomList[i]);
                    createdLevel.MapRoomsUnveiled = roomsVisitedList;
                }
            }
        }

        private void LoadLineageData()
        {
            string fullPath = GetFullSavePath(m_fileNameLineage, false);
            using (Stream stream = File.OpenRead(fullPath))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                List<PlayerLineageData> loadedBranches = new List<PlayerLineageData>();
                int numChildren = reader.ReadInt32();
                for (int i = 0; i < numChildren; i++)
                {
                    PlayerLineageData data = new PlayerLineageData {
                        Name = reader.ReadString(), Spell = reader.ReadByte(), Class = reader.ReadByte(),
                        HeadPiece = reader.ReadByte(), ChestPiece = reader.ReadByte(), ShoulderPiece = reader.ReadByte(),
                        Age = reader.ReadByte(), ChildAge = reader.ReadByte(),
                        Traits = new Vector2(reader.ReadByte(), reader.ReadByte()), IsFemale = reader.ReadBoolean()
                    };
                    if (Game.PlayerStats.RevisionNumber > 0) data.RomanNumeral = reader.ReadString();
                    loadedBranches.Add(data);
                }
                if (loadedBranches.Count > 0)
                    Game.PlayerStats.CurrentBranches = loadedBranches;
                
                List<FamilyTreeNode> familyTree = new List<FamilyTreeNode>();
                int numBranches = reader.ReadInt32();
                for (int i = 0; i < numBranches; i++)
                {
                    FamilyTreeNode data = new FamilyTreeNode {
                        Name = reader.ReadString(), Age = reader.ReadByte(), Class = reader.ReadByte(),
                        HeadPiece = reader.ReadByte(), ChestPiece = reader.ReadByte(), ShoulderPiece = reader.ReadByte(),
                        NumEnemiesBeaten = reader.ReadInt32(), BeatenABoss = reader.ReadBoolean(),
                        Traits = new Vector2(reader.ReadByte(), reader.ReadByte()), IsFemale = reader.ReadBoolean()
                    };
                    if (Game.PlayerStats.RevisionNumber > 0) data.RomanNumeral = reader.ReadString();
                    familyTree.Add(data);
                }
                if (familyTree.Count > 0)
                    Game.PlayerStats.FamilyTreeArray = familyTree;
            }
        }

        public bool FileExists(SaveType saveType)
        {
            GetStoragePath();
            string fileName;
            switch (saveType)
            {
                case SaveType.PlayerData: fileName = m_fileNamePlayer; break;
                case SaveType.UpgradeData: fileName = m_fileNameUpgrades; break;
                case SaveType.Map: fileName = m_fileNameMap; break;
                case SaveType.MapData: fileName = m_fileNameMapData; break;
                case SaveType.Lineage: fileName = m_fileNameLineage; break;
                default: return false;
            }
            string fullPath = Path.Combine(m_storagePath, "Profile" + Game.GameConfig.ProfileSlot, fileName);
            return File.Exists(fullPath);
        }

        public void GetSaveHeader(byte profile, out byte playerClass, out string playerName, out int playerLevel, out bool playerIsDead, out int castlesBeaten, out bool isFemale)
        {
            playerName = null;
            playerClass = 0;
            playerLevel = 0;
            playerIsDead = false;
            castlesBeaten = 0;
            isFemale = false;

            GetStoragePath();
            string playerFilePath = Path.Combine(m_storagePath, "Profile" + profile, m_fileNamePlayer);
            string upgradesFilePath = Path.Combine(m_storagePath, "Profile" + profile, m_fileNameUpgrades);

            if (File.Exists(playerFilePath))
            {
                using (Stream stream = File.OpenRead(playerFilePath))
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    reader.ReadInt32(); reader.ReadInt32(); reader.ReadInt32();
                    reader.ReadByte(); reader.ReadByte(); reader.ReadByte();
                    playerClass = reader.ReadByte();
                    reader.ReadByte(); reader.ReadByte(); reader.ReadByte();
                    playerName = reader.ReadString();
                    reader.ReadByte(); reader.ReadByte(); reader.ReadByte(); reader.ReadByte();
                    reader.ReadInt32(); reader.ReadInt32(); reader.ReadInt32(); reader.ReadInt32(); reader.ReadInt32(); reader.ReadInt32();
                    reader.ReadInt32(); reader.ReadInt32(); reader.ReadSingle();
                    reader.ReadBoolean(); reader.ReadBoolean(); reader.ReadBoolean(); reader.ReadBoolean(); reader.ReadBoolean(); reader.ReadBoolean();
                    castlesBeaten = reader.ReadInt32();
                    reader.ReadInt32();
                    reader.ReadBoolean(); reader.ReadBoolean(); reader.ReadBoolean(); reader.ReadBoolean(); reader.ReadBoolean();
                    reader.ReadBoolean(); reader.ReadBoolean(); reader.ReadBoolean();
                    playerIsDead = reader.ReadBoolean();
                    reader.ReadBoolean(); reader.ReadBoolean();
                    isFemale = reader.ReadBoolean();
                }
            }

            if (File.Exists(upgradesFilePath))
            {
                using (Stream stream = File.OpenRead(upgradesFilePath))
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    reader.BaseStream.Seek(EquipmentCategoryType.Total * EquipmentBaseType.Total, SeekOrigin.Current);
                    reader.BaseStream.Seek(EquipmentCategoryType.Total * EquipmentAbilityType.Total, SeekOrigin.Current);
                    reader.BaseStream.Seek(EquipmentCategoryType.Total * 2, SeekOrigin.Current);

                    int levelCounter = 0;
                    for (int i = 0; i < (int)SkillType.DIVIDER - 2; i++)
                    {
                        int traitLevel = reader.ReadInt32();
                        levelCounter += traitLevel;
                    }
                    playerLevel = levelCounter;
                }
            }
        }
    }

    public enum SaveType
    {
        None,
        PlayerData,
        UpgradeData,
        Map,
        MapData,
        Lineage,
    }
}
