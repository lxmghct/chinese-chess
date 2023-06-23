
using UnityEngine;

public class PopWindow : MonoBehaviour
{
    // 将遮罩层大小设置为父级大小
    void Start()
    {
        GameObject mask = transform.Find("Mask").gameObject;
        RectTransform parentRectTransform = transform.parent.GetComponent<RectTransform>();
        RectTransform maskRectTransform = mask.GetComponent<RectTransform>();
        maskRectTransform.sizeDelta = parentRectTransform.sizeDelta;
    }

    public void CloseWindow()
    {
        gameObject.SetActive(false);
    }
}