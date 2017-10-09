namespace Volplane.Examples
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Volplane;

    public class Player : VolplaneBehaviour
    {
        public GameObject crown;
        public GameObject body;
        public int playerId;

        private PlayerColors currentColor;
        private Renderer objectRenderer;
        private ElementProperties elementProp;

        private enum PlayerColors
        {
            Red = 0,
            Green,
            Blue,
            Yellow,
            White,
            Black
        }

        public void SetMaster(bool value)
        {
            crown.SetActive(value);
        }

        private void Start()
        {
            objectRenderer = body.GetComponent<Renderer>();
            elementProp = new ElementProperties();

            ApplyColor(PlayerColors.Red);
        }

        private void Update()
        {
            if(VInput.GetButtonDown(playerId, "colorChangeLeft"))
            {
                if(currentColor == PlayerColors.Red)
                    currentColor = PlayerColors.Black;
                else
                    currentColor--;

                ApplyColor(currentColor);
            }

            if(VInput.GetButtonDown(playerId, "colorChangeRight"))
            {
                if(currentColor == PlayerColors.Black)
                    currentColor = PlayerColors.Red;
                else
                    currentColor++;

                ApplyColor(currentColor);
            }
        }

        private void ApplyColor(PlayerColors color)
        {
            Color newColor;

            switch(color)
            {
                case PlayerColors.Red:
                    newColor = new Color(231f / 256f,
                                         76f / 256f,
                                         60f / 256f);
                    break;

                case PlayerColors.Green:
                    newColor = new Color(46f / 256f,
                                         204f / 256f,
                                         113f / 256f);
                    break;

                case PlayerColors.Blue:
                    newColor = new Color(52f / 256f,
                                         152f / 256f,
                                         219f / 256f);
                    break;

                case PlayerColors.Yellow:
                    newColor = new Color(255f / 256f,
                                         235f / 256f,
                                         95f / 256f);
                    break;

                case PlayerColors.White:
                    newColor = new Color(189f / 256f,
                                         195f / 256f,
                                         199f / 256f);
                    break;

                default:
                    newColor = new Color(21f / 256f,
                                         31f / 256f,
                                         28f / 256f);
                    break;
                    
            }

            objectRenderer.material.color = newColor;
            elementProp.FontColor = newColor;
            elementProp.Text = color.ToString("G");
            GetPlayer(playerId).ChangeElementProperties("colorText", elementProp);
        }
    }
}
