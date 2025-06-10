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
            selectedDevice = null;
        } 
        else
        {
            selectedDevice = device;
        }

        if (selectedDevice == null)
        {
            HidePanel();
            return;
        }
        ShowPanel();
        minimap.SetPosition(device.transform.position);
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
    public void HidePanel()
    {
        ClearDeviceUIPanel();
        deviceUIPanel.gameObject.SetActive(false);
        mapPanel.localPosition = new Vector2(0f, mapPanel.localPosition.y);
    }

}
