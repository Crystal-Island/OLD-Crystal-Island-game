using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using KoboldTools;

namespace Polymoney
{
    public class IncidentResolutionUi : VCBehaviour<Incident>
    {
        public Text resolutionHelp;
        public Button resolutionButton;
        public Button applyButton;
        public string resolutionHelpTextId = "incidentResolveSelf";

        public override void onModelChanged()
        {
            Player player = Level.instance.authoritativePlayer;
            if (player != null)
            {
                Talent matchingTalent = player.Talents.Find(e => e.EquivalentTags(this.model.Tags));
                if (matchingTalent != null)
                {
                    if (this.resolutionButton != null)
                    {
                        this.resolutionButton.interactable = true;
                        this.resolutionButton.onClick.RemoveAllListeners();
                        this.resolutionButton.onClick.AddListener(this.onClickResolutionButton);
                    }

                    if (this.resolutionHelp != null)
                    {
                        this.resolutionHelp.text = Localisation.instance.getLocalisedFormat(this.resolutionHelpTextId, matchingTalent.Title);
                    }
                }
                else
                {
                    if (this.resolutionButton != null)
                    {
                        this.resolutionButton.interactable = false;
                    }

                    if (this.resolutionHelp != null)
                    {
                        this.resolutionHelp.text = "Ask around to resolve."; //loca incidentResolveOther
                    }
                }

                if (this.applyButton != null)
                {
                    this.applyButton.onClick.RemoveAllListeners();
                    this.applyButton.onClick.AddListener(this.onClickApplyButton);
                }
            }
        }

        public override void onModelRemoved()
        {
        }

        private void onClickResolutionButton()
        {
            Player player = Level.instance.authoritativePlayer;
            if (player != null)
            {
                player.ClientResolveIncident(this.model);
            }
        }

        private void onClickApplyButton()
        {
            Player player = Level.instance.authoritativePlayer;
            if (player != null)
            {
                player.ClientApplyIncident(this.model);
            }
        }
    }
}
