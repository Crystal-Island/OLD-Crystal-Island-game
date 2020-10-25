using System;
using UnityEngine;
using KoboldTools;
using KoboldTools.Logging;

namespace Polymoney
{
    public class PlayerNotifyOfferApplication : VCBehaviour<Player>
    {
        public UiResource Resource;
        public string MayorTitleTextId = "offerNotificationMayorName";
        public string BuyTitleTextId = "offerNotificationBuyTitle";
        public string ReceiveTitleTextId = "offerNotificationReceiveTitle";
        public string WelfareTitleTextId = "offerNotificationWelfareTitle";
        public string CloseButtonTextId = "offerNotificationCloseButton";
        public string FiatCurrencySymbolId = "fiatCurrencyLetter";
        public string QCurrencySymbolId = "qCurrencyLetter";
        public string ForFreeTextId = "forFreeText";

        public override void onModelChanged()
        {
            this.model.OnOfferApplied.AddListener(this.onOfferApplied);
        }

        public override void onModelRemoved()
        {
            this.model.OnOfferApplied.RemoveListener(this.onOfferApplied);
        }

        private void onOfferApplied(Offer offer, Player buyer)
        {
            RootLogger.Info(this, "An offer was applied!");
            if (!offer.EquivalentTags(Level.instance.taxTags))
            {
                string mayorTitle = Localisation.instance.getLocalisedText(this.MayorTitleTextId);
                string buyerName = buyer.Mayor ? mayorTitle : buyer.Person.LocalisedTitle;
                string title = String.Empty;
                string content = String.Empty;
                CurrencyValue balance = offer.SellingBalance;

                if (offer.EquivalentTags(Level.instance.welfareTags))
                {
                    title = Localisation.instance.getLocalisedFormat(this.WelfareTitleTextId, buyerName);
                }
                else if (balance.value > 0)
                {
                    title = Localisation.instance.getLocalisedFormat(this.BuyTitleTextId, buyerName);
                    string textId = balance.GetCurrency() == Currency.FIAT ? this.FiatCurrencySymbolId : this.QCurrencySymbolId;
                    content = String.Format("{0} {1}", balance.value, Localisation.instance.getLocalisedText(textId));
                }
                else if (balance.value < 0)
                {
                    title = Localisation.instance.getLocalisedFormat(this.ReceiveTitleTextId, buyerName);
                    string textId = balance.GetCurrency() == Currency.FIAT ? this.FiatCurrencySymbolId : this.QCurrencySymbolId;
                    content = String.Format("{0} {1}", balance.value, Localisation.instance.getLocalisedText(textId));
                }
                else
                {
                    title = Localisation.instance.getLocalisedFormat(this.ReceiveTitleTextId, buyerName);
                    content = Localisation.instance.getLocalisedText(this.ForFreeTextId);
                }

                Alert.info(content, new Alert.AlertParams {
                    title = title,
                    closeText = Localisation.instance.getLocalisedText(this.CloseButtonTextId),
                    sprite = this.Resource.GetSpriteByTags(offer.tags),
                });
            }
        }
    }
}
