using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KoboldTools;
using KoboldTools.Logging;
using UnityEngine;
using UnityEngine.UI;

namespace Polymoney {
    public class PlayerGetIncident : VCBehaviour<Player> {
        [Header("Fortune Wheel")]
        [Tooltip("The number of times the wheel is spun in one session.")]
        public int maxWheelDraws = 2;
        [Tooltip("The lower bound on the random target angle.")]
        public float minTargetAngle = 720.0f;
        [Tooltip("The upper bound on the random target angle.")]
        public float maxTargetAngle = 2880.0f;
        [Tooltip("The rotation speed of the wheel hand in Hz.")]
        public float rotationSpeed = 1.0f;
        [Tooltip("Points to the transform of the wheel hand.")]
        public Transform wheelHand;
        [Tooltip("Each wheel segment corresponds to a specific type of incident.")]
        public string[] wheelSegmentTypes;
        [Range(0f, 1f)]
        public float matchProbability = Options_Controller.disasterMatchChance/100f;
        public GameObject waitingNotification;
        public UiResource Resource;

        /// <summary>
        /// Counts the number of times, the wheel was spun.
        /// </summary>
        private int numWheelDraws = 0;
        /// <summary>
        /// Stores the initial rotation of the wheel hand.
        /// </summary>
        private Quaternion initialRotation;
        /// <summary>
        /// Holds a set of lucky incidents for random selection.
        /// </summary>
        private Roulette<Incident> luckRoulette = new Roulette<Incident>();
        /// <summary>
        /// Holds a set of disasters for random selection.
        /// </summary>
        private Roulette<Incident> disasterRoulette = new Roulette<Incident>();
        /// <summary>
        /// Holds a set of talent-acquisition incidents for random selection.
        /// </summary>
        private Roulette<Incident> talentRoulette = new Roulette<Incident>();
        /// <summary>
        /// Holds the set of incidents reserved for the city.
        /// </summary>
        private List<Incident> cityIncidents = new List<Incident>();
        /// <summary>
        /// Holds a reference to the selected wheel segment.
        /// </summary>
        private string selectedSegmentType = null;

        //The Chance a city disaster will occur modified by the options menu
        private int disaterChance;

        /// <summary>
        /// Waits for the <see cref="Level"/> instance to appear, then calls
        /// the parent class Start method.
        /// </summary>
        private new IEnumerator Start() {
            while (Level.instance == null || Level.instance.levelData == null) {
                yield return null;
            }

            // Save the initial local rotation of the wheel hand.
            this.initialRotation = this.wheelHand.localRotation;

            base.Start();
        }

        public override void onModelChanged() {
            // Initialize roulette selectors for random selection of incidents
            foreach (Incident incident in Level.instance.levelData.Incidents) {
                if (incident.Type == "Luck") {
                    this.luckRoulette.addPocket(incident, incident.PickPoolSize);
                } else if (incident.Type == "Disaster") {
                    this.disasterRoulette.addPocket(incident, incident.PickPoolSize);
                } else if (incident.Type == "Talent") {
                    this.talentRoulette.addPocket(incident, incident.PickPoolSize);
                } else if (incident.Type == "City") {
                    this.cityIncidents.Add(incident);
                }
            }

            // Sort the city incident list by their indices.
            this.cityIncidents = this.cityIncidents.OrderBy(i => i.Id).ToList();

            //add listeners
            GameFlow.instance.changeState.AddListener(this.gameStateChanged);
            this.model.OnWaitingForTurnCompletion.AddListener(this.waitingForTurnCompletion);
            this.model.OnTurnCompleted.AddListener(this.turnCompleted);
        }

        public override void onModelRemoved() {
            //remove listeners
            if (GameFlow.instance != null) {
                GameFlow.instance.changeState.RemoveListener(this.gameStateChanged);
            }
            this.model.OnWaitingForTurnCompletion.RemoveListener(this.waitingForTurnCompletion);
            this.model.OnTurnCompleted.RemoveListener(this.turnCompleted);
        }

        /// <summary>
        /// When transitioning to the state <see cref="PolymoneyGameFlow.FlowStates.PLAYER_EVENTS"/>,
        /// spins the lucky wheel for players to receive an incident card.
        /// </summary>
        /// <param name="oldState">Old state.</param>
        /// <param name="newState">New state.</param>
        private void gameStateChanged(int oldState, int newState) {
            if (newState == (int) PolymoneyGameFlow.FlowStates.PLAYER_EVENTS)
            {
                if (this.model.Mayor)
                {
                    //Checks if the frequency option has been modified
                    switch(Options_Controller.frequencyFactor)
                    {
                        default:
                            disaterChance = 0;
                            // City incidents are not selected at random, but follow the order defined
                            // by their Id fields.
                            this.addCityEvent();
                            break;
                        //Low Freq 10%
                        case 1:
                            disaterChance = 10;
                            break;
                        //Med Freq 25%
                        case 2:
                            disaterChance = 25;
                            break;
                        //High Freq 40%
                        case 3:
                            disaterChance = 40;
                            break;
                    }

                    //Roll to see if a disaster occurs
                    if(disaterChance > 0)
                    {
                        int chance = UnityEngine.Random.Range(0, 100);
                        if(chance < disaterChance)
                        {
                            this.addCityEvent();
                        }
                    }
                }
                else
                {
                    // Start the wheel turning.
                    this.startWheelSpinning();
                }
            }
        }

        private void startWheelSpinning() {
            //get random angle
            float angle = this.getRandomTargetAngle();

            //get a new talent in 1/12 of all draws
            float discriminant = UnityEngine.Random.Range(0.0f, 1.0f);
            if (discriminant < (1.0f / 12.0f)) {
                angle = this.maxTargetAngle;
            }

            StartCoroutine(this.spinWheel(angle));
        }

        private float getRandomTargetAngle() {
            return UnityEngine.Random.Range(this.minTargetAngle, this.maxTargetAngle);
        }

        private string getSelectedSegmentType(float angle) {
            int idx = Mathf.FloorToInt(this.wheelSegmentTypes.Length * (angle / 360.0f));
            string type = this.wheelSegmentTypes[idx];

            return type;
        }

        private IEnumerator spinWheel(float targetAngle) {
            // Increment the wheel turn counter.
            this.numWheelDraws += 1;

            // Reset the wheel hand back to its original rotation state.
            this.wheelHand.localRotation = this.initialRotation;

            AudioController.Play("fortune_wheel");

            float angle = 0.0f;
            for (float progress = 0.0f; progress <= 1.0f; progress += Time.deltaTime * this.rotationSpeed) {
                // Interpolate the angle.
                angle = Mathf.Lerp(0.0f, targetAngle, Easing.ease(EasingType.QuarticOut, progress)) % 360.0f;

                // Set the wheel hand rotation
                this.wheelHand.localRotation = Quaternion.Euler(0.0f, 0.0f, angle - 15.0f) * this.initialRotation;
                yield return null;
            }

            this.selectedSegmentType = this.getSelectedSegmentType(angle);
            addEventFromSelectedSegment();

        }

        private void waitingForTurnCompletion() {
            this.waitingNotification.SetActive(true);
        }

        private void turnCompleted() {
            this.waitingNotification.SetActive(false);
        }

        private void commitIncident(Incident incident) {
            // Add the selected incident to the player.
            this.model.ClientAddIncident(incident);

             Alert.info(incident.LocalisedDescription, new Alert.AlertParams {
                title = incident.LocalisedTitle,
                hideCloseButton = true,
                sprite = this.Resource.GetSpriteByTags(incident.Tags),
                callbacks = new Alert.AlertCallback[] {
                    new Alert.AlertCallback {
                        buttonText = Localisation.instance.getLocalisedText("btnOk"),
                            mainButton = true,
                            callback = () => {
                                Alert.close();

                                // Either turn the wheel again, or finish the turn.
                                if (!this.model.Mayor && (this.numWheelDraws < this.maxWheelDraws)) {
                                    this.startWheelSpinning();
                                } else {
                                    RootLogger.Info(this, "The player has seen all wheel incidents and thus completes their turn.");
                                    this.numWheelDraws = 0;
                                    this.model.ClientEndTurn();
                                }
                            }
                    }
                }
            });
        }

        private void displayToughLuck(Incident incident) {
            Alert.info(Localisation.instance.getLocalisedText("toughLuck"), new Alert.AlertParams {
                title = incident.LocalisedTitle,
                hideCloseButton = true,
                sprite = this.Resource.GetSpriteByTags(incident.Tags),
                callbacks = new Alert.AlertCallback[] {
                    new Alert.AlertCallback {
                        buttonText = Localisation.instance.getLocalisedText("btnOk"),
                            mainButton = true,
                            callback = () => {
                                Alert.close();

                                // Either turn the wheel again, or finish the turn.
                                if (!this.model.Mayor && (this.numWheelDraws < this.maxWheelDraws)) {
                                    this.startWheelSpinning();
                                } else {
                                    RootLogger.Info(this, "The player has seen all wheel incidents and thus completes their turn.");
                                    this.numWheelDraws = 0;
                                    this.model.ClientEndTurn();
                                }
                            }
                    }
                }
            });
        }

        private void addEventFromSelectedSegment() {
            //select incident and add to player
            if (this.selectedSegmentType == "Luck") {
                Incident incident = this.luckRoulette.spinRoulette().Clone();
                this.commitIncident(incident);
            } else if (this.selectedSegmentType == "Disaster") {
                Incident selectedIncident = null;

                //get a sure match when probability matches
                if (UnityEngine.Random.Range(0f, 1f) < matchProbability) {
                    //we need a sure match
                    int loop = 0; //loop counter to exit loop when unsuccessful after 100 draws
                    while (selectedIncident == null || loop < 100 || !Level.instance.allPlayers.Any(p => p.Talents.Any(t => selectedIncident.Tags.SequenceEqual(t.Tags)))) {
                        selectedIncident = this.disasterRoulette.spinRoulette();
                        loop++;
                    }
                } else {
                    //just get incident randomly
                    selectedIncident = this.disasterRoulette.spinRoulette();
                }
                this.commitIncident(selectedIncident.Clone());
            } else if (this.selectedSegmentType == "Talent") {
                Incident incident = this.talentRoulette.spinRoulette().Clone();
                if (incident.ApplicationBenefit.WouldAddTalent(this.model)) {
                    this.commitIncident(incident);
                } else {
                    this.commitIncident(this.luckRoulette.spinRoulette().Clone());
                }
            }
        }

        private void addCityEvent() {

            Incident currentIncident = this.cityIncidents.FirstOrDefault(e => e.Month == Level.instance.months);

            //Check to see if the frequency option is modified
            if(Options_Controller.frequencyFactor > 0)
            {
                //If changed, select a random incident
                currentIncident = cityIncidents[UnityEngine.Random.Range(0, cityIncidents.Count)];
            }

            //Modify the cost of the incident based on severity option
            if(currentIncident != null)
            {
                int currentCost = 0;
                currentIncident.ApplicationCost.TryGetExpenses(Currency.FIAT, out currentCost);
                currentIncident.ApplicationCost.SetExpenses(Currency.FIAT, (int)(currentCost * Options_Controller.severityFactor));
                currentIncident.Description = Localisation.instance.getLocalisedText(currentIncident.Description);
                currentIncident.Description += "\nThe city has to pay " + (currentCost * Options_Controller.severityFactor) + " in repair costs";
            }
            
            if (currentIncident != null) {
                this.commitIncident(currentIncident);
            } else {
                StartCoroutine(this.delayedEndTurn());
            }
        }

        private IEnumerator delayedEndTurn() {
            while (this.model.TurnFinished) {
                yield return null;
            }
            RootLogger.Info(this, "The mayor will not receive any events this month and thus completes their turn.");
            RootLogger.Info(this, "The mayor will not receive any events this month and thus completes their turn.");
            this.model.ClientEndTurn();
        }
    }
}