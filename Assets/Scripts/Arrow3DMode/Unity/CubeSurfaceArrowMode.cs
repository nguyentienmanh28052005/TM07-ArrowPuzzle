using Arrow3DMode.Core;
using System.Collections.Generic;
using UnityEngine;

namespace Arrow3DMode.Unity
{
    [ExecuteAlways]
    public sealed class CubeSurfaceArrowMode : MonoBehaviour
    {
        private const string GeneratedMaterialName = "Arrow3D Generated Material";

        [Header("Surface")]
        [SerializeField] private float cubeSize = 3f;
        [SerializeField] private float surfaceOffset = 0.035f;
        [SerializeField] private SurfacePoint startHead = new SurfacePoint(CubeFace.PositiveZ, 0.14f, 0.52f);
        [SerializeField] private SurfaceDirection startDirection = SurfaceDirection.NegativeU;

        [Header("Arrow Body")]
        [SerializeField, Min(3)] private int bodyMarkerCount = 10;
        [SerializeField, Min(0.05f)] private float markerSpacing = 0.42f;
        [SerializeField, Min(0.01f)] private float pathBuildStep = 0.06f;
        [SerializeField, Min(0.01f)] private float moveSpeed = 1.25f;
        [SerializeField] private bool autoRunInPlayMode = true;

        [Header("Input")]
        [SerializeField] private bool enableKeyboardInput = true;
        [SerializeField] private bool clickTogglesRun = true;
        [SerializeField] private bool enableRightMouseRotate = true;
        [SerializeField, Min(0.01f)] private float mouseRotateSpeed = 0.25f;

        [Header("Visuals")]
        [SerializeField] private Color cubeColor = new Color(0.12f, 0.18f, 0.24f, 0.42f);
        [SerializeField] private Color arrowColor = new Color(0.1f, 0.85f, 1f, 1f);
        [SerializeField] private Color headColor = new Color(1f, 0.78f, 0.16f, 1f);
        [SerializeField] private Color tailColor = new Color(0.62f, 0.78f, 1f, 1f);
        [SerializeField] private Material cubeMaterialOverride;
        [SerializeField] private Material arrowMaterialOverride;
        [SerializeField] private Material headMaterialOverride;
        [SerializeField] private Material tailMaterialOverride;
        [SerializeField, Min(0.01f)] private float bodyLineWidth = 0.08f;
        [SerializeField, Min(0.02f)] private float bodyMarkerSize = 0.15f;
        [SerializeField, Min(0.04f)] private float headLength = 0.34f;
        [SerializeField, Min(0.04f)] private float headRadius = 0.14f;

        [SerializeField, HideInInspector] private Transform visualRoot;
        [SerializeField, HideInInspector] private Transform cubeVisual;
        [SerializeField, HideInInspector] private LineRenderer bodyLine;
        [SerializeField, HideInInspector] private Transform headVisual;
        [SerializeField, HideInInspector] private Transform tailVisual;
        [SerializeField, HideInInspector] private Transform markerRoot;

        private readonly SurfaceArrowPath _path = new SurfaceArrowPath();
        private readonly List<SurfaceSample> _bodySamples = new List<SurfaceSample>(32);
        private readonly List<Transform> _bodyMarkers = new List<Transform>(32);

        private Material _cubeMaterial;
        private Material _arrowMaterial;
        private Material _headMaterial;
        private Material _tailMaterial;
        private Mesh _headMesh;
        private bool _isRunning;
        private bool _initialized;
        private bool _rightMouseRotating;
        private Vector2 _pointerDownPosition;
        private Vector2 _lastRightMousePosition;

        private void Reset()
        {
            cubeSize = 3f;
            surfaceOffset = 0.035f;
            startHead = new SurfacePoint(CubeFace.PositiveZ, 0.14f, 0.52f);
            startDirection = SurfaceDirection.NegativeU;
            bodyMarkerCount = 10;
            markerSpacing = 0.42f;
            pathBuildStep = 0.06f;
            moveSpeed = 1.25f;
            autoRunInPlayMode = true;
            enableRightMouseRotate = true;
            mouseRotateSpeed = 0.25f;
            Initialize(true);
        }

        private void OnValidate()
        {
            cubeSize = Mathf.Max(0.1f, cubeSize);
            surfaceOffset = Mathf.Max(0f, surfaceOffset);
            bodyMarkerCount = Mathf.Max(3, bodyMarkerCount);
            markerSpacing = Mathf.Max(0.05f, markerSpacing);
            pathBuildStep = Mathf.Max(0.01f, pathBuildStep);
            moveSpeed = Mathf.Max(0.01f, moveSpeed);
            mouseRotateSpeed = Mathf.Max(0.01f, mouseRotateSpeed);
            startHead = new SurfacePoint(startHead.Face, Mathf.Clamp01(startHead.U), Mathf.Clamp01(startHead.V));

            if (!Application.isPlaying)
            {
                Initialize(true);
            }
        }

        private void OnEnable()
        {
            Initialize(true);
            _isRunning = Application.isPlaying && autoRunInPlayMode;
        }

        private void Update()
        {
            Initialize(false);

            if (Application.isPlaying)
            {
                HandleInput();
                if (_isRunning)
                {
                    _path.Move(moveSpeed * Time.deltaTime);
                }
            }

            ApplyVisuals();
        }

        public void ResetArrow()
        {
            SurfacePose pose = CubeSurfaceMath.CreatePose(startHead, startDirection);
            float length = Mathf.Max(markerSpacing, (bodyMarkerCount - 1) * markerSpacing);
            _path.Reset(pose, cubeSize, length, pathBuildStep);
            ApplyVisuals();
        }

        public void TurnLeft()
        {
            _path.TurnHead(false);
            ApplyVisuals();
        }

        public void TurnRight()
        {
            _path.TurnHead(true);
            ApplyVisuals();
        }

        public void SetRunning(bool running)
        {
            _isRunning = running;
        }

        private void Initialize(bool resetPath)
        {
            EnsureVisuals();
            EnsureMaterials();
            EnsureBodyMarkers();
            ApplyStaticVisuals();

            if (resetPath || !_initialized)
            {
                ResetArrow();
                _initialized = true;
            }
        }

        private void HandleInput()
        {
            if (enableKeyboardInput)
            {
                if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    TurnLeft();
                }

                if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
                {
                    TurnRight();
                }

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    _isRunning = !_isRunning;
                }

                if (Input.GetKeyDown(KeyCode.R))
                {
                    ResetArrow();
                }
            }

            HandleRotationInput();
            HandlePointerInput();
        }

        private void HandleRotationInput()
        {
            if (!enableRightMouseRotate)
            {
                return;
            }

            if (Input.GetMouseButtonDown(1))
            {
                _rightMouseRotating = true;
                _lastRightMousePosition = Input.mousePosition;
            }

            if (Input.GetMouseButtonUp(1))
            {
                _rightMouseRotating = false;
            }

            if (!_rightMouseRotating || !Input.GetMouseButton(1))
            {
                return;
            }

            Vector2 currentPosition = Input.mousePosition;
            Vector2 delta = currentPosition - _lastRightMousePosition;
            _lastRightMousePosition = currentPosition;

            if (delta.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            Transform cameraTransform = Camera.main != null ? Camera.main.transform : null;
            Vector3 horizontalAxis = cameraTransform != null ? cameraTransform.right : Vector3.right;

            transform.Rotate(Vector3.up, -delta.x * mouseRotateSpeed, Space.World);
            transform.Rotate(horizontalAxis, delta.y * mouseRotateSpeed, Space.World);
        }

        private void HandlePointerInput()
        {
            if (!clickTogglesRun)
            {
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                _pointerDownPosition = Input.mousePosition;
            }

            if (Input.GetMouseButtonUp(0))
            {
                Vector2 delta = (Vector2)Input.mousePosition - _pointerDownPosition;
                if (delta.magnitude < 20f)
                {
                    _isRunning = !_isRunning;
                }
                else if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                {
                    if (delta.x > 0f) TurnRight();
                    else TurnLeft();
                }
            }
        }

        private void EnsureVisuals()
        {
            if (visualRoot == null)
            {
                visualRoot = FindOrCreateChild(transform, "Arrow3D_VisualRoot");
            }

            if (cubeVisual == null)
            {
                cubeVisual = visualRoot.Find("Cube Surface");
                if (cubeVisual == null)
                {
                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.name = "Cube Surface";
                    cube.transform.SetParent(visualRoot, false);
                    cubeVisual = cube.transform;
                }
            }

            if (bodyLine == null)
            {
                Transform lineTransform = visualRoot.Find("Arrow Body Line");
                if (lineTransform == null)
                {
                    GameObject lineObject = new GameObject("Arrow Body Line");
                    lineObject.transform.SetParent(visualRoot, false);
                    lineTransform = lineObject.transform;
                }

                bodyLine = lineTransform.GetComponent<LineRenderer>();
                if (bodyLine == null)
                {
                    bodyLine = lineTransform.gameObject.AddComponent<LineRenderer>();
                }
            }

            if (headVisual == null)
            {
                Transform existing = visualRoot.Find("Arrow Head");
                if (existing == null)
                {
                    GameObject head = new GameObject("Arrow Head");
                    head.transform.SetParent(visualRoot, false);
                    head.AddComponent<MeshFilter>();
                    head.AddComponent<MeshRenderer>();
                    existing = head.transform;
                }

                headVisual = existing;
            }

            if (tailVisual == null)
            {
                Transform existing = visualRoot.Find("Arrow Tail");
                if (existing == null)
                {
                    GameObject tail = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    tail.name = "Arrow Tail";
                    tail.transform.SetParent(visualRoot, false);
                    DisableCollider(tail);
                    existing = tail.transform;
                }

                tailVisual = existing;
            }

            if (markerRoot == null)
            {
                markerRoot = visualRoot.Find("Body Markers");
                if (markerRoot == null)
                {
                    markerRoot = new GameObject("Body Markers").transform;
                    markerRoot.SetParent(visualRoot, false);
                }
            }
        }

        private void EnsureMaterials()
        {
            if (_cubeMaterial == null)
            {
                _cubeMaterial = CreateMaterial(cubeColor);
            }
            else
            {
                SetMaterialColor(_cubeMaterial, cubeColor);
            }

            if (_arrowMaterial == null)
            {
                _arrowMaterial = CreateMaterial(arrowColor);
            }
            else
            {
                SetMaterialColor(_arrowMaterial, arrowColor);
            }

            if (_headMaterial == null)
            {
                _headMaterial = CreateMaterial(headColor);
            }
            else
            {
                SetMaterialColor(_headMaterial, headColor);
            }

            if (_tailMaterial == null)
            {
                _tailMaterial = CreateMaterial(tailColor);
            }
            else
            {
                SetMaterialColor(_tailMaterial, tailColor);
            }

            if (_headMesh == null)
            {
                _headMesh = CreateConeMesh(18);
            }
        }

        private void EnsureBodyMarkers()
        {
            _bodyMarkers.Clear();
            if (markerRoot == null)
            {
                return;
            }

            for (int i = 0; i < markerRoot.childCount; i++)
            {
                _bodyMarkers.Add(markerRoot.GetChild(i));
            }

            while (_bodyMarkers.Count < bodyMarkerCount)
            {
                GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                marker.name = "Body Marker " + _bodyMarkers.Count.ToString("00");
                marker.transform.SetParent(markerRoot, false);
                DisableCollider(marker);
                _bodyMarkers.Add(marker.transform);
            }

            for (int i = 0; i < _bodyMarkers.Count; i++)
            {
                bool active = i < bodyMarkerCount;
                if (_bodyMarkers[i] != null && _bodyMarkers[i].gameObject.activeSelf != active)
                {
                    _bodyMarkers[i].gameObject.SetActive(active);
                }
            }
        }

        private void ApplyStaticVisuals()
        {
            if (cubeVisual != null)
            {
                cubeVisual.localPosition = Vector3.zero;
                cubeVisual.localRotation = Quaternion.identity;
                cubeVisual.localScale = Vector3.one * cubeSize;

                Renderer renderer = cubeVisual.GetComponent<Renderer>();
                ApplyRendererMaterial(renderer, cubeMaterialOverride, _cubeMaterial);
            }

            if (bodyLine != null)
            {
                bodyLine.useWorldSpace = true;
                bodyLine.startWidth = bodyLineWidth;
                bodyLine.endWidth = bodyLineWidth;
                bodyLine.widthMultiplier = 1f;
                bodyLine.numCornerVertices = 4;
                bodyLine.numCapVertices = 8;
                bodyLine.alignment = LineAlignment.View;
                bodyLine.textureMode = LineTextureMode.Stretch;
                ApplyLineMaterial(bodyLine, arrowMaterialOverride, _arrowMaterial);
                bodyLine.startColor = arrowColor;
                bodyLine.endColor = arrowColor;
            }

            MeshFilter meshFilter = headVisual != null ? headVisual.GetComponent<MeshFilter>() : null;
            if (meshFilter != null) meshFilter.sharedMesh = _headMesh;

            Renderer headRenderer = headVisual != null ? headVisual.GetComponent<Renderer>() : null;
            ApplyRendererMaterial(headRenderer, headMaterialOverride, _headMaterial);

            Renderer tailRenderer = tailVisual != null ? tailVisual.GetComponent<Renderer>() : null;
            ApplyRendererMaterial(tailRenderer, tailMaterialOverride, _tailMaterial);

            for (int i = 0; i < _bodyMarkers.Count; i++)
            {
                Renderer renderer = _bodyMarkers[i] != null ? _bodyMarkers[i].GetComponent<Renderer>() : null;
                ApplyRendererMaterial(renderer, arrowMaterialOverride, _arrowMaterial);
            }
        }

        private void ApplyVisuals()
        {
            _path.GetBodySamples(bodyMarkerCount, _bodySamples);

            if (bodyLine != null)
            {
                IReadOnlyList<SurfaceSample> samples = _path.Samples;
                bodyLine.positionCount = samples.Count;
                for (int i = 0; i < samples.Count; i++)
                {
                    bodyLine.SetPosition(i, ToWorld(samples[i]));
                }
            }

            for (int i = 0; i < _bodyMarkers.Count; i++)
            {
                if (_bodyMarkers[i] == null)
                {
                    continue;
                }

                if (i >= _bodySamples.Count)
                {
                    _bodyMarkers[i].gameObject.SetActive(false);
                    continue;
                }

                _bodyMarkers[i].gameObject.SetActive(true);
                _bodyMarkers[i].position = ToWorld(_bodySamples[i]);
                _bodyMarkers[i].localScale = Vector3.one * bodyMarkerSize;
            }

            if (_bodySamples.Count > 0 && headVisual != null)
            {
                SurfaceSample head = _bodySamples[0];
                ApplyOrientedVisual(headVisual, head, new Vector3(headRadius, headRadius, headLength));
            }

            if (_bodySamples.Count > 0 && tailVisual != null)
            {
                SurfaceSample tail = _bodySamples[_bodySamples.Count - 1];
                tailVisual.position = ToWorld(tail);
                tailVisual.rotation = SampleRotation(tail);
                tailVisual.localScale = Vector3.one * bodyMarkerSize * 1.15f;
            }
        }

        private Vector3 ToWorld(SurfaceSample sample)
        {
            Vector3 local = sample.LocalPosition + sample.LocalNormal.normalized * surfaceOffset;
            return transform.TransformPoint(local);
        }

        private Quaternion SampleRotation(SurfaceSample sample)
        {
            Vector3 forward = transform.TransformDirection(sample.LocalForward);
            Vector3 up = transform.TransformDirection(sample.LocalNormal);
            if (forward.sqrMagnitude <= 0.0001f)
            {
                forward = transform.forward;
            }

            if (up.sqrMagnitude <= 0.0001f)
            {
                up = transform.up;
            }

            return Quaternion.LookRotation(forward.normalized, up.normalized);
        }

        private void ApplyOrientedVisual(Transform visual, SurfaceSample sample, Vector3 scale)
        {
            visual.position = ToWorld(sample);
            visual.rotation = SampleRotation(sample);
            visual.localScale = scale;
        }

        private static Transform FindOrCreateChild(Transform parent, string name)
        {
            Transform child = parent.Find(name);
            if (child != null)
            {
                return child;
            }

            child = new GameObject(name).transform;
            child.SetParent(parent, false);
            return child;
        }

        private static void DisableCollider(GameObject target)
        {
            Collider collider = target.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }
        }

        private static Material CreateMaterial(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Sprites/Default");

            Material material = new Material(shader);
            material.name = GeneratedMaterialName;
            SetMaterialColor(material, color);
            return material;
        }

        private static void ApplyRendererMaterial(Renderer renderer, Material overrideMaterial, Material generatedMaterial)
        {
            if (renderer == null)
            {
                return;
            }

            Material target = overrideMaterial != null ? overrideMaterial : generatedMaterial;
            if (target == null)
            {
                return;
            }

            Material current = renderer.sharedMaterial;
            if (overrideMaterial != null || ShouldReplaceGeneratedMaterial(current))
            {
                renderer.sharedMaterial = target;
            }
        }

        private static void ApplyLineMaterial(LineRenderer lineRenderer, Material overrideMaterial, Material generatedMaterial)
        {
            if (lineRenderer == null)
            {
                return;
            }

            Material target = overrideMaterial != null ? overrideMaterial : generatedMaterial;
            if (target == null)
            {
                return;
            }

            Material current = lineRenderer.sharedMaterial;
            if (overrideMaterial != null || ShouldReplaceGeneratedMaterial(current))
            {
                lineRenderer.sharedMaterial = target;
            }
        }

        private static bool ShouldReplaceGeneratedMaterial(Material material)
        {
            if (material == null)
            {
                return true;
            }

            return material.name.StartsWith(GeneratedMaterialName)
                || material.name == "Default-Material";
        }

        private static void SetMaterialColor(Material material, Color color)
        {
            if (material == null)
            {
                return;
            }

            material.color = color;
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
        }

        private static Mesh CreateConeMesh(int segments)
        {
            segments = Mathf.Max(8, segments);
            Vector3[] vertices = new Vector3[segments + 2];
            int[] triangles = new int[segments * 6];

            vertices[0] = Vector3.forward * 0.5f;
            vertices[1] = Vector3.back * 0.5f;

            for (int i = 0; i < segments; i++)
            {
                float angle = (Mathf.PI * 2f * i) / segments;
                vertices[i + 2] = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), -0.5f);
            }

            int tri = 0;
            for (int i = 0; i < segments; i++)
            {
                int current = i + 2;
                int next = i == segments - 1 ? 2 : current + 1;

                triangles[tri++] = 0;
                triangles[tri++] = current;
                triangles[tri++] = next;

                triangles[tri++] = 1;
                triangles[tri++] = next;
                triangles[tri++] = current;
            }

            Mesh mesh = new Mesh();
            mesh.name = "Arrow3D Cone";
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
