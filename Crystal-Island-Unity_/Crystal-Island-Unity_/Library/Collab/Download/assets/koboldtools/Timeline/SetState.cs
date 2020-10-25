using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
namespace KoboldTools
{
    public class SetState : BasicPlayableBehaviour
    {
        [EnumFlag]
        public Gamestates gameStates;
        public bool addThis = false;
        public bool removeThis = false;
        public bool revertAfter = false;

        private int stateCache = 0;



        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            if (Gamestate.instance == null)
                return;

            if (revertAfter)
            {
                stateCache = Gamestate.instance.currentState;
            }

            if (addThis)
            {
                Gamestate.instance.addState((int)gameStates);
            }
            else if (removeThis)
            {
                Gamestate.instance.removeState((int)gameStates);
            }
            else
            {
                Gamestate.instance.onChangeState((int)gameStates);
            }   
        }



        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            if (Gamestate.instance == null)
                return;

            if (playable.GetTime() <= 0)
                return;
            if (revertAfter)
                Gamestate.instance.onChangeState(stateCache);
        }

        /*public override void OnGraphStop(Playable playable)
        {
            if (revertAfter)
                Gamestate.instance.onChangeState(stateCache);
        }*/

    }
}
