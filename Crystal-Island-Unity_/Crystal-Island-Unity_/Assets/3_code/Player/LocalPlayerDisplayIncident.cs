using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using KoboldTools;
using KoboldTools.Logging;

namespace Polymoney
{
    public class LocalPlayerDisplayIncident : MonoBehaviour
    {
        [Serializable]
        public class Tags {
            public List<string> tags;
        }

        [Header("Filter Settings")]
        public string type;
        public List<Tags> filterTags;

        [Header("Dependencies")]
        public Button IncidentButton;
        public UiResource Resource;
        public string CloseButtonTextId = "incidentPopUpCloseButton";
        public string PayButtonTextId = "incidentPopUpPayButton";
        public string ReceiveButtonTextId = "incidentPopUpReceiveButton";
        public string ResolveButtonTextId = "incidentPopUpResolveButton";
        public string FiatCurrencySymbolId = "fiatCurrencyLetter";
        public string QCurrencySymbolId = "qCurrencyLetter";

        private Player LocalPlayer;
        private Level Level;
        private Incident CurrentIncident;

        public IEnumerator Start()
        {
            while (Level.instance == null)
            {
                yield return null;
            }

            this.Level = Level.instance;
            this.Level.onAuthoritativePlayerChanged.AddListener(this.onAuthoritativePlayerChanged);
            this.onAuthoritativePlayerChanged();

            if (this.IncidentButton != null)
            {
                this.IncidentButton.onClick.AddListener(this.onClickIncidentSymbol);
            }
        }

        private void onAuthoritativePlayerChanged()
        {
            if (this.LocalPlayer != null)
            {
                this.LocalPlayer.PlayerStateChanged.RemoveListener(this.onPlayerStateChanged);
            }
            if (this.Level.authoritativePlayer != null)
            {
                this.LocalPlayer = this.Level.authoritativePlayer;
                this.LocalPlayer.PlayerStateChanged.AddListener(this.onPlayerStateChanged);
                this.onPlayerStateChanged();
            }
        }

        private void onPlayerStateChanged()
        {
            if (this.IncidentButton != null)
            {
                Incident incident = this.LocalPlayer.Incidents.Find(e => e.State == IncidentState.UNTOUCHED && ((e.Type == this.type) || (this.filterTags.Any(f => e.EquivalentTags(f.tags)))));
                if (incident != null)
                {
                    this.IncidentButton.gameObject.SetActive(true);
                    VC<Incident>.addModelToAllControllers(incident, this.IncidentButton.gameObject, true);
                    this.CurrentIncident = incident;
                }
                else
                {
                    this.IncidentButton.gameObject.SetActive(false);
                }
            }
        }

        private void onClickIncidentSymbol()
        {
            Talent matchingTalent = this.LocalPlayer.Talents.FirstOrDefault(e => e.EquivalentTags(this.CurrentIncident.Tags));
            CurrencyValue balance = this.CurrentIncident.ApplicationBalance;

            string textId = balance.GetCurrency() == Currency.FIAT ? this.FiatCurrencySymbolId : this.QCurrencySymbolId;
            string description = String.IsNullOrEmpty(this.CurrentIncident.LocalisedDescription) ? String.Empty : String.Format("{0}\n", this.CurrentIncident.LocalisedDescription);
            string content = String.Format("{0}{1} {2}", description, balance.value, Localisation.instance.getLocalisedText(textId));
            string title = this.CurrentIncident.LocalisedTitle;
            string closeText = Localisation.instance.getLocalisedText(this.CloseButtonTextId);
            string applyText = balance.value >= 0 ? Localisation.instance.getLocalisedText(this.ReceiveButtonTextId) : Localisation.instance.getLocalisedText(this.PayButtonTextId);
            string buttonSound = balance.value >= 0 ? "receive_stuff" : "buy_stuff";
            string resolveText = Localisation.instance.getLocalisedText(this.ResolveButtonTextId);
            string resolveButtonSound = "resolve_things";

            // Always provide a button that applies the incident.
            List<Alert.AlertCallback> callbacks = new List<Alert.AlertCallback> {
                    new Alert.AlertCallback {
                        buttonText = applyText,
                        callback = () => {
                            RootLogger.Info(this, "Applying the incident {0} to player {1}", this.CurrentIncident, this.LocalPlayer);
                            if(Level.instance.authoritativePlayer.Mayor)
                            {
                                int cost = 0;
                                CurrentIncident.ApplicationCost.TryGetExpenses(Currency.FIAT, out cost);
                                print("Mayor Cost: " + cost);
                                int mayorMoney = 0;
                                Level.instance.authoritativePlayer.Pocket.TryGetBalance(Currency.FIAT, out mayorMoney);
                                print("Mayor Money: " + mayorMoney);
                                if (mayorMoney < cost)
                                {
                                    Vector2 alertSize = new Vector2(800, 600);
                                    Alert.info("noMayorDebt", new Alert.AlertParams { useLocalization = true, closeText = "btnOk", size = alertSize });
                                }
                                else
                                {
                                    this.LocalPlayer.ClientApplyIncident(this.CurrentIncident);
                                    this.CurrentIncident = null;
                                }
                            }
                            else
                            {
                                this.LocalPlayer.ClientApplyIncident(this.CurrentIncident);
                                this.CurrentIncident = null;
                            }

                            if (!String.IsNullOrEmpty(buttonSound))
                            {
                                AudioController.Play(buttonSound);
                            }
                            Alert.close();
                        },
                        mainButton = (matchingTalent == null),
                    },
            };

            // If the player has a matching talent, also provide a resolve button.
            if (matchingTalent != null)
            {
                callbacks.Add(new Alert.AlertCallback {
                    buttonText = resolveText,
                    callback = () => {
                        RootLogger.Info(this, "Resolving the incident {0} for player {1} with talent {2}", this.CurrentIncident, this.LocalPlayer, matchingTalent);
                        this.LocalPlayer.ClientResolveIncident(this.CurrentIncident);
                        this.CurrentIncident = null;
                        AudioController.Play(resolveButtonSound);
                        Alert.close();
                    },
                    mainButton = true,
                });
            }

            // Show the alert.
            Alert.info(content, new Alert.AlertParams {
                title = title,
                closeText = closeText,
                sprite = this.Resource.GetSpriteByTags(this.CurrentIncident.Tags),
                callbacks = callbacks.ToArray(),
            });
        }
    }
}
