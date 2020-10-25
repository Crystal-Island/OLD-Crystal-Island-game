using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;
using KoboldTools;
using Cinemachine;

namespace Polymoney
{

    public class LevelControlQIntroduction : VCBehaviour<Level>, IFlowCondition
    {
        public AlertSkin alertSkin = null;
        public Marketplace ComplementaryIntroMarket;
        public Sprite spriteQBuilding;
        public Sprite spriteQMay;
        public GameObject highlightCamera;
        public int highlightPriority = 2000;
        public GameObject buildingIcon;

        public bool conditionMet
        {
            get
            {
                Player major = model.allPlayers.FirstOrDefault(p => p.Mayor);
                int balance = 0;
                if (major != null)
                {
                    //Q is introduced when major has less than base starting balance AND it is not already introduced
                    major.Pocket.TryGetBalance(Currency.FIAT, out balance);
                    if (!model.PolymoneyIntroduced && ((balance < model.MayorBaseStartingMoney && !Options_Controller.manualIntroWater) 
                        || (Options_Controller.manualIntroWater && Level.instance.months >= Options_Controller.waterIntroTurn)))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        public override void onModelChanged()
        {
            PolymoneyGameFlow.instance.changeState.AddListener(changedFlowState);
            PolymoneyGameFlow.instance.addEnterCondition((int)PolymoneyGameFlow.FlowStates.Q_INTRODUCTION, this);
        }

        public override void onModelRemoved()
        {
            if(PolymoneyGameFlow.instance != null)
                PolymoneyGameFlow.instance.changeState.RemoveListener(changedFlowState);
        }

        private void changedFlowState(int oldState, int newState)
        {
            if(newState == (int)PolymoneyGameFlow.FlowStates.Q_INTRODUCTION)
            {
                //handle Q introduction
                model.PolymoneyIntroduced = true;
                StartCoroutine(introductionRoutine());
            }
            if(oldState == (int)PolymoneyGameFlow.FlowStates.Q_INTRODUCTION)
            {
                Alert.close();
            }
            if(newState == (int)PolymoneyGameFlow.FlowStates.Q_MARKET)
            {
                StartCoroutine(marketRoutine());
            }
            if (oldState == (int)PolymoneyGameFlow.FlowStates.Q_MARKET)
            {
                Alert.close();
            }
        }

        private IEnumerator introductionRoutine()
        {
            //Vector2 alertSize = new Vector2(Screen.width, Screen.height);
            Vector2 alertSize = new Vector2(800, 600);
            Vector2 alertBigSize = new Vector2(1200, 900);

            /*
            All players recieve facilitator messages
            Alert.info("tutoF1", new Alert.AlertParams { title = "tutoFTitle", useLocalization = true, closeText = "btnOk", size = alertSize }, alertSkin);
            while (Alert.open)
                yield return null;
            */
            Alert.info("tutoQBegin", new Alert.AlertParams { title = "tutoQBeginHeader", useLocalization = true, hideCloseButton = false, closeText = "btnOk" }, alertSkin);
            while (Alert.open)
                yield return null;
            bool repeat = true;
            while (repeat)
            {
                repeat = false;
                /*
                Alert.tutorial("tutoMQWhat", new Alert.AlertParams { title = "tutoMQWhatTitle", useLocalization = true, closeText = "btnOk", size = alertBigSize }, alertSkin);
                while (Alert.open)
                    yield return null;
                    */
                Alert.tutorial("tutoMQWhat", new Alert.AlertParams { title = "tutoMQWhatTitle", useLocalization = true, closeText = "everyoneReady", size = alertBigSize, sprite = spriteQBuilding, callbacks = new Alert.AlertCallback[] { new Alert.AlertCallback { buttonText = "tellMeAgain", callback = () => { repeat = true; Alert.close(); } } } }, alertSkin);
                while (Alert.open)
                    yield return null;
            }
            model.authoritativePlayer.ClientEndTurn();
        }

        private IEnumerator marketRoutine()
        {
            if (model.authoritativePlayer.Mayor)
            {
                Alert.info("tutoMQStory", new Alert.AlertParams { title = "tutoMQStoryTitle", useLocalization = true, hideCloseButton = true, closeText = "btnOk" }, alertSkin);
                model.authoritativePlayer.ClientEndTurn();
            }
            else
            {
                Alert.tutorial("tutoMQStory", new Alert.AlertParams { title = "tutoMQStoryTitle", useLocalization = true, hideCloseButton = false, closeText = "btnOk" }, alertSkin);
                while (Alert.open)
                    yield return null;

                model.authoritativePlayer.WatchedMarket = this.ComplementaryIntroMarket;
                while (model.authoritativePlayer.WatchedMarket != null)
                {
                    //player did not close his special building yet
                    if (model.authoritativePlayer.OwnedMarketplaces.Count() > 0)
                    {
                        //player has bought a marketplace, close the market
                        model.authoritativePlayer.WatchedMarket = null;
                    }
                    yield return null;
                }

                if (model.authoritativePlayer.OwnedMarketplaces.Count > 0)
                {
                    // tell players to click on their building
                    Building building = Level.instance.Buildings.FirstOrDefault(b => model.authoritativePlayer.OwnedMarketplaces[0] == b.Marketplace);
                    CinemachineVirtualCamera cam = this.highlightCamera.GetComponentInChildren<CinemachineVirtualCamera>();
                    if (building != null)
                    {
                        this.highlightCamera.transform.position = building.transform.position;
                        if(cam != null)
                        {
                            cam.Priority = highlightPriority;
                        }
                    }

                    yield return new WaitForSeconds(0.5f);

                    if (building != null && building.Marketplace.seller.Equals(model.authoritativePlayer))
                    {
                        Alert.tutorial("tutoQSearchBuilding", new Alert.AlertParams { useLocalization = true, closeText = "btnOk", sprite = spriteQBuilding });
                        while (Alert.open)
                            yield return null;
                    }

                    while (
                        building != null
                        && building.Marketplace.seller.Equals(model.authoritativePlayer)
                        && (model.authoritativePlayer.WatchedMarket == null
                            || (model.authoritativePlayer.WatchedMarket == model.authoritativePlayer.OwnMarketplace 
                                || !model.authoritativePlayer.WatchedMarket.seller.Equals(model.authoritativePlayer))))
                    {
                        //player did not open his special building yet
                        yield return null;
                    }

                    if (model.authoritativePlayer.WatchedMarket != null)
                    {
                        Alert.tutorial("tutoQInvest", new Alert.AlertParams { useLocalization = true, title = "tutoQInvestHeader", closeText = "tutoCloseAlertButton" });

                        while (model.authoritativePlayer.WatchedMarket != null)
                        {
                            yield return null;
                        }

                        if (model.authoritativePlayer.OwnedMarketplaces.All(m => m.offers.Count == 0))
                        {
                            Alert.tutorial("tutoQNoOffer", new Alert.AlertParams { useLocalization = true, title = "tutoQNoOfferHeader", closeText = "tutoCloseAlertButton" });
                            while (Alert.open)
                            {
                                yield return null;
                            }
                        }
                    }

                    yield return new WaitForSeconds(0.5f);
                    if(cam != null)
                    {
                        cam.Priority = 0;
                    }

                }
                Alert.info("turnWaitOthers", new Alert.AlertParams { useLocalization = true, hideCloseButton = true });

                buildingIcon.SetActive(true);

                model.authoritativePlayer.ClientEndTurn();

            }
        }
    }
}
