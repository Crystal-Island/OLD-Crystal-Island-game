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
    public class LocalPlayerApplyOffer : VCBehaviour<Offer>
    {
        public Button applyOffer;
        public Player targetPlayer;
        public string applyOfferFailureTextId = "applyOfferFailureMoney";
        private Player localPlayer;
        private Level level;

        public new IEnumerator Start()
        {
            base.Start();

            while (Level.instance == null)
            {
                yield return null;
            }

            this.level = Level.instance;
            this.level.onAuthoritativePlayerChanged.AddListener(this.onAuthoritativePlayerChanged);
            this.onAuthoritativePlayerChanged();
        }

        public override void onModelChanged()
        {
            this.applyOffer.onClick.AddListener(this.onClick);
        }

        public override void onModelRemoved()
        {
            this.applyOffer.onClick.RemoveListener(this.onClick);
        }

        private void onAuthoritativePlayerChanged()
        {
            this.localPlayer = Level.instance.authoritativePlayer;
        }

        private void onClick()
        {
            // The local player will always be the buyer.
            if (this.localPlayer != null)
            {
                // If buying the offer would cost more than the player had money available (minus debt allowance), do not apply the offer.
                CurrencyValue buyingBalance = this.model.BuyingBalance;
                Currency buyingCurrency = buyingBalance.GetCurrency();
                int pocketValue;
                this.localPlayer.Pocket.TryGetBalance(buyingCurrency, out pocketValue);
                CurrencyValue pocketBalance = new CurrencyValue(buyingCurrency, pocketValue);
                CurrencyValue maxDebt = Level.instance.MaximumDebt.SingleOrDefault(c => c.GetCurrency() == buyingCurrency);
                if (buyingBalance >= 0 || pocketBalance + maxDebt + buyingBalance >= 0)
                {
                    //Check the currency type
                    if (buyingCurrency == Currency.FIAT)
                    {
                        localPlayer.numGoldBought++;
                        localPlayer.valueGoldBought += buyingBalance.value;
                        targetPlayer.numGoldSold++;
                        targetPlayer.valueGoldSold += buyingBalance.value;
                    }
                    else if(buyingCurrency == Currency.Q)
                    {
                        localPlayer.numWCBought++;
                        localPlayer.valueWCBought += buyingBalance.value;
                        targetPlayer.numWCSold++;
                        targetPlayer.valueWCSold += buyingBalance.value;
                    }
                    // If defined, the target player indicates the seller of an offer, even if not registered on a marketplace.
                    if (this.targetPlayer == null)
                    {
                        RootLogger.Info(this, "Applying the offer via marketplace: {0}", this.model);
                        this.localPlayer.ClientApplyOffer(this.model.guid.ToString());
                    }
                    else
                    {
                        RootLogger.Info(this, "Applying the offer directly: {0}, buyer: {1}, seller: {2}", this.model, this.localPlayer, this.targetPlayer);
                        this.localPlayer.ClientApplyOffer(this.model, this.localPlayer, this.targetPlayer);
                    }

                }
                else
                {
                    RootLogger.Warning(this, "Cannot apply offer due to insufficient money");
                    Alert.info(this.applyOfferFailureTextId, new Alert.AlertParams { closeText = "btnClose", useLocalization = true });
                }
            }
        }
    }
}
