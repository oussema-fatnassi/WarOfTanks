using UnityEngine;
using UnityEngine.UI;

public class SelectionBox : MonoBehaviour
{
    #region Fields
    private RectTransform _rectTransform;
    private Image _image;
    private Vector2 _startPos;
    private Vector2 _currentPos;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _image = GetComponent<Image>();
        _image.enabled = false;
    }

    public void BeginDrag(Vector2 screenStart)
    { 
        _startPos = screenStart;
        _currentPos = screenStart;
        _image.enabled = true;
    }

    public void UpdateDrag(Vector2 screenCurrentPosition)
    {
        _currentPos = screenCurrentPosition;
        _rectTransform.anchoredPosition = new Vector2(
            Mathf.Min(_startPos.x, _currentPos.x),
            Mathf.Min(_startPos.y, _currentPos.y)
        );
        _rectTransform.sizeDelta = new Vector2(
            Mathf.Abs(_startPos.x - _currentPos.x),
            Mathf.Abs(_startPos.y - _currentPos.y)
        );
    }

    public Rect EndDrag()
    {
        _image.enabled = false;
        return new Rect(Mathf.Min(_startPos.x, _currentPos.x), Mathf.Min(_startPos.y, _currentPos.y), Mathf.Abs(_currentPos.x - _startPos.x), Mathf.Abs(_currentPos.y - _startPos.y));
    }

    #endregion
}
