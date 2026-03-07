using InteractionObjects;
using Quests;
using UnityEngine;

namespace LevelScripts {
    public class Pumpenlevel : MonoBehaviour {
        
        GameObject Sensor_p1;
        SensorPressureObject Sensor_p1_script;

        GameObject Sensor_p2;
        SensorPressureObject Sensor_p2_script;

        GameObject Sensor_V;
        SensorFlowObject Sensor_V_script;

        GameObject Kugelhahn1;
        ValveObject Kugelhahn1_script;

        GameObject Motor;
        MotorObject Motor_script;

        private QuestManager newQuestManager;

        public bool newInteraction = true;

        // Variablen durch User in Level bestimmt
        public float valvePosition = 50;
        public float rotationalSpeed = 0;

        // Variablen berechnet
        public float zeta = 1;

        // Variablen berechnet und in Level angezeigt
        public float p1 = 100000; // ist vorgegeben als Referenzdruck
        public float p2 = 200000;
        public float Vdot = 0;

        public bool motor1isRunning = false;


        private float pi = Mathf.PI; // PI
        private float Psi0 = 0.2f; // Pumpenparameter
        private float k = 100f; // Pumpenparameter
        private float A = 0.00503f; // Rohrquerschnitt
        private float D = 0.08f; // Rohrdurchmesser
        private float rho = 1000f; // Wasserdichte
        private float D2 = 0.2f; // Laufraddurchmesser

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start() {
            Sensor_p1 = GameObject.Find("Sensor_p1");
            if (Sensor_p1 != null) {
                Sensor_p1_script = Sensor_p1.GetComponent<SensorPressureObject>();
                if (Sensor_p1_script != null) {
                    Sensor_p1_script.OnAnyInteraction += Name => { newInteraction = true; };
                }
            } else {
                Debug.Log("Pumpenkennlinie: Object Sensor_p1 not found");
            }

            Sensor_p2 = GameObject.Find("Sensor_p2");
            if (Sensor_p2 != null) {
                Sensor_p2_script = Sensor_p2.GetComponent<SensorPressureObject>();
                if (Sensor_p2_script != null) {
                    Sensor_p2_script.OnAnyInteraction += Name => { newInteraction = true; };
                }
            } else {
                Debug.Log("Pumpenkennlinie: Object Sensor_p1 not found");
            }

            Sensor_V = GameObject.Find("Volumenstrom");
            if (Sensor_V != null) {
                Sensor_V_script = Sensor_V.GetComponent<SensorFlowObject>();
                if (Sensor_V_script != null) {
                    Sensor_V_script.OnAnyInteraction += Name => { newInteraction = true; };
                }
            } else {
                Debug.Log("Pumpenkennlinie: Object Sensor_V not found");
            }

            Kugelhahn1 = GameObject.Find("Kugelhahn");
            if (Kugelhahn1 != null) {
                Kugelhahn1_script = Kugelhahn1.GetComponent<ValveObject>();
                if (Kugelhahn1_script != null) {
                    Kugelhahn1_script.valvePosition = (int)valvePosition;

                    Kugelhahn1_script.OnAnyInteraction += Name => {
                        newInteraction = true;
                        valvePosition = Kugelhahn1_script.valvePosition;
                    };
                }
            } else {
                Debug.Log("Pumpenkennlinie: Object Kugelhahn not found");
            }

            Motor = GameObject.Find("Elektromotor");
            if (Motor != null) {
                Motor_script = Motor.GetComponent<MotorObject>();
                if (Motor_script != null) {
                    Motor_script.speed = (int)rotationalSpeed;

                    Motor_script.OnAnyInteraction += Name => {
                        newInteraction = true;
                        rotationalSpeed = Motor_script.speed / 60f;
                        motor1isRunning = Motor_script.isRunning;
                    };
                }
            } else {
                Debug.Log("Pumpenkennlinie: Object Kugelhahn not found");
            }

            // Questmanager suchen
            newQuestManager = FindAnyObjectByType<QuestManager>();
            if (newQuestManager == null) {
                Debug.LogError("QuestManager not found");
            }
            // Die Quests des Pumpenkennlinien-Levels laden
            newQuestManager.SetQuestLine(QuestLines.Pumpenkennlinie);
        }

        private float timer = 0f;
        private float interval = 2f;

        void Update() {
            if (newInteraction) {
                Debug.Log("newInteraction = true");
                //zeta = 1000f * Mathf.Exp(-0.092f*valvePosition);
                zeta = 1000f * Mathf.Pow(1f - valvePosition / 100f, 10f);

                if (motor1isRunning) {
                    if (valvePosition < 1) {
                        Vdot = 0;
                        p2 = p1 + 0.5f * Psi0 * pi * pi * D2 * D2 * rotationalSpeed * rotationalSpeed * rho;
                        //p2 = p1 + rotationalSpeed;
                    } else {
                        //Vdot = Mathf.Sqrt(Psi0 * Mathf.Pow(pi, 4) * Mathf.Pow(D2,6)* Mathf.Pow(rotationalSpeed, 2)/((zeta*pi*pi* Mathf.Pow(D2, 4)/(A*A))+16*k*Psi0));
                        Vdot = rotationalSpeed * Mathf.Sqrt(Psi0 / ((zeta / (A * A * pi * pi * D2 * D2)) +
                                                                    (16 * k * Psi0 / (pi * pi * pi * pi * D2 * D2 * D2 *
                                                                        D2 * D2 * D2))));
                        //Vdot  = valvePosition + rotationalSpeed;

                        p2 = p1 + zeta * Vdot * Vdot * rho / (2 * A * A);
                        //p2 = p1 + valvePosition + rotationalSpeed;
                    }
                } else {
                    Vdot = 0;
                    p2 = p1;
                }

                Sensor_p2_script.SetPressure(p2);
                Sensor_V_script.SetVolumeFlow(Vdot * 3600);

                Debug.Log($"[Pumpenkennlinie] Vdot={Vdot} m�/s | p2={p2} Pa | zeta={zeta} | n={rotationalSpeed}");
                newInteraction = false;
            }
        }
    }
}