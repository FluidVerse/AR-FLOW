using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace BCI {
    public class DisplayManager : MonoBehaviour
    {
        // ----- Gates -----
        public GameObject gate1; // oben
        public GameObject gate2; // rechts
        public GameObject gate3; // unten
        public GameObject gate4; // links

        // ----- UI -----
        public Image displayImage;
        public Toggle togglePressure;
        public Toggle toggleVelocity;
        public Toggle toggleStreamlines;
        public Toggle toggleBahnlinie;
        public Toggle toggleStreichlinie;
        public Toggle toggleOhneTrail;          // Single-Click ohne Trail
        public Button clearParticlesButton;
        public RectTransform crossPanel;

        // ----- ParticleSystems für Modi -----
        [Header("ParticleSystems (Modi)")]
        public ParticleSystem psBahnlinie;      // Einzel-Emit mit Trail
        public ParticleSystem psStreichlinie;   // Dauer-Emitter am Klickpunkt (wie ClickEmitter, Radius 0)
        public ParticleSystem psOhneTrail;      // Einzel-Emit ohne Trail

        [Header("Lifetimes")]
        public float lifetimeBahnlinie = 5f;
        public float lifetimeStreichlinie = 5f;
        public float lifetimeOhneTrail = 5f;

        [Header("Streichlinie")]
        [Tooltip("Emissionen pro Sekunde im Streichlinien-Modus.")]
        public float streichlinieRate = 120f;
        public int streichlinieMaxParticles = 50000;

        // ----- One-Click Continuous Emitter (Radius-Emitter) -----
        [Header("One-Click Emitter")]
        public Toggle toggleClickEmitter;
        public ParticleSystem clickEmitterSystem;
        public float clickEmitRate = 40f;
        public float clickEmitRadiusFraction = 0.04f;
        public bool clickRequireInsideCross = true;
        public float clickEmitterLifetime = 5f;
        public int clickEmitterMaxParticles = 50000;

        private bool _clickEmitterActive = false;
        private Vector3 _clickEmitterCenter;
        private float _clickEmitAcc = 0f;

        // ----- Streichlinien-Emitter (gleich aufgebaut wie ClickEmitter, Radius 0) -----
        private bool _streichActive = false;
        private Vector3 _streichCenter;
        private float _streichEmitAcc = 0f;

        // === Random-in-Cross Emitter ===
        [Header("Random Cross Emitter")]
        public Toggle toggleRandomCrossEmitter;
        public ParticleSystem randomCrossEmitterSystem;
        public float randomEmitRate = 80f;
        public float randomEmitterLifetime = 2.5f;
        public int randomEmitterMaxParticles = 50000;
        private float _randomEmitAcc = 0f;

        // ----- Force Fields -----
        private GameObject[] forceFieldObjects = new GameObject[4];
        private GameObject activeForceFieldGroup;
        private string lastState = "0000";
        public float forceFieldStrength = 50f;
        private float _lastForceFieldStrength = float.NaN;

        [Header("ForceField Mapping")]
        public float forceFieldPlaneZ = 0f;
        public float forceFieldPrefabUnit = 1f;

        private Quaternion _stateRotation = Quaternion.identity;
        private Vector3 _stateFlipScale = Vector3.one;

        [Header("ForceField Center Offset")]
        [Range(0f, 0.5f)]
        public float centerOffsetFraction = 0.25f;

        // ----- Cross-Geometrie & -Sizing -----
        [Header("Cross Geometry")]
        [Range(0.05f, 0.6f)]
        public float crossThicknessFraction = 1f / 3f;

        [Header("Cross Sizing")]
        [Range(0.1f, 1f)] public float percentOfShortSide = 0.72f;
        public float paddingLeft = 120f;
        public float paddingRight = 120f;
        public float paddingTop = 160f;
        public float paddingBottom = 160f;

        [Header("Gate Layout (World Space)")]
        [Tooltip("Zusätzlicher Abstand der Tore über die halbe Kreuzbreite hinaus (Welt-Einheiten).")]
        public float gateExtraDistance = 0.1f;

        [Header("Cross Wall Culling")]
        [Range(0f, 0.2f)]
        public float crossWallCullMarginFraction = 0.02f; // 2 % des Cross-Durchmessers als Rand

        // ----- Layout Change Detection -----
        private Vector2 _lastRootSize = new Vector2(float.NaN, float.NaN);
        private float _lastPercent = -1f;
        private Vector4 _lastPad = new Vector4(float.NaN, float.NaN, float.NaN, float.NaN);
        private float _lastThickness = -1f;
        private float _lastOffsetFrac = -1f;

        // ----- Init -----
        private bool _initialized;
    
        private InputAction pointAction, clickAction;

        // ================= LIFECYCLE =================

        void Awake()
        {
            pointAction = InputSystem.actions.FindAction("UI/Point", true);
            clickAction = InputSystem.actions.FindAction("UI/Click", true);
            EnsureDisplayImageLayout();
            if (displayImage) displayImage.enabled = false;

            // Einzel-Emit Modi
            SetupModeParticleSystem(psBahnlinie, lifetimeBahnlinie, loop: false, enableTrails: true);
            SetupModeParticleSystem(psOhneTrail,   lifetimeOhneTrail,   loop: false, enableTrails: false);

            // Streichlinie: Aufbau analog zum ClickEmitter (Dauer-Emitter mit manuellem Emit)
            if (psStreichlinie != null)
            {
                var m = psStreichlinie.main;
                m.simulationSpace = ParticleSystemSimulationSpace.World;
                m.loop = false;
                m.startLifetime = lifetimeStreichlinie;
                m.maxParticles = Mathf.Max(1000, streichlinieMaxParticles);
                m.stopAction = ParticleSystemStopAction.None;

                var em = psStreichlinie.emission;
                em.rateOverTime = 0f;

                psStreichlinie.gameObject.SetActive(false);
                psStreichlinie.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            // ClickEmitter
            if (clickEmitterSystem != null)
            {
                var m = clickEmitterSystem.main;
                m.simulationSpace = ParticleSystemSimulationSpace.World;
                m.loop = false;
                m.startLifetime = clickEmitterLifetime;
                m.maxParticles = Mathf.Max(1000, clickEmitterMaxParticles);
                m.stopAction = ParticleSystemStopAction.None;

                var em = clickEmitterSystem.emission;
                em.rateOverTime = 0f;

                clickEmitterSystem.gameObject.SetActive(false);
                clickEmitterSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            // Randomizer
            if (randomCrossEmitterSystem != null)
            {
                var m = randomCrossEmitterSystem.main;
                m.simulationSpace = ParticleSystemSimulationSpace.World;
                m.loop = false;
                m.startLifetime = randomEmitterLifetime;
                m.maxParticles = Mathf.Max(1000, randomEmitterMaxParticles);
                m.stopAction = ParticleSystemStopAction.None;

                var em = randomCrossEmitterSystem.emission;
                em.rateOverTime = 0f;

                randomCrossEmitterSystem.gameObject.SetActive(false);
                randomCrossEmitterSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        IEnumerator Start()
        {
            // Toggle-Events
            togglePressure.onValueChanged.AddListener(OnPressureToggled);
            toggleVelocity.onValueChanged.AddListener(OnVelocityToggled);
            toggleStreamlines.onValueChanged.AddListener(delegate { UpdateSelection(); });

            if (toggleBahnlinie != null)    toggleBahnlinie.onValueChanged.AddListener(delegate { OnModeChanged(); });
            if (toggleStreichlinie != null) toggleStreichlinie.onValueChanged.AddListener(delegate { OnModeChanged(); });
            if (toggleOhneTrail != null)    toggleOhneTrail.onValueChanged.AddListener(delegate { OnModeChanged(); });

            if (clearParticlesButton != null) clearParticlesButton.onClick.AddListener(ClearAllParticles);
            if (toggleClickEmitter != null)  toggleClickEmitter.onValueChanged.AddListener(OnClickEmitterToggled);
            if (toggleRandomCrossEmitter != null)
                toggleRandomCrossEmitter.onValueChanged.AddListener(OnRandomCrossEmitterToggled);

            // Defaults
            togglePressure.isOn = false;
            toggleVelocity.isOn = false;
            toggleStreamlines.isOn = false;

            if (toggleBahnlinie != null)    toggleBahnlinie.isOn    = true;
            if (toggleStreichlinie != null) toggleStreichlinie.isOn = false;
            if (toggleOhneTrail != null)    toggleOhneTrail.isOn    = false;

            OnModeChanged();

            yield return null;
            Canvas.ForceUpdateCanvases();

            ApplyCrossLayout();
            ShowImage();

            if (displayImage) displayImage.enabled = true;
            _initialized = true;
        }

        void OnEnable()
        {
            if (!_initialized) StartCoroutine(Start());
        }

        void Update()
        {
            // ForceFields NUR bei Tor-Änderung updaten
            string currentState = GetGateState();
            if (currentState != lastState)
            {
                lastState = currentState;
                LoadForceFields();
                UpdateSelection();
            }
            // ForceField-Stärke live updaten, wenn Slider geändert wurde
            if (activeForceFieldGroup != null &&
                !Mathf.Approximately(_lastForceFieldStrength, forceFieldStrength))
            {
                ApplyForceFieldStrength();
            }

            if (clickAction.IsPressed())
            {
                HandleMouseClick();
            }

            float dt = Time.deltaTime;

            // Dauer-Emitter am Klickpunkt (Radius)
            if (_clickEmitterActive)
            {
                ClickEmitUpdate(dt);
            }

            // Streichlinie Dauer-Emitter (Radius 0)
            if (_streichActive)
            {
                StreichEmitUpdate(dt);
            }

            // Randomizer
            if (toggleRandomCrossEmitter != null && toggleRandomCrossEmitter.isOn)
            {
                RandomCrossEmitUpdate(dt);
            }

            CullParticlesOutsideCross();
        }

        void LateUpdate()
        {
            // Wenn du wieder dynamisches Resizing brauchst, hier ApplyCrossLayoutIfChanged()
            // lassen, aber das verändert die ForceFields nicht mehr.
            ApplyCrossLayoutIfChanged();
        }

        // =========================================================
        //      KLICK-LOGIK
        // =========================================================

        private void HandleMouseClick()
        {
            // Zuerst: Dauer-Emitter (Radius-Emitter)
            if (toggleClickEmitter != null && toggleClickEmitter.isOn)
            {
                if (clickRequireInsideCross && !IsInsideCrossArea())
                {
                    Debug.Log("Klick für Dauer-Emitter außerhalb des Cross ignoriert.");
                    return;
                }

                Vector3 newPos = GetMouseWorldPosition();
                RestartClickEmitter(newPos);
                return;
            }

            // Normale Modi müssen im Cross liegen
            if (!IsInsideCrossArea())
            {
                Debug.Log("Klick außerhalb des Cross.");
                return;
            }

            Vector3 spawnPos = GetMouseWorldPosition();
            spawnPos.z = forceFieldPlaneZ;

            // Bahnlinie: Einzel-Emit mit Trail
            if (toggleBahnlinie != null && toggleBahnlinie.isOn && psBahnlinie != null)
            {
                if (!psBahnlinie.gameObject.activeSelf)
                    psBahnlinie.gameObject.SetActive(true);

                psBahnlinie.transform.position = spawnPos;

                var em = psBahnlinie.emission;
                em.rateOverTime = 0f;

                psBahnlinie.Emit(1);
            }
            // Streichlinie: Dauer-Emitter genau am Punkt (wie ClickEmitter, Radius 0)
            else if (toggleStreichlinie != null && toggleStreichlinie.isOn && psStreichlinie != null)
            {
                RestartStreichEmitter(spawnPos);
            }
            // OhneTrail: Einzel-Emit ohne Trail
            else if (toggleOhneTrail != null && toggleOhneTrail.isOn && psOhneTrail != null)
            {
                if (!psOhneTrail.gameObject.activeSelf)
                    psOhneTrail.gameObject.SetActive(true);

                psOhneTrail.transform.position = spawnPos;

                var em = psOhneTrail.emission;
                em.rateOverTime = 0f;

                psOhneTrail.Emit(1);
            }
        }

        // =========================================================
        //      MODUS-WECHSEL
        // =========================================================

        private void OnModeChanged()
        {
            // Toggles gegeneinander exklusiv + Bild aktualisieren
            UpdateSelection();

            // Wenn wir den Streichlinien-Toggle verlassen, Dauer-Emitter stoppen
            if ((toggleStreichlinie == null || !toggleStreichlinie.isOn) && _streichActive)
            {
                StopStreichEmitter();
            }
        }

        private void UpdateSelection()
        {
            // Pressure / Velocity exklusiv
            if (togglePressure.isOn && toggleVelocity.isOn)
            {
                if (togglePressure.interactable) toggleVelocity.isOn = false;
                else togglePressure.isOn = false;
            }
            if (togglePressure.isOn && !toggleVelocity.isOn) toggleVelocity.isOn = false;
            if (toggleVelocity.isOn && !togglePressure.isOn) togglePressure.isOn = false;

            // Emit-Modi exklusiv
            if (toggleBahnlinie != null && toggleBahnlinie.isOn)
            {
                if (toggleStreichlinie != null) toggleStreichlinie.isOn = false;
                if (toggleOhneTrail != null)    toggleOhneTrail.isOn    = false;
            }
            else if (toggleStreichlinie != null && toggleStreichlinie.isOn)
            {
                if (toggleBahnlinie != null) toggleBahnlinie.isOn = false;
                if (toggleOhneTrail != null) toggleOhneTrail.isOn = false;
            }
            else if (toggleOhneTrail != null && toggleOhneTrail.isOn)
            {
                if (toggleBahnlinie != null)    toggleBahnlinie.isOn    = false;
                if (toggleStreichlinie != null) toggleStreichlinie.isOn = false;
            }

            ShowImage();
        }

        private void OnPressureToggled(bool isOn)
        {
            if (isOn) toggleVelocity.isOn = false;
            ShowImage();
        }

        private void OnVelocityToggled(bool isOn)
        {
            if (isOn) togglePressure.isOn = false;
            ShowImage();
        }

        // =========================================================
        //      RANDOMIZER & CLICK-EMITTER
        // =========================================================

        private void OnRandomCrossEmitterToggled(bool isOn)
        {
            _randomEmitAcc = 0f;
            if (randomCrossEmitterSystem == null) return;

            var em = randomCrossEmitterSystem.emission;
            em.rateOverTime = 0f;

            if (isOn)
            {
                if (!randomCrossEmitterSystem.gameObject.activeSelf)
                    randomCrossEmitterSystem.gameObject.SetActive(true);
                randomCrossEmitterSystem.Play(true);
            }
            else
            {
                randomCrossEmitterSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        private void OnClickEmitterToggled(bool isOn)
        {
            _clickEmitAcc = 0f;

            if (!isOn)
            {
                StopClickEmitter();
                return;
            }

            _clickEmitterActive = false;

            if (clickEmitterSystem != null)
            {
                clickEmitterSystem.gameObject.SetActive(true);

                var em = clickEmitterSystem.emission;
                em.rateOverTime = 0f;
            }
        }

        // ---- Streichlinien-Emitter (Radius 0, wie ClickEmitter) ----

        private void RestartStreichEmitter(Vector3 newCenter)
        {
            _streichCenter = newCenter;
            _streichCenter.z = forceFieldPlaneZ;
            StopStreichEmitter();
            StartStreichEmitter();
        }

        private void StartStreichEmitter()
        {
            if (psStreichlinie == null) return;

            // Orientierung wie beim Click-Emitter am Kreuz ausrichten
            if (GetCrossWorldFrame(out var bl, out var tl, out var tr, out var br, out var center, out var sizeW))
            {
                Vector3 yDir = (tl - bl).normalized;
                psStreichlinie.transform.rotation = Quaternion.LookRotation(Vector3.forward, yDir);
            }

            psStreichlinie.transform.position = _streichCenter;

            var main = psStreichlinie.main;
            main.loop = false;
            main.startLifetime = lifetimeStreichlinie;
            main.stopAction = ParticleSystemStopAction.None;

            var em = psStreichlinie.emission;
            em.rateOverTime = 0f;

            if (!psStreichlinie.gameObject.activeSelf)
                psStreichlinie.gameObject.SetActive(true);

            psStreichlinie.Play();

            _streichActive = true;
            _streichEmitAcc = 0f;
        }

        private void StopStreichEmitter()
        {
            if (psStreichlinie != null)
            {
                psStreichlinie.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                var em = psStreichlinie.emission;
                em.rateOverTime = 0f;
            }
            _streichActive = false;
            _streichEmitAcc = 0f;
        }

        private void StreichEmitUpdate(float dt)
        {
            if (!_streichActive || psStreichlinie == null) return;

            // gleiche Struktur wie ClickEmitUpdate, aber Radius = 0
            if (!GetCrossWorldFrame(out var bl, out var tl, out var tr, out var br, out var center, out var sizeW))
                return;

            Vector3 yDir = (tl - bl).normalized;
            var t = psStreichlinie.transform;
            t.position = _streichCenter;
            t.rotation = Quaternion.LookRotation(Vector3.forward, yDir);

            _streichEmitAcc += Mathf.Max(0f, streichlinieRate) * dt;
            int count = Mathf.FloorToInt(_streichEmitAcc);
            if (count <= 0) return;
            _streichEmitAcc -= count;

            var main = psStreichlinie.main;
            bool worldSpace = (main.simulationSpace == ParticleSystemSimulationSpace.World);

            for (int i = 0; i < count; i++)
            {
                var emit = new ParticleSystem.EmitParams();
                emit.position = worldSpace ? _streichCenter : Vector3.zero;
                emit.velocity = Vector3.zero;
                psStreichlinie.Emit(emit, 1);
            }
        }

        // ---- Click-Emitter mit Radius ----

        private void RestartClickEmitter(Vector3 newCenter)
        {
            _clickEmitterCenter = newCenter;
            _clickEmitterCenter.z = forceFieldPlaneZ;
            StopClickEmitter();
            StartClickEmitter();
        }

        private void StartClickEmitter()
        {
            if (clickEmitterSystem == null) return;

            if (GetCrossWorldFrame(out var bl, out var tl, out var tr, out var br, out var center, out var sizeW))
            {
                Vector3 yDir = (tl - bl).normalized;
                clickEmitterSystem.transform.rotation = Quaternion.LookRotation(Vector3.forward, yDir);
            }

            clickEmitterSystem.transform.position = _clickEmitterCenter;

            var main = clickEmitterSystem.main;
            main.loop = true;
            main.startLifetime = clickEmitterLifetime;
            main.stopAction = ParticleSystemStopAction.None;
            main.maxParticles = Mathf.Max(1000, clickEmitterMaxParticles);

            var em = clickEmitterSystem.emission;
            em.rateOverTime = 0f;

            clickEmitterSystem.gameObject.SetActive(true);
            clickEmitterSystem.Play();

            _clickEmitterActive = true;
            _clickEmitAcc = 0f;
        }

        private void StopClickEmitter()
        {
            if (clickEmitterSystem != null)
            {
                clickEmitterSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                var em = clickEmitterSystem.emission;
                em.rateOverTime = 0f;
            }
            _clickEmitterActive = false;
            _clickEmitAcc = 0f;
        }

        private void ClickEmitUpdate(float dt)
        {
            if (clickEmitterSystem == null || !_clickEmitterActive) return;

            if (!GetCrossWorldFrame(out var bl, out var tl, out var tr, out var br, out var center, out var sizeW))
                return;

            Vector3 yDir = (tl - bl).normalized;
            var t = clickEmitterSystem.transform;
            t.position = _clickEmitterCenter;
            t.rotation = Quaternion.LookRotation(Vector3.forward, yDir);

            _clickEmitAcc += Mathf.Max(0f, clickEmitRate) * dt;
            int count = Mathf.FloorToInt(_clickEmitAcc);
            if (count <= 0) return;
            _clickEmitAcc -= count;

            float radius = Mathf.Clamp01(clickEmitRadiusFraction) * sizeW;

            Transform tform = clickEmitterSystem.transform;
            Vector3 right = tform.right;
            Vector3 up    = tform.up;

            var main = clickEmitterSystem.main;
            bool worldSpace = (main.simulationSpace == ParticleSystemSimulationSpace.World);

            for (int i = 0; i < count; i++)
            {
                float angle = Random.value * Mathf.PI * 2f;
                float r = Mathf.Sqrt(Random.value) * radius;
                Vector3 offset = right * (Mathf.Cos(angle) * r) + up * (Mathf.Sin(angle) * r);

                var emit = new ParticleSystem.EmitParams();
                emit.position = worldSpace ? (_clickEmitterCenter + offset) : offset;
                emit.velocity = offset.normalized * 1f;

                clickEmitterSystem.Emit(emit, 1);
            }
        }

        private void RandomCrossEmitUpdate(float dt)
        {
            if (randomCrossEmitterSystem == null) return;

            if (!GetCrossWorldFrame(out var bl, out var tl, out var tr, out var br, out var center, out var sizeW))
                return;

            Vector3 xDir = (tr - tl).normalized;
            Vector3 yDir = (tl - bl).normalized;

            float frac  = Mathf.Clamp(crossThicknessFraction, 0.01f, 0.9f);
            float half  = 0.5f * sizeW;
            float halfT = 0.5f * (sizeW * frac);

            _randomEmitAcc += Mathf.Max(0f, randomEmitRate) * dt;
            int count = Mathf.FloorToInt(_randomEmitAcc);
            if (count <= 0) return;
            _randomEmitAcc -= count;

            randomCrossEmitterSystem.transform.rotation = Quaternion.LookRotation(Vector3.forward, yDir);

            bool worldSpace = (randomCrossEmitterSystem.main.simulationSpace == ParticleSystemSimulationSpace.World);
            Vector3 origin = randomCrossEmitterSystem.transform.position;

            for (int i = 0; i < count; i++)
            {
                bool horizontal = (Random.value < 0.5f);

                float lx, ly;
                if (horizontal)
                {
                    lx = Random.Range(-half,  half);
                    ly = Random.Range(-halfT, halfT);
                }
                else
                {
                    lx = Random.Range(-halfT, halfT);
                    ly = Random.Range(-half,  half);
                }

                Vector3 pos = center + xDir * lx + yDir * ly;
                pos.z = forceFieldPlaneZ;

                var ep = new ParticleSystem.EmitParams();
                ep.position = worldSpace
                    ? pos
                    : (Quaternion.Inverse(randomCrossEmitterSystem.transform.rotation) * (pos - origin));
                ep.velocity = Vector3.zero;
                randomCrossEmitterSystem.Emit(ep, 1);
            }
        }

        // =========================================================
        //      PARTICLE CULLING AN DER KREUZ-WAND
        // =========================================================

        private void CullParticlesOutsideCross()
        {
            if (!GetCrossWorldFrame(out var bl, out var tl, out var tr,
                    out var br, out var center, out float sizeW))
                return;

            Vector3 xDir = (tr - tl).normalized;
            Vector3 yDir = (tl - bl).normalized;

            float size = sizeW;
            float half = size * 0.5f;
            float fraction = Mathf.Clamp(crossThicknessFraction, 0.01f, 0.9f);
            float halfThickness = (size * fraction) * 0.5f;

            float margin = Mathf.Clamp01(crossWallCullMarginFraction) * size;

            float innerHalf        = Mathf.Max(0f, half - margin);
            float innerHalfThick   = Mathf.Max(0f, halfThickness - margin);

            // Bahnlinie: spezielles Culling (Lifetime = 0)
            CullBahnlinieParticles(psBahnlinie, center, xDir, yDir, innerHalf, innerHalfThick);

            // Andere Systeme: Standard-Culling
            CullSystemParticles(psStreichlinie, center, xDir, yDir, innerHalf, innerHalfThick);
            CullSystemParticles(psOhneTrail, center, xDir, yDir, innerHalf, innerHalfThick);
            CullSystemParticles(clickEmitterSystem, center, xDir, yDir, innerHalf, innerHalfThick);
            CullSystemParticles(randomCrossEmitterSystem, center, xDir, yDir, innerHalf, innerHalfThick);
        }


        private void CullSystemParticles(ParticleSystem ps,
            Vector3 center,
            Vector3 xDir,
            Vector3 yDir,
            float innerHalf,
            float innerHalfThickness)
        {
            if (ps == null) return;

            var main = ps.main;
            if (main.simulationSpace != ParticleSystemSimulationSpace.World)
                return; // wir rechnen hier nur Welt-Space

            int count = ps.particleCount;
            if (count == 0) return;

            ParticleSystem.Particle[] particles = new ParticleSystem.Particle[count];
            int num = ps.GetParticles(particles);

            int alive = 0;
            for (int i = 0; i < num; i++)
            {
                Vector3 pos = particles[i].position;

                // in lokale Kreuz-Koordinaten umrechnen
                Vector3 v = pos - center;
                float x = Vector3.Dot(v, xDir);
                float y = Vector3.Dot(v, yDir);

                // Kernbereich (schmaler als das echte Kreuz)
                bool inHorizontal = Mathf.Abs(y) <= innerHalfThickness && Mathf.Abs(x) <= innerHalf;
                bool inVertical   = Mathf.Abs(x) <= innerHalfThickness && Mathf.Abs(y) <= innerHalf;
                bool insideCore   = inHorizontal || inVertical;

                if (insideCore)
                {
                    // Partikel behalten
                    particles[alive++] = particles[i];
                }
                // sonst: Partikel wird "weggeworfen"
            }

            ps.SetParticles(particles, alive);
        }
        private void CullBahnlinieParticles(ParticleSystem ps,
            Vector3 center,
            Vector3 xDir,
            Vector3 yDir,
            float innerHalf,
            float innerHalfThickness)
        {
            if (ps == null) return;

            var main = ps.main;
            if (main.simulationSpace != ParticleSystemSimulationSpace.World)
                return;

            int count = ps.particleCount;
            if (count == 0) return;

            ParticleSystem.Particle[] particles = new ParticleSystem.Particle[count];
            int num = ps.GetParticles(particles);

            for (int i = 0; i < num; i++)
            {
                Vector3 pos = particles[i].position;
                Vector3 v = pos - center;
                float x = Vector3.Dot(v, xDir);
                float y = Vector3.Dot(v, yDir);

                bool inHorizontal = Mathf.Abs(y) <= innerHalfThickness && Mathf.Abs(x) <= innerHalf;
                bool inVertical   = Mathf.Abs(x) <= innerHalfThickness && Mathf.Abs(y) <= innerHalf;
                bool insideCore   = inHorizontal || inVertical;

                if (!insideCore)
                {
                    // Partikel + zugehöriger Trail sofort beenden
                    particles[i].remainingLifetime = 0f;
                }
            }

            ps.SetParticles(particles, num);
        }

        // =========================================================
        //      IMAGE
        // =========================================================

        private void ShowImage()
        {
            if (!togglePressure.isOn && !toggleVelocity.isOn && !toggleStreamlines.isOn)
            {
                displayImage.sprite = null;
                return;
            }
            if (displayImage == null)
            {
                Debug.LogError("[FlowImage] displayImage ist nicht zugewiesen!");
                return;
            }

            string state = GetGateState();
            var (found, baseState, rotation, flipX, flipY) = GetEquivalentState(state);

            if (!found)
            {
                displayImage.sprite = null;
                return;
            }

            string tag = GetCombinedVariable();
            Sprite newSprite = LoadFlowSprite("778x778", baseState, tag);
            if (newSprite == null)
            {
                Debug.LogWarning($"[FlowImage MISSING] Erwartet: Assets/Resources/778x778/{baseState}/{baseState}-{tag}.png");
            }

            displayImage.sprite = newSprite;
            if (newSprite != null)
            {
                displayImage.transform.localRotation = rotation;
                Vector3 scale = Vector3.one;
                scale.x *= flipX ? -1f : 1f;
                scale.y *= flipY ? -1f : 1f;
                displayImage.transform.localScale = scale;
            }
        }

        private Sprite LoadFlowSprite(string folder, string baseState, string tag)
        {
            string path = $"{folder}/{baseState}/{baseState}-{tag}";
            var s = Resources.Load<Sprite>(path);
            Debug.Log($"[FlowImage Load] baseState={baseState} tag={tag} path='{path}' -> {(s ? "OK" : "NULL")}");
            return s;
        }

        private string GetCombinedVariable()
        {
            if (!togglePressure.isOn && !toggleVelocity.isOn && toggleStreamlines.isOn) return "SL";
            string main = togglePressure.isOn ? "P" : "V";
            return toggleStreamlines.isOn ? $"{main}-SL" : main;
        }

        private string GetGateState()
        {
            return $"{gate1.GetComponent<GateState>().GetState()}{gate2.GetComponent<GateState>().GetState()}{gate3.GetComponent<GateState>().GetState()}{gate4.GetComponent<GateState>().GetState()}";
        }

        private (bool found, string baseState, Quaternion rotation, bool flipX, bool flipY) GetEquivalentState(string state)
        {
            string[] knownBaseStates = new[]
            {
                "0012","0102","0112","0122","1112","1122","1202","1212","1222","2101"
            };

            foreach (string baseState in knownBaseStates)
            {
                for (int fx = 0; fx <= 1; fx++)
                for (int fy = 0; fy <= 1; fy++)
                for (int rot = 0; rot < 4; rot++)
                {
                    string transformed = ApplyTransformation(baseState, rot, fx == 1, fy == 1);
                    if (transformed == state)
                        return (true, baseState, Quaternion.Euler(0, 0, rot * -90), fx == 1, fy == 1);
                }
            }

            Debug.LogWarning($"[State] Kein passender Grundzustand gefunden für {state}.");
            return (false, state, Quaternion.identity, false, false);
        }

        private string ApplyTransformation(string state, int rotationSteps, bool flipX, bool flipY)
        {
            string result = state;
            if (flipX) result = FlipHorizontal(result);
            if (flipY) result = FlipVertical(result);
            for (int i = 0; i < rotationSteps; i++) result = RotateClockwise(result);
            return result;
        }

        private string RotateClockwise(string s)  => $"{s[3]}{s[0]}{s[1]}{s[2]}";
        private string FlipHorizontal(string s)   => $"{s[0]}{s[3]}{s[2]}{s[1]}";
        private string FlipVertical(string s)     => $"{s[2]}{s[1]}{s[0]}{s[3]}";

        // =========================================================
        //      FORCE FIELDS
        // =========================================================

        private void LoadForceFields()
        {
            string state = GetGateState();
            if (state == "0000")
            {
                ClearForceFields();
                ClearRuntimeParticles();
                return;
            }

            var (found, baseState, rotation, flipX, flipY) = GetEquivalentState(state);
            if (!found)
            {
                ClearForceFields();
                ClearRuntimeParticles();
                return;
            }

            Debug.Log($"[ForceField] Zustand {state} → Basis {baseState}, rot={rotation.eulerAngles.z}°, flipX={flipX}, flipY={flipY}");

            if (activeForceFieldGroup != null) Destroy(activeForceFieldGroup);
            activeForceFieldGroup = new GameObject("ForceFieldGroup");

            for (int i = 0; i < 4; i++)
            {
                string filePath = $"Vektorfelder/{baseState}_3D_{i + 1}";
                GameObject prefab = Resources.Load<GameObject>(filePath);

                if (prefab == null)
                {
                    Debug.LogWarning($"[ForceField MISSING] '{filePath}' nicht gefunden (Assets/Resources/{filePath}.prefab)");
                    continue;
                }

                GameObject inst = Instantiate(prefab);
                inst.transform.SetParent(activeForceFieldGroup.transform, false);

                var field = inst.GetComponent<ParticleSystemForceField>();
                if (field != null) field.vectorFieldAttraction = 1f;//forceFieldStrength;
                if (field != null) field.vectorFieldSpeed = forceFieldStrength;
                if (field != null && state == "0012") field.vectorFieldSpeed = 2f;

                forceFieldObjects[i] = inst;
            }

            _stateRotation = rotation;
            _stateFlipScale = new Vector3(flipX ? -1f : 1f, flipY ? -1f : 1f, 1f);
        
            ApplyForceFieldStrength();
            // Wichtig: Reposition nur hier, also nur bei Tor-Änderung
            RepositionForceFieldsToCross();
        }

        private void ClearForceFields()
        {
            if (activeForceFieldGroup != null) Destroy(activeForceFieldGroup);
            activeForceFieldGroup = null;
            for (int i = 0; i < forceFieldObjects.Length; i++) forceFieldObjects[i] = null;
        }

        private void ApplyForceFieldStrength()
        {
            if (activeForceFieldGroup == null) return;

            _lastForceFieldStrength = forceFieldStrength;

            for (int i = 0; i < forceFieldObjects.Length; i++)
            {
                var obj = forceFieldObjects[i];
                if (obj == null) continue;

                var ff = obj.GetComponent<ParticleSystemForceField>();
                if (ff != null)
                {
                    ff.vectorFieldAttraction = 1f;//forceFieldStrength;
                    ff.vectorFieldSpeed = forceFieldStrength;
                    if (GetGateState() == "0012") ff.vectorFieldSpeed = 2f;
                }
            }
        }

        private void ClearRuntimeParticles()
        {
            // Modus-Systeme
            if (psBahnlinie != null)
            {
                psBahnlinie.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                psBahnlinie.gameObject.SetActive(false);
            }
            if (psStreichlinie != null)
            {
                psStreichlinie.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                psStreichlinie.gameObject.SetActive(false);
            }
            if (psOhneTrail != null)
            {
                psOhneTrail.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                psOhneTrail.gameObject.SetActive(false);
            }
            _streichActive = false;
            _streichEmitAcc = 0f;

            // Click-Emitter
            if (clickEmitterSystem != null)
            {
                clickEmitterSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                clickEmitterSystem.gameObject.SetActive(false);
            }
            _clickEmitterActive = false;
            _clickEmitAcc = 0f;

            // Randomizer
            if (randomCrossEmitterSystem != null)
            {
                randomCrossEmitterSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                randomCrossEmitterSystem.gameObject.SetActive(false);
            }
            _randomEmitAcc = 0f;
        }

        // ---- Mapping: UI Cross -> Welt-Ebene ----

        private bool ScreenToWorldOnPlane(Vector2 screen, Camera cam, float planeZ, out Vector3 world)
        {
            world = Vector3.zero;
            if (cam == null) cam = Camera.main;
            if (cam == null) return false;

            var plane = new Plane(Vector3.forward, new Vector3(0f, 0f, planeZ));
            Ray ray = cam.ScreenPointToRay(screen);
            if (plane.Raycast(ray, out float enter))
            {
                world = ray.GetPoint(enter);
                return true;
            }
            return false;
        }

        private bool GetCrossWorldFrame(out Vector3 bl, out Vector3 tl, out Vector3 tr,
            out Vector3 br, out Vector3 center, out float sizeWorld)
        {
            bl = tl = tr = br = center = Vector3.zero;
            sizeWorld = 0f;
            if (crossPanel == null) return false;

            var canvas = crossPanel.GetComponentInParent<Canvas>();
            Camera uiCam = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                ? canvas.worldCamera : null;

            Vector3[] wc = new Vector3[4];
            crossPanel.GetWorldCorners(wc);
            Vector2[] sc = new Vector2[4];
            for (int i = 0; i < 4; i++) sc[i] = RectTransformUtility.WorldToScreenPoint(uiCam, wc[i]);

            if (!ScreenToWorldOnPlane(sc[0], Camera.main, forceFieldPlaneZ, out bl)) return false;
            if (!ScreenToWorldOnPlane(sc[1], Camera.main, forceFieldPlaneZ, out tl)) return false;
            if (!ScreenToWorldOnPlane(sc[2], Camera.main, forceFieldPlaneZ, out tr)) return false;
            if (!ScreenToWorldOnPlane(sc[3], Camera.main, forceFieldPlaneZ, out br)) return false;

            center = (bl + tl + tr + br) * 0.25f;

            float width  = Vector3.Distance(br, tr);
            float height = Vector3.Distance(bl, tl);
            sizeWorld = Mathf.Min(width, height);
            return true;
        }

        private void RepositionForceFieldsToCross()
        {
            if (activeForceFieldGroup == null) return;

            if (!GetCrossWorldFrame(out var bl, out var tl, out var tr,
                    out var br, out var center, out var sizeW))
                return;

            Vector3 xDir = (tr - tl).normalized;
            Vector3 yDir = (tl - bl).normalized;

            Quaternion uiRot = Quaternion.LookRotation(Vector3.forward, yDir);
            activeForceFieldGroup.transform.position = center;
            activeForceFieldGroup.transform.rotation = uiRot * _stateRotation;
            activeForceFieldGroup.transform.localScale = _stateFlipScale;

            float fraction = Mathf.Clamp(crossThicknessFraction, 0.01f, 0.9f);
            float corner = sizeW * fraction;
            float s = (forceFieldPrefabUnit != 0f) ? (corner / forceFieldPrefabUnit) : corner;
            Vector3 quadScale = new Vector3(s, s, s);

            float offsetFrac = Mathf.Clamp(centerOffsetFraction, 0f, 0.5f);
            float offset = sizeW * offsetFrac;

            Vector3 off = offset * xDir;
            Vector3 up  = offset * yDir;

            Vector3[] localCenters = new Vector3[4];
            localCenters[0] = -off - up;
            localCenters[1] =  off - up;
            localCenters[2] = -off + up;
            localCenters[3] =  off + up;

            for (int i = 0; i < forceFieldObjects.Length; i++)
            {
                var inst = forceFieldObjects[i];
                if (inst == null) continue;

                inst.transform.SetParent(activeForceFieldGroup.transform, false);
                inst.transform.localPosition = localCenters[Mathf.Clamp(i, 0, 3)];
                inst.transform.localRotation = Quaternion.identity;
                inst.transform.localScale   = quadScale;
            }

            if (_clickEmitterActive && clickEmitterSystem != null)
            {
                clickEmitterSystem.transform.rotation = Quaternion.LookRotation(Vector3.forward, yDir);
            }
        }

        // ================= HIT-TEST (Plus-Form) =================

        private bool IsInsideCrossArea()
        {
            if (crossPanel == null) return false;

            var canvas = crossPanel.GetComponentInParent<Canvas>();
            Camera cam = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                ? canvas.worldCamera : null;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(crossPanel, pointAction.ReadValue<Vector2>(),
                    cam, out Vector2 local))
                return false;

            Rect rect = crossPanel.rect;
            float size = Mathf.Min(rect.width, rect.height);
            float half = size * 0.5f;
            float fraction = Mathf.Clamp(crossThicknessFraction, 0.01f, 0.9f);
            float halfThickness = (size * fraction) * 0.5f;

            float x = local.x;
            float y = local.y;

            if (Mathf.Abs(x) > half || Mathf.Abs(y) > half) return false;

            bool inHorizontal = Mathf.Abs(y) <= halfThickness && Mathf.Abs(x) <= half;
            bool inVertical   = Mathf.Abs(x) <= halfThickness && Mathf.Abs(y) <= half;
            return inHorizontal || inVertical;
        }

        // ================= HELPERS =================

        private Vector3 GetMouseWorldPosition()
        {
            if (ScreenToWorldOnPlane(pointAction.ReadValue<Vector2>(), Camera.main, forceFieldPlaneZ, out var world))
            {
                world.z = forceFieldPlaneZ;
                return world;
            }

            Vector3 mouse = pointAction.ReadValue<Vector2>();
            float z = (Camera.main != null ? Camera.main.nearClipPlane + 1f : 1f);
            Vector3 w = (Camera.main != null)
                ? Camera.main.ScreenToWorldPoint(new Vector3(mouse.x, mouse.y, z))
                : new Vector3(mouse.x, mouse.y, 0f);
            w.z = forceFieldPlaneZ;
            return w;
        }

        private void ClearAllParticles()
        {
            ClearRuntimeParticles();
            Debug.Log("Alle Partikel wurden gelöscht.");
        }

        private void ApplyCrossLayout()
        {
            if (crossPanel == null)
            {
                Debug.LogWarning("[CrossLayout] crossPanel ist nicht zugewiesen.");
                return;
            }

            var root = crossPanel.transform.parent as RectTransform;
            if (root == null)
            {
                Debug.LogWarning("[CrossLayout] Kein RectTransform-Elternteil gefunden.");
                return;
            }

            Rect r = root.rect;
            float availW = Mathf.Max(0, r.width  - paddingLeft - paddingRight);
            float availH = Mathf.Max(0, r.height - paddingTop  - paddingBottom);

            float shortSide = Mathf.Min(availW, availH);
            float size = Mathf.Clamp01(percentOfShortSide) * shortSide;

            crossPanel.anchorMin = crossPanel.anchorMax = new Vector2(0.5f, 0.5f);
            crossPanel.pivot = new Vector2(0.5f, 0.5f);
            crossPanel.localScale = Vector3.one;
            crossPanel.localRotation = Quaternion.identity;

            crossPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
            crossPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,   size);

            float cx = (paddingLeft - paddingRight) * 0.5f;
            float cy = (paddingBottom - paddingTop) * 0.5f;
            crossPanel.anchoredPosition = new Vector2(cx, cy);

            EnsureDisplayImageLayout();

            // Wichtig: ForceFields hier NICHT mehr repositionieren.
            // Sie bleiben so, wie sie beim letzten Tor-State-Wechsel gesetzt wurden.

            if (_clickEmitterActive && clickEmitterSystem != null) UpdateClickEmitterOrientationOnly();
        }

        private void UpdateClickEmitterOrientationOnly()
        {
            if (clickEmitterSystem == null) return;
            if (!GetCrossWorldFrame(out var bl, out var tl, out var tr,
                    out var br, out var center, out var sizeW)) return;
            Vector3 yDir = (tl - bl).normalized;
            clickEmitterSystem.transform.rotation = Quaternion.LookRotation(Vector3.forward, yDir);
        }

        private void EnsureDisplayImageLayout()
        {
            if (displayImage == null) return;
            var rt = displayImage.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            displayImage.preserveAspect = true;
        }

        private void ApplyCrossLayoutIfChanged()
        {
            if (crossPanel == null) return;
            var root = crossPanel.transform.parent as RectTransform;
            if (root == null) return;

            var size = root.rect.size;
            var pad = new Vector4(paddingLeft, paddingRight, paddingTop, paddingBottom);

            if (_lastRootSize != size
                || _lastPercent != percentOfShortSide
                || _lastPad != pad
                || !Mathf.Approximately(_lastThickness, crossThicknessFraction)
                || !Mathf.Approximately(_lastOffsetFrac, centerOffsetFraction))
            {
                ApplyCrossLayout();
                _lastRootSize = size;
                _lastPercent = percentOfShortSide;
                _lastPad = pad;
                _lastThickness = crossThicknessFraction;
                _lastOffsetFrac = centerOffsetFraction;
            }
        }

        private void SetupModeParticleSystem(ParticleSystem ps, float lifetime, bool loop, bool enableTrails)
        {
            if (ps == null) return;

            var m = ps.main;
            m.simulationSpace = ParticleSystemSimulationSpace.World;
            m.loop = loop;
            m.startLifetime = lifetime;
            m.stopAction = ParticleSystemStopAction.None;

            var t = ps.trails;
            t.enabled = enableTrails;
            if (enableTrails)
            {
                // WICHTIG: jeder Partikel bekommt seinen eigenen Trail
                t.mode = ParticleSystemTrailMode.PerParticle;

                // Trail stirbt mit den Partikeln
                t.dieWithParticles = true;

                // Trail-Lebensdauer ungefähr wie Partikel-Lebensdauer
                var curve = new ParticleSystem.MinMaxCurve(lifetime);
                t.lifetime = curve;

                // optional, damit der Trail an den Partikel „gebunden“ bleibt
                t.worldSpace = true;
            }

            var em = ps.emission;
            em.rateOverTime = 0f;

            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.gameObject.SetActive(false);
        }


    }
}