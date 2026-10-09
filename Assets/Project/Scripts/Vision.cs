using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Vision : MonoBehaviour
{
    
    public float radius = 5f;
    [Range(30, 360)]
    public float fovAngle = 90f;
    [Range(2, 360)]
    public int rayCount = 2;
    public LayerMask visionLayers;


    void Start()
    {
        
    }

    void Update()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius, visionLayers);
        
        foreach (Collider hit in hits)
        {
            WorldItem worldItem = hit.GetComponent<WorldItem>();
            if(worldItem == null)
            {
                continue;
            }

            Vector3 closestPoint = hit.ClosestPoint(transform.position);
            Debug.DrawLine(transform.position, closestPoint, Color.blue);

            if (Keyboard.current.eKey.isPressed)
            {
                Inventory myInventory = GetComponent<Inventory>();
                worldItem.PickUp(myInventory);
                break;
            }

        }

        float endAngle = fovAngle / 2;
        float startAngle = endAngle * -1;
        float angleStep = fovAngle / (rayCount - 1);

        for(float currentAngle = startAngle; currentAngle <= endAngle; currentAngle += angleStep)
        {
            Vector3 direction = Quaternion.Euler(0f, currentAngle, 0f) * transform.forward;
            
            if(Physics.Raycast(transform.position,direction, out RaycastHit hit, 50f, visionLayers, QueryTriggerInteraction.Ignore))
            {
                if(hit.collider.CompareTag("SingleDoor"))
                {
                    Debug.DrawLine(transform.position, hit.point, Color.green);
                }
                else
                {
                    Debug.DrawLine(transform.position, hit.point, Color.red);
                }
            }
            else
            {
                Debug.DrawRay(transform.position, direction * 10f, Color.white);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, radius);

        //Gizmos.color = Color.cyan;
        //for(float i = _startAngle; i <= _endAngle; i += _angleStep)
        //{
        //    Vector3 direction = Quaternion.Euler(0f, i, 0f) * transform.forward;
        //    Gizmos.DrawRay(transform.position, direction);
        //}
    }
}
