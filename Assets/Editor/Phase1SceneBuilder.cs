using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Creates the complete jam-sized MVP scene for Overloading.
/// This is editor-only so the runtime game stays focused on gameplay scripts.
/// </summary>
public static class Phase1SceneBuilder
{
    private const string MenuPath = "Overloading/Build Complete MVP Scene";
    private const string ScenePath = "Assets/Scenes/SampleScene.unity";
    private const string TruckName = "Truck";

    static Phase1SceneBuilder()
    {
        EditorApplication.delayCall += BuildCompleteSceneIfMissing;
    }

    private static void BuildCompleteSceneIfMissing()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || GameObject.Find("FinishLine") != null)
        {
            return;
        }

        BuildCompleteScene();
    }

    [MenuItem(MenuPath)]
    public static void BuildCompleteScene()
    {
        Scene activeScene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        Material roadMaterial = CreateOrGetMaterial("RoadMat", new Color(0.19f, 0.21f, 0.2f));
        Material truckMaterial = CreateOrGetMaterial("TruckMat", new Color(0.9f, 0.12f, 0.08f));
        Material cargoMaterial = CreateOrGetMaterial("CargoMat", new Color(0.1f, 0.42f, 0.78f));
        Material hazardMaterial = CreateOrGetMaterial("HazardMat", new Color(0.95f, 0.75f, 0.08f));
        Material finishMaterial = CreateOrGetMaterial("FinishMat", Color.black);
        Material finishWhiteMaterial = CreateOrGetMaterial("FinishWhiteMat", Color.white);
        Material finishBlackMaterial = CreateOrGetMaterial("FinishBlackMat", Color.black);
        Material markerMaterial = CreateOrGetMaterial("MarkerMat", new Color(0.94f, 0.94f, 0.86f));

        GameObject gameManagerObject = CreateOrGetGameManager();
        TruckController truck = CreateOrGetTruck(truckMaterial, cargoMaterial);
        GameManager gameManager = GetOrAdd<GameManager>(gameManagerObject);

        CreateOrGetGround(roadMaterial);
        CreateRoadMarkers(markerMaterial);
        CreateHazards(gameManager, hazardMaterial);
        CreateFinishLine(gameManager, finishMaterial, finishWhiteMaterial, finishBlackMaterial);
        CreateOrGetLighting();
        CreateOrGetCamera(truck.transform);
        CreateHud(gameManager);
        LinkGameManager(gameManager, truck);

        EditorSceneManager.MarkSceneDirty(activeScene);
        EditorSceneManager.SaveScene(activeScene, ScenePath);
        Debug.Log("Overloading complete MVP scene is ready. Press Play: reach the green finish, avoid yellow hazards, press R to restart.");
    }

    private static GameObject CreateOrGetGameManager()
    {
        GameObject gameManager = GameObject.Find("GameManager");
        return gameManager != null ? gameManager : new GameObject("GameManager");
    }

    private static void CreateOrGetGround(Material roadMaterial)
    {
        GameObject ground = CreateOrGetPrimitive("Ground", PrimitiveType.Cube);
        ground.transform.SetPositionAndRotation(new Vector3(0f, -0.1f, 32f), Quaternion.identity);
        ground.transform.localScale = new Vector3(9f, 0.2f, 132f);
        ground.isStatic = true;

        AssignMaterial(ground, roadMaterial);
    }

    private static TruckController CreateOrGetTruck(Material truckMaterial, Material cargoMaterial)
    {
        GameObject truck = GameObject.Find(TruckName);

        if (truck == null)
        {
            truck = new GameObject(TruckName);
        }

        truck.transform.SetPositionAndRotation(new Vector3(0f, 1.25f, -26f), Quaternion.identity);
        truck.transform.localScale = Vector3.one;

        Rigidbody rigidbody = GetOrAdd<Rigidbody>(truck);
        rigidbody.mass = 1200f;
        rigidbody.linearDamping = 0.2f;
        rigidbody.angularDamping = 1.5f;
        rigidbody.useGravity = true;
        rigidbody.isKinematic = false;
        rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rigidbody.constraints = RigidbodyConstraints.None;

        BoxCollider collider = GetOrAdd<BoxCollider>(truck);
        collider.center = Vector3.zero;
        collider.size = new Vector3(2f, 2f, 4f);

        TruckController controller = GetOrAdd<TruckController>(truck);
        CreateOrUpdateTruckVisual(truck.transform, truckMaterial, cargoMaterial);
        return controller;
    }

    private static void CreateOrUpdateTruckVisual(Transform truckTransform, Material truckMaterial, Material cargoMaterial)
    {
        CreateTruckPart(truckTransform, "TruckVisual_Base", new Vector3(0f, -0.15f, 0f), new Vector3(2f, 1.1f, 4f), truckMaterial);
        CreateTruckPart(truckTransform, "TruckVisual_Cab", new Vector3(0f, 0.65f, 1.15f), new Vector3(1.8f, 1.1f, 1.2f), truckMaterial);
        CreateTruckPart(truckTransform, "TruckVisual_LoadA", new Vector3(0f, 0.85f, -0.55f), new Vector3(1.8f, 0.8f, 2.1f), cargoMaterial);
        CreateTruckPart(truckTransform, "TruckVisual_LoadB", new Vector3(0f, 1.55f, -0.55f), new Vector3(1.45f, 0.65f, 1.65f), cargoMaterial);
    }

    private static void CreateTruckPart(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Material material)
    {
        Transform partTransform = parent.Find(name);
        GameObject part = partTransform != null ? partTransform.gameObject : GameObject.CreatePrimitive(PrimitiveType.Cube);

        part.name = name;
        part.transform.SetParent(parent);
        part.transform.localPosition = localPosition;
        part.transform.localRotation = Quaternion.identity;
        part.transform.localScale = localScale;

        Collider collider = part.GetComponent<Collider>();

        if (collider != null)
        {
            Object.DestroyImmediate(collider);
        }

        AssignMaterial(part, material);
    }

    private static void CreateRoadMarkers(Material markerMaterial)
    {
        GameObject markerRoot = CreateOrGetContainer("RoadMarkers");

        for (int i = 0; i < 11; i++)
        {
            float z = -22f + i * 10f;
            GameObject marker = CreateChildPrimitive(markerRoot.transform, $"CenterMarker_{i:00}", PrimitiveType.Cube);
            marker.transform.SetPositionAndRotation(new Vector3(0f, 0.03f, z), Quaternion.identity);
            marker.transform.localScale = new Vector3(0.18f, 0.04f, 4f);
            AssignMaterial(marker, markerMaterial);
        }
    }

    private static void CreateHazards(GameManager gameManager, Material hazardMaterial)
    {
        GameObject hazardRoot = CreateOrGetContainer("Hazards");

        CreateHazard(hazardRoot.transform, "LeftBarrier", new Vector3(-5f, 0.75f, 32f), new Vector3(0.5f, 1.5f, 132f), "You scraped the roadside barrier.", gameManager, hazardMaterial);
        CreateHazard(hazardRoot.transform, "RightBarrier", new Vector3(5f, 0.75f, 32f), new Vector3(0.5f, 1.5f, 132f), "You scraped the roadside barrier.", gameManager, hazardMaterial);
        CreateHazard(hazardRoot.transform, "Obstacle_LeftGate", new Vector3(-1.85f, 0.65f, -4f), new Vector3(1.4f, 1.3f, 1.4f), "You hit the cargo blocks.", gameManager, hazardMaterial);
        CreateHazard(hazardRoot.transform, "Obstacle_RightGate", new Vector3(2f, 0.65f, 15f), new Vector3(1.5f, 1.3f, 1.5f), "You hit the cargo blocks.", gameManager, hazardMaterial);
        CreateHazard(hazardRoot.transform, "Obstacle_NarrowLeft", new Vector3(-2.7f, 0.65f, 34f), new Vector3(1.4f, 1.3f, 2.2f), "You clipped the narrow gate.", gameManager, hazardMaterial);
        CreateHazard(hazardRoot.transform, "Obstacle_NarrowRight", new Vector3(2.7f, 0.65f, 34f), new Vector3(1.4f, 1.3f, 2.2f), "You clipped the narrow gate.", gameManager, hazardMaterial);
        CreateHazard(hazardRoot.transform, "Obstacle_Final", new Vector3(0f, 0.65f, 49f), new Vector3(1.5f, 1.3f, 1.5f), "You hit the final obstacle.", gameManager, hazardMaterial);
    }

    private static void CreateHazard(Transform parent, string name, Vector3 position, Vector3 scale, string failReason, GameManager gameManager, Material material)
    {
        GameObject hazard = CreateChildPrimitive(parent, name, PrimitiveType.Cube);
        hazard.transform.SetPositionAndRotation(position, Quaternion.identity);
        hazard.transform.localScale = scale;
        AssignMaterial(hazard, material);

        HazardZone hazardZone = GetOrAdd<HazardZone>(hazard);
        SetSerializedObjectReference(hazardZone, "gameManager", gameManager);
        SetSerializedString(hazardZone, "failReason", failReason);
    }

    private static void CreateFinishLine(GameManager gameManager, Material finishMaterial, Material finishWhiteMaterial, Material finishBlackMaterial)
    {
        GameObject finish = CreateOrGetPrimitive("FinishLine", PrimitiveType.Cube);
        finish.transform.SetPositionAndRotation(new Vector3(0f, 1.5f, 62f), Quaternion.identity);
        finish.transform.localScale = new Vector3(8f, 3f, 0.5f);
        AssignMaterial(finish, finishMaterial);

        BoxCollider finishCollider = GetOrAdd<BoxCollider>(finish);
        finishCollider.isTrigger = true;

        FinishLine finishLine = GetOrAdd<FinishLine>(finish);
        SetSerializedObjectReference(finishLine, "gameManager", gameManager);

        CreateFinishCheckers(finish.transform, finishWhiteMaterial, finishBlackMaterial);
    }

    private static void CreateFinishCheckers(Transform finishTransform, Material whiteMaterial, Material blackMaterial)
    {
        const int columns = 12;
        const int rows = 3;

        float width = finishTransform.localScale.x;
        float height = finishTransform.localScale.y;
        float tileWidth = width / columns;
        float tileHeight = height / rows;

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                GameObject tile = CreateChildPrimitive(finishTransform, $"Checker_{row:00}_{column:00}", PrimitiveType.Cube);
                tile.transform.localPosition = new Vector3(
                    -width * 0.5f + tileWidth * 0.5f + column * tileWidth,
                    -height * 0.5f + tileHeight * 0.5f + row * tileHeight,
                    -0.56f);
                tile.transform.localRotation = Quaternion.identity;
                tile.transform.localScale = new Vector3(tileWidth, tileHeight, 0.08f);

                Collider collider = tile.GetComponent<Collider>();

                if (collider != null)
                {
                    Object.DestroyImmediate(collider);
                }

                AssignMaterial(tile, (row + column) % 2 == 0 ? whiteMaterial : blackMaterial);
            }
        }
    }

    private static void CreateOrGetLighting()
    {
        Light existingLight = Object.FindAnyObjectByType<Light>();

        if (existingLight != null)
        {
            existingLight.transform.SetPositionAndRotation(Vector3.zero, Quaternion.Euler(50f, -30f, 0f));
            existingLight.type = LightType.Directional;
            existingLight.intensity = 1.2f;
            return;
        }

        GameObject lightObject = new("Directional Light");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
    }

    private static void CreateOrGetCamera(Transform truckTransform)
    {
        Camera camera = Camera.main;

        if (camera == null)
        {
            GameObject cameraObject = new("Main Camera");
            camera = cameraObject.AddComponent<Camera>();
            cameraObject.tag = "MainCamera";
        }

        camera.transform.SetPositionAndRotation(new Vector3(0f, 6f, -35f), Quaternion.Euler(25f, 0f, 0f));
        camera.fieldOfView = 65f;

        AudioListener audioListener = camera.GetComponent<AudioListener>();

        if (audioListener == null)
        {
            camera.gameObject.AddComponent<AudioListener>();
        }

        CameraFollow cameraFollow = GetOrAdd<CameraFollow>(camera.gameObject);
        SetSerializedObjectReference(cameraFollow, "target", truckTransform);
    }

    private static void CreateHud(GameManager gameManager)
    {
        GameObject canvasObject = GameObject.Find("HUD");

        if (canvasObject == null)
        {
            canvasObject = new GameObject("HUD");
        }

        Canvas canvas = GetOrAdd<Canvas>(canvasObject);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = GetOrAdd<CanvasScaler>(canvasObject);
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GetOrAdd<GraphicRaycaster>(canvasObject);

        Text statusText = CreateText(canvasObject.transform, "StatusText", "OVERLOADING", 42, TextAnchor.UpperLeft, new Vector2(24f, -24f), new Vector2(760f, 70f));
        Text hintText = CreateText(canvasObject.transform, "HintText", "W/S drive, A/D steer. Reach the finish. R restarts.", 24, TextAnchor.UpperLeft, new Vector2(24f, -84f), new Vector2(1040f, 72f));
        Text debugText = CreateText(canvasObject.transform, "DebugText", "Input W/S: 0  A/D: 0  Controls: True  Velocity: 0.00", 22, TextAnchor.UpperLeft, new Vector2(24f, -154f), new Vector2(1040f, 48f));
        Text speedText = CreateText(canvasObject.transform, "SpeedText", "Speed: 0 km/h", 28, TextAnchor.UpperRight, new Vector2(-24f, -24f), new Vector2(360f, 48f));
        Text balanceText = CreateText(canvasObject.transform, "BalanceText", "Lean: 0 deg", 28, TextAnchor.UpperRight, new Vector2(-24f, -70f), new Vector2(360f, 48f));

        SetSerializedObjectReference(gameManager, "statusText", statusText);
        SetSerializedObjectReference(gameManager, "hintText", hintText);
        SetSerializedObjectReference(gameManager, "debugText", debugText);
        SetSerializedObjectReference(gameManager, "speedText", speedText);
        SetSerializedObjectReference(gameManager, "balanceText", balanceText);
    }

    private static Text CreateText(Transform parent, string name, string text, int fontSize, TextAnchor alignment, Vector2 anchoredPosition, Vector2 size)
    {
        Transform existing = parent.Find(name);
        GameObject textObject = existing != null ? existing.gameObject : new GameObject(name);
        textObject.transform.SetParent(parent, false);

        Text textComponent = GetOrAdd<Text>(textObject);
        textComponent.text = text;
        textComponent.font = GetBuiltinFont();
        textComponent.fontSize = fontSize;
        textComponent.color = Color.white;
        textComponent.alignment = alignment;
        textComponent.horizontalOverflow = HorizontalWrapMode.Wrap;
        textComponent.verticalOverflow = VerticalWrapMode.Overflow;

        RectTransform rectTransform = GetOrAdd<RectTransform>(textObject);
        bool rightAligned = alignment == TextAnchor.UpperRight || alignment == TextAnchor.MiddleRight || alignment == TextAnchor.LowerRight;
        rectTransform.anchorMin = rightAligned ? new Vector2(1f, 1f) : new Vector2(0f, 1f);
        rectTransform.anchorMax = rectTransform.anchorMin;
        rectTransform.pivot = rightAligned ? new Vector2(1f, 1f) : new Vector2(0f, 1f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = size;

        return textComponent;
    }

    private static void LinkGameManager(GameManager gameManager, TruckController truck)
    {
        SetSerializedObjectReference(gameManager, "truck", truck);
    }

    private static GameObject CreateOrGetContainer(string name)
    {
        GameObject container = GameObject.Find(name);
        return container != null ? container : new GameObject(name);
    }

    private static GameObject CreateOrGetPrimitive(string name, PrimitiveType primitiveType)
    {
        GameObject gameObject = GameObject.Find(name);

        if (gameObject != null)
        {
            return gameObject;
        }

        gameObject = GameObject.CreatePrimitive(primitiveType);
        gameObject.name = name;
        return gameObject;
    }

    private static GameObject CreateChildPrimitive(Transform parent, string name, PrimitiveType primitiveType)
    {
        Transform existing = parent.Find(name);

        if (existing != null)
        {
            return existing.gameObject;
        }

        GameObject gameObject = GameObject.CreatePrimitive(primitiveType);
        gameObject.name = name;
        gameObject.transform.SetParent(parent);
        return gameObject;
    }

    private static Material CreateOrGetMaterial(string materialName, Color color)
    {
        string materialPath = $"Assets/{materialName}.mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);

        if (material != null)
        {
            return material;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");

        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        material = new Material(shader)
        {
            name = materialName,
            color = color
        };

        AssetDatabase.CreateAsset(material, materialPath);
        AssetDatabase.SaveAssets();
        return material;
    }

    private static Font GetBuiltinFont()
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return font != null ? font : Resources.GetBuiltinResource<Font>("Arial.ttf");
    }

    private static void AssignMaterial(GameObject gameObject, Material material)
    {
        Renderer renderer = gameObject.GetComponent<Renderer>();

        if (renderer != null)
        {
            renderer.sharedMaterial = material;
        }
    }

    private static void SetSerializedObjectReference(Object target, string propertyName, Object value)
    {
        SerializedObject serializedObject = new(target);
        serializedObject.FindProperty(propertyName).objectReferenceValue = value;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetSerializedString(Object target, string propertyName, string value)
    {
        SerializedObject serializedObject = new(target);
        serializedObject.FindProperty(propertyName).stringValue = value;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static T GetOrAdd<T>(GameObject gameObject) where T : Component
    {
        T component = gameObject.GetComponent<T>();
        return component != null ? component : gameObject.AddComponent<T>();
    }
}
