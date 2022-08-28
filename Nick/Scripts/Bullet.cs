using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Bullet : MonoBehaviour
{
    //store this bullet's damage, size, speed, ricochette, etc (always unchanging for each bullet instance)
    public BulletProperties bulletProperties;

    //store attributes that vary through the life of a bullet
    float life_left;//how many seconds left in this bullet's lifetime
    List<Enemy> enemies_hit;

    private void Start()
    {
        life_left = bulletProperties.lifetime;
        enemies_hit = new List<Enemy>();
    }

    // Update is called once per frame
    void Update()
    {
        //decrease lifetime left
        life_left -= Time.deltaTime;
        if (life_left <= 0)
        {
            Destroy(gameObject);
        }
        //move the bullet forward
        transform.Translate(Vector2.up * Time.deltaTime * bulletProperties.speed);
    }

    void HitEnemy(Enemy enemy)
    {
        enemies_hit.Add(enemy);
        enemy.getHit(bulletProperties.hitProperties);
    }

    public bool TryHitEnemy(Enemy enemy)
    {
        //check if the enemy has already been hit by this bullet
        if (enemies_hit.Contains(enemy))
        {
            return false;
        }

        HitEnemy(enemy);

        //if there is some pierce left, use that
        if (bulletProperties.pierce > 0)
        {
            bulletProperties.pierce--;
            return true;
        }
        //if there is any bounce left, use that
        else if (bulletProperties.bounces > 0)
        {
            bulletProperties.bounces--;
            //track to the next nearest enemy:
            TargetNearest();
            return true;
        }


        //otherwise, we die now
        Destroy(gameObject);
        return true; ;
    }

    public void TargetNearest ()
    {
        List<Enemy> enemies = GameManager.nearbyEnemies;
        enemies = enemies.FindAll(e => !enemies_hit.Contains(e));
        if (enemies.Count == 0)
        {
            return;//there are no good targets after filtering
        }
        float dst = (Vector2.Distance(transform.position, enemies[0].transform.position));
        Enemy target = enemies[0];
        foreach (Enemy e in enemies)
        {
            float tmp = Vector2.Distance(transform.position, e.transform.position);
            if (tmp < dst)
            {
                dst = tmp;
                target = e;
            }
        }

        //now rotate to be pointing at the target
        transform.rotation = Quaternion.identity;
        transform.Rotate(Vector3.forward * Vector2.SignedAngle(Vector2.up, (target.transform.position - transform.position)));
    }

    public static BulletProperties CombineBulletProperties(BulletProperties start, BulletProperties delta)
    {
        start.hitProperties = CombineHitProperties(start.hitProperties, delta.hitProperties);
        start.size *= delta.size;
        start.speed *= delta.speed;
        start.bounces += delta.bounces;
        start.pierce += delta.pierce;
        start.lifetime *= delta.lifetime;
        return start;
    }

    public static HitProperties CombineHitProperties(HitProperties start, HitProperties delta)
    {
        start.damage += delta.damage;
        return start;
    }
}

[System.Serializable]
public struct BulletProperties
{
    public float size;//how big is the bullet compared to the default prefab? (could be located somewhere else)
    public float speed;//how fast does the bullet move (units/second)
    public int bounces;//how many times can the bullet bounce before dying
    public int pierce;//how many enemies can be hit before the bullet begins bouncing
    public float lifetime;//how many seconds does this bullet last (supercedes pierce and bounces)

    public HitProperties hitProperties;//what happens when the bullet hits an enemy

    public static BulletProperties identity => new BulletProperties(1, 1, 0, 0, 1, HitProperties.identity);

    public BulletProperties(
        float size,
        float speed,
        int bounces,
        int pierce,
        float lifetime,
        HitProperties hitProperties)
    {
        this.size = size;
        this.speed = speed;
        this.bounces = bounces;
        this.pierce = pierce;
        this.lifetime = lifetime;
        this.hitProperties = hitProperties;
    }
}

[System.Serializable]
public struct HitProperties
{
    public int damage;
    //NOTE: this can be expanded when elements / debuffs are added

    public static HitProperties identity => new HitProperties(0);

    public HitProperties(int damage)
    {
        this.damage = damage;
    }
}