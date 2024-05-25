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
    public Color lockedColor;
    [Header("Assignables")]
    public Transform doorTransform;
    public GameObject interOpen;
    public GameObject interClose;
    public GameObject interLocked;
    float originalAngle;
    [Header("Info")]
    public bool doorOpened = false;
    public bool doorLocked = false;
    PhotonView view;
    GameManager gm;

    private void Awake()
    {
        gm = FindObjectOfType<GameManager>();
        view = transform.GetComponent<PhotonView>();
        originalAngle = doorTransform.localEulerAngles.y;
        // Sets all other colliders to the open collider
        BoxCollider bc = interOpen.GetComponent<BoxCollider>();
        interClose.GetComponent<BoxCollider>().size = bc.size;
        interClose.GetComponent<BoxCollider>().center = bc.center;
        interLocked.GetComponent<BoxCollider>().size = bc.size;
        interLocked.GetComponent <BoxCollider>().center = bc.center;
        interClose.SetActive(false);
        interLocked.SetActive(false);
        if (isHouseDoor)
        {
            gm.OnRevealRoles += DecideLock;
            NightLock();
        }
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

    public void SetLockText(int line, string text, Color color)
    {
        Interactable i = interLocked.GetComponent<Interactable>();
        i.hovers[line].lore = text;
        i.hovers[line].color = color;
    }

    void DecideLock(bool isCultist)
    {
        //if (isCultist) return;
        gm.OnNightSkip += NightLockInvoke;
        gm.OnDayStart += NightUnlock;
    }

    void NightLockInvoke()
    {
        Invoke("NightLock", 2.9f);
    }

    void NightLock()
    {
        if (doorOpened) ForceClose();
        Lock();
        SetLockText(0, "Curfew", lockedColor);
        SetLockText(1, "You can't leave during the night", lockedColor);
    }

    void NightUnlock()
    {
        Unlock();
    }

    [PunRPC]
    void Lock()
    {
        if (doorLocked) return;
        interOpen.SetActive(false);
        interClose.SetActive(false);
        interLocked.SetActive(true);
    }

    [PunRPC]
    void Unlock()
    {
        interLocked.SetActive(false);
        interOpen.SetActive(!doorOpened);
        interClose.SetActive(doorOpened);
    }

    void ForceClose()
    {
        doorOpened = false;
        doorTransform.localEulerAngles = new Vector3(0f, originalAngle);
        interOpen.SetActive(!doorOpened);
        interClose.SetActive(doorOpened);
    }

    void ForceOpen()
    {
        doorOpened = true;
        doorTransform.localEulerAngles = new Vector3(0f, openAngle);
        interOpen.SetActive(!doorOpened);
        interClose.SetActive(doorOpened);
    }

    IEnumerator DoorOpenAnim()
    {
        float openTimer = 0f;
        interOpen.SetActive(false);
        doorOpened = true;
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
        doorOpened = false;
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
