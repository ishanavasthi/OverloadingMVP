using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Owns the small game loop: drive, win at the finish, lose by tipping or crashing,
/// and restart with R. This keeps state decisions out of the truck physics script.
/// </summary>
public sealed class GameManager : MonoBehaviour
{
    private const string GameOverTip = "Tip: don't press throttle all the way. The truck may fall and topple because it is OVERLOADED!";
    private const string WinMessage = "Congratulations! You delivered the overloaded truck without toppling.";

    [Header("References")]
    [SerializeField] private TruckController truck = null;
    [SerializeField] private Text statusText = null;
    [SerializeField] private Text hintText = null;
    [SerializeField] private Text speedText = null;
    [SerializeField] private Text balanceText = null;
    [SerializeField] private Text debugText = null;

    [Header("Lose Conditions")]
    [SerializeField] private float toppleAngle = 65f;
    [SerializeField] private float toppleDelay = 0.8f;
    [SerializeField] private float fallY = -4f;

    [Header("Win Conditions")]
    [SerializeField] private float finishZ = 61.5f;

    private GameState state = GameState.Playing;
    private float toppleTimer;
    private Vector3 startPosition;
    private Quaternion startRotation;
    private string modalTitle = "";
    private string modalBody = "";
    private GUIStyle modalTitleStyle;
    private GUIStyle modalBodyStyle;
    private GUIStyle modalBoxStyle;

    private enum GameState
    {
        Playing,
        Won,
        Lost
    }

    private void Awake()
    {
        if (truck == null)
        {
            truck = FindAnyObjectByType<TruckController>();
        }

        Debug.Log($"GameManager awake. Truck found={truck != null}");

        if (truck != null)
        {
            startPosition = truck.transform.position;
            startRotation = truck.transform.rotation;
        }
    }

    private void Start()
    {
        SetPlayingUi();
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard != null && keyboard.rKey.wasPressedThisFrame)
        {
            Restart();
            return;
        }

        if (state != GameState.Playing || truck == null)
        {
            return;
        }

        UpdateHud();
        CheckTopple();
        CheckFall();
        CheckFinishDistance();
    }

    public void Win()
    {
        if (state != GameState.Playing)
        {
            return;
        }

        state = GameState.Won;
        truck.SetControlsEnabled(false);
        StopTruckMotion();
        SetMessage("RACE COMPLETE", $"{WinMessage}\nPress R to restart.");
        SetModal("CONGRATULATIONS!", $"{WinMessage}\n\nPress R to restart.");
    }

    public void Lose(string reason)
    {
        if (state != GameState.Playing)
        {
            return;
        }

        state = GameState.Lost;
        truck.SetControlsEnabled(false);
        StopTruckMotion();
        FreezeTruck();
        SetMessage("GAME OVER", $"{reason}\nPress R to restart.\n{GameOverTip}");
        SetModal("GAME OVER", $"{reason}\n\nPress R to restart.\n\n{GameOverTip}");
    }

    private void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void CheckTopple()
    {
        float uprightDot = Vector3.Dot(truck.transform.up, Vector3.up);
        float angleFromUpright = Vector3.Angle(truck.transform.up, Vector3.up);

        if (angleFromUpright >= toppleAngle || uprightDot < 0.15f)
        {
            toppleTimer += Time.deltaTime;
        }
        else
        {
            toppleTimer = 0f;
        }

        if (toppleTimer >= toppleDelay)
        {
            Lose("The truck tipped over.");
        }
    }

    private void CheckFall()
    {
        if (truck.transform.position.y < fallY)
        {
            Lose("The truck left the road.");
        }
    }

    private void CheckFinishDistance()
    {
        if (truck.transform.position.z >= finishZ)
        {
            Win();
        }
    }

    private void UpdateHud()
    {
        if (speedText != null)
        {
            speedText.text = $"Speed: {truck.CurrentSpeed * 3.6f:0} km/h";
        }

        if (balanceText != null)
        {
            float roll = Mathf.Abs(truck.SignedRollAngle);
            balanceText.text = $"Lean: {roll:0} deg";
        }

        if (debugText != null)
        {
            debugText.text = $"Input W/S: {truck.ThrottleInput:0}  A/D: {truck.SteerInput:0}  Controls: {truck.ControlsEnabled}  Velocity: {truck.Rigidbody.linearVelocity.magnitude:0.00}";
        }
    }

    private void SetPlayingUi()
    {
        state = GameState.Playing;
        toppleTimer = 0f;

        if (truck != null)
        {
            truck.SetControlsEnabled(true);
        }

        SetMessage("OVERLOADING", "W/S drive, A/D steer. Reach the finish without tipping or hitting hazards. R restarts.");
        UpdateHud();
    }

    private void SetMessage(string title, string hint)
    {
        if (statusText != null)
        {
            statusText.text = title;
        }

        if (hintText != null)
        {
            hintText.text = hint;
        }
    }

    private void SetModal(string title, string body)
    {
        modalTitle = title;
        modalBody = body;
    }

    private void StopTruckMotion()
    {
        Rigidbody truckRigidbody = truck.Rigidbody;

        if (truckRigidbody == null)
        {
            return;
        }

        truckRigidbody.linearVelocity = Vector3.zero;
        truckRigidbody.angularVelocity = Vector3.zero;
    }

    private void FreezeTruck()
    {
        Rigidbody truckRigidbody = truck.Rigidbody;

        if (truckRigidbody == null)
        {
            return;
        }

        truckRigidbody.isKinematic = true;
    }

    private void OnGUI()
    {
        if (state == GameState.Playing)
        {
            return;
        }

        EnsureModalStyles();

        float width = Mathf.Min(720f, Screen.width - 48f);
        float height = state == GameState.Won ? 250f : 320f;
        Rect boxRect = new((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);

        GUI.Box(boxRect, GUIContent.none, modalBoxStyle);
        GUI.Label(new Rect(boxRect.x + 24f, boxRect.y + 24f, boxRect.width - 48f, 64f), modalTitle, modalTitleStyle);
        GUI.Label(new Rect(boxRect.x + 24f, boxRect.y + 102f, boxRect.width - 48f, boxRect.height - 126f), modalBody, modalBodyStyle);
    }

    private void EnsureModalStyles()
    {
        if (modalBoxStyle != null)
        {
            return;
        }

        modalBoxStyle = new GUIStyle(GUI.skin.box)
        {
            padding = new RectOffset(24, 24, 24, 24)
        };

        modalTitleStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 42,
            fontStyle = FontStyle.Bold,
            wordWrap = true
        };

        modalTitleStyle.normal.textColor = Color.white;

        modalBodyStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.UpperCenter,
            fontSize = 24,
            wordWrap = true
        };

        modalBodyStyle.normal.textColor = Color.white;
    }
}
