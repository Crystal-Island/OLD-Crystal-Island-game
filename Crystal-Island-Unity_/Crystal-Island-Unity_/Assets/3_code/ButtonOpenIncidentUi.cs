using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using KoboldTools;

namespace Polymoney
{
    public class ButtonOpenIncidentUi : VCBehaviour<Incident>
    {
        public Button button;
        private Panel incidentPopUp;

        public void Awake()
        {
            if (this.button == null)
            {
                this.button = GetComponent<Button>();
            }
        }

        public new IEnumerator Start()
        {
            while (IncidentPopUp.instance == null)
            {
                yield return null;
            }
            this.incidentPopUp = IncidentPopUp.instance.GetComponent<Panel>();

            base.Start();
        }

        public override void onModelChanged()
        {
            this.button.onClick.AddListener(this.onClick);
        }

        public override void onModelRemoved()
        {
            this.button.onClick.RemoveListener(this.onClick);
        }

        private void onClick()
        {
            VC<Incident>.addModelToAllControllers(this.model, this.incidentPopUp.gameObject);
            this.incidentPopUp.onOpen();
        }
    }
}
