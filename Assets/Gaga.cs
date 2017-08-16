using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Volplane;

public class Gaga : VolplaneBehaviour
{
    VolplaneAgent agent;

    void Start()
    {
        agent = VolplaneController.Main;

        //VolplaneAgent.GetPlayer(0);
    }
}
