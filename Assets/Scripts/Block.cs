using System.Collections;
using UnityEngine;

public class Block : MonoBehaviour
{
    public event System.Action<Block> OnBlockPressed;
    public event System.Action OnFinishedMoving;

    public Vector2Int _coords;
    private Vector2Int _startingCoords;

    public void Init(Vector2Int startingCoords)
    {
        _startingCoords = startingCoords;
        _coords = startingCoords;
    }

    public bool IsAtStartingCoordinate()
    {
        return _coords == _startingCoords;
    }
    
    public void MoveToPosition(Vector2 targetPos, float duration)
    {
        StartCoroutine(AnimateMove(targetPos, duration));
    }
    
    private void OnMouseDown()
    {
        if (OnBlockPressed != null)
        {
            OnBlockPressed(this);
        }
    }

    private IEnumerator AnimateMove(Vector2 targetPos, float duration)
    {
        Vector2 startPos = transform.position;
        float percent = 0.0f;

        while (percent < 1.0f)
        {
            percent += Time.deltaTime / duration;
            transform.position = Vector2.Lerp(startPos, targetPos, percent);
            yield return null;
        }

        if (OnFinishedMoving != null)
        {
            OnFinishedMoving();
        }
    }
}
