using System.Collections;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UIElements;

public class Door : MonoBehaviour
{
    public float openAngle = 90f;
    public float openTime = .5f;
    public float closeTime = .25f;
    public AnimationCurve doorAnim;
    public bool isHouseDoor = false;
    [Header("Assignables")]
    public Transform doorTransform;
    public GameObject interOpen;
    public GameObject interClose;
    float originalAngle;
    PhotonView view;

    private void Awake()
    {
        view = transform.GetComponent<PhotonView>();
        originalAngle = doorTransform.localEulerAngles.y;
    }

    public void OpenDoorRPC()
    {
        PhotonNetwork.OpCleanRpcBuffer(view);
        OpenDoor();
        view.RPC("OpenDoor", RpcTarget.OthersBuffered);
    }

    public void CloseDoorRPC()
    {
        PhotonNetwork.OpCleanRpcBuffer(view);
        CloseDoor();
        view.RPC("CloseDoor", RpcTarget.OthersBuffered);
    }

    [PunRPC]
    public void OpenDoor()
    {
        StartCoroutine(DoorOpenAnim());
    }

    [PunRPC]
    public void CloseDoor()
    {
        StartCoroutine(DoorCloseAnim());
    }

    IEnumerator DoorOpenAnim()
    {
        float openTimer = 0f;
        interOpen.SetActive(false);
        while (openTimer < openTime)
        {
            yield return null;
            openTimer += Time.deltaTime;
            float openPercent = openTimer / openTime;
            openPercent = doorAnim.Evaluate(openPercent);
            doorTransform.localEulerAngles = new Vector3(0f, Mathf.LerpAngle(originalAngle, openAngle, openPercent));
        }
        doorTransform.localEulerAngles = new Vector3(0f, openAngle);
        interClose.SetActive(true);
    }
    
    IEnumerator DoorCloseAnim()
    {
        float closeTimer = 0f;
        interClose.SetActive(false);
        float oAngle = doorTransform.localEulerAngles.y;
        while (closeTimer < closeTime)
        {
            yield return null;
            closeTimer += Time.deltaTime;
            float closePercent = closeTimer / closeTime;
            closePercent = doorAnim.Evaluate(closePercent);
            doorTransform.localEulerAngles = new Vector3(0f, Mathf.LerpAngle(oAngle, originalAngle, closePercent));
        }
        doorTransform.localEulerAngles = new Vector3(0f, originalAngle);
        interOpen.SetActive(true);
    }
}
