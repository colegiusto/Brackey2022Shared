using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float health = 10;
    public float rarity = 1.1f;

    public void getHit(HitProperties properties) {
        health -= properties.damage;
        if (health <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        //generate an item and give it random properties based on rarity
        GameObject item = GameManager.CreateBlankItem();
        item.GetComponent<Item>().GenerateItemFromRarity(rarity);

        GameManager.ScreenShake(0.2f, 0.2f);
        GameManager.RemoveEnemy(this);
        Destroy(gameObject);
    }

    private void Start()
    {
        GameManager.AddEnemy(this);
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.TryGetComponent(out Bullet b))
        {
            b.TryHitEnemy(this);
        }
    }
}
