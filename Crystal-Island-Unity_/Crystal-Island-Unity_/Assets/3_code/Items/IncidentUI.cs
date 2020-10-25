﻿using System;
using UnityEngine;
using UnityEngine.UI;
using KoboldTools;

namespace Polymoney
{
    public class IncidentUI : VCBehaviour<Incident>
    {
        public UiResource resources;

        [Header("Status")]
        [Tooltip("Allows to communicate whether an incident has been ignored.")]
        public GameObject ignoredStatus;
        [Tooltip("Allows to communicate whether an incident has been resolved.")]
        public GameObject resolvedStatus;
        [Tooltip("Allows to communicate whether an incident has been applied.")]
        public GameObject appliedStatus;

        [Header("Immediacy")]
        [Tooltip("Allows to communicate whether an incident will apply immediately.")]
        public Text immediateText;
        public string immediateTextId = "incidentImmediate";
        public string nonImmediateTextId = "incidentLater";

        [Header("Benefit and Cost")]
        [Tooltip("Selects the currency for which to display financial effects of the incident.")]
        public Currency currency;
        public Text incomeText;
        public Text expensesText;
        public Text balanceText;
        public string incomeTextId = "incidentIncome";
        public string expensesTextId = "incidentExpenses";
        public string balanceTextId = "incidentBalance";
        public string FiatCurrencySymbolId = "fiatCurrencyLetter";
        public string QCurrencySymbolId = "qCurrencyLetter";
        public Image entityTypeIcon;
        public Image currencyIcon;
        /// <summary>
        /// Holds the incident type.
        /// </summary>
        [Header("Type")]
        [Tooltip("Allows to display incident type info as text.")]
        public Text typeText;
        public string typeTextId = "incidentType";
        /// <summary>
        /// Holds an icon that relates to the incident type.
        /// </summary>
        [Tooltip("Allows to display incident type info as icons.")]
        public Image typeIcon;

        [Header("Tags")]
        public Text tagsText;
        public string tagsTextId = "incidentTags";
        [Tooltip("ALlows to display the incident tags as icons.")]
        public Image tagsIcon;

        /// <summary>
        /// Updates the UI text based on the model data.
        /// </summary>
        public override void onModelChanged()
        {
            if (this.ignoredStatus != null)
            {
                this.ignoredStatus.SetActive(this.model.State == IncidentState.IGNORED);
            }

            if (this.resolvedStatus != null)
            {
                this.resolvedStatus.SetActive(this.model.State == IncidentState.RESOLVED);
            }

            if (this.appliedStatus != null)
            {
                this.appliedStatus.SetActive(this.model.State == IncidentState.APPLIED);
            }

            if (this.immediateText != null)
            {
                if (this.model.Immediate)
                {
                    this.immediateText.text = Localisation.instance.getLocalisedText(this.immediateTextId);
                }
                else
                {
                    this.immediateText.text = Localisation.instance.getLocalisedText(this.nonImmediateTextId);
                }
            }

            if (this.incomeText != null)
            {
                int value = 0;
                this.model.ApplicationBenefit.TryGetIncome(this.currency, out value);
                this.incomeText.text = Localisation.instance.getLocalisedFormat(this.incomeTextId, value);
            }

            if (this.expensesText != null)
            {
                int value = 0;
                this.model.ApplicationCost.TryGetExpenses(this.currency, out value);
                this.expensesText.text = Localisation.instance.getLocalisedFormat(this.expensesTextId, value);
            }

            if (this.balanceText != null)
            {
                CurrencyValue balance = this.model.ApplicationBalance;
                string textId = balance.GetCurrency() == Currency.FIAT ? this.FiatCurrencySymbolId : this.QCurrencySymbolId;

                this.balanceText.text = String.Format("{0} {1}", balance.value, Localisation.instance.getLocalisedText(textId));
            }

            if (this.entityTypeIcon != null)
            {
                this.entityTypeIcon.color = this.resources.GetColorByEntityType(this.model.ApplicationEntityType);
            }

            if (this.currencyIcon != null)
            {
                this.currencyIcon.color = this.resources.GetColorByBalance(this.model.ApplicationBalance);
            }

            if (this.typeText != null)
            {
                this.typeText.text = Localisation.instance.getLocalisedFormat(this.typeTextId, this.model.Type.ToUpper());
            }

            if (this.typeIcon != null)
            {
                this.typeIcon.sprite = this.resources.GetSpriteByType(this.model.Type);
            }

            if (this.tagsText != null)
            {
                this.tagsText.text = Localisation.instance.getLocalisedFormat(this.tagsTextId, this.model.Tags);
            }

            if (this.tagsIcon != null)
            {
                this.tagsIcon.sprite = this.resources.GetSpriteByTags(this.model.Tags);
            }

        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        public override void onModelRemoved()
        {
        }
    }
}
