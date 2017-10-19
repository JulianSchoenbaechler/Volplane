namespace Volplane.Examples
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using Volplane;

    public class Player : VolplaneBehaviour
    {
        // Variables

        [SerializeField] protected GameObject crown;    // Crown gameobject
        [SerializeField] protected GameObject body;     // Main body gameobject
        [SerializeField] public int playerId;           // The assigned player identifier for this object

        protected Text playerText;                      // Reference to the text object used for the nickname
        protected PlayerColors currentColor;            // Current selected player color
        protected Renderer objectRenderer;              // Renderer component of the players body
        protected ElementProperties elementProp;        // Element properties for the 'colorText' element on the controller of this player

        /// <summary>
        /// Player colors.
        /// </summary>
        protected enum PlayerColors
        {
            Red = 0,
            Green,
            Blue,
            Yellow,
            White,
            Black
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is ready.
        /// </summary>
        /// <value><c>true</c> if this instance is ready; otherwise, <c>false</c>.</value>
        public bool IsReady { get; protected set; }

        /// <summary>
        /// Gets or sets the text object of this players slot.
        /// </summary>
        /// <value>The player text.</value>
        public Text PlayerText
        {
            get { return playerText; }
            set
            {
                playerText = value;                                             // Set reference
                playerText.gameObject.SetActive(true);                          // Enable this gameobject in the scene
                playerText.text = GetPlayer(playerId).Nickname + "\n(Waiting)"; // Display the players nickname and status
            }
        }

        /// <summary>
        /// Enables this players crown indicating that it is now game master.
        /// </summary>
        /// <param name="value">If set to <c>true</c> enable crown; otherwise disable it.</param>
        public void SetMaster(bool value)
        {
            crown.SetActive(value);
        }

        /// <summary>
        /// 'MonoBehaviour.Start()' method from Unity
        /// Start is called on the frame when a script is enabled just before any of the Update methods
        /// is called the first time.
        /// </summary>
        private void Start()
        {
            objectRenderer = body.GetComponent<Renderer>();     // Get renderer component from players body
            elementProp = new ElementProperties();              // Create new element properties instance

            ApplyColor(PlayerColors.Red);                       // Applying standard color
        }

        /// <summary>
        /// 'MonoBehaviour.Update()' method from Unity
        /// Update is called every frame, if the MonoBehaviour is enabled.
        /// </summary>
        private void Update()
        {
            // Switch through the possible player colors (left)
            if(VInput.GetButtonDown(playerId, "colorChangeLeft"))
            {
                if(currentColor == PlayerColors.Red)
                    currentColor = PlayerColors.Black;
                else
                    currentColor--;

                ApplyColor(currentColor);
            }

            // Switch through the possible player colors (right)
            if(VInput.GetButtonDown(playerId, "colorChangeRight"))
            {
                if(currentColor == PlayerColors.Black)
                    currentColor = PlayerColors.Red;
                else
                    currentColor++;

                ApplyColor(currentColor);
            }

            // Toggle this players ready state
            if(VInput.GetButtonDown(playerId, "submitButton"))
            {
                // Get the Volplane player object from this player
                VPlayer thisDevice = GetPlayer(playerId);

                if(!IsReady)
                {
                    // This player is ready!

                    // Hide elements for choosing colors and indicate that the player is ready
                    thisDevice.HideElement("colorChangeLeft");
                    thisDevice.HideElement("colorChangeRight");
                    thisDevice.HideElement("colorText");
                    thisDevice.ChangeElementText("infoText", "Waiting for game to start...");
                    thisDevice.ChangeElementText("submitButton", "Cancel");

                    if(playerText != null)
                        playerText.text = GetPlayer(playerId).Nickname + "\n(Ready)";
                    
                    IsReady = true;

                    // Try to start the game by calling the lobbys 'StartGame()' method
                    GameObject.FindWithTag("Lobby").GetComponent<Lobby>().StartGame();
                }
                else
                {
                    // This player is still not ready after all...

                    // Give the player ability to choose its color again
                    thisDevice.ShowElement("colorChangeLeft");
                    thisDevice.ShowElement("colorChangeRight");
                    thisDevice.ShowElement("colorText");
                    thisDevice.ChangeElementText("infoText", "Choose your color!");
                    thisDevice.ChangeElementText("submitButton", "Ready");

                    if(playerText != null)
                        playerText.text = GetPlayer(playerId).Nickname + "\n(Waiting)";
                    
                    IsReady = false;
                }
            }

            // Players can move when game starts
            // -> the element 'joystick' lies on the game view
            transform.Translate(new Vector3(
                VInput.GetAxis(playerId, "joystick", VInput.Axis.Horizontal),
                0f,
                VInput.GetAxis(playerId, "joystick", VInput.Axis.Vertical)
            ).normalized * 0.1f);
        }

        /// <summary>
        /// Applies the color on the player object in the scene and on the controller 'colorText' element.
        /// </summary>
        /// <param name="color">Selected color.</param>
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

            objectRenderer.material.color = newColor;                               // Apply color on material
            elementProp.FontColor = newColor;                                       // Change fontcolor for element
            elementProp.Text = color.ToString("G");                                 // Change text of element to font name
            GetPlayer(playerId).ChangeElementProperties("colorText", elementProp);  // Apply properties for element
        }
    }
}
