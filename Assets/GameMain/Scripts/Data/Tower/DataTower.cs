﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.Data;
using GameFramework.DataTable;
using UnityGameFramework.Runtime;
using GameFramework;

namespace Flower
{
    public sealed class DataTower : DataBase
    {
        private IDataTable<DRTower> dtTower;
        private IDataTable<DRTowerLevel> dtTowerLevel;

        private Dictionary<int, TowerData> dicTowerData;
        private Dictionary<int, TowerLevelData> dicTowerLevelData;
        private Dictionary<int, Tower> dicTower;

        private int serialId = 0;

        protected override void OnInit()
        {

        }

        protected override void OnPreload()
        {
            LoadDataTable("Tower");
            LoadDataTable("TowerLevel");
        }

        protected override void OnLoad()
        {
            dtTower = GameEntry.DataTable.GetDataTable<DRTower>();
            if (dtTower == null)
                throw new System.Exception("Can not get data table Tower");

            dtTowerLevel = GameEntry.DataTable.GetDataTable<DRTowerLevel>();
            if (dtTowerLevel == null)
                throw new System.Exception("Can not get data table TowerLevel");

            dicTowerData = new Dictionary<int, TowerData>();
            dicTowerLevelData = new Dictionary<int, TowerLevelData>();
            dicTower = new Dictionary<int, Tower>();

            DRTowerLevel[] drTowerLevels = dtTowerLevel.GetAllDataRows();
            foreach (var drTowerLevel in drTowerLevels)
            {
                if (dicTowerLevelData.ContainsKey(drTowerLevel.Id))
                {
                    throw new System.Exception(string.Format("Data tower level id '{0}' duplicate.", drTowerLevel.Id));
                }

                TowerLevelData towerLevelData = new TowerLevelData(drTowerLevel);
                dicTowerLevelData.Add(drTowerLevel.Id, towerLevelData);
            }

            DRTower[] drTowers = dtTower.GetAllDataRows();
            foreach (var drTower in drTowers)
            {
                TowerLevelData[] towerLevelDatas = new TowerLevelData[drTower.Levels.Length];
                for (int i = 0; i < drTower.Levels.Length; i++)
                {
                    if (!dicTowerLevelData.ContainsKey(drTower.Levels[i]))
                    {
                        throw new System.Exception(string.Format("Can not find tower level id '{0}' in DataTable TowerLevel.", drTower.Levels[i]));
                    }

                    towerLevelDatas[i] = dicTowerLevelData[drTower.Levels[i]];
                }

                TowerData towerData = new TowerData(drTower, towerLevelDatas);
                dicTowerData.Add(drTower.Id, towerData);
            }
        }

        private int GenrateSerialId()
        {
            return ++serialId;
        }

        public TowerData GetTowerData(int id)
        {
            if (!dicTowerData.ContainsKey(id))
            {
                Log.Error("Can not find tower data id '{0}'.", id);
                return null;
            }

            return dicTowerData[id];
        }

        public Tower CreateTower(int towerId, int level = 0)
        {
            if (!dicTowerData.ContainsKey(towerId))
            {
                Log.Error("Can not find tower data id '{0}'.", towerId);
                return null;
            }

            int serialId = GenrateSerialId();
            Tower tower = Tower.Create(dicTowerData[towerId], serialId, level);
            dicTower.Add(serialId, tower);

            return tower;
        }

        public void DestroyTower(int serialId)
        {
            if (!dicTower.ContainsKey(serialId))
            {
                Log.Error("Can not find tower serialId '{0}'.", serialId);
                return;
            }

            ReferencePool.Release(dicTower[serialId]);
        }

        public void DestroyTower(Tower tower)
        {
            DestroyTower(tower.SerialId);
        }

        public void UpgradeTower(int serialId)
        {
            if (!dicTower.ContainsKey(serialId))
            {
                Log.Error("Can not find tower serialId '{0}'.", serialId);
                return;
            }

            Tower tower = dicTower[serialId];
            if (tower.Level >= tower.MaxLevel)
            {
                Log.Error("Tower (id:'{0}') has reached the highest level", serialId);
                return;
            }

            tower.Upgrade();
        }

        public void UpgradeTower(Tower tower)
        {
            UpgradeTower(tower.SerialId);
        }

        public void SellTower()
        {

        }

        public void BuyTower()
        {

        }

        protected override void OnUnload()
        {
            GameEntry.DataTable.DestroyDataTable<DRTower>();
            GameEntry.DataTable.DestroyDataTable<DRTowerLevel>();

            dicTowerData = null;
            dicTowerLevelData = null;

            serialId = 0;
        }

        protected override void OnShutdown()
        {
        }
    }

}