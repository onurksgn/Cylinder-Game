using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Current; // static de�i�kenler herhangi bir s�n�f taraf�ndan eri�ilebilir.

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
        else if (Input.GetMouseButton(0)) // mouse ile t�klama
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
                _creatingBridgeTimer = 0.01f; // ne kadar s�kl�kla k�pr� par�as� koyaca��n� kodlad�k
                IncrementCylinderVolume(-0.02f); // alt�m�zdan silindirin azalma h�z�.
                GameObject createdBridgePiece = Instantiate(bridgePiecePrefab);
                Vector3 direction = _bridgeSpawner.endReference.transform.position - _bridgeSpawner.startReference.transform.position; // 2 nokta aras�ndaki mesafe vekt�r�nu olu�turduk.
                float distance = direction.magnitude; // magnitude a��rl�k verir. direction yani y�n vekt�r�n�n a��rl���.
                direction = direction.normalized;
                createdBridgePiece.transform.forward = direction;
                float characterDistance = transform.position.z - _bridgeSpawner.startReference.transform.position.z;
                characterDistance = Mathf.Clamp(characterDistance, 0 ,distance);
                Vector3 newPiecePosition = _bridgeSpawner.startReference.transform.position + direction * characterDistance;
                newPiecePosition.x = transform.position.x;
                createdBridgePiece.transform.position = newPiecePosition;

                if (_finished)
                {
                    _scoreTimer -= Time.deltaTime; // e�er k�pr� yarat�rken oyunu bitirdiysen timerdan geriye do�ru saymaya ba�la.
                    if (_scoreTimer < 0) // 0 olduysa yan� time bittiyse g�ncelle.
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
        if (other.tag == "AddCylinder") // �arp��t���nda yok etme kodu tagg� addcylinder ise 
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
    public void IncrementCylinderVolume(float value) // art�� de�eri al�cak
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
    public void CreatCylinder(float value) // aya��m�z�n alt�na silindir yaratmak.
    {
        RidingCylinder createdCylinder = Instantiate(ridingCylinderPrefab, transform).GetComponent<RidingCylinder>(); // instantiate yarat�lm�� olan objeyi d�nd�r�r.
        cylinders.Add(createdCylinder);
        createdCylinder.IncrementCylinderVolume(value); // silindirin ne kadar b�y�k olmas�n� istiyorsak boyutunu g�ncellemi� olduk value de�eriyle.
    }
    public void DestroyCylinder(RidingCylinder cylinder)
    {
        cylinders.Remove(cylinder); // // silindiri yok edebilmek i�in silindir listesinden ��kartmam�z laz�m.
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