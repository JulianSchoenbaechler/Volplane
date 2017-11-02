namespace Volplane.Examples
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Volplane;

    [RequireComponent(typeof(Collider2D))]
    public class Racket : VolplaneBehaviour
    {
        // Public variables
        public int playerNumber = 0;        // Player id from which the input should be taken
        public float speed = 1f;            // Speed of the racket

        // Private variables
        private float boundsHeight;         // Collider bounds of this racket
        private GameController main;


        /// <summary>
        /// 'MonoBehaviour.Start()' method from Unity
        /// Start is called on the frame when a script is enabled just before any of the Update methods
        /// is called the first time.
        /// </summary>
        private void Start()
        {
            // Reference to the Game Controller
            main = GameObject.FindWithTag("GameController").GetComponent<GameController>();

            // Get the height from the bounds from this collider
            // Divide by 2 to get the relative height from the gameobjects origin
            boundsHeight = GetComponent<Collider2D>().bounds.size.y / 2f;
        }

        /// <summary>
        /// 'MonoBehaviour.Update()' method from Unity
        /// Update is called every frame, if the MonoBehaviour is enabled.
        /// </summary>
        private void Update()
        {
            // Get button input from controller (name 'buttonUp')
            if(VInput.GetButton(playerNumber, "buttonUp"))
            {
                // Move racket up
                if(transform.position.y < 4f)
                    transform.Translate(0f, 0.1f * speed, 0f);
            }

            // Get button input from controller (name 'buttonDown')
            if(VInput.GetButton(playerNumber, "buttonDown"))
            {
                // Move racket down
                if(transform.position.y > -4f)
                    transform.Translate(0f, -0.1f * speed, 0f);
            }
        }

        /// <summary>
        /// 'MonoBehaviour.OnCollisionEnter2D()' method from Unity
        /// Sent when an incoming collider makes contact with this object's collider (2D physics only).
        /// </summary>
        /// <param name="collision">Collision.</param>
        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Determine where the ball has hit this racket
            float hitPosition = (collision.transform.position.y - transform.position.y) / boundsHeight;
            Vector2 newDir;

            // Reflect ball -> invert the direction it came from
            if(collision.transform.position.x > 0f)
            {
                newDir = new Vector2(-1f, hitPosition);
            }
            else
            {
                newDir = new Vector2(1f, hitPosition);
            }

            // Slightly increase ball speed
            main.ballSpeed += 0.2f;

            // Apply new movement direction to ball
            collision.transform.GetComponent<Rigidbody2D>().velocity = newDir.normalized * main.ballSpeed;
        }
    }
}
