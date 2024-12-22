using UnityEngine;
using UnityEngine.EventSystems;

public class ChatResizer : MonoBehaviour, IDragHandler
{
    [SerializeField] private RectTransform chatRect;
    [SerializeField] private RectTransform resizeHandle; 

    public Vector2 minSize = new Vector2(800, 600); 
    public Vector2 maxSize = new Vector2(1900, 1060); 

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 mousePos = Input.mousePosition;

        Vector2 newSize = new Vector2(
            Mathf.Clamp(mousePos.x - chatRect.position.x, minSize.x, maxSize.x),
            Mathf.Clamp(mousePos.y - chatRect.position.y, minSize.y, maxSize.y)
        );

        chatRect.sizeDelta = newSize;
    }
}
