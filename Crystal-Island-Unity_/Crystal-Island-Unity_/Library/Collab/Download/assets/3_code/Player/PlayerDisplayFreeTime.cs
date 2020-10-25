using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using KoboldTools;

namespace Polymoney
{
    public class PlayerDisplayFreeTime : VCBehaviour<Player>
    {
        public Text valueText;
        public Text deltaText;
        public int deltaTextTimeout = 5;
        public Color positiveTextColor;
        public Color neutralTextColor;
        public Color negativeTextColor;
        private int currentValue;

        public override void onModelChanged()
        {
            this.model.PlayerStateChanged.AddListener(this.onPlayerStateChanged);
            this.onPlayerStateChanged();
            if (this.deltaText != null)
            {
                this.deltaText.enabled = false;
            }
        }

        public override void onModelRemoved()
        {
            this.model.PlayerStateChanged.RemoveListener(this.onPlayerStateChanged);
        }

        private void onPlayerStateChanged()
        {
            int newValue = this.model.Pocket.TimeAllowance;
            int delta = newValue - this.currentValue;
            this.currentValue = newValue;

            if (this.valueText != null)
            {
                this.valueText.color = this.neutralTextColor;
                this.valueText.text = String.Format("{0:D} %", this.currentValue);
            }

            if (this.deltaText != null)
            {
                if (delta != 0)
                {
                    if (delta > 0)
                    {
                        this.deltaText.color = this.positiveTextColor;
                    }
                    else
                    {
                        this.deltaText.color = this.negativeTextColor;
                    }
                    this.deltaText.text = String.Format("{0:D} %", delta);
                    StartCoroutine(this.showDeltaText());
                }
            }
        }

        private IEnumerator showDeltaText()
        {
            if (this.deltaText != null)
            {
                this.deltaText.enabled = true;
                yield return new WaitForSeconds(this.deltaTextTimeout);
                this.deltaText.enabled = false;
            }
        }
    }
}
