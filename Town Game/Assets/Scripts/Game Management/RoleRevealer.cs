//using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoleRevealer : MonoBehaviour
{
    public GameObject camTPrefab;
    public LayerMask uiFrontMask;
    public LayerMask defaultMask;
    public Color cultistColor;
    public Color innoColor;
    CameraManager cm;
    Transform playerTransform;
    ClientGFX cgfx;
    RoleText rtxt;
    BlackScreen bs;

    public GetRole OnGetRole;
    public RevealerEvent OnSequenceEnd;
    public delegate void GetRole(bool isCultist);
    public delegate void RevealerEvent(); // doesntwork

    private void Awake()
    {
        cm = FindFirstObjectByType<CameraManager>();
        bs = FindFirstObjectByType<BlackScreen>();
        rtxt = FindFirstObjectByType<RoleText>();;
        FindObjectOfType<PlayerManager>().onInstantiatePlayer += GetReferences;
        //if (PhotonNetwork.CurrentRoom != null) bs.ShowCover();
    }

    void GetReferences(GameObject player)
    {
        playerTransform = player.transform;
        cgfx = player.GetComponentInChildren<ClientGFX>();
    }

    public void RevealRole(bool isCultist)
    {
        Debug.Log("RR Start");
        OnGetRole?.Invoke(isCultist);
        StartCoroutine(StartReveal(isCultist));
    }

    // make wait for other players later
    IEnumerator StartReveal(bool isCultist)
    {
        yield return new WaitForSeconds(1f);
        bs.HideCover();
        GameObject camPrefab = Instantiate(camTPrefab);
        camPrefab.transform.position = playerTransform.position;
        camPrefab.transform.rotation = playerTransform.rotation;
        cm.SetTrackedCinematicTransform(camPrefab.transform.GetChild(0));
        cm.ChangeCameraMode(CameraManager.CameraMode.Cinematic);
        bs.SetAlpha(1f);
        cgfx.ShowRenderers();
        cgfx.SetRenderersLayer(uiFrontMask);
        StartCoroutine(SnapToFPS(5f, 1.5f));
        StartCoroutine(FadeBlackScreen(3f));
        rtxt.StartCursorBlink(1f, 5, 0f);
        rtxt.StartCursorBlink(5, 25, 2f);
        if (isCultist)
        {
            rtxt.StartTextRoll("Cultist", cultistColor, 1f, 1f);
        }
        else
        {
            rtxt.SetFontSize(60);
            rtxt.StartTextRoll("Civilian", innoColor, 1f, 1f);
        }
    }

    IEnumerator SnapToFPS(float waitDuration, float disappearDuration)
    {
        yield return new WaitForSeconds(waitDuration);
        cm.StartFPSTransition(2f);
        yield return new WaitForSeconds(disappearDuration);
        cgfx.HideRenderers();
        cgfx.SetRenderersLayer(defaultMask);
    }

    IEnumerator FadeBlackScreen(float fadeBlackDuration)
    {
        yield return new WaitForSeconds(fadeBlackDuration);
        bs.StartAlphaTransition(0f, 1f);
    }
}
