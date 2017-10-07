using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using Volplane;

public class Gaga : VolplaneBehaviour
{
    VPlayer myPlayer;

    void OnReady()
    {
        Debug.Log("Ready");
        SetStandardView("view4");
    }

    void OnConnect(VPlayer player)
    {
        if(player.IsActive)
        {
            player.ChangeView("view1");
            myPlayer = player;
        }
        else
        {
            player.ChangeView("view2");
        }
        
        Debug.Log(player);
        Debug.LogFormat("View: {0:G}", player.CurrentView);
        Debug.LogFormat("Browser: {0:F}", player.IsUsingBrowser);
        Debug.LogFormat("Connected: {0:F}", player.IsConnected);
        Debug.LogFormat("Hero: {0:F}", player.IsHero);
        Debug.LogFormat("Slow Connection: {0:F}", player.HasSlowConnection);
        Debug.LogFormat("UID: {0:G}", player.UID);
        Debug.LogFormat("Nickname: {0:G}", player.Nickname);
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.H))
            ChangeViewAllActive("view1");
        
        transform.position += new Vector3(VInput.GetAxis(myPlayer, "dpad", VInput.Axis.Horizontal), 0f, VInput.GetAxis(myPlayer, "dpad", VInput.Axis.Vertical)) * 0.1f;

        if(Input.GetKeyDown(KeyCode.V))
            RequestAd();

        if(Input.GetKeyDown(KeyCode.X))
            Debug.LogFormat("Master id: {0:D} / myPlayer: {1:D}", GetMasterId(), myPlayer.PlayerId);
    }

    void OnButton(int playerId, string name, bool state)
    {
        Debug.LogFormat("PlayerId: {0:D} /  {1:G} / {2:F}", playerId, name, state);

        if(state && name == "button-top")
        {
            gameObject.GetComponent<Renderer>().material.color = new Color(
                UnityEngine.Random.Range(0f, 1f),
                UnityEngine.Random.Range(0f, 1f),
                UnityEngine.Random.Range(0f, 1f)
            );

            SimpleJSON.JSONObject data = new SimpleJSON.JSONObject();
            data["wowow"]["haha"] = "brezel";
            myPlayer.SaveUserData(data);
            Debug.Log(data.ToString(2));
        }
    }

    void OnAdShow()
    {
        Debug.Log("There's an ad...");
    }

    void OnAdComplete(bool wasShown)
    {
        Debug.LogFormat("Ad finished with status: {0:F}", wasShown);
    }

    void OnUserDataSaved(VPlayer player)
    {
        Debug.LogFormat("Saved data for player '{0:G}'", player.Nickname);
    }
}
