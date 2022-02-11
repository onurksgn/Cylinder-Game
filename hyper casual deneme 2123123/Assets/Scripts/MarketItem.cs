using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MarketItem : MonoBehaviour
{
    public int itemId, wearId;
    public int price;

    public Button buyButton, equipButton, unequipButton;
    public Text priceText;

    public GameObject itemPrefab;
    public bool HasItem()
    {
        // 0 : Daha sat�n al�nmam��.
        // 1 : sat�n al�nm�� giyilmemi�
        // 2 : sat�n al�nm�s giyilmi�
        bool hasItem = PlayerPrefs.GetInt("item" + itemId.ToString()) !=0;
        return hasItem;
    }
    public bool IsEquipped()
    {
        // 0 : Daha sat�n al�nmam��.
        // 1 : sat�n al�nm�� giyilmemi�
        // 2 : sat�n al�nm�s giyilmi�
        bool equippedItem = PlayerPrefs.GetInt("item" + itemId.ToString()) == 2;
        return equippedItem;
    }
    public void InitializeItem()
    {
        priceText.text = price.ToString();
        if (HasItem())
        {
            buyButton.gameObject.SetActive(false);
            if (IsEquipped())
            {
                Equip�tem();
            }
            else
            {
                equipButton.gameObject.SetActive(true);
            }
        }
        else
        {
            buyButton.gameObject.SetActive(true);
        }
    } // tan�mlama foksiyonu
    public void BuyItem()
    {
        if (!HasItem())
        {
            int money = PlayerPrefs.GetInt("money");
            if (money >= price)
            {
                PlayerController.Current.itemAudioSource.PlayOneShot(PlayerController.Current.buyAudioClip, 0.1f);
                LevelController.Current.GiveMoneyToPlayer(-price);
                PlayerPrefs.SetInt("item" + itemId.ToString(), 1);
                buyButton.gameObject.SetActive(false);
                equipButton.gameObject.SetActive(true);
            }
        }
    } // sat�n alma fonksiyonu
    public void Equip�tem()
    {
        UnequipItem();
        MarketController.Current.equippedItems[wearId] = Instantiate(itemPrefab,PlayerController.Current.wearSpots[wearId].transform).GetComponent<Item>();
        MarketController.Current.equippedItems[wearId].itemId = itemId;
        equipButton.gameObject.SetActive(false);
        unequipButton.gameObject.SetActive(true);
        PlayerPrefs.SetInt("item"+ itemId.ToString(), 2);
    }
    public void UnequipItem()
    {
        Item equippedItem = MarketController.Current.equippedItems[wearId];
        if (equippedItem != null)
        {
            MarketItem marketItem = MarketController.Current.items[equippedItem.itemId]; // market controllerdak� tum e�yalara eri�yoruz .  giyili olan e�yan�n ID sine kar��l�k gelene market Id objesini bana ver.
            PlayerPrefs.SetInt("item" + marketItem.itemId, 1);
            marketItem.equipButton.gameObject.SetActive(true);
            marketItem.unequipButton.gameObject.SetActive(false);
            Destroy(equippedItem.gameObject); 
        }
    }
    public void EquipItemButton()
    {
        PlayerController.Current.itemAudioSource.PlayOneShot(PlayerController.Current.equipItemAudioClip, 0.1f);
        Equip�tem();
    }
    public void UnequipItemButton()
    {
        PlayerController.Current.itemAudioSource.PlayOneShot(PlayerController.Current.unequipItemAudio, 0.1f);
        UnequipItem();
    }
}
 