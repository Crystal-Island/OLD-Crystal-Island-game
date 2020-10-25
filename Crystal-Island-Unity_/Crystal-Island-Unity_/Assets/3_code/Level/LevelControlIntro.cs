using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoboldTools;
using UnityEngine.Playables;

namespace Polymoney
{

    public class LevelControlIntro : VCBehaviour<Level>
    {
        public Camera Camera;
        public float minimumDuration = 5f;
        private PlayableDirector Director;

        public void Awake()
        {
            this.Director = this.Camera.GetComponent<PlayableDirector>();
        }

        public override void onModelChanged()
        {
            this.model.onAllPlayersReady.AddListener(this.onAllPlayersReady);
        }

        public override void onModelRemoved()
        {
            this.model.onAllPlayersReady.RemoveListener(this.onAllPlayersReady);
        }

        private void onAllPlayersReady()
        {
            StartCoroutine(introRoutine());
        }

        private IEnumerator introRoutine()
        {
            Vector2 alertBigSize = new Vector2(1200, 900);

            this.Director.Play();

            //wait for animation to stop before advancing to next turn

            while (this.Director.time < minimumDuration)
            {
                yield return null;
            }

            //(Ryan) Added the faciliator role alert. Shows the intro message about the island's story to all players. Plays the audio intro clip
            //through the mayor's Ipad

            //The Narrator Audio Source
            AudioSource _Narrator = Camera.main.transform.Find("Narrator").GetComponent<AudioSource>();

            Alert.info("tutoIntroQuest", new Alert.AlertParams { useLocalization = true, title = "tutoMStoryIslandTitle", closeText = "btnLetPlay" });
            while (Alert.open)
                yield return null;
            //Show the introduction story to mayor, allow mayor to skip 
            Alert.info("tutoMStoryIsland", new Alert.AlertParams { useLocalization = true, title = "tutoMStoryIslandTitle", closeText = "btnOk", size = alertBigSize });
            if(model.authoritativePlayer.Mayor)
                //Play Audio Clip
                _Narrator.Play();
            while (Alert.open)
                yield return null;
            if(model.authoritativePlayer.Mayor)
                //Stop playing the intro AudioClip
                _Narrator.Stop();

            /*
            if (model.authoritativePlayer.Mayor)
            {
                //introduction for mayor
                Alert.info("tutoMWelcome", new Alert.AlertParams { useLocalization = true, title = "tutoMWelcomeTitle", closeText = "btnOk" });
                while (Alert.open)
                    yield return null;
                Alert.info("tutoMSurvive", new Alert.AlertParams { useLocalization = true, title = "tutoMSurviveTitle", closeText = "btnOk" });
                while (Alert.open)
                    yield return null;
            }
            else
            {
               
            }
            */
            model.authoritativePlayer.ClientEndTurn();
        }
    }
}


