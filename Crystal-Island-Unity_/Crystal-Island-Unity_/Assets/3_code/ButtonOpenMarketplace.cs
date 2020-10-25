using UnityEngine.UI;
using KoboldTools;

namespace Polymoney
{
    public class ButtonOpenMarketplace : VCBehaviour<Player>
    {
        public Marketplace market;
        private Button button;

        public void Awake()
        {
            if (this.button == null)
            {
                this.button = GetComponent<Button>();
            }
        }

        public override void onModelChanged()
        {
            if (this.market == null)
            {
                this.market = this.model.OwnMarketplace;
            }
            this.button.onClick.AddListener(this.onClick);
        }

        public override void onModelRemoved()
        {
            this.button.onClick.RemoveListener(this.onClick);
        }

        private void onClick()
        {
            if (this.model != null && this.market != null)
            {
                Level.instance.authoritativePlayer.WatchedMarket = this.market;
            }
        }
    }
}
