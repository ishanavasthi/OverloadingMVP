using UnityEngine;

/// <summary>
/// Trigger volume that wins the run when the truck reaches the end of the road.
/// </summary>
public sealed class FinishLine : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private int checkerColumns = 12;
    [SerializeField] private int checkerRows = 3;

    private void Awake()
    {
        if (gameManager == null)
        {
            gameManager = FindAnyObjectByType<GameManager>();
        }

        BuildCheckerVisual();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<TruckController>() != null)
        {
            gameManager.Win();
        }
    }

    private void BuildCheckerVisual()
    {
        Renderer ownRenderer = GetComponent<Renderer>();

        if (ownRenderer != null)
        {
            ownRenderer.enabled = false;
        }

        if (transform.Find("Checker_00_00") != null)
        {
            return;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");

        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        Material whiteMaterial = new(shader)
        {
            color = Color.white
        };

        Material blackMaterial = new(shader)
        {
            color = Color.black
        };

        float width = transform.localScale.x;
        float height = transform.localScale.y;
        float tileWidth = width / checkerColumns;
        float tileHeight = height / checkerRows;

        for (int row = 0; row < checkerRows; row++)
        {
            for (int column = 0; column < checkerColumns; column++)
            {
                GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tile.name = $"Checker_{row:00}_{column:00}";
                tile.transform.SetParent(transform, false);
                tile.transform.localPosition = new Vector3(
                    -width * 0.5f + tileWidth * 0.5f + column * tileWidth,
                    -height * 0.5f + tileHeight * 0.5f + row * tileHeight,
                    -0.56f);
                tile.transform.localScale = new Vector3(tileWidth, tileHeight, 0.08f);

                Collider tileCollider = tile.GetComponent<Collider>();

                if (tileCollider != null)
                {
                    Destroy(tileCollider);
                }

                Renderer tileRenderer = tile.GetComponent<Renderer>();

                if (tileRenderer != null)
                {
                    tileRenderer.material = (row + column) % 2 == 0 ? whiteMaterial : blackMaterial;
                }
            }
        }
    }
}
