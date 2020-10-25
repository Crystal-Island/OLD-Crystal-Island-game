using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KoboldTools;

namespace Polymoney
{
    public class PlayerDisplayIncidents : VCBehaviour<Player>
    {
        [Tooltip("If set to true, also shows ignored incidents.")]
        public bool showIgnored = false;
        [Tooltip("If set to true, also shows resolved incidents.")]
        public bool showResolved = false;
        [Tooltip("If set to true, also shows applied incidents.")]
        public bool showApplied = false;
        [Tooltip("If set to true, also shows non-influenceable incidents.")]
        public bool showNonInfluenceable = false;
        [Tooltip("Selects those incident types thate are not to be shown.")]
        public List<string> excludeIncidentTypes = new List<string>();
        [Tooltip("The template game object to use to display each incident.")]
        public Transform incidentUiTemplate;
        private Pool<Transform> incidentUiPool;

        public void Awake()
        {
            this.incidentUiPool = new Pool<Transform>(this.incidentUiTemplate);
        }

        public override void onModelChanged()
        {
            this.model.PlayerStateChanged.AddListener(this.onPlayerStateChanged);
        }
        public override void onModelRemoved()
        {
            this.model.PlayerStateChanged.RemoveListener(this.onPlayerStateChanged);
        }
        private void onPlayerStateChanged()
        {
            // Ignore those incidents that will be removed by other incidents.
            List<Incident> currentIncidents = new List<Incident>(this.model.Incidents);

            this.incidentUiPool.releaseAll();
            foreach (Incident incident in currentIncidents)
            {
                if ((incident.State == IncidentState.UNTOUCHED) || (incident.State == IncidentState.APPLIED && this.showApplied) || (incident.State == IncidentState.RESOLVED && this.showResolved) || (incident.State == IncidentState.IGNORED && this.showIgnored))
                {
                    if (incident.Influenceable || (this.showNonInfluenceable && !incident.Influenceable))
                    {
                        if (!this.excludeIncidentTypes.Contains(incident.Type))
                        {
                            Transform incidentUi = this.incidentUiPool.pop();
                            incidentUi.gameObject.SetActive(true);
                            VC<Incident>.addModelToAllControllers(incident, incidentUi.gameObject);
                        }
                    }
                }
            }
        }
    }
}
