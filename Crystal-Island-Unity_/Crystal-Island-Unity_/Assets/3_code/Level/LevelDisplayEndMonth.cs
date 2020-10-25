using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoboldTools;
using UnityEngine.UI;
using System.Linq;
using KoboldTools.Logging;

namespace Polymoney
{

    public class LevelDisplayEndMonth : VCBehaviour<Level>, IFlowCondition
    {
        public Button nextButton;
        public Button endTurnButton;

        public Panel cityOverviewPanel;
        public Panel cityCritcalPanel;
        public Panel playerCriticalPanel;
        public Panel playerGoodbyePanel;
        public Sprite gameOverSprite;

        public Panel gameOverSoloPanel;
        public GameObject gameOverNew;

        public Transform criticalPlayerTemplate;
        public Transform gameOverPlayerTemplate;

        private Pool<Transform> criticalPlayerPool = null;
        private Pool<Transform> gameOverPlayerPool = null;
        private float currentMonth = 0f;
        Player mayorPlayer = null;

        public bool conditionMet
        {
            get
            {        
                return mayorPlayer != null && mayorPlayer.GameOver;
            }
        }

        public override void onModelChanged()
        {
            if(criticalPlayerPool == null)
                criticalPlayerPool = new Pool<Transform>(criticalPlayerTemplate);
            if (gameOverPlayerPool == null)
                gameOverPlayerPool = new Pool<Transform>(gameOverPlayerTemplate);

            GameFlow.instance.changeState.AddListener(flowStateChanged);
            GameFlow.instance.addEnterCondition((int)PolymoneyGameFlow.FlowStates.GAMEOVER, this);
            currentMonth = model.months;
        }

        public override void onModelRemoved()
        {
            if(GameFlow.instance != null)
                GameFlow.instance.changeState.RemoveListener(flowStateChanged);
        }

        private void flowStateChanged(int oldState, int newState)
        {
            if (newState == (int)PolymoneyGameFlow.FlowStates.END_MONTH_DISPLAY)
            {
                //initialize new end month
                mayorPlayer = model.allPlayers.FirstOrDefault(p => p.Mayor);
                StopCoroutine(displayRoutine());
                StartCoroutine(displayRoutine());
            }
            if(oldState == (int)PolymoneyGameFlow.FlowStates.END_MONTH_DISPLAY)
            {
                //reset end month
                cityOverviewPanel.onOpen();
                cityCritcalPanel.onClose();
                playerCriticalPanel.onClose();
                playerGoodbyePanel.onClose();
                nextButton.gameObject.SetActive(true);
                endTurnButton.gameObject.SetActive(false);
                if (newState != (int)PolymoneyGameFlow.FlowStates.GAMEOVER && newState != (int)PolymoneyGameFlow.FlowStates.END)
                {
                    updateGameOverPlayer();
                }
                else
                {
                    gameOverSoloPanel.onClose();
                }
            }
        }

        private IEnumerator displayRoutine()
        {

            nextButton.gameObject.SetActive(false);
            endTurnButton.gameObject.SetActive(false);

            //update lists
            criticalPlayerPool.releaseAll();
            gameOverPlayerPool.releaseAll();

            foreach (Player p in model.allPlayers.Where(allP => model.criticalPlayers.Contains(allP.netId.Value)))
            {
                Transform element = criticalPlayerPool.pop();
                VC<Player>.addModelToAllControllers(p, element.gameObject, true);
                element.gameObject.SetActive(true);
            }

            foreach (Player p in model.allPlayers.Where(allP => model.gameOverPlayers.Contains(allP.netId.Value)))
            {
                Transform element = gameOverPlayerPool.pop();
                VC<Player>.addModelToAllControllers(p, element.gameObject, true);
                element.gameObject.SetActive(true);
            }

            nextButton.gameObject.SetActive(true);

            //show overview anyways
            if (model.criticalPlayers.Count <= 0 && model.gameOverPlayers.Count <= 0 )
            {
                //enable endturn button
                enableEndturnButton();
            }
            else
            {
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(() => cityOverviewPanel.onClose());
            }

            while (cityOverviewPanel.isOpen && PolymoneyGameFlow.instance.currentState == (int)PolymoneyGameFlow.FlowStates.END_MONTH_DISPLAY)
                yield return null;

            //check if city is critical
            if (mayorPlayer != null && model.criticalPlayers.Contains(mayorPlayer.netId.Value))
            {
                if (model.criticalPlayers.Count <= 1 && model.gameOverPlayers.Count <= 0)
                {
                    //enable endturn button
                    enableEndturnButton();
                }
                else
                {
                    nextButton.onClick.RemoveAllListeners();
                    nextButton.onClick.AddListener(() => cityCritcalPanel.onClose());
                }
                cityCritcalPanel.onOpen();
                while (cityCritcalPanel.isOpen && PolymoneyGameFlow.instance.currentState == (int)PolymoneyGameFlow.FlowStates.END_MONTH_DISPLAY)
                    yield return null;
            }

            //check if any players are critical
            if (model.criticalPlayers.Count > 0 && PolymoneyGameFlow.instance.currentState == (int)PolymoneyGameFlow.FlowStates.END_MONTH_DISPLAY)
            {
                if (model.gameOverPlayers.Count <= 0)
                {
                    //enable endturn button
                    enableEndturnButton();
                }
                else
                {
                    nextButton.onClick.RemoveAllListeners();
                    nextButton.onClick.AddListener(() => playerCriticalPanel.onClose());
                }

                playerCriticalPanel.onOpen();
                while (playerCriticalPanel.isOpen && PolymoneyGameFlow.instance.currentState == (int)PolymoneyGameFlow.FlowStates.END_MONTH_DISPLAY)
                    yield return null;
            }

            //check if any players are gameover
            if (model.gameOverPlayers.Count > 0 && PolymoneyGameFlow.instance.currentState == (int)PolymoneyGameFlow.FlowStates.END_MONTH_DISPLAY)
            {
                //enable endturn button
                enableEndturnButton();
                playerGoodbyePanel.onOpen();
            }

            currentMonth = model.months;
            updateGameOverPlayer();
        }

        private void enableEndturnButton()
        {
            nextButton.gameObject.SetActive(false);
            endTurnButton.gameObject.SetActive(true);
            RootLogger.Debug(this, "[ENDMONTH] Enable Endturn");
        }

        private void updateGameOverPlayer()
        {
            for(int i=model.gameOverPlayers.Count-1; i >= 0; i--)
            {
                Player p = model.allPlayers.FirstOrDefault(pl => pl.netId.Value == model.gameOverPlayers[i]);
                if (p.isLocalPlayer)
                {
                    RootLogger.Debug(this, "Player {0} went gameover.", p.name);
                    if (!p.Mayor)
                    {
                        //Alert.info("Better Luck next Time! You will have to wait until the others complete their game.", new Alert.AlertParams { title = "Game Over", hideCloseButton = true, sprite = gameOverSprite });
                        gameOverSoloPanel.onOpen();

                        //New Game Over Screen
                        //gameOverNew.SetActive(true);
                    }
                    p.ClientSetGameOver(true);
                }
            }
        }

        
    }
}
