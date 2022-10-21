using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public static class RectTransformExtensions
{

    public static bool Overlaps(this RectTransform a, RectTransform b)
    {
        return a.WorldRect().Overlaps(b.WorldRect());
    }
    public static bool Overlaps(this RectTransform a, RectTransform b, bool allowInverse)
    {
        return a.WorldRect().Overlaps(b.WorldRect(), allowInverse);
    }

    public static Rect WorldRect(this RectTransform rectTransform)
    {
        Vector2 sizeDelta = rectTransform.sizeDelta;
        Vector2 pivot = rectTransform.pivot;

        float rectTransformWidth = sizeDelta.x * rectTransform.lossyScale.x;
        float rectTransformHeight = sizeDelta.y * rectTransform.lossyScale.y;

        //With this it works even if the pivot is not at the center
        Vector3 position = rectTransform.TransformPoint(rectTransform.rect.center);
        float x = position.x - rectTransformWidth * 0.5f;
        float y = position.y - rectTransformHeight * 0.5f;

        return new Rect(x, y, rectTransformWidth, rectTransformHeight);
    }

    public static Rect FuckyWorldRect(this RectTransform rectTransform, RectTransform inheritSizeRectTransform)
    {
        Vector2 sizeDelta = inheritSizeRectTransform.sizeDelta;
        Vector2 pivot = rectTransform.pivot;

        float rectTransformWidth = sizeDelta.x * rectTransform.lossyScale.x;
        float rectTransformHeight = sizeDelta.y * rectTransform.lossyScale.y;

        //With this it works even if the pivot is not at the center
        Vector3 position = rectTransform.TransformPoint(rectTransform.rect.center);
        float x = position.x - rectTransformWidth * 0.5f;
        float y = position.y - rectTransformHeight * 0.5f;

        return new Rect(x, y, rectTransformWidth, rectTransformHeight);
    }

    public static float GetWorldRectHeight(this RectTransform rectTransform, RectTransform scaleCheck)
    {
        Vector2 sizeDelta = rectTransform.sizeDelta;
        float rectTransformHeight = sizeDelta.y * scaleCheck.lossyScale.y;
        Debug.Log(rectTransformHeight);
        return rectTransformHeight * 2;
    }
}