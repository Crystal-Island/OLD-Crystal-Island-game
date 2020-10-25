using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using KoboldTools;
using KoboldTools.Logging;

namespace Polymoney
{
    public class PlayerDisplaySymbols : VCBehaviour<Player>
    {
        [Header("Taxe Collection")]
        public Button TaxCollectionButton;
        public string TaxTitleTextId = "offerPopUpTaxTitle";
        public string TaxDescriptionTextId = "offerPopUpTaxDescription";

        [Header("Welfare")]
        public Button WelfareButton;
        public Offer WelfareOffer;
        public string WelfareTitleTextId = "offerPopUpWelfareTitle";
        public string WelfareDescriptionTextId = "offerPopUpWelfareDescription";

        [Header("General")]
        public UiResource Resource;
        public GameObject turnComplete;
        public string CloseButtonTextId = "offerPopUpCloseButton";
        public string ApplyButtonTextId = "offerPopUpApplyButton";

        private Marketplace OwnMarketplace;
        private Player LocalPlayer;
        private Offer CurrentTaxOffer;

        public new IEnumerator Start()
        {
            base.Start();

            while (Level.instance == null)
            {
                yield return null;
            }

            Level.instance.onAuthoritativePlayerChanged.AddListener(this.onAuthoritativePlayerChanged);
            this.onAuthoritativePlayerChanged();
        }

        public override void onModelChanged()
        {
            // Register to all events that can cause the symbols to be updated.
            if (GameFlow.instance != null) {
                GameFlow.instance.changeState.AddListener(this.onStateChanged);
            }
            if (this.model != null) {
                this.model.PlayerStateChanged.AddListener(this.onPlayerStateChanged);
                this.OwnMarketplace = this.model.OwnMarketplace;
                if (this.OwnMarketplace != null)
                {
                    this.OwnMarketplace.onOfferAdd.AddListener(this.onOffersChanged);
                    this.OwnMarketplace.onOfferRemove.AddListener(this.onOffersChanged);
                }
                this.updateSymbols();

                // Register to events for the turn completion symbol.
                if (this.turnComplete != null)
                {
                    this.model.OnTurnCompleted.AddListener(this.onTurnCompleted);
                    this.model.OnWaitingForTurnCompletion.AddListener(this.onWaitingForTurnCompletion);
                    this.onTurnCompleted();
                }

                // Register the tax collection symbol button.
                if (this.TaxCollectionButton != null)
                {
                    this.TaxCollectionButton.onClick.AddListener(this.onClickTaxSymbol);
                }

                // Register the welfare symbol button.
                if (this.WelfareButton != null)
                {
                    this.WelfareButton.onClick.AddListener(this.onClickWelfareSymbol);
                }
            }
        }

        public override void onModelRemoved()
        {
            if (GameFlow.instance != null) {
                GameFlow.instance.changeState.RemoveListener(this.onStateChanged);
            }
            if (this.model != null) {
                this.model.PlayerStateChanged.RemoveListener(this.onPlayerStateChanged);
                if (this.OwnMarketplace != null)
                {
                    this.OwnMarketplace.onOfferAdd.RemoveListener(this.onOffersChanged);
                    this.OwnMarketplace.onOfferRemove.RemoveListener(this.onOffersChanged);
                }
                if (this.turnComplete != null)
                {
                    this.model.OnTurnCompleted.RemoveListener(this.onTurnCompleted);
                    this.model.OnWaitingForTurnCompletion.RemoveListener(this.onWaitingForTurnCompletion);
                }
                if (this.TaxCollectionButton != null)
                {
                    this.TaxCollectionButton.onClick.RemoveListener(this.onClickTaxSymbol);
                }
                if (this.WelfareButton != null)
                {
                    this.WelfareButton.onClick.RemoveListener(this.onClickWelfareSymbol);
                }
            }
        }

        private void onAuthoritativePlayerChanged()
        {
            this.LocalPlayer = Level.instance.authoritativePlayer;
        }

        private void onTurnCompleted()
        {
            if (this.turnComplete != null)
            {
                this.turnComplete.SetActive(false);
            }
        }

        private void onWaitingForTurnCompletion()
        {
            if (this.turnComplete != null)
            {
                this.turnComplete.SetActive(true);
            }
        }

        private void onOffersChanged(Offer offer)
        {
            this.updateSymbols();
        }

        private void onPlayerStateChanged()
        {
            this.updateSymbols();
        }

        private void onStateChanged(int oldState, int newState)
        {
            if (newState == (int)PolymoneyGameFlow.FlowStates.PLAYER_TRADE)
            {
                this.updateSymbols();
            }
        }

        private void updateSymbols()
        {

            if (this.TaxCollectionButton != null)
            {
                if (Level.instance.authoritativePlayer.Mayor)
                {
                    Offer taxOffer = this.OwnMarketplace.offers.Find(e => e.EquivalentTags(Level.instance.taxTags));
                    if (taxOffer != null)
                    {
                        this.TaxCollectionButton.gameObject.SetActive(true);
                        this.CurrentTaxOffer = taxOffer;
                    }
                    else
                    {
                        this.TaxCollectionButton.gameObject.SetActive(false);
                    }
                }
                else
                {
                    this.TaxCollectionButton.gameObject.SetActive(false);
                }
            }

            if (this.WelfareButton != null)
            {
                if (Level.instance.authoritativePlayer.Mayor && !this.model.GameOver)
                {
                    Incident taxIncident = this.model.Incidents.Find(e => e.EquivalentTags(Level.instance.taxTags) && e.State == IncidentState.UNTOUCHED);
                    if (taxIncident != null)
                    {
                        this.WelfareButton.gameObject.SetActive(true);
                    }
                    else
                    {
                        this.WelfareButton.gameObject.SetActive(false);
                    }
                }
                else
                {
                    this.WelfareButton.gameObject.SetActive(false);
                }
            }
        }

        private void onClickTaxSymbol()
        {
            string title = Localisation.instance.getLocalisedText(this.TaxTitleTextId);
            string content = Localisation.instance.getLocalisedText(this.TaxDescriptionTextId);
            string closeText = Localisation.instance.getLocalisedText(this.CloseButtonTextId);
            string applyText = Localisation.instance.getLocalisedText(this.ApplyButtonTextId);

            Alert.info(content, new Alert.AlertParams {
                title = title,
                closeText = closeText,
                sprite = this.Resource.GetSpriteByTags(this.CurrentTaxOffer.tags),
                callbacks = new Alert.AlertCallback[] {
                    new Alert.AlertCallback {
                        buttonText = applyText,
                        callback = () => {
                            RootLogger.Info(this, "Applying the tax offer ({0}) to buyer {1} and seller {2}", this.CurrentTaxOffer, this.LocalPlayer, this.model);
                            this.LocalPlayer.ClientApplyOffer(this.CurrentTaxOffer, this.LocalPlayer, this.model);
                            this.CurrentTaxOffer = null;
                            Alert.close();
                        },
                        mainButton = true,
                    },
                },
            });
        }

        private void onClickWelfareSymbol()
        {
            string title = Localisation.instance.getLocalisedText(this.WelfareTitleTextId);
            string content = Localisation.instance.getLocalisedText(this.WelfareDescriptionTextId);
            string closeText = Localisation.instance.getLocalisedText(this.CloseButtonTextId);
            string applyText = Localisation.instance.getLocalisedText(this.ApplyButtonTextId);

            Alert.info(content, new Alert.AlertParams {
                title = title,
                closeText = closeText,
                sprite = this.Resource.GetSpriteByTags(this.WelfareOffer.tags),
                callbacks = new Alert.AlertCallback[] {
                    new Alert.AlertCallback {
                        buttonText = applyText,
                        callback = () => {
                            RootLogger.Info(this, "Applying the welfare offer ({0}) to buyer {1} and seller {2}", this.WelfareOffer, this.LocalPlayer, this.model);
                            this.LocalPlayer.ClientApplyOffer(this.WelfareOffer, this.LocalPlayer, this.model);
                            Alert.close();
                        },
                        mainButton = true,
                    },
                },
            });
        }
    }
}
