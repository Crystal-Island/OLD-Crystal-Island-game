using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using KoboldTools;

namespace Polymoney
{
    public class ButtonDisplayTurnCompletion : MonoBehaviour
    {
        public Text displayText;
        public string waitingTextId = "turnWait";
        public string turnCompleteTextId = "turnOk";
        public Button turnCompleteButton = null;
        private Player player;

        public IEnumerator Start()
        {
            while (Level.instance == null)
            {
                yield return null;
            }

            Level.instance.onAuthoritativePlayerChanged.AddListener(this.authoritativePlayerChanged);
            //change authoritative player if already existing
            if (Level.instance.authoritativePlayer != null)
            {
                authoritativePlayerChanged();
            }
            //add turn completion listener if button is set
            if (turnCompleteButton != null)
            {
                turnCompleteButton.onClick.AddListener(turnCompleteButtonClicked);
            }

        }
        private void authoritativePlayerChanged()
        {
            if (this.player != null)
            {
                this.player.OnTurnCompleted.RemoveListener(this.turnCompleted);
                this.player.OnWaitingForTurnCompletion.RemoveListener(this.waitingForTurnCompletion);
            }

            this.player = Level.instance.authoritativePlayer;
            this.player.OnTurnCompleted.AddListener(this.turnCompleted);
            this.player.OnWaitingForTurnCompletion.AddListener(this.waitingForTurnCompletion);
        }
        private void turnCompleted()
        {
            this.displayText.text = Localisation.instance.getLocalisedText(this.turnCompleteTextId);
        }
        private void waitingForTurnCompletion()
        {
            this.displayText.text = Localisation.instance.getLocalisedText(this.waitingTextId);
        }
        private void turnCompleteButtonClicked()
        {
            if (this.player != null)
            {
                this.player.ClientEndTurn();
            }
        }
    }
}
