using UnityEngine;
using UnityEngine.UI;

namespace Code.Tool
{
    /// <inheritdoc />
    /// <summary>
    /// Wrote this to help keep font around the same size (in relation to screen size) on different resolutions.
    /// </summary>
    public class FontScaler : MonoBehaviour
    {
        public Text[] ChildTextObjects;
        public float DefaultHeight = 60.97894f;
        // Used in the Unity engine to tell what the current height is of the GameObject.
        public float HeightAtLaunch;
        public float DefaultFontSize = 12;
        private Text _text;
        private RectTransform _rectTransform;
        private float _lastHeight;

        /// <summary>
        /// Called whenever this MonoBehavior is activated.
        /// </summary>
        private void Awake()
        {
            _text = GetComponent<Text>();
            _rectTransform = GetComponent<RectTransform>();
            HeightAtLaunch = _rectTransform.rect.height;
        }

        /// <summary>
        /// Called every frame, checks if the screen size has changed.
        /// </summary>
        private void Update()
        {
            if (_rectTransform.rect.height != _lastHeight)
            {
                var ratio = _rectTransform.rect.height / DefaultHeight;
                _text.fontSize = (int)(DefaultFontSize * ratio);
                _lastHeight = _rectTransform.rect.height;
            
                foreach (var textObj in ChildTextObjects)
                {
                    textObj.fontSize = _text.fontSize;
                }
            }
        } 
    }
}