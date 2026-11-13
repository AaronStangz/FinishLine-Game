
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerInteract : MonoBehaviour
{
    

    [Header("Interaction Settings")]
    public Camera playerCamera;
    public float interactRange = 3f;
    public Key interactKey = Key.E;
    public LayerMask interactLayers = ~0;

    public GameObject[] Note;
    public Transform NoteParent;

    MainManager IM;
    public GameObject mainManager;
    public GameObject[] hand;
    public GameObject[] handBar;

    private void Start()
    {
        IM = mainManager.GetComponent<MainManager>();
    }

    void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current[interactKey].wasPressedThisFrame)
        {
            Interact();
        }
    }

    void Interact()
    {
        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactLayers))
        {
            BuyPistol BP =
                hit.collider.GetComponent<BuyPistol>();

            if (BP != null)
            {
                if (mainManager != null && IM.Cash > 75)
                {
                    IM.Cash -= 75;
                    hand[1].gameObject.SetActive(true);
                    handBar[0].gameObject.SetActive(true);
                    Transform parent = NoteParent != null ? NoteParent : transform;
                    Instantiate(Note[0], NoteParent);

                }
            }

            VendingMachine VM =
                hit.collider.GetComponent<VendingMachine>();

            if (VM != null)
            {
                VM.SpawnRandomItem();
            }


        }


    }
}
