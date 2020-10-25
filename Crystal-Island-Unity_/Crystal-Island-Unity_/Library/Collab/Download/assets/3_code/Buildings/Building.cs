using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using KoboldTools;
using KoboldTools.Logging;

namespace Polymoney
{
    public class Building : NetworkBehaviour, IInteractive
    {
        [ContextMenuItem("Repair Building", "ServerRepairBuilding")]
        [ContextMenuItem("Break Building", "ServerBreakBuilding")]
        [SyncVar(hook = "onStateSynced")]
        public float State = 1.0f;
        [ContextMenuItem("Increase Luminance", "IncrementLuminance")]
        [SyncVar(hook = "onLuminanceSynced")]
        public float Luminance = 0.0f;
        public float BaseLuminance = 0.1f;
        public Marketplace Marketplace;
        public UnityEvent OnBuildingStateChanged = new UnityEvent();
        public UnityEvent OnBuildingRepaired = new UnityEvent();
        public UnityEvent OnBuildingRepair = new UnityEvent();
        public UnityEvent OnBuildingBroken = new UnityEvent();
        public UnityEvent OnLuminanceChanged = new UnityEvent();
        public UnityEvent OnLuminanceHalf = new UnityEvent();
        public UnityEvent OnLuminanceFull = new UnityEvent();

        private UnityEvent _interacted = new UnityEvent();
        private BuildingDisplayState _buildingStateDisplay;
        private BuildingDisplayLuminance _luminanceDisplay;
        private BuildingLinkWithIncident _linkWithIncident;

        public void Awake()
        {
            this._buildingStateDisplay = GetComponent<BuildingDisplayState>();
            this._luminanceDisplay = GetComponent<BuildingDisplayLuminance>();
            this._linkWithIncident = GetComponent<BuildingLinkWithIncident>();
        }

        public IEnumerator Start()
        {
            while (Level.instance == null)
            {
                yield return null;
            }

            Level.instance.AddBuilding(this);
            this.Luminance = this.BaseLuminance;
        }

        public void onPointerDown()
        {
            //do nothing
        }

        public void onPointerUp()
        {
            //raise interacted event
            interacted.Invoke();
        }

        public void ServerRepairBuilding()
        {
            if (this.isServer)
            {

                if (Mathf.Abs(this.State - 1.0f) > float.Epsilon)
                {

                    this.State = 1.0f;
                    this.Luminance = this.BaseLuminance;
                }

                RootLogger.Info(this, "Server: Repairing the building");
                RpcRepairBuilding();

            }
            else
            {
                RootLogger.Exception(this, "The method Building.ServerRepairBuilding() may only be called on the server");
            }
        }

        public void ServerBreakBuilding()
        {
            if (this.isServer)
            {
                //incrementally decrease state to track how often a building broke
                if (Mathf.Abs(this.State) > float.Epsilon)
                {
                    RootLogger.Info(this, "Server: Breaking the building");
                    this.Luminance = 0.0f;
                }
                else
                {
                    RootLogger.Info(this, "Server: Decreasing state of broken building");
                }
                this.State -= 1f;
            }
            else
            {
                RootLogger.Exception(this, "The method Building.ServerBreakBuilding() may only be called on the server");
            }
        }

        public void IncrementLuminance()
        {
            this.Luminance += 0.1f;
        }

        public UnityEvent interacted
        {
            get
            {
                return _interacted;
            }
        }

        public bool IncursInfrastructureCosts
        {
            get
            {
                return !(this.Marketplace != null && this.Marketplace.offers.Count > 0);
            }
        }

        public bool MayBreak
        {
            get
            {
                return this._buildingStateDisplay != null;
            }
        }

        public bool DisplaysLuminance
        {
            get
            {
                return this._luminanceDisplay != null;
            }
        }

        public bool IsLinkedWith(Incident incident)
        {
            if (this._linkWithIncident != null)
            {
                return this._linkWithIncident.IsLinkedWith(incident);
            }
            else
            {
                return false;
            }
        }
        [ClientRpc]
        private void RpcRepairBuilding()
        {
            OnBuildingRepair.Invoke();
        }

        private void onStateSynced(float value)
        {
            if (value > 1.0f)
            {
                this.State = 1.0f;
            }
            else
            {
                this.State = value;
            }


            // Fire the appropriate events.
            if (this.State <= float.Epsilon)
            {
                this.OnBuildingBroken.Invoke();
            }
            else if (Math.Abs(this.State - 1.0f) <= float.Epsilon)
            {
                this.OnBuildingRepaired.Invoke();
            }
            this.OnBuildingStateChanged.Invoke();
        }

        private void onLuminanceSynced(float value)
        {
            // Update the luminance value.
            if (Math.Abs(this.State) > float.Epsilon)
            {
                if (value > 1.0f)
                {
                    this.Luminance = 1.0f;
                }
                else if (value < 0.0f)
                {
                    this.Luminance = 0.0f;
                }
                else
                {
                    this.Luminance = value;
                }
            }
            else
            {
                this.Luminance = 0.0f;
            }

            RootLogger.Info(this, "The luminance was changed to {0}", this.Luminance);

            // Fire the appropriate events.
            if (Math.Abs(this.Luminance - 0.5f) <= float.Epsilon)
            {
                this.OnLuminanceHalf.Invoke();
            }
            else if (Math.Abs(this.Luminance - 1.0f) <= float.Epsilon)
            {
                this.OnLuminanceFull.Invoke();
            }
            this.OnLuminanceChanged.Invoke();
        }
    }
}
