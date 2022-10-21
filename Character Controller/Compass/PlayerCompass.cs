using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class PlayerCompass : MonoBehaviour
{
    [Tooltip("Offset distance text above markers so they do not overlap.")]
    public bool offsetOverlappingText = true;
    [Tooltip("The vertical offset applied for each overlapping text field.")]
    public float verticalOffset = 10f;
    [Tooltip("Scale markers with distance to the player, so further markers are smaller.")]
    public bool scaleMarkersWithDistance = true;
    [Tooltip("The minimum scale markers can be reduced to at the farthest distance.")]
    public float minScale = .6f;
    [Tooltip("The maximum distance from the player at which the marker has the minimum scale or opacity.")]
    public float maximumDistance = 50f;
    [Tooltip("The minimum distance from the player at which the scale or opacity begins reducing.")]
    public float minimumDistance = 10f;
    [Tooltip("Change the opacity of markers with distance to the player, so further markers are invisble.")]
    public bool opacityMarkersWithDistance = true;

    public Material compassMat;
    public TMP_Text headingText;
    public RawImage compass, compassBackdrop;
    public GameObject iconPrefab;
    float heading = 0;
    public List<CompassMarker> compassMarkers = new List<CompassMarker>();
    float compassUnit;
    public List<CompassMarker> debugMarkers = new List<CompassMarker>();
    
    // Start is called before the first frame update
    void Start()
    {
        compassUnit = compass.rectTransform.rect.width / 360f;
        foreach(CompassMarker marker in debugMarkers)
        {
            AddCompassMarker(marker);
        }
    }

    // Update is called once per frame
    void Update()
    {
        heading = transform.rotation.eulerAngles.y;
        headingText.text = Mathf.Floor(heading).ToString();
        float compassFill = Mathf.InverseLerp(0, 359, heading);
        compass.uvRect = new Rect(compassFill, 0, 1, 1);
        compassBackdrop.uvRect = new Rect(compassFill, 0, 1, 1);
        compassMat.SetFloat("_OffsetCorrection", compassFill * -1f);
        debugs.Clear();
        CleanupMarkers();
        foreach(CompassMarker marker in compassMarkers)
        {
            marker.image.rectTransform.anchoredPosition = GetPosOnCompass(marker);
            float absPos = Mathf.Abs(marker.image.rectTransform.anchoredPosition.x);
            float opacity = Mathf.InverseLerp((compass.rectTransform.rect.width / 4.5f) + 40f, compass.rectTransform.rect.width / 4.5f, absPos);
            marker.canvasGroup.alpha = opacity;
            marker.distance = GetDistanceToMarker(marker);
            marker.textRectTransform.anchoredPosition = new Vector2(marker.textRectTransform.anchoredPosition.x, marker.yPosOffset + marker.initialYPos);
            if(scaleMarkersWithDistance)
            {
                float newScale = Mathf.InverseLerp(maximumDistance, minimumDistance, marker.distance);
                newScale = Mathf.Clamp(newScale, minScale, 1);
                marker.image.gameObject.transform.localScale = new Vector2(newScale, newScale);
            }
        }
    }

    public void AddCompassMarker(CompassMarker marker)
    {
        GameObject newMarker = Instantiate(iconPrefab, compass.transform);
        marker.image = newMarker.GetComponent<Image>();
        marker.image.sprite = marker.icon;
        marker.distanceText = newMarker.GetComponentInChildren<TMP_Text>();
        marker.parentRectTransform = marker.image.rectTransform;
        marker.textRectTransform = marker.distanceText.rectTransform;
        marker.canvasGroup = newMarker.GetComponent<CanvasGroup>();
        marker.markerIcon = newMarker;
        marker.canvas = newMarker.GetComponent<Canvas>();
        marker.initialYPos = marker.textRectTransform.anchoredPosition.y;

        compassMarkers.Add(marker);
    }

    Vector2 GetPosOnCompass(CompassMarker marker)
    {
        Vector2 playerPos = new Vector2(transform.position.x, transform.position.z);
        Vector2 playerForward = new Vector2(transform.forward.x, transform.forward.z);
        float angle = Vector2.SignedAngle(marker.position - playerPos, playerForward);
        return new Vector2(compassUnit * angle, 0f);
    }

    float GetDistanceToMarker(CompassMarker marker)
    {
        float distance = Vector3.Distance(transform.position, marker.transform.position);
        return distance;
    }
    public List<CompassMarker> markers = new List<CompassMarker>();
    public Dictionary<CompassMarker, List<CompassMarker>> dict = new Dictionary<CompassMarker, List<CompassMarker>>();

    void CleanupMarkers()
    {
        markers = new List<CompassMarker>(compassMarkers);
        dict = new Dictionary<CompassMarker, List<CompassMarker>>();
        // create new list of markers

        LoopCleanupMarkers(markers);
        foreach(CompassMarker marker in compassMarkers)
        {
            marker.yPosOffset = 0;
            marker.canvas.sortingOrder = 0;
        }

        SortCompassMarkers(dict);
    }

    void SortCompassMarkers(Dictionary<CompassMarker, List<CompassMarker>> dict)
    {
        foreach (CompassMarker key in dict.Keys)
        {
            if (dict[key].Count > 1)
            {
                var sortedList = dict[key].OrderBy(x => x.distance).ToList();
                for (int i = 0; i < sortedList.Count; i++)
                {
                    if (i == 0)
                    {
                        sortedList[i].yPosOffset = 30 * i;
                    }
                    else
                    {
                        if(offsetOverlappingText)
                        {
                            RectTransform rectTransform = sortedList[i - 1].textRectTransform;
                            var yOffset = rectTransform.rect.height;
                            yOffset = (yOffset + verticalOffset) * i;
                            var maxOffset = yOffset / sortedList[i - 1].minScale;
                            var inverseCalc = Mathf.InverseLerp(sortedList[i - 1].minScale, 1, sortedList[i - 1].parentRectTransform.localScale.y);
                            var divisor = Mathf.Lerp(1, sortedList[i - 1].minScale, inverseCalc);
                            yOffset = yOffset / divisor;
                            sortedList[i].yPosOffset = yOffset;
                        }
                        if(opacityMarkersWithDistance)
                        {

                        }
                        sortedList[i].canvas.sortingOrder = i * -1;
                    }

                }
            }
        }
    }

    
    void LoopCleanupMarkers(List<CompassMarker> markers)
    {
        if (markers.Count <= 0) return;

        var compareMarker = markers[markers.Count-1];
        markers.RemoveAt(markers.Count-1);
        for (int i = markers.Count-1; i >= 0; i--)
        {
            Rect r1 = compareMarker.parentRectTransform.FuckyWorldRect(compareMarker.textRectTransform);
            Rect r2 = markers[i].parentRectTransform.FuckyWorldRect(markers[i].textRectTransform);
            debugs.Add(r1);
            debugs.Add(r2);
            if (r1.Overlaps(r2) && i != 0)
            {
                if (dict.ContainsKey(compareMarker))
                {
                    dict[compareMarker].Add(markers[i]);
                }
                else
                {
                    dict.Add(compareMarker, new List<CompassMarker>());
                    dict[compareMarker].Add(markers[i]);
                    dict[compareMarker].Add(compareMarker);
                }
                markers.RemoveAt(i);
            }
        }
        
        LoopCleanupMarkers(markers);
    }

    public List<Rect> debugs = new List<Rect>();

    Rect GetWorldSpaceRect(RectTransform rt, RectTransform textRt)
    {
        var r = textRt.rect;
        r.center = rt.TransformPoint(r.center);
        r.size = textRt.TransformVector(r.size);
        debugs.Add(r);
        return r;
    }

    public Texture myTexture;
    private void OnDrawGizmos()
    {
        for(int i = 0; i < compassMarkers.Count-1; i++)
        {
            if (debugs[i] == null) return;
            Gizmos.DrawGUITexture(debugs[i], myTexture);
        }
        
    }

    void DrawQuad(Rect position, Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        GUI.skin.box.normal.background = texture;
        GUI.Box(position, GUIContent.none);
    }
}
