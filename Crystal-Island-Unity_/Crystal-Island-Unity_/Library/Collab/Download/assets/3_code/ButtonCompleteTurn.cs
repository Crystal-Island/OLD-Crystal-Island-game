using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using KoboldTools;

namespace Polymoney
{
    public class ButtonCompleteTurn : MonoBehaviour
    {
        public Panel incidentSheet;
        public Button completeTurn;
        public Text buttonText;
        public string completeTurnTextId = "turnOk";
        public string waitingTextId = "turnWait";
        private Level level;
        private Player player;

        public IEnumerator Start()
        {
            while (Level.instance == null)
            {
                yield return null;
            }

            this.level = Level.instance;
            this.level.onAuthoritativePlayerChanged.AddListener(this.onAuthoritativePlayerChanged);
            this.onAuthoritativePlayerChanged();

            if (this.completeTurn == null)
            {
                this.completeTurn = GetComponent<Button>();
            }
            this.completeTurn.onClick.AddListener(this.onClick);
        }

        private void onAuthoritativePlayerChanged()
        {
            if (this.player != null)
            {
                this.player.OnTurnCompleted.RemoveListener(this.onTurnCompleted);
                this.player.OnWaitingForTurnCompletion.RemoveListener(this.onWaitingForTurnCompletion);
            }

            if (Level.instance.authoritativePlayer != null)
            {
                this.player = Level.instance.authoritativePlayer;
                this.player.OnTurnCompleted.AddListener(this.onTurnCompleted);
                this.player.OnWaitingForTurnCompletion.AddListener(this.onWaitingForTurnCompletion);
            }
        }

        private void onTurnCompleted()
        {
            this.buttonText.text = Localisation.instance.getLocalisedText(this.completeTurnTextId);
        }

        private void onWaitingForTurnCompletion()
        {
            this.buttonText.text = Localisation.instance.getLocalisedText(this.waitingTextId);
        }

        private void onClick()
        {
            if (this.player != null)
            {
                if (this.player.Incidents.Count(e => e.State == IncidentState.UNTOUCHED && !e.Ignorable) > 0)
                {
                    this.incidentSheet.onOpen();
                }
                else
                {
                    this.player.ClientEndTurn();
                }
            }
        }
    }
}
