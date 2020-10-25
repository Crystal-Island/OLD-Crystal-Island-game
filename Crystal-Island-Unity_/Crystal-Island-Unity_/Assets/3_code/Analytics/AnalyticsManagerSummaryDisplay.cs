using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoboldTools;
using System.Linq;
using UnityEngine.UI;
using SA.Analytics.Google;
using UnityEngine.Networking;

namespace Polymoney
{
    public class AnalyticsManagerSummaryDisplay : VCBehaviour<AnalyticsManager>
    {
        public Transform monthDisplayTemplate;
        public Button moreInformationButton;
        public Button quitGameButton;
        public Panel soloGameOverPanel;

        private Pool<Transform> monthPool;
        private uint mayorID = 0;

        private void Start()
        {
            base.Start();
            this.onSetModel(RootAnalytics.GetAnalyticsManager());
        }

        public override void onModelChanged()
        {
            monthPool = new Pool<Transform>(monthDisplayTemplate);
            GameFlow.instance.changeState.AddListener(gameStateChanged);
            moreInformationButton.onClick.AddListener(moreInfoClicked);
            quitGameButton.onClick.AddListener(quitGameClicked);
        }

        public override void onModelRemoved()
        {
            if(GameFlow.instance != null)
                GameFlow.instance.changeState.RemoveListener(gameStateChanged);
            moreInformationButton.onClick.RemoveListener(moreInfoClicked);
            quitGameButton.onClick.RemoveListener(quitGameClicked);
        }

        private void gameStateChanged(int oldState, int newState)
        {
            if (newState == (int)PolymoneyGameFlow.FlowStates.SUMMARY)
            {
                //hide solo gameover panel
                if (soloGameOverPanel != null)
                    soloGameOverPanel.onClose();

                //get mayor id for later use
                Player mayor = Level.instance.allPlayers.FirstOrDefault(m => m.Mayor);
                if(mayor != null)
                {
                    mayorID = mayor.netId.Value;
                }

            }
            else if(newState == (int)PolymoneyGameFlow.FlowStates.QUITTING)
            {
                enterQuittingState();
            }
        }

        private void moreInfoClicked()
        {
            Application.OpenURL("http://www.polymoney.org/CI-Feedback");
        }

        private void quitGameClicked()
        {
            //ask players if they did enjoy the game
            Alert.info("didEnjoyText", new Alert.AlertParams
            {
                useLocalization = true,
                hideCloseButton = true,
                callbacks = new Alert.AlertCallback[]
                {
                    new Alert.AlertCallback{ buttonText = "didEnjoyYes", mainButton = true, callback = playerDidEnjoy },
                    new Alert.AlertCallback{ buttonText = "didEnjoyNo", callback = playerDidNotEnjoy }
                }
            });
        }

        private void playerDidEnjoy()
        {
            //set enjoyment
            Level.instance.authoritativePlayer.ClientSetEnjoyment(1);

            //end turn and inform player to wait
            Level.instance.authoritativePlayer.ClientEndTurn();
            Alert.info("waitingForPlayers", new Alert.AlertParams { hideCloseButton = true, useLocalization = true });
        }

        private void playerDidNotEnjoy()
        {
            Level.instance.authoritativePlayer.ClientSetEnjoyment(0);
            Level.instance.authoritativePlayer.ClientEndTurn();
            Alert.info("waitingForPlayers", new Alert.AlertParams { hideCloseButton = true, useLocalization = true });
        }

        private void enterQuittingState()
        {
            if (Level.instance.authoritativePlayer.isServer)
            {
                Debug.LogFormat("Has now state {0}",(PolymoneyGameFlow.FlowStates)PolymoneyGameFlow.instance.currentState);
                //this is host. ask for sending tracked data
                Alert.info("sendAnalyticsText", new Alert.AlertParams
                {
                    title = "sendAnalyticsTitle",
                    hideCloseButton = true,
                    useLocalization = true,
                    callbacks = new Alert.AlertCallback[]
                    {
                        new Alert.AlertCallback{ buttonText = "sendAnalyticsYes", mainButton = true, callback = playerSendsAnalytics },
                        new Alert.AlertCallback{ buttonText = "sendAnalyticsNo", callback = playerDoesntSendAnalytics }
                    }
                });
            }
            else
            {
                //this is no host. just quit
                QuitGame();
            }
        }

        private void playerSendsAnalytics()
        {
            //start session only if accepted
            GA_Manager.Client.StartSession();
            GA_Manager.Client.Send();

            //Player Hits
            foreach (PolymoneyPlayerData p in model.Players)
            {
                if (p.PlayerId != mayorID)
                {
                    GA_Manager.Client.CreateHit(HitType.SCREENVIEW);
                    GA_Manager.Client.SetScreenName("Summary");
                    GA_Manager.Client.SetCustomDimension(1, p.FiatAccountBalance.ToString());
                    GA_Manager.Client.SetCustomDimension(2, p.QAccountBalance.ToString());
                    GA_Manager.Client.SetCustomDimension(3, p.FiatNumberOfOffers.ToString());
                    GA_Manager.Client.SetCustomDimension(4, p.QNumberOfOffers.ToString());
                    GA_Manager.Client.SetCustomDimension(5, p.NumberOfTalents.ToString());
                    GA_Manager.Client.SetCustomDimension(6, p.FoodHealthStatus.ToString());
                    GA_Manager.Client.SetCustomDimension(7, p.RecreationHealthStatus.ToString());
                    GA_Manager.Client.SetCustomDimension(8, p.FiatDebt.ToString());
                    GA_Manager.Client.SetCustomDimension(9, p.TotalWelfare.ToString());

                    GA_Manager.Client.SetCustomMetric(1, p.FiatAccountBalance);
                    GA_Manager.Client.SetCustomMetric(2, p.QAccountBalance);
                    GA_Manager.Client.SetCustomMetric(3, p.FiatNumberOfOffers);
                    GA_Manager.Client.SetCustomMetric(4, p.QNumberOfOffers);
                    GA_Manager.Client.SetCustomMetric(5, p.NumberOfTalents);
                    GA_Manager.Client.SetCustomMetric(6, Mathf.RoundToInt(p.FoodHealthStatus*100f));
                    GA_Manager.Client.SetCustomMetric(7, Mathf.RoundToInt(p.RecreationHealthStatus*100f));
                    GA_Manager.Client.SetCustomMetric(8, p.FiatDebt);
                    GA_Manager.Client.SetCustomMetric(9, p.TotalWelfare);
                    GA_Manager.Client.Send();
                }
            }
            //City Hit
            GA_Manager.Client.CreateHit(HitType.SCREENVIEW);
            GA_Manager.Client.SetScreenName("Summary");
            GA_Manager.Client.SetCustomDimension(10, model.Game.FiatCirculationRate.ToString());
            GA_Manager.Client.SetCustomDimension(11, model.Game.QCirculationRate.ToString());
            GA_Manager.Client.SetCustomDimension(12, model.Game.CityAccountBalance.ToString());
            GA_Manager.Client.SetCustomDimension(13, model.Game.MaxDarkness.ToString());
            GA_Manager.Client.SetCustomDimension(14, model.Game.MaxBrightness.ToString());

            GA_Manager.Client.SetCustomMetric(10, Mathf.RoundToInt(model.Game.FiatCirculationRate));
            GA_Manager.Client.SetCustomMetric(11, Mathf.RoundToInt(model.Game.QCirculationRate));
            GA_Manager.Client.SetCustomMetric(12, model.Game.CityAccountBalance);
            GA_Manager.Client.SetCustomMetric(13, Mathf.RoundToInt(model.Game.MaxDarkness*100f));
            GA_Manager.Client.SetCustomMetric(14, Mathf.RoundToInt(model.Game.MaxBrightness*100f));
            GA_Manager.Client.Send();

            //end session
            GA_Manager.Client.EndSession();
            GA_Manager.Client.Send();

            StartCoroutine(quitDelayed());
        }

        private IEnumerator quitDelayed()
        {
            yield return new WaitForSeconds(0.5f);
            QuitGame();
        }

        private void playerDoesntSendAnalytics()
        {
            QuitGame();
        }

        private void QuitGame()
        {
            if (NetworkClient.active && NetworkServer.active)
            {
                PolymoneyNetworkManager.singleton.StopHost();
            }
            else
            {
                PolymoneyNetworkManager.singleton.StopClient();
            }

            if (Application.platform != RuntimePlatform.IPhonePlayer)
            {
                Application.Quit();
            }
        }
    }
}
