using DG.Tweening.Core.Easing;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;

public class InteractionUI : MonoBehaviour
{
    public static InteractionUI Instance;

    //[SerializeField] private Canvas _canvas;
    [SerializeField] private TextMeshProUGUI promptText;

    private GameObject currentObject;
    private Vector3 currentTargetPosition;
    private Collider currentCollider;
    [SerializeField] private Transform mainCameraTransform;
    [SerializeField] private RectTransform promptRoot;


    private void Awake()
    {
        Instance = this;
    }

    private void LateUpdate()
    {
        if(currentTargetPosition != null)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(currentTargetPosition);
            promptRoot.transform.position = screenPos;
        }
    


    }

    public void Show(string key, string action, Interactable target)
    {
        if(target.GetUiPoint() == default)
        {
            currentCollider = target.GetComponentInChildren<Collider>();

            if(currentCollider != null)
            {
                Bounds bounds = currentCollider.bounds;
                currentTargetPosition = new Vector3(bounds.center.x, bounds.max.y + 0.35f, bounds.center.z);
                //currentTargetPosition = currentCollider.transform.position;
            }
        }
        else
        {
            currentTargetPosition = target.GetUiPoint();
        }

        promptText.text = $"<b><color=white>[{key}]</color></b> <color=white>{action}</color>";
        //_canvas.enabled = true;
    }

    public void Hide()
    {
        //_canvas.enabled = false;
        currentCollider = null;
        currentTargetPosition = default;
    }
}
