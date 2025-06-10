using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapMenuUI : MonoBehaviour
{
    public RectTransform mapPanel;
    public RectTransform deviceButtonHolder;
    public RectTransform deviceUIPanel;
    public PhysDevice selectedDevice;
    public GameObject deviceButtonPrefab;
    public Minimap minimap;
    public float deviceOpenedX = -566.3641f;
    private List<string> listedDevices = new List<string>();
    MinimapManager minimapManager;
    int idCounter = 0;

    public void ClearDeviceUIPanel()
    {
        foreach (Transform child in deviceUIPanel)
        {
            Destroy(child.gameObject);
        }
    }

    public void ClearDeviceButtons()
    {
        foreach (Transform child in deviceButtonHolder)
        {
            Destroy(child.gameObject);
        }
        if (minimapManager == null) minimapManager = FindAnyObjectByType<MinimapManager>();
        foreach (string device in listedDevices) minimapManager.RemoveIcon(device);
        listedDevices.Clear();
    }

    /// <summary>
    /// Adds a device button to the map panel
    /// </summary>
    /// <param name="device"></param>
    public void AddDeviceButton(PhysDevice device)
    {
        GameObject newDeviceButton = Instantiate(deviceButtonPrefab, deviceButtonHolder);
        DeviceButtonUI dvbui = newDeviceButton.GetComponent<DeviceButtonUI>();
        dvbui.SetIcon(device.icon);
        Button button = newDeviceButton.GetComponent<Button>();
        PhysDevice p = device;
        button.onClick.AddListener(delegate { SelectDevice(p); });

        if (minimapManager == null) minimapManager = FindAnyObjectByType<MinimapManager>();
        idCounter++;
        string addedDeviceIcon = "Device" + idCounter;
        minimapManager.AddIcon(addedDeviceIcon, device.icon, device.transform.position, 0f, false);
        listedDevices.Add(addedDeviceIcon);
    }

    /// <summary>
    /// Selects a device
    /// </summary>
    /// <param name="device"></param>
    public void SelectDevice(PhysDevice device)
    {
        Debug.Log("Selecting device");
        if (device == selectedDevice)
        {
            selectedDevice.DeviceClosed();
            selectedDevice = null;
        } 
        else
        {
            if (selectedDevice != null)
            {
                selectedDevice.DeviceClosed();
            }
            selectedDevice = device;
        }

        if (selectedDevice == null)
        {
            HidePanel();
            return;
        }
        ShowPanel();
        minimap.SetPosition(device.transform.position);
        minimap.SetZoom(5f);
        ClearDeviceUIPanel();
        GameObject uiObject = Instantiate(selectedDevice.uiObject, deviceUIPanel);
        uiObject.transform.localPosition = Vector2.zero;
        selectedDevice.DeviceOpened(uiObject);
    }

    /// <summary>
    /// Shows the panel and moves the minimap to the correct location
    /// </summary>
    public void ShowPanel()
    {
        deviceUIPanel.gameObject.SetActive(true);
        mapPanel.localPosition = new Vector2(deviceOpenedX, mapPanel.localPosition.y);
    }

    /// <summary>
    /// Hides the panel and movse the minimap to the correct location
    /// </summary>
    public void HidePanel(bool closeDevices = false)
    {
        ClearDeviceUIPanel();
        deviceUIPanel.gameObject.SetActive(false);
        mapPanel.localPosition = new Vector2(0f, mapPanel.localPosition.y);
        // for when the device is closed from control panel stuff
        if (closeDevices)
        {
            if (selectedDevice != null)
            {
                selectedDevice = null;
                selectedDevice.DeviceClosed();
            }
        }
    }

}
