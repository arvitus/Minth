using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [HideInInspector]
    public new CircleCollider2D collider;

    void Awake()
    {
        collider = GetComponent<CircleCollider2D>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == GameSystem.Instance.player.gameObject)
        {
            GameSystem.Instance.GetCurrentLevel().Cancel();
        }
    }

    public IEnumerator StartFollowing(Level level)
    {
        yield return new WaitForSeconds(level.enemyDelay);
        gameObject.SetActive(true);
        while (level.running)
        {
            var target = level.GetNextTarget();
            yield return MoveOverSpeed(target, level.enemySpeed);
        }
    }

    public IEnumerator MoveOverSpeed(Vector3 end, float speed)
    {
        // speed should be 1 unit per second
        while (transform.position != end)
        {
            transform.position = Vector3.MoveTowards(transform.position, end, speed * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
    }

    public IEnumerator MoveOverSeconds(Vector3 end, float seconds)
    {
        float elapsedTime = 0;
        Vector3 startingPos = transform.position;
        while (elapsedTime < seconds)
        {
            transform.position = Vector3.Lerp(startingPos, end, (elapsedTime / seconds));
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        transform.position = end;
    }
}
