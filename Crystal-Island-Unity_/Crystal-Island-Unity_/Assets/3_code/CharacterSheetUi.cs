using UnityEngine;
using KoboldTools;

namespace Polymoney
{
    public class CharacterSheetUi : VCBehaviour<Player>
    {
        public GameObject personUi;
        public GameObject homeUi;
        public GameObject jobUi;
        public Transform talentUi;
        private Pool<Transform> talentUiPool;

        public void Awake()
        {
            this.talentUiPool = new Pool<Transform>(this.talentUi);
        }

        public override void onModelChanged()
        {
            this.model.PlayerStateChanged.AddListener(this.playerStateChanged);
        }
        public override void onModelRemoved()
        {
            this.model.PlayerStateChanged.RemoveListener(this.playerStateChanged);
        }
        private void playerStateChanged()
        {
            VC<Person>.addModelToAllControllers(this.model.Person, this.personUi);
            VC<Home>.addModelToAllControllers(this.model.Home, this.homeUi);
            VC<Job>.addModelToAllControllers(this.model.Job, this.jobUi);

            this.talentUiPool.releaseAll();
            foreach (var talentModel in this.model.Talents)
            {
                Transform talentUiGroup = this.talentUiPool.pop();
                talentUiGroup.gameObject.SetActive(true);
                VC<Talent>.addModelToAllControllers(talentModel, talentUiGroup.gameObject);
            }
        }
    }
}
