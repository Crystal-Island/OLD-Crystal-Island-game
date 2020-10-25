using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using KoboldTools;

namespace Polymoney
{
    public class ButtonOpenTaxOfferUi : VCBehaviour<Offer>
    {
        public Button button;
        private Panel offerPopUp;

        public void Awake()
        {
            if (this.button == null)
            {
                this.button = GetComponent<Button>();
            }
        }

        public new IEnumerator Start()
        {
            while (TaxOfferPopUp.instance == null)
            {
                yield return null;
            }
            this.offerPopUp = TaxOfferPopUp.instance.GetComponent<Panel>();

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
            VC<Offer>.addModelToAllControllers(this.model, this.offerPopUp.gameObject);
            this.offerPopUp.onOpen();
        }
    }
}
