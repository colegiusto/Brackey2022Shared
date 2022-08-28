using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    float shakeLeft;
    float shakeMagnetude;
    Vector3 originalPosition;
    bool shaking;
    Transform player;
    public float followRadius = 2.0f;

    // Start is called before the first frame update
    void Start()
    {
        player = GameManager.gameManager.transform;
        shaking = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (shaking)
        {
            transform.position = originalPosition + ((Vector3)Random.insideUnitCircle * shakeMagnetude);
            shakeLeft -= Time.deltaTime;
            if (shakeLeft <= 0.0f)
            {
                shaking = false;
                transform.position = originalPosition;
            }
        }

        if (Vector2.Distance(transform.position, player.position) > followRadius)
        {
            transform.Translate(Vector2.Lerp(transform.position, player.position, Time.deltaTime) - (Vector2)transform.position);
        }
    }

    public void Shake(float time, float magnetude)
    {
        if (!shaking)
        {
            originalPosition = transform.position;
        }

        shaking = true;
        shakeLeft = time>shakeLeft?time:shakeLeft;
        shakeMagnetude = magnetude>shakeMagnetude?magnetude: shakeMagnetude;
    }
}
