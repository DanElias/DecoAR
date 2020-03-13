using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(ARRaycastManager))]
public class DecoAR : MonoBehaviour
{
    [SerializeField]
    private GameObject placedPrefab;

    [SerializeField]
    private GameObject furniturePanel;

    [SerializeField]
    private Button dismissButton;

    [SerializeField]
    private Button selectButton;

    [SerializeField]
    private Button bedButton;

    [SerializeField]
    private Button cabinetButton, chairButton, lampButton, mirrorButton, tableButton;

    [SerializeField]
    private Camera arCamera;

    private PlacementObject[] placedObjects;

    private Vector2 touchPosition = default;

    private ARRaycastManager arRaycastManager;

    private bool onTouchHold = false;

    private static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private PlacementObject lastSelectedObject;

    private GameObject PlacedPrefab
    {
        get
        {
            return placedPrefab;
        }
        set
        {
            placedPrefab = value;
        }
    }


    void Awake()
    {
        arRaycastManager = GetComponent<ARRaycastManager>();
        dismissButton.onClick.AddListener(Dismiss);
        selectButton.onClick.AddListener(Select);

        if (bedButton != null && cabinetButton != null && chairButton != null && lampButton != null && mirrorButton != null && tableButton != null)
        {
            bedButton.onClick.AddListener(() => ChangePrefabSelection("bed_1"));
            cabinetButton.onClick.AddListener(() => ChangePrefabSelection("cabinet_1"));
            chairButton.onClick.AddListener(() => ChangePrefabSelection("chair_1"));
            lampButton.onClick.AddListener(() => ChangePrefabSelection("coffee_table_1"));
            mirrorButton.onClick.AddListener(() => ChangePrefabSelection("mirror_2"));
            tableButton.onClick.AddListener(() => ChangePrefabSelection("torchere_1"));
        }
    }

    private void ChangePrefabSelection(string name)
    {
        GameObject loadedGameObject = Resources.Load<GameObject>($"Prefabs/{name}");
        if (loadedGameObject != null)
        {
            PlacedPrefab = loadedGameObject;
            Debug.Log($"Game object with name {name} was loaded");
        }
        else
        {
            Debug.Log($"Unable to find a game object with name {name}");
        }
    }

    private void Dismiss() => furniturePanel.SetActive(false);

    private void Select() => furniturePanel.SetActive(true);

    void Update()
    {
        // do not capture events unless the welcome panel is hidden
        if (furniturePanel.activeSelf)
            return;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            touchPosition = touch.position;

            if (touch.phase == TouchPhase.Began)
            {
                Ray ray = arCamera.ScreenPointToRay(touch.position);
                RaycastHit hitObject;
                if (Physics.Raycast(ray, out hitObject))
                {
                    lastSelectedObject = hitObject.transform.GetComponent<PlacementObject>();
                    if (lastSelectedObject != null)
                    {
                        PlacementObject[] allOtherObjects = FindObjectsOfType<PlacementObject>();
                        foreach (PlacementObject placementObject in allOtherObjects)
                        {
                            placementObject.Selected = placementObject == lastSelectedObject;
                        }
                    }
                }
            }

            if (touch.phase == TouchPhase.Ended)
            {
                lastSelectedObject.Selected = false;
            }

            if (arRaycastManager.Raycast(touchPosition, hits, UnityEngine.XR.ARSubsystems.TrackableType.PlaneWithinPolygon))
            {
                Pose hitPose = hits[0].pose;

                if (lastSelectedObject == null)
                {
                    lastSelectedObject = Instantiate(placedPrefab, hitPose.position, hitPose.rotation).GetComponent<PlacementObject>();
                }
                else
                {
                    if (lastSelectedObject.Selected)
                    {
                        lastSelectedObject.transform.position = hitPose.position;
                        lastSelectedObject.transform.rotation = hitPose.rotation;
                    }
                }
            }
        }
    }
}