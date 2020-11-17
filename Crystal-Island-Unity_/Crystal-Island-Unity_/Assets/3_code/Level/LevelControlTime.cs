using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using KoboldTools;
using KoboldTools.Logging;

namespace Polymoney
{
    /// <summary>
    /// Controls the progression of time in the game. This applies to the
    /// players' free time, luminance, incidents. It also governs the end-game
    /// condition, because it depends on the time passed.
    /// </summary>
    public class LevelControlTime : NetworkBehaviour, IFlowCondition
    {
        /// <summary>
        /// The upper bound of how long the game may last in months.
        /// </summary>

        /// <summary>
        /// The month increment per two turns.
        /// </summary>
        public int monthIncrement = 1;
        /// <summary>
        /// The template for the tax offer.
        /// </summary>
        public Offer taxOffer = null;

        public IEnumerator Start()
        {
            // Wait for the level singleton.
            while (Level.instance == null)
            {
                yield return null;
            }

            // Wait for the game state machine singleton.
            while (GameFlow.instance == null)
            {
                yield return null;
            }

            GameFlow.instance.addEnterCondition((int)PolymoneyGameFlow.FlowStates.END, this);
            GameFlow.instance.changeState.AddListener(this.flowStateChanged);
        }

        /// <summary>
        /// Returns <c>true</c>, if the current number of months is equal or greater than the
        /// maximum number of months per game.
        /// </summary>
        /// <value><c>true</c> if condition met; otherwise, <c>false</c>.</value>
        public bool conditionMet
        {
            get
            {
                return Mathf.FloorToInt(Level.instance.months) > (Level.instance.maximumMonths - 1);
            }
        }

        /// <summary>
        /// On all devices, increases the current month counter at the end of
        /// each month. On the server, also resets the free time and the
        /// luminance of every player.
        /// </summary>
        private void flowStateChanged(int oldState, int newState)
        {
            if (newState == (int)PolymoneyGameFlow.FlowStates.END_MONTH)
            {
                this.commitEndMonth();
            }

            if (newState == (int)PolymoneyGameFlow.FlowStates.BEGIN_MONTH)
            {
                this.commitBeginMonth();
            }
        }

        private void commitBeginMonth()
        {
            if (this.isServer)
            {
                RootLogger.Info(this, "Server: Adding recurrent incidents to each player");
                List<Incident> allIncidents = Level.instance.levelData.Incidents;
                foreach (Player player in Level.instance.allPlayers)
                {
                    if (player.Mayor)
                    {
                        // For mayors, add all incidents of type
                        // "RecurrentCity". If any incidents is an
                        // infrastructure cost incident and any building
                        // reports that it is linked to that incident and that
                        // it incurs costs, the infrastructure cost is adjusted
                        // for the number of players and added to the mayor.
                        IEnumerable<Incident> cityIncidents = allIncidents.FindAll(e => e.Type == "RecurrentCity").Select(e => e.Clone());
                        foreach (Incident incident in cityIncidents)
                        {
                            if (incident.ContainsTags(Level.instance.infrastructureTags))
                            {
                                Building linkedBuilding = Level.instance.Buildings.FirstOrDefault(e => e.IncursInfrastructureCosts && e.IsLinkedWith(incident));
                                if (linkedBuilding != null)
                                {
                                    Incident taxIncident = allIncidents.Find(e => e.EquivalentTags(Level.instance.taxTags));
                                    if (taxIncident != null)
                                    {
                                        float debtMultiplier = Mathf.Abs(linkedBuilding.State-2f);
                                        int taxes = 0;
                                        taxIncident.ApplicationCost.TryGetExpenses(Currency.FIAT, out taxes);

                                        //Set the maintenance cost 
                                        taxes = Options_Controller.baseMaintCost;

                                        int playerCount = Level.instance.allPlayers.Count(e => !e.Mayor);
                                        print(debtMultiplier + " * " + playerCount + " * " + taxes + " * " + Level.instance.InfrastructureCostFactor);
                                        int infrastructureCost = Mathf.FloorToInt(debtMultiplier * playerCount * taxes /* * Level.instance.InfrastructureCostFactor*/);
                                        incident.ApplicationCost.SetExpenses(Currency.FIAT, infrastructureCost);
                                        player.ServerAddIncident(incident);
                                    }
                                    else
                                    {
                                        RootLogger.Exception(this, "Cannot find the tax incident, which makes it impossible to calculate infrastructure costs");
                                    }
                                }
                            }
                            else
                            {
                                player.ServerAddIncident(incident);
                            }
                        }
                    }
                    else
                    {
                        // For regular players, add the rent, salary and tax
                        // incidents, adjusting both rent and salary to the
                        // settings defined in the player's home and job. The
                        // tax incident will create a new offer upon
                        // application that will allow mayors to collect taxes.
                        IEnumerable<Incident> regularIncidents = allIncidents.FindAll(e => e.Type == "Recurrent").Select(e => e.Clone());
                        foreach (Incident incident in regularIncidents)
                        {
                            if (incident.EquivalentTags(Level.instance.rentTags))
                            {
                                incident.ApplicationCost.SetExpenses(Currency.FIAT, player.Home.Rent);
                            }
                            else if (incident.EquivalentTags(Level.instance.salaryTags))
                            {
                                incident.ApplicationBenefit.SetIncome(Currency.FIAT, player.Job.Salary);
                            }
                            else if (incident.EquivalentTags(Level.instance.taxTags))
                            {
                                Offer tmp = ScriptableObject.CreateInstance<Offer>();
                                JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(this.taxOffer), tmp);
                                tmp.guid = Guid.NewGuid();
                                tmp.buyingCost = new Cost(incident.ApplicationBenefit);
                                tmp.buyingBenefit = new Benefit(incident.ApplicationCost);

                                //Is it a flat tax?
                                if (Options_Controller.flatTax)
                                {
                                    //Set the value to the base tax amount
                                    tmp.buyingBenefit.Income[0].value = Options_Controller.baseTaxAmount;
                                    int modValue = (int)(player.Job.Salary * (Options_Controller.baseTaxRate / 100f));
                                    print("Added value due to flat tax: " + modValue + " at a " + Options_Controller.baseTaxRate);
                                    tmp.buyingBenefit.Income[0].value += modValue;
                                    print("The new tax value is : " + tmp.buyingBenefit.Income[0].value);
                                }
                                //Is it a progressive tax?
                                else if (Options_Controller.progTax)
                                {
                                    int modValue = 0;
                                    if (player.Job.Salary > 0)
                                    {
                                        tmp.buyingBenefit.Income[0].value = Options_Controller.baseTaxAmount;
                                        modValue = (int)((Remap(player.Job.Salary, 0, 1300, Options_Controller.baseTaxRate, Options_Controller.progressiveTaxUpper) / 100) * player.Job.Salary);
                                        print("Remapped: " + (Remap(player.Job.Salary, 0, 1300, Options_Controller.baseTaxRate, Options_Controller.progressiveTaxUpper)));
                                        print("Added value due to progressive tax: " + modValue);
                                        tmp.buyingBenefit.Income[0].value += modValue;
                                        print("The new tax value is : " + tmp.buyingBenefit.Income[0].value);
                                    }
                                    else
                                    {
                                        tmp.buyingBenefit.Income[0].value = Options_Controller.baseTaxAmount;
                                        print("The new tax value is : " + tmp.buyingBenefit.Income[0].value);
                                    }
                                }

                                /*
                                //Add the tax modifications
                                for(int i = 0; i < tmp.buyingBenefit.Income.Count; i++)
                                {


                                }
                                */

                                incident.AddSerializedOffer = JsonUtility.ToJson(tmp);
                            }
                            player.ServerAddIncident(incident);
                        }
                    }
                }
            }
        }

        //Helper function to remap numbers
        public static float Remap(float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

        private void commitEndMonth()
        {
            if (this.isServer)
            {
                RootLogger.Info(this, "Server: Applying each player's incidents, resetting their time, luminance and offers");
                foreach (Player player in Level.instance.allPlayers)
                {
                    player.ServerApplyAllIncidents();
                    player.ServerClearIncidents();
                    if (!player.Mayor)
                    {
                        player.ServerSetPocketTime(100 - player.Job.TimeCost);
                    }
                    player.ServerSetLuminance(0.0f);


                    //player.ServerClearOffers(false);
                }

                //define critical and game over lists of the level instance and sync with clients
                updateCriticalPlayers();

                //deside what to do on the end of the month
                if (!this.conditionMet)
                {
                    RootLogger.Info(this, "Incrementing the month counter");
                    Level.instance.months += this.monthIncrement;
                }
                else
                {
                    RootLogger.Info(this, "The end-game condition is met");
                }

                //sync with clients
                RpcSyncPlayerStateLists(Level.instance.criticalPlayers.Cast<uint>().ToArray(), Level.instance.gameOverPlayers.Cast<uint>().ToArray());
                RpcSyncLevelMonths(Level.instance.months);
            }

            //do a data snapshot on server and clients
            RootAnalytics.SnapshotMonth(Mathf.FloorToInt(Level.instance.months));

        }
        /// <summary>
        /// check if any of the players are critical or game over.
        /// the lists are maintained only for the current month. E.g. if a client with player authority set his player to game over it will be removed from the lists for every month after
        /// </summary>
        private void updateCriticalPlayers()
        {

            int fiat = 0;
            int q = 0;

            foreach (Player p in Level.instance.allPlayers)
            {
                if (p.GameOver)
                    continue;

                p.Pocket.TryGetBalance(Currency.FIAT, out fiat);
                p.Pocket.TryGetBalance(Currency.Q, out q);

                RootLogger.Debug(this, "update critical player {0} with fiat: {1} and mayor: {2} and NotUpkeptBuildings: {3}", p.Person.Title, fiat, p.Mayor, Level.instance.UpkeptBuildings);
                if (fiat < Level.instance.RegularStartingMoney * .25f || (p.Mayor && Level.instance.UpkeptBuildings < Level.instance.MinimumUpkeep))
                {
                    if (p.GameOver && Level.instance.gameOverPlayers.Contains(p.netId.Value))
                    {
                        //remove from game over list if the client did commit the game over
                        Level.instance.gameOverPlayers.Remove(p.netId.Value);
                        RootLogger.Debug(this, "remove player from gameover");
                    }

                    if (!Level.instance.criticalPlayers.Contains(p.netId.Value))
                    {
                        //player is critical
                        Level.instance.criticalPlayers.Add(p.netId.Value);
                        RootLogger.Debug(this, "add player to critical");
                    }
                    else
                    {
                        //player is gameover
                        Level.instance.criticalPlayers.Remove(p.netId.Value);

                        //add to game over list if the client did not yet commit the game over
                        Level.instance.gameOverPlayers.Add(p.netId.Value);
                        RootLogger.Debug(this, "add player to gameover");                   
                    }
                }
                else
                {
                    if (Level.instance.criticalPlayers.Contains(p.netId.Value))
                    {
                        //saved a critical player
                        Level.instance.criticalPlayers.Remove(p.netId.Value);
                        RootLogger.Debug(this, "remove player from critical");
                    }
                }
            }
        }
        [ClientRpc]
        private void RpcSyncLevelMonths(int months)
        {
            Level.instance.months = months;
        }
        [ClientRpc]
        private void RpcSyncPlayerStateLists(uint[] criticalPlayers, uint[] gameOverPlayers)
        {
            Level.instance.criticalPlayers = criticalPlayers.Cast<uint>().ToList();
            Level.instance.gameOverPlayers = gameOverPlayers.Cast<uint>().ToList();
            Level.instance.onLevelStateChanged.Invoke();
        }
    }
}
