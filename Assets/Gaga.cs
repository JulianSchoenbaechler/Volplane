﻿using System;
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
        VolplaneController.AirConsole.onConnect += Connect;
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
            player.ChangeView("view3");

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
            VolplaneController.Main.RequestEmailAddress(0);
    }

    /*
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F))
        {
            player = new VPlayer(1);
            StartCoroutine(player.LoadProfilePicture());
            flag = true;
        }

        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log(VolplaneController.AirConsole.GetCustomDeviceState(1).ToString(2));
        }

        if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log(VolplaneController.AirConsole.GetCustomDeviceState(2).ToString(2));
        }
    }

    void OnGUI()
    {
        if(flag)
            GUI.DrawTexture(new Rect(10, 10, 256, 256), player.ProfilePicture);
    }
    */
}
