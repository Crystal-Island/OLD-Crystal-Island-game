using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KoboldTools.Logging;

namespace Polymoney
{
    [Serializable]
    public class PolymoneyPlayerData
    {
        public readonly uint PlayerId;
        public int FiatAccountBalance;
        public int QAccountBalance;
        public int FiatDebt;
        public int QDebt;
        public int FiatNumberOfOffers;
        public int QNumberOfOffers;
        public int NumberOfTalents;
        public int TotalWelfare;
        public float RecreationHealthStatus;
        public float FoodHealthStatus;
        public int Enjoyment;

        public PolymoneyPlayerData(uint playerId)
        {
            this.PlayerId = playerId;
            this.FiatAccountBalance = 0;
            this.QAccountBalance = 0;
            this.FiatDebt = 0;
            this.QDebt = 0;
            this.FiatNumberOfOffers = 0;
            this.QNumberOfOffers = 0;
            this.NumberOfTalents = 0;
            this.TotalWelfare = 0;
            this.RecreationHealthStatus = 0.0f;
            this.FoodHealthStatus = 0.0f;
            this.Enjoyment = 0;
        }

        public PolymoneyPlayerData Copy()
        {
            // Too lazy to write a copy constructor, eh? :)
            PolymoneyPlayerData copy = new PolymoneyPlayerData(this.PlayerId);
            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(this), copy);
            return copy;
        }
    }

    [Serializable]
    public class PolymoneyGameData
    {
        public float FiatCirculationRate;
        public int FiatTurnover;
        public float QCirculationRate;
        public int QTurnover;
        public float FairyDustCirculationRate;
        public int CityAccountBalance;
        public float MaxDarkness;
        public float MaxBrightness;

        public PolymoneyGameData()
        {
            this.FiatCirculationRate = 0.0f;
            this.FiatTurnover = 0;
            this.QCirculationRate = 0.0f;
            this.QTurnover = 0;
            this.FairyDustCirculationRate = 0.0f;
            this.CityAccountBalance = 0;
            this.MaxDarkness = 0.0f;
            this.MaxBrightness = 0.0f;
        }

        public PolymoneyGameData Copy()
        {
            PolymoneyGameData copy = new PolymoneyGameData();
            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(this), copy);
            return copy;
        }
    }

    [Serializable]
    public class PolymoneyMonthData
    {
        public int id = 0;
        public List<PolymoneyPlayerData> Players;
        public PolymoneyGameData Game;
    }

    public class AnalyticsManager : MonoBehaviour
    {
        public List<PolymoneyPlayerData> Players;
        public PolymoneyGameData Game;
        public List<PolymoneyMonthData> Months;

        public AnalyticsManager()
        {
            this.Players = new List<PolymoneyPlayerData>();
            this.Game = new PolymoneyGameData();
        }

        public void SnapshotMonth(int month)
        {
            if(!Months.Any(m => m.id == month))
            {
                //this is a new month
                PolymoneyMonthData newMonth = new PolymoneyMonthData();
                newMonth.id = month;
                newMonth.Game = this.Game.Copy();
                newMonth.Players = new List<PolymoneyPlayerData>();
                foreach(PolymoneyPlayerData player in this.Players)
                {
                    newMonth.Players.Add(player.Copy());
                }
                this.Months.Add(newMonth);
            }
        }

        public void AddPlayer(uint playerId)
        {
            if (!this.HasPlayer(playerId))
            {
                this.Players.Add(new PolymoneyPlayerData(playerId));
            }
            else
            {
                RootLogger.Exception(this, "The player with ID {0} is already present.", playerId);
            }
        }

        public void SetPlayerEnjoyment(uint playerId, int value)
        {
            PolymoneyPlayerData data = this.GetPlayer(playerId);
            if (data != null)
            {
                data.Enjoyment = value;
            }
            else
            {
                RootLogger.Exception(this, "The player with ID {0} was not found.", playerId);
            }
        }

        public void SetAccountBalance(uint playerId, Currency currency, int value)
        {
            PolymoneyPlayerData data = this.GetPlayer(playerId);
            if (data != null)
            {
                switch (currency)
                {
                    case Currency.FIAT:
                        data.FiatAccountBalance = value;
                        if (value < 0)
                        {
                            data.FiatDebt = -value;
                        }
                        break;

                    case Currency.Q:
                        data.QAccountBalance = value;
                        if (value < 0)
                        {
                            data.QDebt = -value;
                        }
                        break;
                }
            }
            else
            {
                RootLogger.Exception(this, "The player with ID {0} was not found.", playerId);
            }
        }

        public void AddWelfarePayment(uint playerId, int value)
        {
            PolymoneyPlayerData data = this.GetPlayer(playerId);
            if (data != null)
            {
                data.TotalWelfare += value;
            }
            else
            {
                RootLogger.Exception(this, "The player with ID {0} was not found.", playerId);
            }
        }

        public void AddOffer(uint playerId, Currency currency)
        {
            PolymoneyPlayerData data = this.GetPlayer(playerId);
            if (data != null)
            {
                switch (currency)
                {
                    case Currency.FIAT:
                        data.FiatNumberOfOffers += 1;
                        break;

                    case Currency.Q:
                        data.QNumberOfOffers += 1;
                        break;
                }
            }
            else
            {
                RootLogger.Exception(this, "The player with ID {0} was not found.", playerId);
            }
        }

        public void SetFoodHealthStatus(uint playerId, float status)
        {
            PolymoneyPlayerData data = this.GetPlayer(playerId);
            if (data != null)
            {
                data.FoodHealthStatus = status;
            }
            else
            {
                RootLogger.Exception(this, "The player with ID {0} was not found.", playerId);
            }
        }

        public void SetCirculationRate(Currency currency, int value, float timeFrame)
        {
            switch (currency)
            {
                case Currency.FIAT:
                    this.Game.FiatTurnover += value;
                    this.Game.FiatCirculationRate = this.Game.FiatTurnover / timeFrame;
                    break;

                case Currency.Q:
                    this.Game.QTurnover += value;
                    this.Game.QCirculationRate = this.Game.QTurnover / timeFrame;
                    break;
            }
        }

        public void SetTalentNumber(uint playerId, int talents)
        {
            PolymoneyPlayerData data = this.GetPlayer(playerId);
            if (data != null)
            {
                data.NumberOfTalents = talents;
            }
            else
            {
                RootLogger.Exception(this, "The player with ID {0} was not found.", playerId);
            }
        }

        public void SetCityAccountBalance(int value)
        {
            this.Game.CityAccountBalance = value;
        }

        public void UpdateCityState(float value)
        {
            float darkness = 1.0f - value;
            if (this.Game.MaxDarkness < darkness)
            {
                this.Game.MaxDarkness = darkness;
            }
        }

        public void UpdateCityLuminance(float value)
        {
            if (this.Game.MaxBrightness < value)
            {
                this.Game.MaxBrightness = value;
            }
        }

        public int GetMaxBalance()
        {
            int balance = 0;
            foreach(PolymoneyPlayerData p in this.Players)
            {
                if (p.QAccountBalance + p.FiatAccountBalance > balance)
                    balance = p.QAccountBalance + p.FiatAccountBalance;
            }
            if (this.Game.CityAccountBalance > balance)
                balance = this.Game.CityAccountBalance;

            return balance;
        }

        private bool HasPlayer(uint playerId)
        {
            return this.Players.Count(e => e.PlayerId == playerId) > 0;
        }

        private PolymoneyPlayerData GetPlayer(uint playerId)
        {
            return this.Players.Where(e => e.PlayerId == playerId).SingleOrDefault();
        }

    }
}
