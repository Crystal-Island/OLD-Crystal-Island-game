using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using KoboldTools;
using KoboldTools.Logging;

namespace Polymoney
{
    public class PlayerAddedEvent : UnityEvent<Player> { }

    /// <summary>
    /// Describes the level model. It holds all level and game-progress-specific data.
    /// </summary>
    public interface ILevel
    {
        /// <summary>
        /// Holds the current number of in-game months.
        /// </summary>
        /// <value>The new number of months.</value>
        int months { get; set; }
        /// <summary>
        /// Determines the maximum duration of the game in months.
        /// </summary>
        int maximumMonths { get; }
        /// <summary>
        /// Defines the set of tags that describe the tax incident.
        /// </summary>
        List<string> taxTags { get; }
        /// <summary>
        /// Defines the set of tags that describe the welfare offer.
        /// </summary>
        List<string> welfareTags { get; }
        /// <summary>
        /// Defines the set of tags that describe a salary incident.
        /// </summary>
        List<string> salaryTags { get; }
        /// <summary>
        /// Defines the set of tags that describe a rent incident.
        /// </summary>
        List<string> rentTags { get; }
        /// <summary>
        /// Defines the set of tags that describe an infrastructure costs incident.
        /// </summary>
        List<string> infrastructureTags { get; }
        /// <summary>
        /// Defines the set of tags that stand in for the food incident or talent.
        /// </summary>
        List<string> foodTags { get; }
        /// <summary>
        /// Holds the level data (<see cref="LevelData"/>), a set of game resources.
        /// </summary>
        /// <value>The new level data.</value>
        LevelData levelData { get; set; }
        /// <summary>
        /// The authoritative player is the <see cref="Player"/> that controls the game.
        /// This is not the same as the city player / mayor.
        /// </summary>
        Player authoritativePlayer { get; set; }
        /// <summary>
        /// Holds references to all in-game <see cref="Player"/> instances.
        /// </summary>
        /// <value>All players.</value>
        List<Player> allPlayers { get; set; }
        /// <summary>
        /// Adds a new player to the level.
        /// </summary>
        void AddPlayer(Player player);
        /// <summary>
        /// Holds references to all <see cref="Building"/>s in the scene.
        /// </summary>
        List<Building> Buildings { get; set; }
        /// <summary>
        /// Adds a new building to the level.
        /// </summary>
        void AddBuilding(Building building);
        /// <summary>
        /// Calculates and returns the total state of all buildings.
        /// </summary>
        
        int MinimumUpkeep { get; set; }
        float CityState { get; }
        /// <summary>
        /// Calculates and returns the total luminance of all buildings.
        /// </summary>
        float TotalLuminance { get; }
        /// <summary>
        /// Returns <c>true</c> if the complementary currency has been introduced.
        /// </summary>
        bool PolymoneyIntroduced { get; }
        /// <summary>
        /// Shall be invoked, when the authoritative player is reassigned.
        /// </summary>
        /// <value>A <see cref="UnityEvent"/>.</value>
        UnityEvent onAuthoritativePlayerChanged { get; }
        /// <summary>
        /// Shall be invoked, when the level state changes.
        /// </summary>
        /// <value>A <see cref="UnityEvent"/>.</value>
        UnityEvent onLevelStateChanged { get; }
    }

    /// <summary>
    /// Describes the level model. It holds all level and game-progress-specific data.
    /// </summary>
    public class Level : Singleton<Level>, ILevel
    {
        public int RegularStartingMoney = 3000;
        public int MayorBaseStartingMoney = 1500;
        public float MayorStartingMoneyFactor = 1.0f;
        public float InfrastructureCostFactor = 0.333333f;
        public float LuminancePerPoint = 0.01f;
        public int PolymoneyPerFreeTime = 100;
        public List<CurrencyValue> MaximumDebt = new List<CurrencyValue> {
            new CurrencyValue(Currency.FIAT, 100),
            new CurrencyValue(Currency.Q, 0),
        };

        [SerializeField]
        private int _months = 1;
        [SerializeField]
        private int _maximumMonths = 12;
        [SerializeField]
        private List<string> _taxTags = new List<string> { "Taxes" };
        [SerializeField]
        private List<string> _welfareTags = new List<string> { "Welfare" };
        [SerializeField]
        private List<string> _salaryTags = new List<string> { "Salary" };
        [SerializeField]
        private List<string> _rentTags = new List<string> { "Rent" };
        [SerializeField]
        private List<string> _infrastructureTags = new List<string> { "Recurrent", "City", "Infrastructure" };
        [SerializeField]
        private List<string> _foodTags = new List<string> { "Food" };
        private LevelData _levelData = null;
        private Player _authoritativePlayer = null;
        private List<Player> _allPlayers = new List<Player>();
        private List<uint> _cricicalPlayers = new List<uint>();
        private List<uint> _gameOverPlayers = new List<uint>();
        private List<Building> _buildings = new List<Building>();
        private UnityEvent _onAuthoritativePlayerChanged = new UnityEvent();
        private UnityEvent _onLevelStateChanged = new UnityEvent();
        public PlayerAddedEvent onPlayerAdded = new PlayerAddedEvent();
        public UnityEvent onAllPlayersReady = new UnityEvent();
        public PlayerAddedEvent onPlayerRemoved = new PlayerAddedEvent();
        public bool _polyMoneyIntroduced = false;

        public int maximumMonths
        {
            get
            {
                return _maximumMonths;
            }
        }

        public int months
        {
            get
            {
                return _months;
            }

            set
            {
                if (value != _months)
                {
                    _months = value;
                    onLevelStateChanged.Invoke();
                }
            }
        }

        public List<string> taxTags
        {
            get
            {
                return this._taxTags;
            }
        }

        public List<string> welfareTags
        {
            get
            {
                return this._welfareTags;
            }
        }

        public List<string> salaryTags
        {
            get
            {
                return this._salaryTags;
            }
        }

        public List<string> rentTags
        {
            get
            {
                return this._rentTags;
            }
        }

        public List<string> infrastructureTags
        {
            get
            {
                return this._infrastructureTags;
            }
        }

        public List<string> foodTags
        {
            get
            {
                return this._foodTags;
            }
        }

        public LevelData levelData
        {
            get
            {
                return _levelData;
            }

            set
            {
                _levelData = value;
            }
        }

        public Player authoritativePlayer
        {
            get
            {
                return _authoritativePlayer;
            }

            set
            {
                if (_authoritativePlayer != value)
                {
                    _authoritativePlayer = value;
                    onAuthoritativePlayerChanged.Invoke();
                }
            }
        }

        public List<uint> criticalPlayers
        {
            get
            {
                return _cricicalPlayers;
            }

            set
            {
                _cricicalPlayers = value;
            }
        }

        public List<uint> gameOverPlayers
        {
            get
            {
                return _gameOverPlayers;
            }

            set
            {
                _gameOverPlayers = value;
            }
        }

        public List<Player> allPlayers
        {
            get
            {
                return _allPlayers;
            }

            set
            {
                _allPlayers = value;
            }
        }

        public void AddPlayer(Player player)
        {
            RootLogger.Debug(this, "Player '{0}' (netid: {1}) was registered", player.name, player.netId);
            RootAnalytics.AddPlayer(player.netId.Value);
            this._allPlayers.Add(player);
            this.onPlayerAdded.Invoke(player);
        }

        public void RemovePlayer(Player player)
        {
            RootLogger.Debug(this, "Player '{0}' (netid: {1}) was unregistered", player.name, player.netId);
            this._allPlayers.Remove(player);
            this.onPlayerRemoved.Invoke(player);
        }


        public List<Building> Buildings
        {
            get
            {
                return this._buildings;
            }

            set
            {
                this._buildings = value;
            }
        }

        public void AddBuilding(Building building)
        {
            RootLogger.Debug(this, "Building '{0}' (netid: {1}) was registered", building.name, building.netId);
            this._buildings.Add(building);
            this.handleLevelStateChange();
            building.OnLuminanceChanged.AddListener(this.handleLevelStateChange);
            building.OnBuildingStateChanged.AddListener(this.handleLevelStateChange);
        }

        public int MinimumUpkeep { get; set; } = 1;
        public float UpkeptBuildings
        {
            get
            {
                List<Building> bldgs = this._buildings.FindAll(e => e.MayBreak).ToList();
                return bldgs.Sum(e => Mathf.Clamp01(e.State));
            }
        }

        public float CityState
        {
            get
            {
                List<Building> bldgs = this._buildings.FindAll(e => e.MayBreak).ToList();
                return bldgs.Sum(e => Mathf.Clamp01(e.State)) / bldgs.Count;
            }
        }

        public float TotalLuminance
        {
            get
            {
                List<Building> bldgs = this._buildings.FindAll(e => e.DisplaysLuminance).ToList();
                return bldgs.Sum(e => e.Luminance) / bldgs.Count;
            }
        }

        public bool PolymoneyIntroduced
        {
            get
            {
                return this._polyMoneyIntroduced;
            }
            set
            {
                if (this._polyMoneyIntroduced != value)
                {
                    this._polyMoneyIntroduced = value;
                    this.onLevelStateChanged.Invoke();
                }
            }
        }

        public UnityEvent onAuthoritativePlayerChanged
        {
            get
            {
                return _onAuthoritativePlayerChanged;
            }
        }

        public UnityEvent onLevelStateChanged
        {
            get
            {
                return _onLevelStateChanged;
            }
        }

        private void handleLevelStateChange()
        {
            RootAnalytics.UpdateCityState(this.CityState);
            RootAnalytics.UpdateCityLuminance(this.TotalLuminance);
            this.onLevelStateChanged.Invoke();
        }

        void Start()
        {
            if (PersistentOptionSettings.instance != null)
            {
                this.MinimumUpkeep = PersistentOptionSettings.instance.MinimumUpkeep;
            }
        }
    }
}
