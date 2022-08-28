using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Item : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public enum ItemType
    {
        RING,
        AMULET,
        CHEST,
        WEAPON
    }

    public static string[] typeNames = new string[] { "Ring", "Amulet", "Chest", "Weapon" };
    public bool equipped;
    public UnityEngine.UI.Image checkmark;
    public UnityEngine.UI.Image background;
    public WeaponProperties itemModifier = WeaponProperties.identity;
    public ItemType itemType;
    public GameObject weapon = null;
    float rarity;
    public bool inShop;

    // Start is called before the first frame update
    void Start()
    {
        background = transform.GetComponent<UnityEngine.UI.Image>();
        background.color = GameManager.ItemColors[(int)itemType];
        checkmark.enabled = false;
        equipped = false;
    }

    public virtual void OnClick()
    {
        if (inShop)
        {
            GameManager.TryBuyItem(this);
            return;
        }

        if (!equipped && GameManager.RegisterItem(this))
        {
            equipped = true;
            checkmark.enabled = true;
        }

        else if (equipped)
        {
            GameManager.UnregisterItemSlot(this);
            equipped = false;
            checkmark.enabled = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        string text = "Item Type: " + typeNames[(int)itemType];
        if (weapon!= null)
        {
            text += "\nWeapon: " + weapon.GetComponent<Weapon>().weaponName();
        }
        text += "\nRarity: " + rarity;
        text += "\n" + GameManager.PropertyString(itemModifier,true);
        GameManager.DisplayString(text);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GameManager.ClearPropertiesDisplay();
    }

    public void GenerateWeaponFromRarity(float rarity)
    {
        List<GameObject> weaponPrefs = GameManager.WeaponPrefabs;
        int i = Random.Range(0, weaponPrefs.Count);
        weapon = weaponPrefs[i];
        itemModifier = weapon.GetComponent<Weapon>().GenerateFromRarity(rarity);
        itemType = ItemType.WEAPON;
    }

    public void GenerateItemFromRarity(float rarity)
    {
        itemModifier.value = (int)GameManager.AttributeScale(rarity * 100);
        this.rarity = rarity;
        if (Random.value < 0.3f)
        {
            GenerateWeaponFromRarity(rarity);
            return;
        }
        itemType = (ItemType)Random.Range(0, 3);
        itemModifier = WeaponProperties.identity;
        if (itemType == ItemType.RING)
        {
            //ring
            if (Random.value < 1.0f)
            {
                itemModifier.bullet.hitProperties.damage += ((int)GameManager.AttributeScale(rarity) + Random.Range(0,2));
            }
        }
        else if (itemType == ItemType.AMULET)
        {
            //amulet
            if (Random.value < 0.5f)
            {
                itemModifier.bullet.bounces += ((int)GameManager.AttributeScale(rarity) + Random.Range(0, 2));
            }
            else
            {
                itemModifier.bullet.pierce += ((int)GameManager.AttributeScale(rarity + 1.0f) + Random.Range(0, 2));
            }
        }
        else
        {
            //chestplate
            itemModifier.fireRateModifier += Mathf.Sqrt(4 + rarity/4)/4;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            Debug.Log("Left click");
        else if (eventData.button == PointerEventData.InputButton.Middle)
            Debug.Log("Middle click");
        else if (!inShop && eventData.button == PointerEventData.InputButton.Right)
        {
            Debug.Log("Right click");
            GameManager.TrySellItem(this);
        }
    }
}
