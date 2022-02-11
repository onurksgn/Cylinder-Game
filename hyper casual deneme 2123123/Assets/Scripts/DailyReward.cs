using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DailyReward : MonoBehaviour
{
    public bool initialized;
    public long rewardGivingTimeTicks; // long tan�mlamanalar�m�z tickler �ok b�y�k rakamlar oldu�u i�in long tan�mlamas� yap�yoruz.

    public GameObject rewardMenu;

    public Text remainingTimeText;
    public void InitializedDailyReward()
    {
        
        if (PlayerPrefs.HasKey("lastDailyReward"))
        {
            rewardGivingTimeTicks = long.Parse(PlayerPrefs.GetString("lastDailyReward")) + 864000000000; // bir g�nl�k tick say�s�n�n e�iti 864.000.000.000 d�r 
            long currentTime = System.DateTime.Now.Ticks;
            if (currentTime >=rewardGivingTimeTicks)
            {
                GiveReward();
            }
        }
        else
        {
            GiveReward();
        }
        initialized = true;
    }

    public void GiveReward()
    {
        LevelController.Current.GiveMoneyToPlayer(100);
        rewardMenu.SetActive(true);
        PlayerPrefs.SetString("lastDailyReward",System.DateTime.Now.Ticks.ToString());
        rewardGivingTimeTicks = long.Parse(PlayerPrefs.GetString("lastDailyReward")) + 864000000000;

    }
    void Update()
    {//geri say�m sistemi
        if (initialized)
        {
            if (LevelController.Current.startMenu.activeInHierarchy)
            {
                long currentTime = System.DateTime.Now.Ticks;
                long remainingTime = rewardGivingTimeTicks - currentTime; // ne kadar zaman kald���n� bulabiliyoruz.
                if (remainingTime <= 0)
                {
                    GiveReward();

                }
                else
                {
                    System.TimeSpan timeSpan = System.TimeSpan.FromTicks(remainingTime); // kalan saati dakikay� g�n� bulmak i�in kullan�yoruz.
                    remainingTimeText.text = string.Format("{0}:{1}:{2}",timeSpan.Hours.ToString("D2"), timeSpan.Minutes.ToString("D2"), timeSpan.Seconds.ToString("D2")); // d2yi iki basamaktan k���kse e�er say� 05 , 06 olarak yazmas� i�in
                }
            }

        }
    }
    public void TapToReturnButton()
    {
        rewardMenu.SetActive(false);
    }
}
