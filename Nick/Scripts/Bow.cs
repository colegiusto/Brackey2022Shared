using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bow : Weapon
{
    public bool charging;
    [SerializeField]
    public float chargeTime;
    private float charge;

    public override WeaponProperties GenerateFromRarity(float rarity)
    {
        WeaponProperties basep = new WeaponProperties(1.0f, new BulletProperties(3, 10, 0, 1, 3, new HitProperties(3)), 100);
        //rarity scaled damage by its square root:
        basep.bullet.hitProperties.damage = (int)(GameManager.AttributeScale(rarity) * basep.bullet.hitProperties.damage);
        return basep;
    }

    public override void StartFire()
    {
        charging = true;
        updateProperties();
    }

    public override void StopFire()
    {
        if (charge >= chargeTime)
        {
            //we can shoot now
            ShootAtMouse(0.0f);
        }
        charging = false;
        charge = 0.0f;
    }

    // Start is called before the first frame update
    void Start()
    {
        charging = false;
        charge = 0.0f;
    }

    public override string weaponName()
    {
        return "Bow";
    }

    // Update is called once per frame
    void Update()
    {
        if (charging)
        {
            charge += Time.deltaTime * varyingProperties.fireRateModifier;
            if (charge >= chargeTime)
            {
                GameManager.ScreenShake(0.05f, 0.2f);
                print("ready to shoot!");
            }
        }
    }
}
