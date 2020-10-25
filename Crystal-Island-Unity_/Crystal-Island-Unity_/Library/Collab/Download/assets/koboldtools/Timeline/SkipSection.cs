using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
namespace KoboldTools
{
    public class SkipSection : BasicPlayableBehaviour
    {
        public ExposedReference<SkipController> skipController;
        public bool loopUntilSkipped = false;

        SkipController _skipController = null;
        bool _skipped = false;

        public override void OnGraphStart(Playable playable)
        {
            _skipController = skipController.Resolve(playable.GetGraph().GetResolver());        
            base.OnGraphStart(playable);
        }

        public override void OnGraphStop(Playable playable)
        {
            _skipController.skip.RemoveListener(skipped);
            _skipped = false;
            _skipController.onDeactivate();
            base.OnGraphStop(playable);
        }

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            if (_skipController == null)
                return;

            //Debug.Log("[SKIP] skip added");
            _skipController.skip.AddListener(skipped);
            _skipped = false;
            base.OnBehaviourPlay(playable, info);
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            if (_skipController == null)
                return;
       
            else
            {
                if (loopUntilSkipped && !_skipped)
                {
                    //reset
                    playable.GetGraph().GetRootPlayable(0).SetTime(playable.GetGraph().GetRootPlayable(0).GetTime() - playable.GetDuration() + info.deltaTime);
                    _skipController.skip.RemoveListener(skipped);
                }
                else
                {

                    _skipController.skip.RemoveListener(skipped);
                    _skipped = false;
                    _skipController.onDeactivate();                
                }

                base.OnBehaviourPause(playable, info);
            }
            
        }
        
        public override void PrepareFrame(Playable playable, FrameData info)
        {
            //Debug.Log(playable.GetGraph().GetRootPlayable(0).GetTime());       
            if (_skipController == null || playable.GetTime() <= 0f || playable.GetTime() > playable.GetDuration())
            {
                return;
            }

            if (!_skipController.controlActive)
            {
                _skipController.onActivate();
            }

            if (!loopUntilSkipped)
            {
                if (_skipped)
                {
                    //skip instantly
                    playable.GetGraph().GetRootPlayable(0).SetTime(playable.GetGraph().GetRootPlayable(0).GetTime() + playable.GetDuration() - playable.GetTime());
                    _skipped = false;
                    //playable.GetGraph().Evaluate((float)playable.GetDuration() - (float)playable.GetTime());
                }
            }

            base.PrepareFrame(playable, info);
        }

        private void skipped()
        {
            _skipped = true;
        }
    }
}
