using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{

    public static GameManager gameManager;

    public static List<WeaponProperties> itemModifiers () {
        List<WeaponProperties> result = new List<WeaponProperties>();
        foreach (Item item in gameManager.equipped)
        {
            if (item != null && item.itemType != Item.ItemType.WEAPON)
            {
                result.Add(item.itemModifier);
            }
            
        }
        return result;
    }

    public static float AttributeScale(float x)
    {
        return 1 + x * 0.8f;
    }

    public static Color[] ItemColors { get => gameManager.itemColors; }

    public static List<GameObject> WeaponPrefabs { get => gameManager.weaponPrefabs;  }

    public static List<Enemy> nearbyEnemies { get => gameManager.enemies; }

    public static void AddEnemy (Enemy enemy) { gameManager.enemies.Add(enemy); }
    public static void RemoveEnemy (Enemy enemy) { gameManager.enemies.Remove(enemy); }

    public static Vector3 MousePosition { get => Camera.main.ScreenToWorldPoint(Input.mousePosition); }

    //instance variables
    public Weapon wep;
    public GameObject wep_obj;
    public float speed;
    public List<Enemy> enemies;
    public Item[] equipped;
    public TextMeshProUGUI propertiesText;
    public List<GameObject> weaponPrefabs;
    public TextMeshProUGUI goldText;
    public GameObject itemPrefab;
    public GameObject inventory;
    public GameObject enemyPrefab;
    public int level;
    public GameObject enemyHolder;
    public Color[] itemColors;
    public int gold;


    private void Awake()
    {
        gameManager = this;
        enemies = new List<Enemy>();
        equipped = new Item[4];
        wep = GetComponentInChildren<Weapon>();
        wep_obj = gameManager.gameObject;
    }

    // Start is called before the first frame update
    void Start()
    {
        gold = 0;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 mov = Vector2.zero;
        mov += Vector2.right * Input.GetAxisRaw("Horizontal");
        mov += Vector2.up * Input.GetAxisRaw("Vertical");
        mov *= Time.deltaTime;
        mov *= speed;
        transform.Translate(mov);

        if (Input.GetMouseButtonDown(0) && wep != null)
        {
            wep.StartFire();
        }
        else if (Input.GetMouseButtonUp(0) && wep != null) {
            wep.StopFire();
        }

        if (Input.GetMouseButtonDown(1))
        {
            SpawnEnemiesInsideCircle(Vector2.zero, 10.0f, 10);
        }
    }

    public static void SpawnEnemiesInsideCircle(Vector2 worldPosition, float radius, int number)
    {
        Vector2 offset;
        for (int i = 0; i < number; i++)
        {
            offset = Random.insideUnitCircle * radius;
            GameObject temp = Instantiate(
                gameManager.enemyPrefab, 
                (Vector3)(worldPosition + offset), 
                Quaternion.identity, gameManager.enemyHolder.transform);

            temp.GetComponent<Enemy>().health = gameManager.level * 5 + 10;
            temp.GetComponent<Enemy>().rarity = gameManager.level;
        }
    }

    public static bool RegisterItem(Item item)
    {
        if (gameManager.equipped[(int)item.itemType] != null)
        {
            print("item is already equipped in this slot!");
            return false;
        }

        //replace weapon component
        if (item.itemType == Item.ItemType.WEAPON)
        {
            gameManager.wep = Instantiate(item.weapon, gameManager.wep_obj.transform).GetComponent<Weapon>();
            gameManager.wep.base_properties = item.itemModifier;
        }

        gameManager.equipped[(int)item.itemType] = item;
        WepUpdate();
        return true;
    }

    public static bool UnregisterItemSlot(Item item)
    {
        //use this to remove an item to equip another item
        if (gameManager.equipped[(int)item.itemType] == null)
        {
            return false;
        }
        if (item.itemType == Item.ItemType.WEAPON)
        {
            Destroy(gameManager.wep.gameObject);
            gameManager.wep = null;
        }
        gameManager.equipped[(int)item.itemType] = null;
        WepUpdate();
        return true;
    }

    public static void ScreenShake(float time, float magnetude)
    {
        Camera.main.transform.GetComponent<CameraScript>().Shake(time, magnetude);
    }

    public static void WepUpdate()
    {
        if (gameManager.wep != null)
        {
            gameManager.wep.updateProperties();
        }
    }

    public static string PropertyString(WeaponProperties properties)
    {
        return PropertyString(properties, false);
    }

    public static string PropertyString(WeaponProperties p, bool strict)
    {
        WeaponProperties c = WeaponProperties.identity;
        string text = "";
        text += !strict || p.fireRateModifier != c.fireRateModifier ? "\nFire Rate: " + p.fireRateModifier : "";
        text += !strict || p.bullet.hitProperties.damage != c.bullet.hitProperties.damage ? "\nDamage: " + p.bullet.hitProperties.damage : "";
        text += !strict || p.bullet.bounces != c.bullet.bounces ? "\nBounce: " + p.bullet.bounces : "";
        text += !strict || p.bullet.pierce != c.bullet.pierce ? "\nPierce: " + p.bullet.pierce : "";
        text += !strict || p.bullet.size != c.bullet.size ? "\nSize: " + p.bullet.size : "";
        text += !strict || p.bullet.speed != c.bullet.speed ? "\nSpeed: " + p.bullet.speed : "";
        text += true || !strict || p.value != c.value ? "\nValue: " + p.value : "";

        return text;
    }

    public static void DisplayString(string s)
    {
        gameManager.propertiesText.text = s;
    }

    public static void ClearPropertiesDisplay()
    {
        //gameManager.propertiesText.enabled = false;
        if (gameManager.wep != null)
        {
            GameManager.DisplayString(PropertyString(gameManager.wep.varyingProperties));
        }
        else
        {
            DisplayString(PropertyString(WeaponProperties.identity));
        }
    }

    public static GameObject CreateBlankItem()
    {
        return Instantiate(gameManager.itemPrefab, gameManager.inventory.transform);
    }

    public static void TryBuyItem(Item item)
    {
        if (gameManager.gold >= item.itemModifier.value)
        {
            item.inShop = false;
            UpdateGold(-item.itemModifier.value);
            item.itemModifier.value = 0;
            item.transform.SetParent(gameManager.inventory.transform, false);
        }
    }

    public static void TrySellItem(Item item)
    {
        if (!item.equipped)
        {
            UpdateGold(item.itemModifier.value);
            Destroy(item.gameObject);
        }
    }

    public static void UpdateGold(int delta)
    {
        gameManager.gold += delta;
        gameManager.goldText.text = "Gold: " + gameManager.gold;
    }
}
