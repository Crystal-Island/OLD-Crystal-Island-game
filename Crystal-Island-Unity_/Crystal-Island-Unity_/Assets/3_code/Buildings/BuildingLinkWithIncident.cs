using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using KoboldTools.Logging;

namespace Polymoney
{
    [RequireComponent(typeof(Building))]
    public class BuildingLinkWithIncident : MonoBehaviour {
        [Serializable]
        public class Tags {
            public List<string> tags;
        }

        public List<Tags> filterTags;
        private Building building;
        private Level level;
        private Player player;
        private Incident linkedIncident = null;

        public void Awake()
        {
            this.building = GetComponent<Building>();
        }

        public IEnumerator Start()
        {
            while (Level.instance == null)
            {
                yield return null;
            }

            this.level = Level.instance;
            this.level.onAuthoritativePlayerChanged.AddListener(this.onAuthoritativePlayerChanged);
            this.onAuthoritativePlayerChanged();

            //add a listener to the offer creation of the associated marketplace
            addMarketplaceResolveListener();
        }

        public bool IsLinkedWith(Incident incident)
        {
            uint buildingNetId = this.building.netId.Value;
            return this.filterTags.Any(f => incident.EquivalentTags(f.tags)) && incident.State == IncidentState.UNTOUCHED && incident.IgnoranceCost.BreakBuilding != buildingNetId && incident.ApplicationBenefit.RepairBuilding != buildingNetId;
        }

        private void onAuthoritativePlayerChanged()
        {
            if (this.player != null)
            {
                this.player.PlayerStateChanged.RemoveListener(this.onPlayerStateChanged);
            }
            if (this.level.authoritativePlayer != null)
            {
                this.player = this.level.authoritativePlayer;
                this.player.PlayerStateChanged.AddListener(this.onPlayerStateChanged);
                this.onPlayerStateChanged();
            }
        }

        private void onPlayerStateChanged()
        {
            uint buildingNetId = this.building.netId.Value;
            Incident newLinkedIncident = this.player.Incidents.Find(e => this.IsLinkedWith(e));
            if (newLinkedIncident != null)
            {
                this.linkedIncident = newLinkedIncident;
                this.linkedIncident.IgnoranceCost.BreakBuilding = buildingNetId;
                this.linkedIncident.ApplicationBenefit.RepairBuilding = buildingNetId;
                RootLogger.Debug(this, "Linking the incident {0} to the building", linkedIncident);
                this.player.ClientUpdateIncident(linkedIncident);
            }
        }

        //add a listener to offers in the building marketplace to resolve infrastructure incident cost when an offer is applied
        private void addMarketplaceResolveListener()
        {
            if (this.building.Marketplace != null)
            {
                this.building.Marketplace.onOfferAdd.AddListener(resolveLinkedIncident);
            }
        }

        //resolve when done
        private void resolveLinkedIncident(Offer o)
        {
            if (this.linkedIncident != null)
            {
                Incident currentIncident = this.player.Incidents.FirstOrDefault(e => e.Equals(this.linkedIncident));
                if (currentIncident != null && currentIncident.State == IncidentState.UNTOUCHED)
                {
                    this.player.ClientResolveIncident(linkedIncident);
                }
            }
        }
    }
}
