using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KoboldTools;
using KoboldTools.Logging;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace Polymoney {
    /// <summary>
    /// Strictly provides access to player data.
    /// As a model in the MVC sense, it should not encode behaviour.
    /// </summary>
    public class Player : NetworkBehaviour, IEquatable<Player> {
        // Local-only members, thus not synchronized over the network.
        [ContextMenuItem("Game Over", "ClientMakeGameOver")]
        public MarketplaceSet _marketplaceDb = null;

        private Character _loadedCharacter = null;
        private Marketplace _watchedMarket = null;

        // Server-determined members, thus only readable on the client.
        private uint _suspensionLock = 0;
        private bool _mayor = false;
        private Person _person = null;
        private Home _home = null;
        private Job _job = null;
        private List<Talent> _talents = new List<Talent>();
        private List<Incident> _incidents = new List<Incident>();
        private Pocket _pocket = new Pocket();
        private Guid _ownMarketplace;
        private int _points;
        private bool _gameOver = false;
        private int _enjoyment = 0;
        private float _foodHealthStatus = 0.0f;
        private int _goodFoodNumber = 0;
        private int _badFoodNumber = 0;

        // Client-determined members.
        private bool _runsForMayor = false;
        private bool _turnFinished = false;
        private Vector3 _steeringTarget = Vector3.zero;
        private float _luminance = 0.0f;

        // Events.
        private UnityEvent _playerStateChanged = new UnityEvent();
        private UnityEvent _changedWatchingMarketplace = new UnityEvent();
        private UnityEvent _onTurnCompleted = new UnityEvent();
        private UnityEvent _onWaitingForTurnCompletion = new UnityEvent();
        private UnityEvent _characterChanged = new UnityEvent();
        private UnityEvent _onPlayerGenerated = new UnityEvent();
        private UnityEvent _onGameOver = new UnityEvent();
        private OfferApplyEvent _onOfferApplied = new OfferApplyEvent();

        /// <summary>
        /// Player initialization waits for the <see cref="Level" /> to appear
        /// and sets the player authority accordingly.
        /// </summary>
        public IEnumerator Start() {
            // Wait for the level to appear.
            while (Level.instance == null) {
                yield return null;
            }

            // Add the player to the level
            Level.instance.AddPlayer(this);

            // Set player as authoritative player of the level if it is the local player
            if (this.isLocalPlayer) {
                //set authoritative player of the level instance
                RootLogger.Info(this, "The authoritative player's name is '{0}'", this.name);
                Level.instance.authoritativePlayer = this;
            }
        }

        public void OnDestroy() {
            Level.instance.RemovePlayer(this);
        }

        /// <summary>
        /// Holds a reference to the loaded <see cref="Character"/> for that
        /// <see cref="Player"/>. This field is not synchronized over the
        /// network.
        /// </summary>
        public Character LoadedCharacter {
            get {
                return this._loadedCharacter;
            }

            set {
                if (this._loadedCharacter != value) {
                    this._loadedCharacter = value;
                    this._characterChanged.Invoke();
                }
            }
        }

        /// <summary>
        /// The <see cref="IMarketplace"/> the player is watching at the
        /// moment. This field is not synchronized over the network.
        /// </summary>
        public Marketplace WatchedMarket {
            get {
                return this._watchedMarket;
            }

            set {
                if (this._watchedMarket != value) {
                    this._watchedMarket = value;
                    this._changedWatchingMarketplace.Invoke();
                }
            }
        }

        /// <summary>
        /// Allows the server to set a player's name.
        /// </summary>
        public void ServerSetName(string name) {
            this.RpcSetName(name);
        }

        /// <summary>
        /// If set to true, the <see cref="Player"/> is the city player or
        /// mayor. In each game, there may only be one such player. This field
        /// is determined by the server.
        /// </summary>
        public bool Mayor {
            get {
                return this._mayor;
            }
        }

        /// <summary>
        /// When called on the server, sets the mayor status for the client.
        /// Due to the fact that the server is also a client, this call will be
        /// issued on the server as well.
        /// </summary>
        /// <param name="mayorStatus"><c>true</c> if the player is mayor, <c>false<c/> otherwise.</param>
        public void ServerSetMayorStatus(bool mayorStatus) {
            if (this.isServer) {
                this._mayor = mayorStatus;
                this.RpcSetMayorStatus(mayorStatus);
            } else {
                RootLogger.Exception(this, "The method Player.ServerSetMayorStatus() may only be called on the server");
            }
        }

        /// <summary>
        /// The description of the player's person is set during character
        /// generation and is randomly assigned from a set of valid characters.
        /// This field is determined by the server.
        /// </summary>
        public Person Person {
            get {
                return this._person;
            }
        }

        /// <summary>
        /// When called on the server, sets the person for the client.
        /// Due to the fact that the server is also a client, this call will be
        /// issued on the server as well.
        /// </summary>
        /// <param name="personId">The unique id of the <see cref="Person" /> object.</param>
        public void ServerSetPerson(Person person) {
            if (this.isServer) {
                this._person = person;
                this.RpcSetPerson(JsonUtility.ToJson(person));
            } else {
                RootLogger.Exception(this, "The method Player.ServerSetPerson() may only be called on the server");
            }
        }

        /// <summary>
        /// If set to true, the <see cref="Player"/> is game over and not part of the game anymore.
        /// </summary>
        public bool GameOver {
            get {
                return this._gameOver;
            }
        }

        public void ClientMakeGameOver() {
            this.ClientSetGameOver(true);
        }

        public void ClientSetGameOver(bool gameOver) {
            this.CmdSetGameOver(gameOver);
        }

        /// <summary>
        /// When called on the server, sets the game over status for the client.
        /// Due to the fact that the server is also a client, this call will be
        /// issued on the server as well.
        /// </summary>
        /// <param name="gameOver">The game over state.</param>
        public void ServerSetGameOver(bool gameOver) {
            if (this.isServer) {
                this._gameOver = gameOver;
                this.RpcSetGameOver(gameOver);
            } else {
                RootLogger.Exception(this, "The method Player.ServerSetGameOver() may only be called on the server");
            }
        }

        /// <summary>
        /// Indicates whether a player has enjoyed the game (scale can be defined freely in the UI).
        /// </summary>
        public int Enjoyment {
            get {
                return this._enjoyment;
            }
        }

        /// <summary>
        /// Allows clients to set their game enjoyment value.
        /// </summary>
        public void ClientSetEnjoyment(int value) {
            this.CmdSetEnjoyment(value);
        }

        /// <summary>
        /// Indicates the food health status.
        /// </summary>
        public float FoodHealthStatus {
            get {
                return this._foodHealthStatus;
            }
        }

        public void ServerAddGoodFood() {
            this._goodFoodNumber += 1;
            this.ServerUpdateFoodHealthStatus(this._goodFoodNumber, this._badFoodNumber);
        }

        public void ServerAddBadFood() {
            this._badFoodNumber += 1;
            this.ServerUpdateFoodHealthStatus(this._goodFoodNumber, this._badFoodNumber);
        }

        public void ServerUpdateFoodHealthStatus(int goodFood, int badFood) {
            this.RpcUpdateFoodHealthStatus((float) goodFood - badFood);
        }

        /// <summary>
        /// The player's home is set during character generation and is
        /// randomly chosen from a set of homes defined by the game rules. The
        /// home may change over the course of the game. This field is
        /// determined by the server.
        /// </summary>
        public Home Home {
            get {
                return this._home;
            }
        }

        /// <summary>
        /// When called on the server, sets the home for the client.
        /// Due to the fact that the server is also a client, this call will be
        /// issued on the server as well.
        /// </summary>
        /// <param name="homeId">The unique id of the <see cref="Home" /> object.</param>
        public void ServerSetHome(Home home) {
            if (this.isServer) {
                this._home = home;
                this.RpcSetHome(JsonUtility.ToJson(home));
            } else {
                RootLogger.Exception(this, "The method Player.ServerSetHome() may only be called on the server");
            }
        }

        /// <summary>
        /// The player's job is set during character generation and is randomly
        /// chosen from a set of available jobs defined by the game rules. The
        /// job may change over the course of the game. This field is
        /// determined by the server.
        /// </summary>
        public Job Job {
            get {
                return this._job;
            }
        }

        /// <summary>
        /// When called on the server, sets the job for the client.
        /// Due to the fact that the server is also a client, this call will be
        /// issued on the server as well.
        /// </summary>
        /// <param name="homeId">The unique id of the <see cref="Job" /> object.</param>
        /// <param name="unemployed"><c>true</c> if the lookup should be performed in the list of unemployed jobs.</param>
        public void ServerSetJob(Job job) {
            if (this.isServer) {
                this._job = job;
                this.RpcSetJob(JsonUtility.ToJson(job));
            } else {
                RootLogger.Exception(this, "The method Player.ServerSetJob() may only be called on the server");
            }
        }

        /// <summary>
        /// The player's talents are chosen randomly during character
        /// generation from predefined talents. They can also change over the
        /// course of the game. This field is determined by the server.
        /// </summary>
        public List<Talent> Talents {
            get {
                return this._talents;
            }
        }

        /// <summary>
        /// When called on the server, sets the talents for the client.
        /// Due to the fact that the server is also a client, this call will be
        /// issued on the server as well.
        /// </summary>
        /// <param name="talentIds">The unique ids of the <see cref="Talent" /> objects.</param>
        public void ServerSetTalents(List<Talent> talents) {
            if (this.isServer) {
                this._talents = talents;
                string[] jsonData = talents.Select(e => JsonUtility.ToJson(e)).ToArray();
                this.RpcSetTalents(jsonData);
            } else {
                RootLogger.Exception(this, "The method Player.ServerSetTalents() may only be called on the server");
            }
        }

        /// <summary>
        /// When called on the server, adds a new talent to the client.
        /// Due to the fact that the server is also a client, this call will be
        /// issued on the server as well.
        /// </summary>
        /// <param name="talentId">The unique id of the <see cref="Talent" /> object.</param>
        public void ServerAddTalent(Talent talent) {
            if (this.isServer) {
                this.RpcAddTalent(JsonUtility.ToJson(talent));
            } else {
                RootLogger.Exception(this, "The method Player.ServerAddTalent() may only be called on the server");
            }
        }

        /// <summary>
        /// The player's incident inventory reflects the set of things that
        /// will affect the player either immediately or at the end of the
        /// month. This field is determined by the server.
        /// </summary>
        public List<Incident> Incidents {
            get {
                return this._incidents;
            }
        }

        public void ClientAddIncident(Incident incident) {
            string jsonData = JsonUtility.ToJson(incident);
            if (incident.Immediate) {
                this.CmdApplyIncident(jsonData, false);
            } else {
                this.CmdAddIncident(jsonData);
            }
        }

        public void ClientUpdateIncident(Incident incident) {
            string jsonData = JsonUtility.ToJson(incident);
            this.CmdUpdateIncident(jsonData);
        }

        public void ClientIgnoreIncident(Incident incident) {
            string jsonData = JsonUtility.ToJson(incident);
            this.CmdIgnoreIncident(jsonData);
        }

        public void ClientResolveIncident(Incident incident) {
            string jsonData = JsonUtility.ToJson(incident);
            this.CmdResolveIncident(jsonData);
        }

        public void ClientApplyIncident(Incident incident) {
            string jsonData = JsonUtility.ToJson(incident);
            this.CmdApplyIncident(jsonData, false);
        }

        public void ClientRemoveIncident(Incident incident, bool apply) {
            string jsonData = JsonUtility.ToJson(incident);
            this.CmdRemoveIncident(jsonData, apply);
        }

        public void ClientApplyIncident(Incident incident, bool remove) {
            string jsonData = JsonUtility.ToJson(incident);
            this.CmdApplyIncident(jsonData, remove);
        }

        public void ServerIgnoreIncident(Incident incident) {
            if (this.isServer) {
                if (incident.State == IncidentState.UNTOUCHED) {
                    List<Incident> currentIncidents = new List<Incident>(this._incidents);
                    Pocket currentPocket = new Pocket(this._pocket);

                    // Apply the cost of ignoring the incident.
                    incident.IgnoranceCost.applyCost(currentPocket);

                    // Mark the incident as ignored.
                    int idx = currentIncidents.FindIndex(e => e.Equals(incident));
                    if (idx >= 0) {
                        currentIncidents[idx].State = IncidentState.IGNORED;
                    } else {
                        RootLogger.Warning(this, "Rpc: The incident {0} was not found on the player {1}.", incident, this.name);
                    }

                    // Broadcast the new set of incidents over the network.
                    this.RpcSetIncidents(currentIncidents.Select(e => JsonUtility.ToJson(e)).ToArray());

                    // Broadcast the state of the pocket over the network.
                    this.RpcSetPocket(JsonUtility.ToJson(currentPocket));
                } else {
                    RootLogger.Warning(this, "Rpc: The incident {0} was already applied, resolved or ignored.", incident);
                }
            } else {
                RootLogger.Exception(this, "The method Player.ServerIgnoreIncident() may only be called on the server.");
            }
        }

        public void ServerResolveIncident(Incident incident) {
            if (this.isServer) {
                if (incident.State == IncidentState.UNTOUCHED) {
                    List<Incident> currentIncidents = new List<Incident>(this._incidents);
                    int idx = currentIncidents.FindIndex(e => e.Equals(incident));
                    if (idx >= 0) {
                        currentIncidents[idx].State = IncidentState.RESOLVED;
                        if (currentIncidents[idx].EquivalentTags(Level.instance.foodTags)) {
                            this.ServerAddGoodFood();
                        }
                    } else {
                        RootLogger.Warning(this, "Rpc: The incident {0} was not found on the player {1}.", incident, this.name);
                    }

                    // Broadcast the new set of incidents over the network.
                    this.RpcSetIncidents(currentIncidents.Select(e => JsonUtility.ToJson(e)).ToArray());
                } else {
                    RootLogger.Warning(this, "Rpc: The incident {0} was already applied, resolved or ignored.", incident);
                }
            } else {
                RootLogger.Exception(this, "The method Player.ServerResolveIncident() may only be called on the server.");
            }
        }

        public void ServerApplyIncident(Incident incident, bool remove) {
            if (this.isServer) {
                if (incident.State == IncidentState.UNTOUCHED) {
                    List<Talent> currentTalents = new List<Talent>(this._talents);
                    List<Incident> currentIncidents = new List<Incident>(this._incidents);
                    Pocket currentPocket = new Pocket(this._pocket);

                    // Determine, which incidents the specified incident will remove.
                    HashSet<int> toResolve = new HashSet<int>();
                    toResolve.UnionWith(incident.ApplicationBenefit.getRemovableIncidents(currentIncidents));

                    // Mark those incidents as resolved.
                    foreach (int i in toResolve.OrderByDescending(q => q)) {
                        currentIncidents[i].State = IncidentState.RESOLVED;

                        if (currentIncidents[i].EquivalentTags(Level.instance.foodTags)) {
                            this.ServerAddGoodFood();
                        }
                    }

                    // Apply both benefit and cast of the specified incident.
                    incident.ApplicationBenefit.applyBenefit(currentTalents, currentIncidents, currentPocket);
                    incident.ApplicationCost.applyCost(currentPocket);

                    // If the incident defines an offer to be added to the player's own marketplace, add it.
                    if (!String.IsNullOrEmpty(incident.AddSerializedOffer)) {
                        this.ServerCreateOffer(this.OwnMarketplace.guid.ToString(), incident.AddSerializedOffer);
                    }

                    // Try to find the incident in the player's list of incidents.
                    int idx = currentIncidents.FindIndex(e => e.Equals(incident));
                    if (idx >= 0) {
                        // Mark the incident as applied.
                        currentIncidents[idx].State = IncidentState.APPLIED;

                        if (currentIncidents[idx].EquivalentTags(Level.instance.foodTags)) {
                            this.ServerAddBadFood();
                        }

                        // Remove the incident if it should be.
                        if (remove) {
                            currentIncidents.RemoveAt(idx);
                        }
                    } else {
                        RootLogger.Warning(this, "Rpc: The incident {0} was not found on the player {1}.", incident, this.name);
                    }

                    // Broadcast the new set of talents over the network.
                    this.RpcSetTalents(currentTalents.Select(e => JsonUtility.ToJson(e)).ToArray());

                    // Broadcast the new set of incidents over the network.
                    this.RpcSetIncidents(currentIncidents.Select(e => JsonUtility.ToJson(e)).ToArray());

                    // Broadcast the state of the pocket over the network.
                    this.RpcSetPocket(JsonUtility.ToJson(currentPocket));
                } else {
                    RootLogger.Warning(this, "Rpc: The incident {0} was already applied or resolved.", incident);
                }
            } else {
                RootLogger.Exception(this, "The method Player.ServerApplyIncident() may only be called on the server.");
            }
        }

        /// <summary>
        /// First, determines which incidents will remove other incidents. Subsequently performs
        /// the removal of all incidents-to-be-removed. Then, all remainaing incidents are applied
        /// to the player, and all non-recurrent incidents are removed from the inventory.
        /// </summary>
        public void ServerApplyAllIncidents() {
            if (this.isServer) {
                List<Talent> currentTalents = new List<Talent>(this._talents);
                List<Incident> currentIncidents = new List<Incident>(this._incidents);
                Pocket currentPocket = new Pocket(this._pocket);

                // Determine, which incidents each incident will remove.
                HashSet<int> toResolve = new HashSet<int>();
                foreach (Incident incident in currentIncidents) {
                    if (incident.State == IncidentState.UNTOUCHED) {
                        toResolve.UnionWith(incident.ApplicationBenefit.getRemovableIncidents(currentIncidents));
                    }
                }
                // Remove those incidents.
                foreach (int idx in toResolve.OrderByDescending(q => q)) {
                    currentIncidents[idx].State = IncidentState.RESOLVED;
                    if (currentIncidents[idx].EquivalentTags(Level.instance.foodTags)) {
                        this.ServerAddGoodFood();
                    }
                }
                // Apply the rest of the incidents.
                foreach (Incident incident in currentIncidents) {
                    if (incident.State == IncidentState.UNTOUCHED) {
                        if (incident.Ignorable) {
                            incident.IgnoranceCost.applyCost(currentPocket);
                            incident.State = IncidentState.IGNORED;
                        } else {
                            incident.ApplicationBenefit.applyBenefit(currentTalents, currentIncidents, currentPocket);
                            incident.ApplicationCost.applyCost(currentPocket);
                            incident.State = IncidentState.APPLIED;

                            if (incident.EquivalentTags(Level.instance.foodTags)) {
                                this.ServerAddBadFood();
                            }

                            // If the incident defines an offer to be added to the player's own marketplace, add it.
                            if (!String.IsNullOrEmpty(incident.AddSerializedOffer)) {
                                this.ServerCreateOffer(this.OwnMarketplace.guid.ToString(), incident.AddSerializedOffer);
                            }
                        }
                    }
                }

                // Broadcast the new set of talents over the network.
                this.RpcSetTalents(currentTalents.Select(e => JsonUtility.ToJson(e)).ToArray());

                // Broadcast the state of the pocket over the network.
                this.RpcSetPocket(JsonUtility.ToJson(currentPocket));
            } else {
                RootLogger.Exception(this, "The method Player.ServerApplyAllIncidents() may only be called on the server.");
            }
        }

        public void ServerAddIncident(Incident incident) {
            if (this.isServer) {
                string jsonData = JsonUtility.ToJson(incident);
                this.RpcAddIncident(jsonData);
            } else {
                RootLogger.Exception(this, "The method Player.ServerAddIncident() may only be called on the server");
            }
        }

        public void ServerSetIncidents(List<Incident> incidents) {
            string[] jsonData = incidents.Select(e => JsonUtility.ToJson(e)).ToArray();
            this.RpcSetIncidents(jsonData);
        }

        public void ServerClearIncidents() {
            this.RpcClearIncidents();
        }

        /// <summary>
        /// The player's pocket tracks their money, time and fairy dust
        /// resources. This field is determined by the server.
        /// </summary>
        public Pocket Pocket {
            get {
                return this._pocket;
            }
        }

        public void ClientGiveFairydust(NetworkInstanceId recipient, NetworkInstanceId building, CurrencyValue cost, int benefit, float playerLuminanceIncrement, float buildingLuminanceIncrement) {
            this.CmdGiveFairydust(recipient, building, cost.GetCurrency(), cost.value, benefit, playerLuminanceIncrement, buildingLuminanceIncrement);
        }

        public void ServerSetPocket(Pocket pocket) {
            string jsonData = JsonUtility.ToJson(pocket);
            this.RpcSetPocket(jsonData);
        }

        public void ServerSetPocketTime(int time) {
            this.RpcSetPocketTime(time);
        }

        /// <summary>
        /// A list of marketplaces for which the <see cref="Player"/> is
        /// owner. They are allowed to manage <see cref="Offer"/>s in each of
        /// them. This field is determined by the server.
        /// </summary>
        public Marketplace OwnMarketplace {
            get {
                return this._marketplaceDb.getByGuid(this._ownMarketplace.ToString());
            }
        }

        public List<Marketplace> OwnedMarketplaces {
            get {
                return this._marketplaceDb.marketplaces.FindAll(e => !object.ReferenceEquals(this.OwnMarketplace, e) && object.ReferenceEquals(this, e.seller));
            }
        }

        public void ServerSetOwnMarketplace(string guid) {
            this.RpcSetOwnMarketplace(guid);
        }

        public int Points {
            get {
                return this._points;
            }
        }

        public void ServerSetPoints(int points) {
            this.RpcSetPoints(points);
        }

        /// <summary>
        /// On the server, this field shall tell consumers whether the client
        /// wants to run for mayor. Raises an exception if accessed on the
        /// client.
        /// </summary>
        public bool RunsForMayor {
            get {
                if (this.isServer) {
                    return this._runsForMayor;
                } else {
                    RootLogger.Exception(this, "The parameter 'Player.runsForMayor' can only be read on the server.");
                    return false;
                }
            }
            set {
                this._runsForMayor = value;
            }
        }

        /// <summary>
        /// Set to true if the player has finished their turn.
        /// </summary>
        public bool TurnFinished {
            get {
                return this._turnFinished;
            }
        }

        /// <summary>
        /// Clients may use this command to tell the server that they have
        /// completed their turn.
        /// </summary>
        public void ClientEndTurn() {
            AudioController.Play("well_done");
            this.CmdEndTurn();
        }

        /// <summary>
        /// On the server, resets the internal end-turn value to <c>false</c>.
        /// Raises an exception if called on the client.
        /// </summary>
        public void ServerResetEndTurn() {
            if (this.isServer) {
                this.RpcResetEndTurn();
            } else {
                RootLogger.Exception(this, "The method Player.ServerResetEndTurn() may only be called on the server.");
            }
        }

        public Vector3 SteeringTarget {
            get {
                if (this.isServer) {
                    return this._steeringTarget;
                } else {
                    RootLogger.Exception(this, "The parameter Player.steeringTarget may only be accessed on the server.");
                    return Vector3.zero;
                }
            }
        }

        public void ClientSetSteeringTarget(Vector3 target) {
            this.CmdSetSteeringTarget(target);
        }

        public void ServerSetSteeringTarget(Vector3 target) {
            this.RpcSetSteeringTarget(target);
        }

        public float Luminance {
            get {
                return this._luminance;
            }
        }

        public void ClientSetLuminance(float value) {
            this.CmdSetLuminance(value);
        }

        public void ServerSetLuminance(float value) {
            this.RpcSetLuminance(value);
        }

        public void ClientSetBuildingLuminance(NetworkInstanceId buildingId, float value) {
            this.CmdSetBuildingLuminance(buildingId, value);
        }

        public void ServerSetBuildingLuminance(NetworkInstanceId buildingId, float value) {
            GameObject obj = NetworkServer.FindLocalObject(buildingId);
            if (obj != null) {
                Building building = obj.GetComponent<Building>();
                if (building != null) {
                    building.Luminance = value;
                } else {
                    RootLogger.Exception(this, "The supplied network identity does not have a Building component.");
                }
            } else {
                RootLogger.Exception(this, "The supplied network identity is not valid.");
            }
        }

        public void ClientCreateOffer(string marketGuid, Offer newOffer) {
            this.CmdCreateOffer(marketGuid, JsonUtility.ToJson(newOffer));
        }

        public void ClientApplyOffer(string offerGuid) {
            this.CmdApplyOffer(offerGuid);
        }

        public void ClientApplyOffer(Offer offer, Player buyer, Player seller) {
            this.CmdApplyOfferDirect(JsonUtility.ToJson(offer), buyer.netId, seller.netId);
        }

        public void ClientRemoveOffer(string offerGuid) {
            this.CmdRemoveOffer(offerGuid);
        }

        public void ServerCreateOffer(string marketGuid, string offerData) {
            Guid offerGuid = Guid.NewGuid();
            Offer deserializedOffer = ScriptableObject.CreateInstance<Offer>();
            JsonUtility.FromJsonOverwrite(offerData, deserializedOffer);

            deserializedOffer.creationCost.applyCost(this._pocket);
            deserializedOffer.creationBenefit.applyBenefit(this._talents, this._incidents, this._pocket);

            this._marketplaceDb.syncProvider.AddOffer(marketGuid, offerData, offerGuid.ToString());
            this.RpcSetPocket(JsonUtility.ToJson(this._pocket));
            Destroy(deserializedOffer);
        }

        public void ServerApplyOffer(Offer offer, Player buyer, Player seller) {
            if (object.ReferenceEquals(buyer, seller)) {
                RootLogger.Exception(this, "Players should not buy their own offers (buyer: {0}, seller: {1})", buyer, seller);
            }

            RootLogger.Info(this, "Server: Applying the offer {0}, buyer: {1}, seller: {2}", offer, buyer, seller);

            // Determine, whether the offer that's being applied trades in complementary currency (anything not FIAT).
            int revenue = buyer.CalculateRevenue(offer, Currency.Q);

            // Apply the player points based on revenue.
            if (revenue > 0) {
                buyer.ServerSetPoints(buyer.Points + revenue);

                // Increase every building's luminance by the amount determined in the threshold list.
                Building building = Level.instance.Buildings.FirstOrDefault(e => e.DisplaysLuminance && e.Marketplace != null && e.Marketplace.offers.Contains(offer));
                if (building != null) {
                    building.Luminance += Level.instance.LuminancePerPoint * revenue;
                }
            }

            // Apply the buyer portion
            offer.buyingCost.applyCost(buyer.Pocket);
            offer.buyingBenefit.applyBenefit(buyer.Talents, buyer.Incidents, buyer.Pocket);

            if (seller != null) {
                // Apply the seller points based on revenue.
                if (revenue > 0) {
                    seller.ServerSetPoints(seller.Points + revenue);
                }

                // Apply the seller portion.
                offer.sellingCost.applyCost(seller.Pocket);
                offer.sellingBenefit.applyBenefit(seller.Talents, seller.Incidents, seller.Pocket);

                // Broadcast the new set of talents over the network.
                seller.ServerSetTalents(seller.Talents);

                // Broadcast the new set of incidents over the network.
                seller.ServerSetIncidents(seller.Incidents);

                // Broadcast the state of the pocket over the network.
                seller.ServerSetPocket(seller.Pocket);
            } else {
                RootLogger.Warning(this, "Server: The seller is not known");
            }

            // Broadcast the new set of talents over the network.
            buyer.ServerSetTalents(buyer.Talents);

            // Broadcast the new set of incidents over the network.
            buyer.ServerSetIncidents(buyer.Incidents);

            // Broadcast the state of the pocket over the network.
            buyer.ServerSetPocket(buyer.Pocket);

            // Invoke the offer applied event and remove the offer unless it is persistent.
            this.ServerNotifyOfferApplication(offer, buyer, seller);

            // Remove the offer if it exists somewhere on the market.
            if (!offer.persistent) {
                foreach (Marketplace market in this._marketplaceDb.marketplaces) {
                    foreach (Offer tmpOffer in market.offers) {
                        if (tmpOffer.guid.Equals(offer.guid)) {
                            this._marketplaceDb.syncProvider.RemoveOffer(market.guid.ToString(), offer.guid.ToString());
                            return;
                        }
                    }
                }
            }
        }

        public void ServerClearOffers(bool persistent) {
            if (this.isServer) {
                foreach (Marketplace market in this._marketplaceDb.marketplaces) {
                    if (object.ReferenceEquals(this, market.seller)) {
                        this._marketplaceDb.syncProvider.ClearOffers(market.guid.ToString(), persistent);
                    }
                }
            } else {
                RootLogger.Exception(this, "The method Player.ServerClearOffers() may only be called on the server.");
            }
        }

        public void ServerNotifyOfferApplication(Offer offer, Player buyer, Player seller) {
            if (this.isServer) {
                offer.offerApplied.Invoke(offer, buyer);
                string jsonData = JsonUtility.ToJson(offer);
                this.RpcNotifyOfferApplication(jsonData, buyer.netId, seller.netId);
            } else {
                RootLogger.Exception(this, "The method Player.ServerNotifyOfferApplication() may only be called on the server.");
            }
        }

        public void ServerFinalizePlayerGeneration() {
            if (this.isServer) {
                this.RpcFinalizePlayerGeneration();
            } else {
                RootLogger.Exception(this, "The method Player.ServerFinalizePlayerGeneration() may only be called on the server.");
            }
        }

        public UnityEvent OnGameOver {
            get {
                return this._onGameOver;
            }
        }

        public UnityEvent PlayerStateChanged {
            get {
                return this._playerStateChanged;
            }
        }

        public UnityEvent ChangedWatchingMarketplace {
            get {
                return this._changedWatchingMarketplace;
            }
        }

        public UnityEvent OnTurnCompleted {
            get {
                return this._onTurnCompleted;
            }
        }

        public UnityEvent OnWaitingForTurnCompletion {
            get {
                return this._onWaitingForTurnCompletion;
            }
        }

        public UnityEvent CharacterChanged {
            get {
                return this._characterChanged;
            }
        }

        public UnityEvent OnPlayerGenerated {
            get {
                return this._onPlayerGenerated;
            }
        }

        public OfferApplyEvent OnOfferApplied {
            get {
                return this._onOfferApplied;
            }
        }

        public bool Equals(Player other) {
            return this.netId == other.netId;
        }

        private int CalculateRevenue(Offer offer, Currency currency) {
            Cost bC = offer.buyingCost;
            Benefit bB = offer.buyingBenefit;
            Cost sC = offer.sellingCost;
            Benefit sB = offer.sellingBenefit;

            int bCValue = 0;
            bC.TryGetExpenses(currency, out bCValue);

            int bBValue = 0;
            bB.TryGetIncome(currency, out bBValue);

            int sCValue = 0;
            sC.TryGetExpenses(currency, out sCValue);

            int sBValue = 0;
            sB.TryGetIncome(currency, out sBValue);

            return Math.Abs(bBValue - bCValue);
        }

        [ClientRpc]
        private void RpcSetName(string name) {
            RootLogger.Debug(this, "Rpc: The player (netid: {0}) is now called '{1}'.", this.netId, name);
            this.name = name;
            this._playerStateChanged.Invoke();
        }

        [ClientRpc]
        private void RpcSetGameOver(bool gameOver) {
            RootLogger.Debug(this, "Rpc: The player (netid: {0}) is now gameover: {1}", this.netId, gameOver);
            this._gameOver = gameOver;
            this._onGameOver.Invoke();
        }

        [ClientRpc]
        private void RpcSetEnjoyment(int value) {
            RootLogger.Debug(this, "Rpc: The player enjoys the game this much: {0}", value);
            this._enjoyment = value;
            RootAnalytics.SetPlayerEnjoyment(this.netId.Value, value);
        }

        [ClientRpc]
        private void RpcUpdateFoodHealthStatus(float status) {
            this._foodHealthStatus = status;
            RootAnalytics.SetFoodHealthStatus(this.netId.Value, status);
        }

        [ClientRpc]
        private void RpcSetMayorStatus(bool mayorStatus) {
            RootLogger.Debug(this, "Rpc: The player (netid: {0}) is now mayor: {1}", this.netId, mayorStatus);
            this._mayor = mayorStatus;
            this._playerStateChanged.Invoke();
        }

        [ClientRpc]
        private void RpcSetPerson(string jsonData) {
            Person person = JsonUtility.FromJson<Person>(jsonData);
            if (person != null) {
                RootLogger.Debug(this, "Rpc: The player '{0}' (netid: {1}) now has the person {2}.", this.name, this.netId, person);
                this._person = person;
                this._playerStateChanged.Invoke();
            } else {
                RootLogger.Exception(this, "Unable to deserialize the following data into a Person: '{0}'", jsonData);
            }
        }

        [ClientRpc]
        private void RpcSetHome(string jsonData) {
            Home home = JsonUtility.FromJson<Home>(jsonData);
            if (home != null) {
                RootLogger.Debug(this, "Rpc: The player '{0}' (netid: {1}) now has the home {2}.", this.name, this.netId, home);
                this._home = home;
                this._playerStateChanged.Invoke();
            } else {
                RootLogger.Exception(this, "Unable to deserialize the following data into a Home: '{0}'", jsonData);
            }
        }

        [ClientRpc]
        private void RpcSetJob(string jsonData) {
            Job job = JsonUtility.FromJson<Job>(jsonData);
            if (job != null) {
                RootLogger.Debug(this, "Rpc: The player '{0}' (netid: {1}) now has the job {2}.", this.name, this.netId, job);
                this._job = job;
                this._playerStateChanged.Invoke();
            } else {
                RootLogger.Exception(this, "Unable to deserialize the following data into a Job: '{0}'", jsonData);
            }
        }

        [ClientRpc]
        private void RpcSetTalents(string[] jsonData) {
            List<Talent> talents = jsonData.Select(e => JsonUtility.FromJson<Talent>(e)).ToList();
            if (talents.Count(e => e == null) == 0) {
                this._talents = talents;
                this._playerStateChanged.Invoke();
            } else {
                RootLogger.Exception(this, "Unable to deserialize the following data into Talents: '[{0}]'", String.Join(", ", jsonData));
            }
        }

        [ClientRpc]
        private void RpcAddTalent(string jsonData) {
            Talent talent = JsonUtility.FromJson<Talent>(jsonData);
            if (talent != null) {
                if (this._talents.FindIndex(e => e.Equals(talent)) < 0) {
                    RootLogger.Debug(this, "Rpc: Adding a talent: {0} (json: {1})", talent, jsonData);
                    this._talents.Add(talent);
                    RootAnalytics.SetTalentNumber(this.netId.Value, this._talents.Count);
                    this._playerStateChanged.Invoke();
                } else {
                    RootLogger.Warning(this, "The player already has the specified talent '{0}'. Not doing anything.", talent);
                }
            } else {
                RootLogger.Exception(this, "Unable to deserialize the following data into a Talent: '{0}'", jsonData);
            }
        }

        [ClientRpc]
        private void RpcAddIncident(string jsonData) {
            Incident incident = JsonUtility.FromJson<Incident>(jsonData);
            if (incident != null) {
                RootLogger.Debug(this, "Rpc: Adding an incident: {0} (json: {1})", incident, jsonData);
                this._incidents.Add(incident);
                this._playerStateChanged.Invoke();
            } else {
                RootLogger.Exception(this, "Unable to deserialize the following data into an Incident: '{0}'", jsonData);
            }
        }

        [ClientRpc]
        private void RpcUpdateIncident(string jsonData) {
            Incident incident = JsonUtility.FromJson<Incident>(jsonData);
            if (incident != null) {
                int idx = this._incidents.FindIndex(e => e.Equals(incident));
                if (idx >= 0) {
                    if (!this._incidents[idx].Identical(incident)) {
                        RootLogger.Debug(this, "Rpc: Updating an incident: {0} (json: {1})", incident, jsonData);
                        this._incidents[idx] = incident;
                        this._playerStateChanged.Invoke();
                    }
                } else {
                    RootLogger.Exception(this, "The incident {0} was not found on the player {1}.", incident, this.name);
                }
            } else {
                RootLogger.Exception(this, "Unable to deserialize the following data into an Incident: '{0}'", jsonData);
            }
        }

        [ClientRpc]
        private void RpcRemoveIncident(string jsonData) {
            Incident incident = JsonUtility.FromJson<Incident>(jsonData);
            if (incident != null) {
                int idx = this._incidents.FindIndex(e => e.Equals(incident));
                if (idx >= 0) {
                    RootLogger.Debug(this, "Rpc: Removing an incident: {0} (json: {1})", incident, jsonData);
                    this._incidents.RemoveAt(idx);
                } else {
                    RootLogger.Exception(this, "The incident {0} was not found on the player {1}.", incident, this.name);
                }
                this._playerStateChanged.Invoke();
            } else {
                RootLogger.Exception(this, "Unable to deserialize the following data into an Incident: '{0}'", jsonData);
            }
        }

        [ClientRpc]
        private void RpcSetIncidents(string[] jsonData) {
            List<Incident> incidents = jsonData.Select(e => JsonUtility.FromJson<Incident>(e)).ToList();
            if (incidents.Count(e => e == null) == 0) {
                this._incidents = incidents;
                this._playerStateChanged.Invoke();
            } else {
                RootLogger.Exception(this, "Unable to deserialize the following data into Incidents: '[{0}]'", String.Join(", ", jsonData));
            }
        }

        [ClientRpc]
        private void RpcClearIncidents() {
            this._incidents.Clear();
            this._playerStateChanged.Invoke();
        }

        [ClientRpc]
        private void RpcSetPocket(string jsonData) {
            Pocket pkt = JsonUtility.FromJson<Pocket>(jsonData);
            if (pkt != null) {
                int fiatBalance;
                pkt.TryGetBalance(Currency.FIAT, out fiatBalance);
                RootAnalytics.SetAccountBalance(this.netId.Value, Currency.FIAT, fiatBalance);
                if (this.Mayor) {
                    RootAnalytics.SetCityAccountBalance(fiatBalance);
                }

                int qBalance;
                pkt.TryGetBalance(Currency.Q, out qBalance);
                RootAnalytics.SetAccountBalance(this.netId.Value, Currency.Q, qBalance);

                this._pocket = pkt;
                this._playerStateChanged.Invoke();
            } else {
                RootLogger.Exception(this, "Unable to deserialize the following data into a Pocket: '{0}'", jsonData);
            }
        }

        [ClientRpc]
        private void RpcSetPocketTime(int time) {
            RootLogger.Debug(this, "Rpc: Player '{0}' (netid: {1}) now has {2} % of their time left.", this.name, this.netId, time);
            this._pocket.TimeAllowance = time;
            this._playerStateChanged.Invoke();
        }

        [ClientRpc]
        private void RpcSetOwnMarketplace(string guid) {
            RootLogger.Debug(this, "Rpc: The player {0} now owns the marketplace {1}.", this.name, guid);
            this._ownMarketplace = new Guid(guid);
        }

        [ClientRpc]
        private void RpcNotifyOfferApplication(string offerData, NetworkInstanceId buyerId, NetworkInstanceId sellerId) {
            Offer offer = ScriptableObject.CreateInstance<Offer>();
            JsonUtility.FromJsonOverwrite(offerData, offer);

            GameObject sellerObj = ClientScene.FindLocalObject(sellerId);
            if (sellerObj != null) {
                Player seller = sellerObj.GetComponent<Player>();
                if (seller != null) {
                    GameObject buyerObj = ClientScene.FindLocalObject(buyerId);
                    if (buyerObj != null) {
                        Player buyer = buyerObj.GetComponent<Player>();
                        if (buyer != null) {
                            seller.OnOfferApplied.Invoke(offer, buyer);
                            CurrencyValue buyingBalance = offer.BuyingBalance;
                            if (offer.EquivalentTags(Level.instance.welfareTags)) {
                                RootAnalytics.AddWelfarePayment(seller.netId.Value, offer.SellingBalance.value);
                            } else if (!offer.EquivalentTags(Level.instance.taxTags) && buyingBalance.value != 0) {
                                RootAnalytics.SetCirculationRate(buyingBalance.GetCurrency(), Math.Abs(buyingBalance.value), Level.instance.months);
                            }
                        } else {
                            RootLogger.Exception(this, "Rpc: The supplied network identity '{0}' (obj: {1}) has no Player component.", buyerId, buyerObj);
                        }
                    } else {
                        RootLogger.Exception(this, "Rpc: The supplied network identity '{0}' was not found on this client.", buyerId);
                    }
                } else {
                    RootLogger.Exception(this, "Rpc: The supplied network identity '{0}' (obj: {1}) has no Player component.", sellerId, sellerObj);
                }
            } else {
                RootLogger.Exception(this, "Rpc: The supplied network identity '{0}' was not found on this client.", sellerId);
            }
        }

        [ClientRpc]
        private void RpcEndTurn() {
            this._turnFinished = true;
            this._onWaitingForTurnCompletion.Invoke();
        }

        [ClientRpc]
        private void RpcResetEndTurn() {
            this._turnFinished = false;
            this._onTurnCompleted.Invoke();
        }

        [ClientRpc]
        private void RpcSetSteeringTarget(Vector3 target) {
            this._steeringTarget = target;
            this._loadedCharacter.steeringTarget = target;
        }

        [ClientRpc]
        private void RpcFinalizePlayerGeneration() {
            this._onPlayerGenerated.Invoke();
        }

        [ClientRpc]
        private void RpcSetLuminance(float value) {
            if (Math.Abs(this._luminance - value) > float.Epsilon) {
                if (value > 1.0f) {
                    this._luminance = 1.0f;
                } else if (value < 0.0f) {
                    this._luminance = 0.0f;
                } else {
                    this._luminance = value;
                }

                RootLogger.Debug(this, "Rpc: Setting the luminance of player '{0}' (netid: {1}) to {2}.", this.name, this.netId, this._luminance);
                this._loadedCharacter.Luminance = this._luminance;
            }
        }

        [ClientRpc]
        private void RpcSetPoints(int points) {
            RootLogger.Debug(this, "Rpc: Setting player {0}'s (netid: {1}) points to {2}.", this.name, this.netId, points);
            this._points = points;
        }

        [Command]
        private void CmdSetGameOver(bool gameOver) {
            RootLogger.Debug(this, "Cmd: The player {0} requests game over.", this.name);
            this.ServerSetGameOver(gameOver);
        }

        [Command]
        private void CmdSetEnjoyment(int value) {
            RootLogger.Debug(this, "Cmd: The player requests to set enjoyment.");
            this.RpcSetEnjoyment(value);
        }

        [Command]
        private void CmdAddIncident(string jsonData) {
            RootLogger.Debug(this, "Cmd: The player {0} requests to add an incident.", this.name);
            this.RpcAddIncident(jsonData);
        }

        [Command]
        private void CmdUpdateIncident(string jsonData) {
            RootLogger.Debug(this, "Cmd: The player {0} requests to update an incident.", this.name);
            this.RpcUpdateIncident(jsonData);
        }

        [Command]
        private void CmdIgnoreIncident(string jsonData) {
            Incident incident = JsonUtility.FromJson<Incident>(jsonData);
            if (incident != null) {
                RootLogger.Debug(this, "Cmd: The player {0} requests to ignore the incident {1}.", this.name, incident);
                this.ServerIgnoreIncident(incident);
            } else {
                RootLogger.Exception(this, "Unable to deserialize the following data into an Incident: '{0}'", jsonData);
            }
        }

        [Command]
        private void CmdResolveIncident(string jsonData) {
            Incident incident = JsonUtility.FromJson<Incident>(jsonData);
            if (incident != null) {
                RootLogger.Debug(this, "Cmd: The player {0} requests to resolve the incident {1}.", this.name, incident);
                this.ServerResolveIncident(incident);
            } else {
                RootLogger.Exception(this, "Unable to deserialize the following data into an Incident: '{0}'", jsonData);
            }
        }

        [Command]
        private void CmdRemoveIncident(string jsonData, bool apply) {
            Incident incident = JsonUtility.FromJson<Incident>(jsonData);
            if (incident != null) {
                RootLogger.Debug(this, "Cmd: The player {0} requests to remove the incident {1}.", this.name, incident);
                if (apply) {
                    RootLogger.Debug(this, "Cmd: Removing the incident via Player.ServerApplyIncident.");
                    this.ServerApplyIncident(incident, true);
                } else {
                    RootLogger.Debug(this, "Cmd: Removing the incident via Player.RpcRemoveIncident.");
                    this.RpcRemoveIncident(jsonData);
                }
            } else {
                RootLogger.Exception(this, "Unable to deserialize the following data into an Incident: '{0}'", jsonData);
            }
        }

        [Command]
        private void CmdApplyIncident(string jsonData, bool remove) {
            Incident incident = JsonUtility.FromJson<Incident>(jsonData);
            if (incident != null) {
                RootLogger.Debug(this, "Cmd: The player {0} requests to apply the incident {1}.", this.name, incident);
                this.ServerApplyIncident(incident, remove);
            } else {
                RootLogger.Exception(this, "Unable to deserialize the following data into an Incident: '{0}'", jsonData);
            }
        }

        [Command]
        private void CmdEndTurn() {
            this.RpcEndTurn();
        }

        [Command]
        private void CmdSetSteeringTarget(Vector3 target) {
            this._steeringTarget = target;
            this._loadedCharacter.steeringTarget = target;
            this.RpcSetSteeringTarget(target);
        }

        [Command]
        private void CmdCreateOffer(string marketGuid, string offerData) {
            this.ServerCreateOffer(marketGuid, offerData);
        }

        [Command]
        private void CmdApplyOfferDirect(string offerData, NetworkInstanceId buyerId, NetworkInstanceId sellerId) {
            Offer offer = ScriptableObject.CreateInstance<Offer>();
            JsonUtility.FromJsonOverwrite(offerData, offer);
            GameObject buyerObj = NetworkServer.FindLocalObject(buyerId);
            Player buyer = buyerObj.GetComponent<Player>();
            GameObject sellerObj = NetworkServer.FindLocalObject(sellerId);
            Player seller = sellerObj.GetComponent<Player>();
            this.ServerApplyOffer(offer, buyer, seller);
        }

        [Command]
        private void CmdApplyOffer(string offerGuid) {
            Guid oGuid = new Guid(offerGuid);
            foreach (Marketplace market in this._marketplaceDb.marketplaces) {
                foreach (Offer offer in market.offers) {
                    if (offer.guid == oGuid) {
                        this.ServerApplyOffer(offer, this, market.seller);
                        return;
                    }
                }
            }

            RootLogger.Exception(this, "The offer '{0}' with GUID '{0}' was not found and could not be applied.", offerGuid);
        }

        [Command]
        private void CmdRemoveOffer(string offerGuid) {
            Guid oGuid = new Guid(offerGuid);
            foreach (Marketplace market in this._marketplaceDb.marketplaces) {
                foreach (Offer offer in market.offers) {
                    if (offer.guid == oGuid) {
                        this._marketplaceDb.syncProvider.RemoveOffer(market.guid.ToString(), oGuid.ToString());
                        return;
                    }
                }
            }

            RootLogger.Exception(this, "The offer with GUID '{0}' was not found and could not be removed.", offerGuid);
        }

        [Command]
        private void CmdSetLuminance(float value) {
            this.RpcSetLuminance(value);
        }

        [Command]
        private void CmdSetBuildingLuminance(NetworkInstanceId buildingId, float value) {
            GameObject obj = NetworkServer.FindLocalObject(buildingId);
            if (obj != null) {
                Building building = obj.GetComponent<Building>();
                if (building != null) {
                    RootLogger.Debug(this, "Cmd: Setting the building (netid: {0}) luminance to {1}.", buildingId, value);
                    building.Luminance = value;
                } else {
                    RootLogger.Exception(this, "The supplied network identity does not have a Building component.");
                }
            } else {
                RootLogger.Exception(this, "The supplied network identity is not valid.");
            }
        }

        [Command]
        private void CmdGiveFairydust(NetworkInstanceId recipientId, NetworkInstanceId buildingId, Currency currency, int cost, int benefit, float playerLuminanceIncrement, float buildingLuminanceIncrement) {
            RootLogger.Debug(this, "Cmd: Player '{0}' (netid: {1}) gives fairydust to player (netid: {2}).", this.name, this.netId, recipientId);

            // Obtain a reference to the recipient player and the building.
            GameObject recipientObj = NetworkServer.FindLocalObject(recipientId);
            if (recipientObj != null) {
                Player recipient = recipientObj.GetComponent<Player>();
                if (recipient != null) {
                    GameObject buildingObj = NetworkServer.FindLocalObject(buildingId);
                    if (buildingObj != null) {
                        Building building = buildingObj.GetComponent<Building>();
                        if (building != null) {
                            // Detract the cost of the gift from the player.
                            Pocket playerPocket = new Pocket(this._pocket);
                            int balance = 0;
                            playerPocket.TryGetBalance(currency, out balance);
                            playerPocket.SetBalance(currency, balance - cost);
                            this.ServerSetPocket(playerPocket);

                            // Add the benefit of the gift to the recipient.
                            Pocket recipientPocket = new Pocket(recipient.Pocket);
                            recipientPocket.FairyDust += benefit;
                            recipient.ServerSetPocket(recipientPocket);

                            // Increase the luminance of the recipient.
                            recipient.ServerSetLuminance(recipient.Luminance + playerLuminanceIncrement);

                            // Increase the luminance of the building.
                            building.Luminance += buildingLuminanceIncrement;
                        } else {
                            RootLogger.Exception(this, "The supplied network identity for the building does not have a Building component.");
                        }
                    } else {
                        RootLogger.Exception(this, "The supplied network identity is not valid.");
                    }
                } else {
                    RootLogger.Exception(this, "The supplied network identitiy for the recipient does not have a Player component.");
                }
            } else {
                RootLogger.Exception(this, "The supplied network identity is not valid.");
            }
        }
    }
}