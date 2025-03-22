using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MapUI : WindowUI
{
    [SerializeField] private RectTransform playerPositionIndicator;
    [SerializeField] private Vector3 worldReferencePosition1;
    [SerializeField] private Vector3 worldReferencePosition2;
    [SerializeField] private RectTransform uiReferencePosition1;
    [SerializeField] private RectTransform uiReferencePosition2;

    [SerializeField] private GameObject houseNumerPrefab;
    [SerializeField] private Transform houseNumerParent;

    [SerializeField] private Transform locationNamesParent;
    private string[] locationNames;

    private Vector2 scale;

    internal override void Awake()
    {
        base.Awake();
        scale.x = (worldReferencePosition1.z - worldReferencePosition2.z) / (uiReferencePosition1.position.x - uiReferencePosition2.position.x);
        scale.y = (worldReferencePosition1.x - worldReferencePosition2.x) / (uiReferencePosition1.position.y - uiReferencePosition2.position.y);

        locationNames = new string[locationNamesParent.childCount];
        for (int i = 0; i < locationNamesParent.childCount; i++) {
            locationNames[i] = locationNamesParent.GetChild(i).GetComponentInChildren<TMP_Text>(includeInactive: true).text;
            locationNamesParent.GetChild(i).GetComponentInChildren<TMP_Text>().text = "?";
        }
    }

    private void Update()
    {
        if (isOpen) {
            UpdateIndicatorPosition();
        }
    }

    private void UpdateIndicatorPosition()
    {
        Vector3 playerPosition = PlayerInteractions.Instance.GetPlayerPosition();
        playerPositionIndicator.position = WorldToMapPosition(playerPosition);
        playerPositionIndicator.eulerAngles = new Vector3(0, 0, -PlayerInteractions.Instance.GetPlayerRotation().y + 180);
    }

    private Vector3 WorldToMapPosition(Vector3 worldPosition)
    {
        Vector3 mapPosition = new Vector3((worldPosition.z - worldReferencePosition1.z) / scale.x, (worldPosition.x - worldReferencePosition1.x) / scale.y);
        return mapPosition + uiReferencePosition1.position;
    }

    public void UpdateLocationNames()
    {
        for(int i = 0; i < locationNamesParent.childCount; i++) {
            if (ShopData.instance.isLocationUnlocked[i])
                locationNamesParent.GetChild(i).GetComponentInChildren<TMP_Text>().text = locationNames[i];
            else {
                locationNamesParent.GetChild(i).GetComponentInChildren<TMP_Text>().text = "?";
            }
        }
    }
}
