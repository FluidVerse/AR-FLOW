using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BCI {
    public class ReynoldsFlowController : MonoBehaviour {
        
        [Header("Rohr-Objekt")] 
        public GameObject rohr;

        [Header("Steuerung")] 
        public Slider reynoldsSlider;
        public Toggle togglePowerLaw;
        public Toggle toggleSalama;
        public Toggle GeschwFelder;
        public Toggle togglekd;

        public Toggle velTick;
        public Toggle etaTick;
        public Toggle rhoTick;

        public Button ButtonPartikelReset;

        [Header("Anzeige")] 
        public TMP_Text reynoldsText;
        public Text kdText;
        public Image plotFlaeche;
        public Text uTextVerlauf;
        public TMP_Text uTextWandnah;

        [Header("Vektorfelder (Dateinamen ohne Erweiterung)")]
        public string laminarField = "laminares_geschwindigkeitsprofil";

        public string powerLawField = "turbulentes_geschwindigkeitsprofil_power_law";
        public string salamaField = "turbulentes_geschwindigkeitsprofil_salama";

        [Header("Feldstärke und Rotation")] 
        public float fieldStrength = 50f;
        public Vector3 fieldRotation = new Vector3(0f, 0f, 90f); // Beispiel: 90° im Uhrzeigersinn

        [Header("Partikelsystem")] 
        public ParticleSystem farbfadenSystem;

        //[Header("Fadenstart-Position")]
        //public Vector3 fadenStartPosition = new Vector3(0, 0, 0);
        //private Vector3 fadenStartPosition = farbfadenSystem.transform.position;

        [Header("LineRenderer")] 
        public LineRenderer line;
        public LineRenderer lineGross;
        public Toggle u_normierer;

        private GameObject currentField;
        private bool partikelGestartet = false;
        private bool istTurbulent = false;
        private float wackelTimer = 0f;
        private float Re = 0f;
        private float u_vel = 0f;
        private float rho = 50f;
        private float eta = 0.01f;

        private bool u_vel_bool = true;
        private bool rho_bool = false;
        private bool eta_bool = false;

        private bool u_normieren = true;

        //private float dudy = 0f;
        private bool geschwFelderOn = true;
        private bool kd = false; // false = 1*10^-2, true = 5*10^-2
        private GameObject geschwFeldSprite;

        private int turbThreshold = 2300;
        //private float m = 2f;
        //private float n = 4f;

        private double powerlawG = 0;
        private double powerlaw = 0;

        private double salamaG = 0;
        private double salama = 0;

        private double laminarG = 0;
        private double laminar = 0;

        private Vector3 positions;
        private Vector3 plotUrsprung;

        private Vector3 plotUrsprungRohr;

        //public string zustand = '';
        void Start() {
            reynoldsSlider.onValueChanged.AddListener(delegate { OnReynoldsChanged(); });
            // OnReynoldsChanged()
            togglePowerLaw.onValueChanged.AddListener(delegate { OnReynoldsChanged(); });
            toggleSalama.onValueChanged.AddListener(delegate { OnReynoldsChanged(); });
            togglekd.onValueChanged.AddListener(delegate {
                kd = !kd;
                OnReynoldsChanged();
            });

            velTick.onValueChanged.AddListener(delegate {
                u_vel_bool = !u_vel_bool;
                resetSlider();
            });
            etaTick.onValueChanged.AddListener(delegate {
                eta_bool = !eta_bool;
                resetSlider();
            });
            rhoTick.onValueChanged.AddListener(delegate {
                rho_bool = !rho_bool;
                resetSlider();
            });

            u_normierer.onValueChanged.AddListener(delegate {
                u_normieren = !u_normieren;
                OnReynoldsChanged();
            });

            GeschwFelder.onValueChanged.AddListener(delegate {
                geschwFelderOn = !geschwFelderOn;
                OnReynoldsChanged();
            });
            ButtonPartikelReset.onClick.AddListener(
                delegate { farbfadenSystem.GetComponent<ParticleSystem>().Clear(); });

            //togglePowerLaw.onValueChanged.AddListener(delegate { if (togglePowerLaw.isOn) LoadField(powerLawField, Re); });
            //toggleSalama.onValueChanged.AddListener(delegate { if (toggleSalama.isOn) LoadField(salamaField, Re); });

            togglePowerLaw.gameObject.SetActive(false);
            toggleSalama.gameObject.SetActive(false);

            LoadField(laminarField, Re);

            if (farbfadenSystem != null) {
                farbfadenSystem.Stop();
            }
        }

        void resetSlider() {
            if (u_vel_bool == true) {
                reynoldsSlider.value = u_vel / 20f;
            }

            if (eta_bool == true) {
                reynoldsSlider.value = (float)(Math.Pow((eta - 0.001f) / 0.119f, 0.5));
            }

            if (rho_bool == true) {
                reynoldsSlider.value = (float)(Math.Pow((rho - 1f) / 999f, 0.5));
            }
        }

        void OnReynoldsChanged() {
            // Depending on which tickbox is activated, change value for u, rho or eta and calculate resulting Re
            //     public Toggle velTick;u_vel_bool = true;
            //     public Toggle etaTick;     private bool rho_bool = false;
            //     public Toggle rhoTick;     private bool eta_bool = false;

            // 0 < Re < 120000 // 720000
            // 0 < u < 20 m/s
            // 1 < rho < 1000 kg/m^3
            // 0.001 < eta < 0.12 Pa s
            // d = 0.036 m


            if (u_vel_bool == true) {
                //u_vel = (float)(Math.Pow(reynoldsSlider.value/ 120000, 2.5) / (Math.Pow(40, 1.5)));
                u_vel = (float)(reynoldsSlider.value * 20f);
            }

            if (eta_bool == true) {
                //eta = (float)(reynoldsSlider.value * 0.119f + 0.001f);
                eta = (float)(Math.Pow(reynoldsSlider.value, 2) * 0.119f + 0.001f);
            }

            if (rho_bool == true) {
                //rho = (float)(reynoldsSlider.value * 999f + 1f);
                rho = (float)(Math.Pow(reynoldsSlider.value, 2) * 999f + 1f);

                //rho = (float)(Math.Pow(reynoldsSlider.value/120000, 2.5) / (Math.Pow(1000, 1.5)));
                // rho = (float)(Math.Pow(reynoldsSlider.value, 2.5) / (Math.Pow(120000, 1.5)));
            }

            Re = u_vel * rho * 0.036f / eta;
            System.Random r = new System.Random();
            //turbThreshold = r.Next(2000, 2600);
            turbThreshold = 2300;

            if (reynoldsText != null)
                reynoldsText.text = $"Re = {Mathf.RoundToInt(Re)}"; //\n u = {u_vel} \n rho = {rho}";

            istTurbulent = (Re >= turbThreshold);
            if (istTurbulent) {
                reynoldsText.color = Color.red;
                reynoldsText.text =
                    $"Re = {Mathf.RoundToInt(Re)} \n turbulente Strömung"; // \n u = {u_vel} \n rho = {rho}\n eta = {eta}";//ū(y)/u_max wird gezeigt!";
                uTextVerlauf.text = "ū(y) Verlauf anzeigen";
                uTextWandnah.text = "ū(y) in Wandnähe";
            } else {
                reynoldsText.color = Color.green;
                uTextVerlauf.text = "u(y) Verlauf anzeigen";
                uTextWandnah.text = "u(y) in Wandnähe";
            }

            if (Re < turbThreshold) {
                togglePowerLaw.gameObject.SetActive(false);
                toggleSalama.gameObject.SetActive(false);
                LoadField(laminarField, Re);
                changeVelProfile("laminar");
                // Re = speed/1000 oder so einbringen
                // in Funktion LoadField weiter unten?!
            } else {
                togglePowerLaw.gameObject.SetActive(true);
                toggleSalama.gameObject.SetActive(true);

                if (togglePowerLaw.isOn) {
                    LoadField(powerLawField, Re);
                    farbfadenSystem.Play();
                    changeVelProfile("powerLaw");
                } else if (toggleSalama.isOn) {
                    LoadField(salamaField, Re);
                    farbfadenSystem.Play();
                    changeVelProfile("salama");
                }
            }


            Bounds bounds = GetObjectBounds(rohr);

            Vector3 fadenStartPosition = new Vector3(bounds.min.x + 5f, bounds.center.y, -1f);

            if (farbfadenSystem != null) {
                Vector3 pos = fadenStartPosition;
                pos.z = currentField != null ? currentField.transform.position.z - 0.1f : 0f;
                farbfadenSystem.transform.position = pos;

                farbfadenSystem.Play();
            }

            if (!partikelGestartet) {
                if (farbfadenSystem != null) {
                    farbfadenSystem.transform.position = fadenStartPosition;
                    farbfadenSystem.Play();
                    partikelGestartet = true;
                }
            }
        }

        void LoadField(string fieldName, float ReynoldsNumber) {
            if (currentField != null)
                Destroy(currentField);

            Texture3D feld = Resources.Load<Texture3D>($"FGAs/{fieldName}");

            if (feld == null) {
                Debug.LogError($"Feld nicht gefunden: {fieldName}");
                return;
            }

            Bounds bounds = GetObjectBounds(rohr);
            Vector3 rohrPosition = rohr.transform.position;
            Vector3 boundingExtends = bounds.extents;

            currentField = new GameObject("ForceField_" + fieldName);
            currentField.transform.position = new Vector3(rohrPosition.x, rohrPosition.y, rohrPosition.z - 3f);

            Bounds fieldbounds = GetObjectBounds(currentField);
            Vector3 scaleVector = new Vector3(boundingExtends.y / fieldbounds.size.x,
                boundingExtends.x / fieldbounds.size.y, boundingExtends.z / fieldbounds.size.z);
            //Vector3 vfScale = Vector3.Scale(scaleVector,currentField.transform.localScale); //Vector3.Scale(scaleVector, boundingExtends);

            currentField.transform.localScale = scaleVector; //new Vector3(119f, 620f, 0.2f);
            currentField.transform.rotation = Quaternion.Euler(0f, 0f, -90f);


            var ff = currentField.AddComponent<ParticleSystemForceField>();
            ff.shape = ParticleSystemForceFieldShape.Box;
            ff.vectorField = feld;
            ff.vectorFieldSpeed = u_vel / 40; //ReynoldsNumber / 120000;
            ff.vectorFieldAttraction = fieldStrength;

            //Debug.Log($"scaleVector '{scaleVector}'");
        }

        void Update() {
            if (istTurbulent && farbfadenSystem != null) {
                wackelTimer += Time.deltaTime;
                Bounds bounds = GetObjectBounds(rohr);

                ParticleSystem.Particle[] particles = new ParticleSystem.Particle[farbfadenSystem.particleCount];
                int count = farbfadenSystem.GetParticles(particles);

                // Skaliert Reynolds-Faktor von 0 (Re=2300) bis 1 (Re=120000)
                float reScale = Mathf.Clamp01((Re - turbThreshold) / (120000f - turbThreshold));

                // Dynamischer Wackel-Startpunkt: von 0 (bei Re=2000) bis -650 (bei Re=5000)
                // Selbst wählbare Orte
                // float wackelStartX = Mathf.Lerp(0f, -550f, reScale);
                float wackelStartX = bounds.min.x + 6f; // -550f;

                for (int i = 0; i < count; i++) {
                    Vector3 pos = particles[i].position;
                    Vector3 partVel = particles[i].velocity;
                    //float uQuer = partVel.x + partVel.y + partVel.z;
                    float uQuer = partVel.magnitude;

                    if (pos.x > wackelStartX && partVel.x >= 0.1) {
                        float dist = pos.x - wackelStartX;
                        float amplitude = Mathf.Clamp(dist * 0.01f * reScale, 0f, 10f);
                        float randomOffset = UnityEngine.Random.Range(-1f, 1f);
                        float k = 10f;

                        //u(x,y,z,t) = u_quer + k*y* du/dy  * random-Normalverteilung
                        // evtl. u_quer raus ?
                        //pos.y += uQuer + k * pos.y * randomOffset;
                        //pos.y += uQuer *k * pos.y * randomOffset;

                        // dudy aus ff
                        //Texture3D ffTexture = currentField.GetComponent<ParticleSystemForceField>().vectorField;
                        //ffTexture.GetPixel()
                        //ParticleSystemForceField actualForceField = currentField.GetComponent<ParticleSystemForceField>();
                        //float diff = 10f;
                        //Vector3 posVecNeg = new Vector3(pos.x,pos.y+diff, pos.z);
                        //Vector3 posVecy = new Vector3(pos.x,pos.y, pos.z);
                        //Vector3 posVecPos = new Vector3(pos.x,pos.y-diff, pos.z);

                        //Vector3 uVecNeg = SampleVectorFieldAtWorldPosition(actualForceField, posVecNeg);
                        //Vector3 uVecy = SampleVectorFieldAtWorldPosition(actualForceField, posVecNeg);
                        //Vector3 uVecPos = SampleVectorFieldAtWorldPosition(actualForceField, posVecPos);

                        //float diffPos = Math.Abs(uVecPos.y - uVecy.y);
                        //float diffNeg = Math.Abs(uVecy.y - uVecNeg.y);
                        // uVec.y = velocity u 

                        //if (diffNeg <= diffPos)
                        //{
                        //dudy = (diffPos)/(diff);
                        //}
                        //else
                        //{
                        //dudy = (diffNeg)/(diff);
                        //}
                        //u(x,y,z,t) = u_quer + k*y* du/dy  * random-Normalverteilung

                        //float dudy = partVel.x;
                        //float uStrich = pos.y/125f * dudy * k * randomOffset;
                        //float uStrich = uQuer + dudy * k * randomOffset ;
                        float uStrich = uQuer * k * randomOffset * reScale;
                        // float uStrich = dudy * k * randomOffset;

                        //partVel.x += uStrich;
                        if (Math.Abs(uStrich) <= Math.Abs(2.5 * partVel.x)) {
                            partVel.y += uStrich;
                            particles[i].velocity = partVel;
                        } else {
                        }
                        // Log-Nachricht
                        //
                        // Debug.Log($"uStrich '{uStrich}', uQuer '{uQuer}', partVel.x '{partVel.x}', partVel.y '{partVel.y}'.");

                        //pos.y += 20f * Time.deltaTime * randomOffset;
                        //pos.y += Mathf.Sin(wackelTimer + i) * 20f * amplitude * Time.deltaTime;

                        //particles[i].position = pos;
                        //particles[i].velocity += partVel;
                    }
                }

                farbfadenSystem.SetParticles(particles, count);
            }
        }

        void changeVelProfile(string zustand) {
            if (geschwFelderOn) {
                if (zustand == "laminar") {
                    //SpawnGeschwSprite("laminar_kurve");
                    SalamaLine(zustand);
                } else if (zustand == "powerLaw") {
                    //SpawnGeschwSprite("powerLaw_kurve");
                    SalamaLine(zustand);
                } else if (zustand == "salama") {
                    //SpawnGeschwSprite("salama_kurve");
                    SalamaLine(zustand); //"salama_kurve");
                }
            } else {
                Destroy(geschwFeldSprite);
                if (line.positionCount >= 1) {
                    line.positionCount = 0;
                }

                if (lineGross.positionCount >= 1) {
                    lineGross.positionCount = 0;
                }
            }
        }


        Bounds GetObjectBounds(GameObject obj) {
            var sr = obj.GetComponent<SpriteRenderer>();
            if (sr != null) return sr.bounds;

            var col = obj.GetComponent<Collider2D>();
            if (col != null) return col.bounds;

            return new Bounds(obj.transform.position, new Vector3(1, 1, 1));
        }

        public Vector3 SampleVectorFieldAtWorldPosition(ParticleSystemForceField forceField, Vector3 worldPosition) {
            if (forceField.vectorField == null) return Vector3.zero;

            Texture3D texture = forceField.vectorField;

            // Transformiere Weltkoordinate in lokalen Raum des ForceFields
            Vector3 local = forceField.transform.InverseTransformPoint(worldPosition);

            // Das Vektorfeld geht von –0.5 bis +0.5 lokal
            Vector3 normalized = local + Vector3.one * 0.5f;

            // Normalisieren auf Texturgröße
            int x = Mathf.Clamp((int)(normalized.x * texture.width), 0, texture.width - 1);
            int y = Mathf.Clamp((int)(normalized.y * texture.height), 0, texture.height - 1);
            int z = Mathf.Clamp((int)(normalized.z * texture.depth), 0, texture.depth - 1);

            Color voxel = texture.GetPixel(x, y, z);

            // Angenommen, Vektoren wurden in RGB zwischen 0 und 1 codiert -> zurück nach [–1,1]
            Vector3 force = new Vector3(voxel.r, voxel.g, voxel.b); // * 2f - Vector3.one;

            return force;
        }

        void SpawnGeschwSprite(string imgName) {
            if (geschwFeldSprite != null) {
                Destroy(geschwFeldSprite);
            }


            if (line.positionCount >= 1) {
                line.positionCount = 0;
            }

            // Lade die Texture2D von der angegebenen Datei
            string path = "Images/" + imgName; // + ".img";
            Texture2D texture = Resources.Load<Texture2D>(path);

            if (texture == null) {
                Debug.LogError($"Die Textur konnte nicht geladen werden. Überprüfen Sie den Pfad. '{path}'");
                return;
            }

            Vector3 rohrPosition = rohr.transform.position;
            geschwFeldSprite = new GameObject("geschwFeld" + imgName);
            geschwFeldSprite.transform.position = new Vector3(rohrPosition.x - 400f, rohrPosition.y, rohrPosition.z);

            //Vector3 scaleVector = new Vector3(GetObjectBounds(rohr).extents.y / GetObjectBounds(geschwFeldSprite).size.x, GetObjectBounds(rohr).extents.x / GetObjectBounds(geschwFeldSprite).size.y,1f);

            geschwFeldSprite.transform.rotation = Quaternion.Euler(0f, 0f, -90f);
            //geschwFeldSprite.transform.localScale = scaleVector;
            geschwFeldSprite.transform.localScale = new Vector3(19f, 19f, 19f);

            SpriteRenderer spriteRendererObj = geschwFeldSprite.AddComponent<SpriteRenderer>();

            // Erstelle ein Sprite aus der Texture2D
            spriteRendererObj.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f));
            return;
        }


        void SalamaLine(string zustand) {
            //LineRenderer line = GetComponent<LineRenderer>();
            //line = GetComponent<LineRenderer>();
            if (line.positionCount >= 1) {
                line.positionCount = 0;
            }

            if (lineGross.positionCount >= 1) {
                lineGross.positionCount = 0;
            }

            //if (geschwFeldSprite != null)
            //{
            //    Destroy(geschwFeldSprite);
            //}

            plotUrsprung = plotFlaeche.transform.position; //plotFlaeche.GetComponent<RectTransform>().localPosition;

            plotUrsprungRohr =
                new Vector3((float)(GetObjectBounds(rohr).center.x - GetObjectBounds(rohr).extents.x * 0.75f),
                    GetObjectBounds(rohr).center.y, GetObjectBounds(rohr).center.z + 1f);

            // Vector3 Ursprung = new Vector3(-975f,-604f,-1f);
            //line.transform.position(Ursprung);
            line.transform.position = plotUrsprung;
            lineGross.transform.position = plotUrsprungRohr + new Vector3(0, 0, -10);

            //float Re = reynoldsSlider.value;
            //kd = false; // false = 1*10^-2, true = 5*10^-2
            int reMax = 2000;

            // 260f bedeutet halber kanal wird dargestellt
            // 520f bedeutet viertel kanal wird dargestellt
            // 1040f bedeutet achtel kanal wird dargestellt
            float zoom = 520f;

            if (kd == true) {
                kdText.text = "k/d = 0.05";
                reMax = 15000;
            } else {
                kdText.text = "k/d = 0.01";
                reMax = 100000;
            }

            if (zustand == "salama") {
                //u_max * (1-(abs(y)/R)**m)** (1/n)
                float m = 2f;
                float n = 4f;
                //salama_max = Math.Pow(1 - Math.Pow(Math.Abs(1040f - y) / 1040f, m), 1 / n)
                if (Re <= reMax) {
                    m = 2f + (2f - 1f) / (reMax) * Re;
                    n = 4f + (11f - 4f) / (reMax) * Re;
                } else {
                    m = 2f + (2f - 1f);
                    n = 4f + (11f - 4f);
                }

                for (int i = 0; i <= 260; i++) {
                    double y = (float)(i) / 10;

                    //double salama = -975f + 240 * Math.Pow(1 - Math.Pow(Math.Abs(250f - y) / 250f, m), 1 / n);
                    //double salama = plotUrsprung.x + 240 * Math.Pow(1 - Math.Pow(Math.Abs(250f - y) / 250f, m), 1 / n);
                    //double salama = -125f + 240 * Math.Pow(1 - Math.Pow(Math.Abs(250f - y) / 250f, m), 1 / n);

                    // (1f-i/260f) bedeutet halber kanal wird dargestellt
                    // (1f-i/520f) bedeutet viertel kanal wird dargestellt
                    // (1f-i/1040f) bedeutet achtel kanal wird dargestellt
                    if (u_normieren) {
                        double salama = 250f * Math.Pow(1 - Math.Pow(Math.Abs(1f - i / zoom), m), 1 / n);
                        double salamaG = 250f * Math.Pow(1 - Math.Pow(Math.Abs(1f - i / 130f), m), 1 / n);
                    } else {
                        double salama = 250f * Math.Pow(1 - Math.Pow(Math.Abs(1f - i / zoom), m), 1 / n) * u_vel;
                        double salamaG = 250f * Math.Pow(1 - Math.Pow(Math.Abs(1f - i / 130f), m), 1 / n) * u_vel;
                    }
                    //Vector3 currentPosition = new Vector3((float)(salama), (float)(i) - 604f, -1f);
                    //Vector3 currentPosition = new Vector3((float)(salama), (float)(i) - plotUrsprung.y, -1f);
                    //Vector3 currentPosition = new Vector3((float)(salama) + plotUrsprung.x, (float)(i) + plotUrsprung.y-125f, -1f);
                    Vector3 currentPosition = new Vector3((float)(salama) - 120f, (float)(i) - 127f, -1f);


                    Vector3 currentPositionG = new Vector3((float)(salamaG) - 120f, (float)(i) * 340 / 260 - 170f, -1f);

                    //if (i == 0)
                    //{
                    //    Debug.Log($"x= '{plotUrsprung.x}', y= '{plotUrsprung.y}', currentPos= '{currentPosition}' ");
                    //}
                    //422 251
                    //306 479
                    line.positionCount++;
                    line.SetPosition(line.positionCount - 1, currentPosition);

                    lineGross.positionCount++;
                    lineGross.SetPosition(lineGross.positionCount - 1, currentPositionG);
                }
            } else if (zustand == "powerLaw") {
                //u_max * (1 - abs(x_values) / R)**(1/10)
                float m = 2f;

                if (Re <= reMax) {
                    m = 1 / (6 + 4 * Re / reMax); //  1/6 bis 1/10
                    //Debug.Log($"m = '{m}' ");
                } else {
                    m = 0.1f;
                    //Debug.Log($"m = '{m}' ");
                }

                for (int i = 0; i <= 260; i++) {
                    double y = (float)(i) / 10;

                    //double powerlaw = -975f + 240 * Math.Pow(1 - Math.Abs(250f - y) / 250f, m);
                    //double powerlaw =  260f * Math.Pow(1 - Math.Abs(260f - y) / 260f, m);

                    // (1f-i/260f) bedeutet halber kanal wird dargestellt
                    // (1f-i/520f) bedeutet viertel kanal wird dargestellt
                    // (1f-i/1040f) bedeutet achtel kanal wird dargestellt


                    if (u_normieren) {
                        powerlawG = 250f * Math.Pow(1 - Math.Abs((1f - i / 130f)), m);
                        powerlaw = 250f * Math.Pow(1 - Math.Abs((1f - i / zoom)), m);
                    } else {
                        powerlawG = 250f * Math.Pow(1 - Math.Abs((1f - i / 130f)), m) * u_vel;
                        powerlaw = 250f * Math.Pow(1 - Math.Abs((1f - i / zoom)), m) * u_vel;
                    }

                    Vector3 currentPosition = new Vector3((float)(powerlaw) - 120f, (float)(i) - 127f, -1f);

                    Vector3 currentPositionG =
                        new Vector3((float)(powerlawG) - 120f, (float)(i) * 340 / 260 - 170f, -1f);

                    line.positionCount++;
                    line.SetPosition(line.positionCount - 1, currentPosition);

                    lineGross.positionCount++;
                    lineGross.SetPosition(lineGross.positionCount - 1, currentPositionG);
                }
            } else if (zustand == "laminar" && Re > 0) {
                float m = (Re / turbThreshold);

                for (int i = 0; i <= 260; i++) {
                    //double y = (float)(i) / 10;
                    //double laminar = 260f * m * Math.Pow(((260f-y)/260f), 2);// * m;
                    //
                    // (1f-i/260f) bedeutet halber kanal wird dargestellt
                    // (1f-i/520f) bedeutet viertel kanal wird dargestellt
                    // (1f-i/1040f) bedeutet achtel kanal wird dargestellt

                    if (u_normieren) {
                        laminar = m * (260f - 260f * Math.Pow((1f - i / zoom), 2));
                        laminarG = m * (260f - 260f * Math.Pow((1f - i / 130f), 2));
                    } else {
                        laminar = m * (260f - 260f * Math.Pow((1f - i / zoom), 2)) * u_vel;
                        laminarG = m * (260f - 260f * Math.Pow((1f - i / 130f), 2)) * u_vel;
                    }

                    //Vector3 currentPosition = new Vector3((float)(laminar)-120f, (float)(i)-127f, -1f);
                    Vector3 currentPosition = new Vector3((float)(laminar) - 120f, (float)(i) - 127f, -1f);

                    Vector3 currentPositionG =
                        new Vector3((float)(laminarG) - 120f, (float)(i) * 340 / 260 - 170f, -1f);

                    line.positionCount++;
                    line.SetPosition(line.positionCount - 1, currentPosition);

                    lineGross.positionCount++;
                    lineGross.SetPosition(lineGross.positionCount - 1, currentPositionG);
                }
            }
        }

    }
}