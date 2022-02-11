using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RidingCylinder : MonoBehaviour
{
    private bool _fiiled;
    private float _value;
    // Start is called before the first frame update
    public void IncrementCylinderVolume(float value)
    {
        _value += value;
        if (_value > 1)
        {
            float leftvalue = _value - 1;
            int cylindercount = PlayerController.Current.cylinders.Count;
            transform.localPosition = new Vector3(transform.localPosition.x, -0.5f * (cylindercount - 1) - 0.25f, transform.localPosition.z);
            transform.localScale = new Vector3(0.5f, transform.localScale.y, 0.5f);
            PlayerController.Current.CreatCylinder(leftvalue);  // 1'den ne kadar büyükse o büyüklükte bir silindir yarat.
        }
        else if (_value < 0)
        {
            PlayerController.Current.DestroyCylinder(this);
            // karakterimize bu silindiri yok etmesini söyle
        }
        else
        {
            int cylindercount = PlayerController.Current.cylinders.Count;
            transform.localPosition = new Vector3(transform.localPosition.x, -0.5f * (cylindercount - 1) - 0.25f * _value, transform.localPosition.z);
            transform.localScale = new Vector3(0.5f * _value, transform.localScale.y, 0.5f * _value);
            // silindirin boyutunu güncelleyeceðiz.
        }
    }
}
