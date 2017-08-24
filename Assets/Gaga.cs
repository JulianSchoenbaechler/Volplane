using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using Volplane;

public class Gaga : VolplaneBehaviour
{
    VPlayer player;
    bool flag = false;

    void Start()
    {
        VolplaneController.AirConsole.OnConnect += Connect;
        VolplaneAgent.StandardView = "view2";
    }

    void Connect(int id)
    {
        player = VolplaneController.Main.GetPlayer(0);

        Debug.LogFormat("View: {0:G}", player.CurrentView);
        Debug.LogFormat("Browser: {0:F}", player.IsUsingBrowser);
        Debug.LogFormat("Connected: {0:F}", player.IsConnected);
        Debug.LogFormat("Hero: {0:F}", player.IsHero);
        Debug.LogFormat("Slow Connection: {0:F}", player.HasSlowConnection);
        Debug.LogFormat("UID: {0:G}", player.UID);
        Debug.LogFormat("Nickname: {0:G}", player.Nickname);

        if(player.CurrentView != "view3")
            VolplaneController.Main.ChangeView(0, "view1");
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.U))
            player.ChangeView("view2");

        if(Input.GetKeyDown(KeyCode.I))
        {
            Debug.LogFormat("Connected: {0:F}", player.IsConnected);
            Debug.LogFormat("Player state: {0:G}", player.State.ToString());
            Debug.Log(VolplaneController.AirConsole.GetMasterControllerDeviceId());
        }

        if(Input.GetKeyDown(KeyCode.H))
            VolplaneController.Main.ChangeViewAllActive("view1");

        if(Input.GetKeyDown(KeyCode.J))
        {
            Debug.Log(VolplaneController.Main.GetPlayer(1).State.ToString());
            player.SetActive(false);
            VolplaneController.Main.SetActive(1, true);
        }

        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            player.ChangeView("view4");
        }

        if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            ElementProperties properties = new ElementProperties();
            properties.FontColor = new Color(21f / 255f, 167f / 255f, 211f / 255f);
            properties.FontSize = 60;
            properties.Font = properties.WebFontToString(ElementProperties.WebFont.TrebuchetMS);
            player.ChangeElementProperties("text", properties);
            player.ChangeElementImage("dpad", "swipe.png");
        }

        if(Input.GetKeyDown(KeyCode.Z))
        {
            SimpleJSON.JSONNode lala = new SimpleJSON.JSONObject();

            lala["type"] = "swipe";

            for(int i = 0; i < 16; i++)
            {
                for(int j = 0; j < 20; j++)
                {
                    lala["name"] = String.Format("{0:D}", UnityEngine.Random.Range(0, 65535));
                    VolplaneController.InputHandling.ProcessInput(i, lala);
                }
            }
            
        }

        if(VInput.GetDPadDown())
        {
            Debug.Log("DPad down");
        }

        if(VInput.GetDPad())
        {
            Debug.Log(VInput.GetDPadAxis());
        }

        if(VInput.GetDPadUp())
        {
            Debug.Log("DPad up");
        }
    }
}
