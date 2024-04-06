using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoleRevealer : MonoBehaviour
{
    public GameObject camTPrefab;
    public LayerMask uiFrontMask;
    public LayerMask defaultMask;
    CameraManager cm;
    Transform playerTransform;
    ClientGFX cgfx;
    BlackScreen bs;

    private void Awake()
    {
        cm = FindObjectOfType<CameraManager>();
        bs = FindAnyObjectByType<BlackScreen>();
        FindObjectOfType<PlayerManager>().OnInstantiatePlayer += GetReferences;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            RevealRole(true);
        }
    }

    void GetReferences(GameObject player)
    {
        playerTransform = player.transform;
        cgfx = player.GetComponentInChildren<ClientGFX>();
    }

    public void RevealRole(bool role)
    {
        Debug.Log("playing sequence");
        GameObject camPrefab = Instantiate(camTPrefab);
        camPrefab.transform.position = playerTransform.position;
        camPrefab.transform.rotation = playerTransform.rotation;
        cm.SetTrackedCinematicTransform(camPrefab.transform.GetChild(0));
        cm.ChangeCameraMode(CameraManager.CameraMode.Cinematic);
        bs.SetAlpha(1f);
        cgfx.ShowRenderers();
        cgfx.SetRenderersLayer(uiFrontMask);
        StartCoroutine(SnapToFPS(3f, 1.5f));
        StartCoroutine(FadeBlackScreen(2f));
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
