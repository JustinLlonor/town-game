using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoleRevealer : MonoBehaviour
{
    public GameObject camTPrefab;
    CameraManager cm;
    Transform playerTransform;
    ClientGFX cgfx;

    private void Awake()
    {
        cm = FindObjectOfType<CameraManager>();
        FindObjectOfType<PlayerManager>().OnInstantiatePlayer += GetReferences;
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
        cgfx.ShowRenderers();
        StartCoroutine(SnapToFPS(3f, 1.5f));
    }

    IEnumerator SnapToFPS(float waitDuration, float disappearDuration)
    {
        yield return new WaitForSeconds(waitDuration);
        cm.StartFPSTransition(2f);
        yield return new WaitForSeconds(disappearDuration);
        cgfx.HideRenderers();
    }
}
