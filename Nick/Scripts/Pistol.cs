using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pistol : Weapon
{
    [SerializeField]
    public float delay;//how many seconds between bullets
    private float curCooldown;
    [SerializeField]
    private bool firing;

    public override WeaponProperties GenerateFromRarity(float rarity)
    {
        WeaponProperties basep = new WeaponProperties(1.0f, new BulletProperties(2, 8, 0, 0, 3, new HitProperties(1)), 100);
        //rarity scaled damage by its square root:
        basep.bullet.hitProperties.damage = (int)(Mathf.Sqrt(rarity) * basep.bullet.hitProperties.damage);
        return basep;
    }

    public void Start()
    {
        firing = false;
        curCooldown = 0.0f;
        updateProperties();
    }

    public override void StartFire()
    {
        firing = true;
    }

    public override void StopFire()
    {
        firing = false;
    }

    public override string weaponName()
    {
        return "Pistol";
    }

    // Update is called once per frame
    void Update()
    {
        curCooldown -= Time.deltaTime * varyingProperties.fireRateModifier;
        if (firing && curCooldown <= 0.0f)
        {
            //TODO: find angle to fire at
            ShootAtMouse(10.0f);
            curCooldown = delay;
        }
    }
}
