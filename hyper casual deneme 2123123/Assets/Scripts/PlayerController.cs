using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Current; // static deðiþkenler herhangi bir sýnýf tarafýndan eriþilebilir.

    public float limitX;

    public float xSpeed;
    public float runningSpeed;
    private float _currentRunningSpeed;

    public GameObject ridingCylinderPrefab;
    public List<RidingCylinder> cylinders;

    private bool _spawninBridge;
    public GameObject bridgePiecePrefab;
    private BridgeSpawner _bridgeSpawner;

    private float _dropSoundTimer;
    public AudioSource cylinderAudioSource,triggerAudioSource, itemAudioSource;
    public AudioClip gatherAudioClip, dropAudioClip, coinAudioClip,buyAudioClip, equipItemAudioClip, unequipItemAudio;

    private float _creatingBridgeTimer;

    public bool _finished;

    private float _scoreTimer = 0;


    public Animator animator;

    private float _lastTouchedX;

    public List<GameObject> wearSpots;

    // Update is called once per frame
    void Update()
    {
        if (LevelController.Current == null  || !LevelController.Current.gameActive)
        {
            return;
        }

        float newX = 0;
        float touchXDelta = 0;
        if (Input.touchCount > 0) // parmakla dokunma
        {
            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                _lastTouchedX = Input.GetTouch(0).position.x;
            }
            else if (Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                touchXDelta = 5 * (Input.GetTouch(0).position.x - _lastTouchedX) / Screen.width;
                _lastTouchedX = Input.GetTouch(0).position.x;
            } 


        }
        else if (Input.GetMouseButton(0)) // mouse ile týklama
        {
            touchXDelta = Input.GetAxis("Mouse X");
            
        }
        newX = transform.position.x + xSpeed * touchXDelta * Time.deltaTime;
        newX = Mathf.Clamp(newX, -limitX, limitX);

        // karakter hareket ettirme kodu
        Vector3 newPosition = new Vector3(newX, transform.position.y, transform.position.z + _currentRunningSpeed * Time.deltaTime);
        transform.position = newPosition;

        if (_spawninBridge)
        {
            PlayDropSound();
            _creatingBridgeTimer -= Time.deltaTime;
            if (_creatingBridgeTimer < 0)
            {
                _creatingBridgeTimer = 0.01f; // ne kadar sýklýkla köprü parçasý koyacaðýný kodladýk
                IncrementCylinderVolume(-0.02f); // altýmýzdan silindirin azalma hýzý.
                GameObject createdBridgePiece = Instantiate(bridgePiecePrefab);
                Vector3 direction = _bridgeSpawner.endReference.transform.position - _bridgeSpawner.startReference.transform.position; // 2 nokta arasýndaki mesafe vektörünu oluþturduk.
                float distance = direction.magnitude; // magnitude aðýrlýk verir. direction yani yön vektörünün aðýrlýðý.
                direction = direction.normalized;
                createdBridgePiece.transform.forward = direction;
                float characterDistance = transform.position.z - _bridgeSpawner.startReference.transform.position.z;
                characterDistance = Mathf.Clamp(characterDistance, 0 ,distance);
                Vector3 newPiecePosition = _bridgeSpawner.startReference.transform.position + direction * characterDistance;
                newPiecePosition.x = transform.position.x;
                createdBridgePiece.transform.position = newPiecePosition;

                if (_finished)
                {
                    _scoreTimer -= Time.deltaTime; // eðer köprü yaratýrken oyunu bitirdiysen timerdan geriye doðru saymaya baþla.
                    if (_scoreTimer < 0) // 0 olduysa yaný time bittiyse güncelle.
                    {
                        _scoreTimer = 0.3f;
                        LevelController.Current.ChangeScore(1);

                    }
                }
            }
        }

    }
    public void ChangedSpeed(float value)
    {
        _currentRunningSpeed = value;
    }
    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "AddCylinder") // çarpýþtýðýnda yok etme kodu taggý addcylinder ise 
        {
            cylinderAudioSource.PlayOneShot(gatherAudioClip,0.1f);
            IncrementCylinderVolume(0.1f);
            Destroy(other.gameObject); // yok etme komutu
        }
        else if (other.tag == "SpawnBridge")
        {
            StartSpawningBridge(other.transform.parent.GetComponent<BridgeSpawner>());
        }
        else if (other.tag == "StopSpawnBridge")
        {
            StopSpawningBridge();

        }
        else if (other.tag == "Finish")
        {
            _finished = true;
            StartSpawningBridge(other.transform.parent.GetComponent<BridgeSpawner>());
        }
        else if (other.tag == "Coin")
        {
            triggerAudioSource.PlayOneShot(coinAudioClip, 0.1f);
            other.tag = "Untagged";
            LevelController.Current.ChangeScore(10);
            Destroy(other.gameObject);
        }


    }     
    public void OnTriggerStay(Collider other)
    {
        if (LevelController.Current.gameActive)
        {
            if (other.tag == "Trap")
            {
                PlayDropSound();
                IncrementCylinderVolume(-Time.fixedDeltaTime);
            }
            else if (other.tag=="Trap2")
            {
                IncrementCylinderVolume(3 * -Time.fixedDeltaTime);
            }
            else if (other.tag == "Trap3")
            {
                IncrementCylinderVolume(1.5f * -Time.fixedDeltaTime);
            }

        }
 
    }
    public void IncrementCylinderVolume(float value) // artýþ deðeri alýcak
    {
        if (cylinders.Count == 0)
        {
            if (value > 0)
            {
                CreatCylinder(value);
            }
            else
            {
                if (_finished)
                {
                    LevelController.Current.FinishGame();

                }
                else
                {
                    Die();
                }
                // gameover
            }
        }
        else
        {
            cylinders[cylinders.Count - 1].IncrementCylinderVolume(value);
        }
    }
    public void Die()
    {
        animator.SetBool("dead", true);
        gameObject.layer = 6;
        Camera.main.transform.SetParent(null);
        LevelController.Current.GameOver();
    }
    public void CreatCylinder(float value) // ayaðýmýzýn altýna silindir yaratmak.
    {
        RidingCylinder createdCylinder = Instantiate(ridingCylinderPrefab, transform).GetComponent<RidingCylinder>(); // instantiate yaratýlmýþ olan objeyi döndürür.
        cylinders.Add(createdCylinder);
        createdCylinder.IncrementCylinderVolume(value); // silindirin ne kadar büyük olmasýný istiyorsak boyutunu güncellemiþ olduk value deðeriyle.
    }
    public void DestroyCylinder(RidingCylinder cylinder)
    {
        cylinders.Remove(cylinder); // // silindiri yok edebilmek için silindir listesinden çýkartmamýz lazým.
        Destroy(cylinder.gameObject); // oyun sahnesinden yok ettik.
    }

    public void StartSpawningBridge (BridgeSpawner spawner)
    {
        _bridgeSpawner = spawner;
        _spawninBridge = true;

    }
    public void StopSpawningBridge()
    {
        _spawninBridge = false;
    }
    
    public void PlayDropSound()
    {
        _dropSoundTimer -= Time.deltaTime;
        if (_dropSoundTimer < 0)
        {
            _dropSoundTimer = 0.15f;
            cylinderAudioSource.PlayOneShot(dropAudioClip, 0.1f);
        }
    }
}