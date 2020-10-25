using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using KoboldTools;

namespace Polymoney
{
    public class PlayerDisplayBalanceSheet : VCBehaviour<Player>
    {
        public Currency currency;
        public Text totalIncomeText;
        public Text totalExpensesText;
        public Text totalBalanceText;
        public Text projectedBalanceText;

        public override void onModelChanged()
        {
            this.model.PlayerStateChanged.AddListener(this.onPlayerStateChanged);
            this.onPlayerStateChanged();
        }

        public override void onModelRemoved()
        {
            this.model.PlayerStateChanged.RemoveListener(this.onPlayerStateChanged);
        }

        private void onPlayerStateChanged()
        {
            // Ignore those incidents that will be removed by other incidents.
            List<Incident> currentIncidents = new List<Incident>(this.model.Incidents);
            HashSet<int> toRemove = new HashSet<int>();
            foreach (Incident incident in currentIncidents)
            {
                toRemove.UnionWith(incident.ApplicationBenefit.getRemovableIncidents(currentIncidents));
            }
            foreach (int idx in toRemove.OrderByDescending(q => q))
            {
                currentIncidents.RemoveAt(idx);
            }

            // Obtain the current balance.
            int currentBalance;
            this.model.Pocket.TryGetBalance(this.currency, out currentBalance);

            // Sum up the total income and expenses over all incidents.
            int totalIncome = 0;
            int totalExpenses = 0;
            foreach (Incident incident in currentIncidents)
            {
                int incidentIncome = 0;
                incident.ApplicationBenefit.TryGetIncome(this.currency, out incidentIncome);

                int incidentExpenses = 0;
                incident.ApplicationCost.TryGetExpenses(this.currency, out incidentExpenses);

                totalIncome += incidentIncome;
                totalExpenses += incidentExpenses;
            }

            this.totalIncomeText.text = String.Format("{0:D}", totalIncome);
            this.totalExpensesText.text = String.Format("{0:D}", totalExpenses);
            this.totalBalanceText.text = String.Format("{0:D}", totalIncome - totalExpenses);
            this.projectedBalanceText.text = String.Format("{0:D}", currentBalance + totalIncome - totalExpenses);
        }
    }
}
