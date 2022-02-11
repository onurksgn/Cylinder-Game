using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DailyReward : MonoBehaviour
{
    public bool initialized;
    public long rewardGivingTimeTicks; // long tanýmlamanalarýmýz tickler çok büyük rakamlar olduðu için long tanýmlamasý yapýyoruz.

    public GameObject rewardMenu;

    public Text remainingTimeText;
    public void InitializedDailyReward()
    {
        
        if (PlayerPrefs.HasKey("lastDailyReward"))
        {
            rewardGivingTimeTicks = long.Parse(PlayerPrefs.GetString("lastDailyReward")) + 864000000000; // bir günlük tick sayýsýnýn eþiti 864.000.000.000 dýr 
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
    {//geri sayým sistemi
        if (initialized)
        {
            if (LevelController.Current.startMenu.activeInHierarchy)
            {
                long currentTime = System.DateTime.Now.Ticks;
                long remainingTime = rewardGivingTimeTicks - currentTime; // ne kadar zaman kaldýðýný bulabiliyoruz.
                if (remainingTime <= 0)
                {
                    GiveReward();

                }
                else
                {
                    System.TimeSpan timeSpan = System.TimeSpan.FromTicks(remainingTime); // kalan saati dakikayý günü bulmak için kullanýyoruz.
                    remainingTimeText.text = string.Format("{0}:{1}:{2}",timeSpan.Hours.ToString("D2"), timeSpan.Minutes.ToString("D2"), timeSpan.Seconds.ToString("D2")); // d2yi iki basamaktan küçükse eðer sayý 05 , 06 olarak yazmasý için
                }
            }

        }
    }
    public void TapToReturnButton()
    {
        rewardMenu.SetActive(false);
    }
}
