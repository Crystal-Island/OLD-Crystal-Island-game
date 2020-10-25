using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoboldTools;
using UnityEngine.UI;
using UnityEngine.Playables;

namespace Polymoney
{
    public class LevelControlPlayerIntroduction : VCBehaviour<Level>
    {
        public Panel characterSheetPanel;
        public Button characterSheetCloseButton;
        public PlayableDirector intoIntroductionTimeline;
        public PlayableDirector intoGameTimeline;

        public override void onModelChanged()
        {
            PolymoneyGameFlow.instance.changeState.AddListener(gameStateChanged);
        }

        public override void onModelRemoved()
        {
            if(PolymoneyGameFlow.instance != null)
                PolymoneyGameFlow.instance.changeState.RemoveListener(gameStateChanged);
        }

        private void gameStateChanged(int oldState, int newState)
        {
            if(newState == (int)PolymoneyGameFlow.FlowStates.PLAYER_INTRODUCTION)
            {
                Alert.close();
                characterSheetCloseButton.interactable = false;
                StopCoroutine(introductionRoutine());
                StartCoroutine(introductionRoutine());
                intoIntroductionTimeline.Play();
            }
            if(oldState == (int)PolymoneyGameFlow.FlowStates.PLAYER_INTRODUCTION)
            {
                characterSheetCloseButton.interactable = true;
                characterSheetPanel.onClose();
                intoGameTimeline.Play();
            }
        }

        private IEnumerator introductionRoutine()
        {
            Vector2 alertBigSize = new Vector2(800, 600);

            if (model.authoritativePlayer.Mayor)
            {
                //introduction for mayor
                Alert.info("tutoMWelcome", new Alert.AlertParams { useLocalization = true, title = "tutoMWelcomeTitle", closeText = "btnOk", size = alertBigSize });
                while (Alert.open)
                    yield return null;

                //Round of introductions!
                Alert.info("tutoMIntroduce", new Alert.AlertParams { useLocalization = true, title = "tutoMIntroduceTitle", closeText = "everyoneReady" });
                while (Alert.open)
                    yield return null;

                Alert.info("tutoMIntroduceConfirm", new Alert.AlertParams { useLocalization = true, title = "tutoMIntroduceTitle", closeText = "everyoneReady" });
                while (Alert.open)
                    yield return null;

                model.authoritativePlayer.ClientEndTurn();
            }
            else
            {
                //introduction for players
                Alert.info("tutoPIntro1", new Alert.AlertParams { useLocalization = true, title = "tutoPWelcomeTitle", closeText = "btnOk", size = alertBigSize });
                while (Alert.open)
                    yield return null;
                Alert.info("tutoPIntro4", new Alert.AlertParams { useLocalization = true, title = "tutoMIntroduceTitle", closeText = "btnOk" });
                while (Alert.open)
                    yield return null;
                characterSheetPanel.onOpen();
                model.authoritativePlayer.ClientEndTurn();
            }
        }
    }
}
