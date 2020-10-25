using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
namespace KoboldTools
{
    public class LazyBarAnimation : ViewController<ILazyBar>
    {
        public float lazyDuration = 0.7f;
        public Color positiveColor = Color.green;
        public Color negativeColor = Color.red;
        public MaskableGraphic barGraphic = null;
        public MaskableGraphic lazyGraphic = null;

        private float _maxBeforeAnim = 0f;
        private float _max = 0f;
        private bool _animating = false;
        private float _animationProgres = 0f;

        public override void onModelChanged()
        {
            model.changeValue.AddListener(changedValue);
            _maxBeforeAnim = model.sliderValue;
            model.onChangeValue(model.sliderValue,false);
        }

        public override void onModelRemoved()
        {
            model.changeValue.RemoveListener(changedValue);
        }

        private void adjustRects()
        {
            if (barGraphic is Image && ((Image)barGraphic).type == Image.Type.Filled)
            {
                ((Image)barGraphic).fillAmount = _max;
                ((Image)lazyGraphic).fillAmount = _maxBeforeAnim;
            }
            else
            {
                model.fillRect.anchorMax = Vector2.up + Vector2.right * _max;
                model.lazyRect.anchorMax = Vector2.up + Vector2.right * Mathf.Max(_maxBeforeAnim, _max);
                model.lazyRect.anchorMin = Vector2.right * Mathf.Min(_maxBeforeAnim, _max);
            }
        }

        private void changedValue(bool lazy)
        {
            if (!lazy)
            {
                _maxBeforeAnim = Mathf.Clamp01(_maxBeforeAnim - (_max - model.sliderValue));
                _max = model.sliderValue;
            }
            else
            {
                _max = model.sliderValue;
                _animationProgres = 0f;
                if (!_animating)
                {
                    StartCoroutine(lazyAnimation());
                }
            }
            adjustRects();
        }

        private IEnumerator lazyAnimation()
        {
            while (_animationProgres < 1f)
            {
                //is the transition good or bad?
                if (_max > _maxBeforeAnim)
                {
                    //good
                    lazyGraphic.color = Color.LerpUnclamped(positiveColor, barGraphic.color, _animationProgres);
                }
                else
                {
                    //bad
                    lazyGraphic.color = Color.LerpUnclamped(negativeColor, barGraphic.color, _animationProgres);
                }
                _animationProgres += Time.deltaTime / lazyDuration;
                yield return null;
            }

            _maxBeforeAnim = model.sliderValue;
            _animating = false;
            adjustRects();
        }

    }
}
