using DG.Tweening.Core.Easing;
using TMPro;
using UnityEngine;

public class InteractionUI : MonoBehaviour
{
    public static InteractionUI Instance;

    [SerializeField] private Canvas _canvas;
    [SerializeField] private TextMeshProUGUI promptText;

    private Transform currentTarget;
    private Collider currentCollider;
    [SerializeField] private Transform mainCameraTransform;
    

    private void Awake()
    {
        Instance = this;
    }

    private void LateUpdate()
    {
        Vector3 targetPosition;

        if(currentCollider != null)
        {
            Bounds bounds = currentCollider.bounds;
            targetPosition = new Vector3(bounds.center.x, bounds.max.y + 0.25f, bounds.center.z);
            _canvas.transform.position = targetPosition;

        }
        else
        {
            if(currentTarget != null)
            {
                targetPosition = currentTarget.position + new Vector3(0f, 0.35f, 0f);
                _canvas.transform.position = targetPosition;

            }
        }
        _canvas.transform.rotation = mainCameraTransform.rotation;


    }

    public void Show(string key, string action, Transform target)
    {
        currentTarget = target;
        currentCollider = target.GetComponentInChildren<Collider>();
        promptText.text = $"<b><color=orange>[{key}]</color></b> <color=white>{action}</color>";
        _canvas.enabled = true;
    }

    public void Hide()
    {
        _canvas.enabled = false;
        currentCollider = null;
        currentTarget = null;
    }
}
