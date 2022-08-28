using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public abstract class Weapon : MonoBehaviour
{
    public WeaponProperties base_properties = WeaponProperties.identity;
    public WeaponProperties varyingProperties;//this is what is used when actually shooting
    public abstract string weaponName();

    public GameObject bulletPrefab;//prefab of bullet for the weapon

    public void updateProperties()
    {
        //here is where we query items for their parameter changes
        List<WeaponProperties> itemModifiers = GameManager.itemModifiers();
        varyingProperties = base_properties;
        for (int i = 0; i < itemModifiers.Count; i++)
        {
            varyingProperties = CombineWeaponProperties(varyingProperties, itemModifiers[i]);
        }
    }

    public abstract void StartFire();

    public abstract void StopFire();


    //shoots a bullet at an angle relative to up
    protected void ShootAtAngle(float zRot)
    {
        GameObject spawned = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        //give the bullet its properties
        spawned.GetComponent<Bullet>().bulletProperties = varyingProperties.bullet;
        //scale it up based on scale
        spawned.transform.localScale *= varyingProperties.bullet.size;
        //have it rotate to be facing the correct way
        spawned.transform.Rotate(Vector3.forward * zRot);
    }

    protected void ShootAtAngle(float zRot, float maxErr)
    {
        float err = Random.Range(-maxErr, maxErr);
        ShootAtAngle(zRot + err);
    }

    protected void ShootAtMouse(float maxErr)
    {
        Vector2 mouseDir = GameManager.MousePosition - transform.position;
        float ang = Vector2.SignedAngle(Vector2.up, mouseDir);
        ShootAtAngle(ang, maxErr);
    }

    protected void ShootAtMouse()
    {
        ShootAtMouse(0.0f);
    }

    public static WeaponProperties CombineWeaponProperties(WeaponProperties start, WeaponProperties delta)
    {
        start.fireRateModifier *= delta.fireRateModifier;
        start.bullet = Bullet.CombineBulletProperties(start.bullet, delta.bullet);
        return start;
    }

    public abstract WeaponProperties GenerateFromRarity(float rarity);
}

[System.Serializable]
public struct WeaponProperties //description of base properties for the weapon. changes for weapon type, rarity, etc.
{
    public float fireRateModifier;//defaults to one, changes percentage-wise based on items to speed up/slow down weapon

    //properties of the bullet that the weapon shoots
    public BulletProperties bullet;

    public int value;

    public static WeaponProperties identity => new WeaponProperties(1, BulletProperties.identity, 0);

    public WeaponProperties(float fireRateModifier, BulletProperties bullet, int value)
    {
        this.fireRateModifier = fireRateModifier;
        this.bullet = bullet;
        this.value = value;
    }
}
