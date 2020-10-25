using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;
using KoboldTools;
using KoboldTools.Logging;

namespace Polymoney
{
    public class PlayerTutorial : VCBehaviour<Player>
    {
        public Button showTutorialButton;
        public Button endTurnButton;
        public bool enableTutorial = true;
        public Sprite spriteMovement;
        public Sprite spriteTask;
        public Sprite spritePayStuff;
        public Sprite spriteMoneyBalance;
        public Sprite spriteEndTurn;
        public Sprite spriteCreateOffer;
        public Sprite spriteTalent;
        public Sprite spriteTrade;
        public Sprite spriteMayBuilding;
        public Sprite spriteMayBrokenBuilding;
        public Sprite spriteMayTax;
        public Sprite spriteMayWelfare;
        public Sprite spriteMayAufgaben;
        public Sprite spriteMayCityStatus;

        [Header("Movement Tutorial")]
        public GameObject walkArea;
        public float areaRadius = 1f;

        [Header("Incident Tutorial")]
        public PlayerGetIncident playerGetIncidentVC = null;

        [Header("Polymoney Tutorial")]
        public Marketplace ComplementaryIntroMarket;
        public OfferSetMarketPairSet OfferMarketPairSet;

        private Marketplace[] SpecialMarkets;
        private PlayableDirector TutorialButtonPlayable;

        public new void Start()
        {
            base.Start();
            this.SpecialMarkets = this.OfferMarketPairSet.offerMarketPairs.Select(e => e.marketplace).ToArray();
            this.showTutorialButton.onClick.AddListener(this.onClickShowTutorial);
            this.TutorialButtonPlayable = this.showTutorialButton.GetComponent<PlayableDirector>();
            this.showTutorialButton.gameObject.SetActive(false);
        }

        public override void onModelChanged()
        {
            walkArea.SetActive(false);

            model.OnPlayerGenerated.AddListener(onPlayerGenerated);
        }

        public override void onModelRemoved()
        {
            StopCoroutine(PlayerTutorialRoutine());
            StopCoroutine(MayorTutorialRoutine());
            model.OnPlayerGenerated.RemoveListener(onPlayerGenerated);
        }

        public void onPlayerGenerated()
        {
            if (model.isLocalPlayer && enableTutorial)
            {
                if (model.Mayor)
                {
                    StartCoroutine(MayorTutorialRoutine());
                }
                else
                {
                    StartCoroutine(PlayerTutorialRoutine());
                }
            }
        }

        private void onClickShowTutorial()
        {
            Alert.tutorial();
        }

        private IEnumerator MayorTutorialRoutine()
        {
            Vector2 alertBigSize = new Vector2(800, 600);

            RootLogger.Info(this, "started for major");

            endTurnButton.interactable = false;
            showTutorialButton.gameObject.SetActive(true);

            //MOVEMENT -----------------------------------------------------

            while (!PolymoneyGameFlow.instance.hasState((int)PolymoneyGameFlow.FlowStates.MOVEMENT_TUTO))
                yield return null;

            RootLogger.Info(this, "movement start");
            walkArea.SetActive(true);

            Alert.tutorial("tutoMoveMajor", new Alert.AlertParams { useLocalization = true, closeText = "tutoCloseAlertButton", sprite = spriteMovement });
            this.TutorialButtonPlayable.Play();

            while (Alert.open)
                yield return null;

            RootLogger.Info(this, "check movement");

            bool completedMovement = false;
            while (!completedMovement)
            {
                //check for tutorial completed
                if (model.LoadedCharacter != null && Vector3.Distance(model.LoadedCharacter.transform.position, walkArea.transform.position) < areaRadius)
                {
                    completedMovement = true;
                }
                yield return null;
            }

            Alert.tutorial("tutoMoveEndMajor", new Alert.AlertParams { useLocalization = true, closeText = "tutoCloseAlertButton" });
            this.TutorialButtonPlayable.Play();
            while (Alert.open)
                yield return null;

            model.ClientEndTurn();

            while (PolymoneyGameFlow.instance.hasState((int)PolymoneyGameFlow.FlowStates.MOVEMENT_TUTO))
                yield return null;
            this.TutorialButtonPlayable.Stop();

            walkArea.SetActive(false);

            RootLogger.Info(this, "movement end");

            RootLogger.Info(this, "maintenance start");

            while (!PolymoneyGameFlow.instance.hasState((int)PolymoneyGameFlow.FlowStates.PLAYER_TRADE))
                yield return null;

            endTurnButton.interactable = false;
            //tell major to pay maintenance
            Alert.tutorial("tutoMMaintenance", new Alert.AlertParams { title = "tutoMMaintenanceTitle", useLocalization = true, closeText = "tutoCloseAlertButton", sprite = spriteMayBuilding });
            this.TutorialButtonPlayable.Play();
            while (!model.Incidents.Any(i => i.Tags.Contains("Infrastructure") && i.State == IncidentState.APPLIED))
            {
                yield return null;
            }

            //mayor payed one
            Alert.tutorial("tutoMDump", new Alert.AlertParams { title = "tutoMDumpTitle", useLocalization = true, closeText = "tutoCloseAlertButton", sprite = spriteMayBrokenBuilding });
            this.TutorialButtonPlayable.Play();
            while(model.Incidents.Any(i=>i.Tags.Contains("Infrastructure") && i.State != IncidentState.APPLIED))
            {
                yield return null;
            }

            //mayor payed all
            Alert.tutorial("tutoMTaxes", new Alert.AlertParams { title = "tutoMTaxesTitle", useLocalization = true, closeText = "tutoCloseAlertButton", sprite = spriteMayTax });
            while (Alert.open)
                yield return null;

            Alert.tutorial("tutoMWelfare", new Alert.AlertParams { title = "tutoMWelfareTitle", useLocalization = true, closeText = "tutoCloseAlertButton", sprite = spriteMayWelfare });
            while (Alert.open)
                yield return null;

            Alert.tutorial("tutoMStoryTaxes", new Alert.AlertParams { title = "tutoMStoryTaxesTitle", useLocalization = true, closeText = "tutoCloseAlertButton", size = alertBigSize });
            this.TutorialButtonPlayable.Play();
            while (Alert.open)
                yield return null;

            while (model.Incidents.Count(i => i.State == IncidentState.UNTOUCHED) > 0)
            {
                yield return null;
            }

            Alert.tutorial("tutoMInterface", new Alert.AlertParams { title = "tutoMInterfaceTitle", useLocalization = true, closeText = "tutoCloseAlertButton", sprite = spriteMayAufgaben });
            while (Alert.open)
                yield return null;

            Alert.tutorial("tutoMLooseCondition", new Alert.AlertParams { title = "tutoMLooseConditionTitle", useLocalization = true, closeText = "tutoCloseAlertButton", sprite = spriteMayCityStatus });
            this.TutorialButtonPlayable.Play();
            while (Alert.open)
                yield return null;

            endTurnButton.interactable = true;
        }

        private IEnumerator PlayerTutorialRoutine()
        {
            Vector2 alertBigSize = new Vector2(800, 600);

            RootLogger.Info(this, "started for player");
            endTurnButton.interactable = false;
            showTutorialButton.gameObject.SetActive(true);

            // Disable markets during the tutorial.
            while (this.model.LoadedCharacter == null)
            {
                yield return null;
            }
            PlayerOpenMarket pom = this.model.LoadedCharacter.GetComponent<PlayerOpenMarket>();
            pom.enabled = false;

            //MOVEMENT -----------------------------------------------------

            while (!PolymoneyGameFlow.instance.hasState((int)PolymoneyGameFlow.FlowStates.MOVEMENT_TUTO))
                yield return null;

            RootLogger.Info(this, "movement start");
            walkArea.SetActive(true);

            Alert.tutorial("tutoMoveCitizen", new Alert.AlertParams { useLocalization = true, closeText = "tutoCloseAlertButton", sprite = spriteMovement});
            this.TutorialButtonPlayable.Play();

            while (Alert.open)
                yield return null;

            RootLogger.Info(this, "check movement");

            bool completedMovement = false;
            while (!completedMovement)
            {
                //check for tutorial completed
                if(model.LoadedCharacter != null && Vector3.Distance(model.LoadedCharacter.transform.position,walkArea.transform.position) < areaRadius)
                {
                    completedMovement = true;
                }
                yield return null;
            }

            Alert.tutorial("tutoMoveEndCitizen", new Alert.AlertParams { useLocalization = true, closeText = "tutoCloseAlertButton" });
            this.TutorialButtonPlayable.Play();
            while (Alert.open)
                yield return null;

            model.ClientEndTurn();

            while (PolymoneyGameFlow.instance.hasState((int)PolymoneyGameFlow.FlowStates.MOVEMENT_TUTO))
                yield return null;
            this.TutorialButtonPlayable.Stop();

            walkArea.SetActive(false);

            RootLogger.Info(this, "movement end");

            //GOOD INCIDENTS -----------------------------------------------------

            RootLogger.Info(this, "good incidents start");

            //set fortune wheel to good incidents
            float cacheAngleLower = playerGetIncidentVC.minTargetAngle;
            float cacheAngleUpper = playerGetIncidentVC.maxTargetAngle;
            playerGetIncidentVC.minTargetAngle = 1185f;
            playerGetIncidentVC.maxTargetAngle = 1185f;

            while (!GameFlow.instance.hasState((int)PolymoneyGameFlow.FlowStates.PLAYER_TRADE))
                yield return null;
            endTurnButton.interactable = false;
            Alert.tutorial("tutoWheelGood", new Alert.AlertParams { useLocalization = true, closeText = "tutoCloseAlertButton" });
            while (Alert.open)
                yield return null;

            RootLogger.Info(this, "good incidents end");

            //END TURN -----------------------------------------------------

            RootLogger.Info(this, "endturn start");

            Alert.tutorial("tutoEndTurn", new Alert.AlertParams { useLocalization = true, closeText = "tutoCloseAlertButton", sprite = spriteTask });
            while (Alert.open)
                yield return null;

            Alert.tutorial("tutoIncidentsBegin", new Alert.AlertParams { useLocalization = true, closeText = "tutoCloseAlertButton", sprite = spritePayStuff });
            this.TutorialButtonPlayable.Play();
            while (Alert.open)
                yield return null;

            while (model.Incidents.Count(i => i.State == IncidentState.UNTOUCHED) > 0)
            {
                yield return null;
            }

            Alert.tutorial("tutoIncidentsEnd", new Alert.AlertParams { useLocalization = true, closeText = "tutoCloseAlertButton", sprite = spriteEndTurn });
            this.TutorialButtonPlayable.Play();
            while (Alert.open)
                yield return null;
            /*
            Alert.tutorial("tutoF2", new Alert.AlertParams { useLocalization = true, closeText = "tutoCloseAlertButton", sprite = spriteEndTurn });
            this.TutorialButtonPlayable.Play();
            while (Alert.open)
                yield return null;
            Alert.tutorial("tutoF3", new Alert.AlertParams { useLocalization = true, closeText = "tutoCloseAlertButton", sprite = spriteEndTurn });
            this.TutorialButtonPlayable.Play();
            while (Alert.open)
                yield return null;
            Alert.tutorial("tutoF4", new Alert.AlertParams { useLocalization = true, closeText = "tutoCloseAlertButton", sprite = spriteEndTurn });
            this.TutorialButtonPlayable.Play();
            while (Alert.open)
                yield return null;
                */


            endTurnButton.interactable = true;

            while (PolymoneyGameFlow.instance.hasState((int)PolymoneyGameFlow.FlowStates.PLAYER_TRADE))
                yield return null;
            this.TutorialButtonPlayable.Stop();

            RootLogger.Info(this, "endturn end");

            //BAD INCIDENTS -----------------------------------------------------

            RootLogger.Info(this, "bad incidents start");



            //set fortune wheel to bad incidents
            playerGetIncidentVC.minTargetAngle = 1215f;
            playerGetIncidentVC.maxTargetAngle = 1215f;

            while (!GameFlow.instance.hasState((int)PolymoneyGameFlow.FlowStates.PLAYER_TRADE))
                yield return null;
            endTurnButton.interactable = false;
            Alert.tutorial("tutoAskHelp", new Alert.AlertParams { title = "tutoWheelBad", useLocalization = true, closeText = "tutoCloseAlertButton", sprite = spriteMoneyBalance, size = alertBigSize });
            while (Alert.open)
                yield return null;

            // Re-enable the market.
            pom.enabled = true;

            Alert.tutorial("tutoCreateOffer", new Alert.AlertParams { title = "tutoOpenTrade", useLocalization = true, closeText = "tutoCloseAlertButton", sprite = spriteCreateOffer });
            this.TutorialButtonPlayable.Play();
            while (Alert.open)
                yield return null;

            while (model.WatchedMarket != model.OwnMarketplace)
                yield return null;

            Alert.tutorial("tutoCreateOffer2", new Alert.AlertParams { title = "tutoOpenTrade2", useLocalization = true, closeText = "tutoCloseAlertButton",sprite = spriteTalent });
            this.TutorialButtonPlayable.Play();
            while (Alert.open)
                yield return null;

            while (model.WatchedMarket != null)
                yield return null;

            Alert.tutorial("tutoTradeFriend", new Alert.AlertParams { title = "tutoOpenTrade3", useLocalization = true, closeText = "tutoCloseAlertButton", sprite = spriteTrade });
            this.TutorialButtonPlayable.Play();
            while (Alert.open)
                yield return null;

            Alert.tutorial("tutoBuy", new Alert.AlertParams { title = "tutoOpenTrade4", useLocalization = true, closeText = "tutoCloseAlertButton" });
            this.TutorialButtonPlayable.Play();
            while (Alert.open)
                yield return null;

            while (model.WatchedMarket == null || model.WatchedMarket == model.OwnMarketplace)
                yield return null;

            Alert.tutorial("tutoEndSecond", new Alert.AlertParams { title = "tutoOpenTrade4", useLocalization = true, closeText = "tutoCloseAlertButton" });
            while (Alert.open)
                yield return null;

            while (model.WatchedMarket != null)
                yield return null;

            //set fortune wheel to default
            playerGetIncidentVC.minTargetAngle = cacheAngleLower;
            playerGetIncidentVC.maxTargetAngle = cacheAngleUpper;

            RootLogger.Info(this, "bad incidents end");

            endTurnButton.interactable = true;
        }
    }
}
