using InteractionObjects;
using Quests;
using Toolbar;
using UnityEngine;

namespace LevelScripts {
    public class ReihenParallelSchaltung : MonoBehaviour {
        
        GameObject Sensor_p1;
        SensorPressureObject Sensor_p1_script;

        GameObject Sensor_p2;
        SensorPressureObject Sensor_p2_script;

        GameObject Sensor_V;
        SensorFlowObject Sensor_V_script;

        GameObject Kugelhahn1;
        ValveObject Kugelhahn1_script;

        GameObject Kugelhahn2;
        ValveObject Kugelhahn2_script;

        GameObject Kugelhahn3;
        ValveObject Kugelhahn3_script;

        GameObject Motor1;
        MotorObject Motor1_script;

        GameObject Motor2;
        MotorObject Motor2_script;

        GameObject Maschine;
        WashingMachineObject Maschine_script;

        protected Quests.QuestManager newQuestManager;

        public bool newInteraction = true;

        // Variablen durch User in Level bestimmt
        public float valvePosition1 = 100;
        public float valvePosition2 = 100;
        public float valvePosition3 = 100;
        public float rotationalSpeed1 = 0;
        public float rotationalSpeed2 = 0;

        // Variablen berechnet
        public float zeta = 1;

        // Variablen berechnet und in Level angezeigt
        public float p1 = 100000; // ist vorgegeben als Referenzdruck
        public float p2 = 200000;
        public float Vdot = 0;

        public bool motor1isRunning = false;
        public bool motor2isRunning = false;

        public int machineProgram = 0;


        private float pi = Mathf.PI; // PI
        private float Psi0 = 0.2f; // Pumpenparameter
        private float k = 100f; // Pumpenparameter
        private float A = 0.00503f; // Rohrquerschnitt
        private float D = 0.08f; // Rohrdurchmesser
        private float rho = 1000f; // Wasserdichte
        private float D2 = 0.2f; // Laufraddurchmesser

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start() {
            ToolbarManager tm = FindAnyObjectByType<ToolbarManager>();
            tm.ShowCameraButtonContainer();

            Sensor_p1 = GameObject.Find("Sensor_p1");
            if (Sensor_p1 != null) {
                Sensor_p1_script = Sensor_p1.GetComponent<SensorPressureObject>();
                if (Sensor_p1_script != null) {
                    Sensor_p1_script.OnAnyInteraction += Name => { newInteraction = true; };
                }
            } else {
                Debug.Log("ReihenParallelSchaltung: Object Sensor_p1 not found");
            }

            Sensor_p2 = GameObject.Find("Sensor_p2");
            if (Sensor_p2 != null) {
                Sensor_p2_script = Sensor_p2.GetComponent<SensorPressureObject>();
                if (Sensor_p2_script != null) {
                    Sensor_p2_script.OnAnyInteraction += Name => { newInteraction = true; };
                }
            } else {
                Debug.Log("ReihenParallelSchaltung: Object Sensor_p1 not found");
            }

            Sensor_V = GameObject.Find("Volumenstrom");
            if (Sensor_V != null) {
                Sensor_V_script = Sensor_V.GetComponent<SensorFlowObject>();
                if (Sensor_V_script != null) {
                    Sensor_V_script.OnAnyInteraction += Name => { newInteraction = true; };
                }
            } else {
                Debug.Log("ReihenParallelSchaltung: Object Sensor_V not found");
            }

            Kugelhahn1 = GameObject.Find("Kugelhahn 1");
            if (Kugelhahn1 != null) {
                Kugelhahn1_script = Kugelhahn1.GetComponent<ValveObject>();
                if (Kugelhahn1_script != null) {
                    Kugelhahn1_script.valvePosition = (int)valvePosition1;

                    Kugelhahn1_script.OnAnyInteraction += Name => {
                        newInteraction = true;
                        valvePosition1 = Kugelhahn1_script.valvePosition;
                    };
                }
            } else {
                Debug.Log("ReihenParallelSchaltung: Object Kugelhahn 1 not found");
            }

            Kugelhahn2 = GameObject.Find("Kugelhahn 2");
            if (Kugelhahn2 != null) {
                Kugelhahn2_script = Kugelhahn2.GetComponent<ValveObject>();
                if (Kugelhahn2_script != null) {
                    Kugelhahn2_script.valvePosition = (int)valvePosition2;

                    Kugelhahn2_script.OnAnyInteraction += Name => {
                        newInteraction = true;
                        valvePosition2 = Kugelhahn2_script.valvePosition;
                    };
                }
            } else {
                Debug.Log("ReihenParallelSchaltung: Object Kugelhahn 2 not found");
            }

            Kugelhahn3 = GameObject.Find("Kugelhahn 3");
            if (Kugelhahn3 != null) {
                Kugelhahn3_script = Kugelhahn3.GetComponent<ValveObject>();
                if (Kugelhahn3_script != null) {
                    Kugelhahn3_script.valvePosition = (int)valvePosition3;

                    Kugelhahn3_script.OnAnyInteraction += Name => {
                        newInteraction = true;
                        valvePosition3 = Kugelhahn3_script.valvePosition;
                    };
                }
            } else {
                Debug.Log("ReihenParallelSchaltung: Object Kugelhahn 3 not found");
            }

            Motor1 = GameObject.Find("Elektromotor 1");
            if (Motor1 != null) {
                Motor1_script = Motor1.GetComponent<MotorObject>();
                if (Motor1_script != null) {
                    Motor1_script.speed = (int)rotationalSpeed1;

                    Motor1_script.OnAnyInteraction += Name => {
                        newInteraction = true;
                        rotationalSpeed1 = Motor1_script.speed / 60f;
                        motor1isRunning = Motor1_script.isRunning;
                    };
                }
            } else {
                Debug.Log("ReihenParallelSchaltung: Object Elektromotor 1 not found");
            }

            Motor2 = GameObject.Find("Elektromotor 2");
            if (Motor2 != null) {
                Motor2_script = Motor2.GetComponent<MotorObject>();
                if (Motor2_script != null) {
                    Motor2_script.speed = (int)rotationalSpeed2;

                    Motor2_script.OnAnyInteraction += Name => {
                        newInteraction = true;
                        rotationalSpeed2 = Motor2_script.speed / 60f;
                        motor2isRunning = Motor2_script.isRunning;
                    };
                }
            } else {
                Debug.Log("ReihenParallelSchaltung: Object Elektromotor 2 not found");
            }

            Maschine = GameObject.Find("Textilwaschmaschine");
            if (Maschine != null) {
                Maschine_script = Maschine.GetComponentInChildren<WashingMachineObject>();
                if (Maschine_script != null) {
                    Maschine_script.OnAnyInteraction += Name => {
                        newInteraction = true;
                        machineProgram = Maschine_script.program;
                    };
                } else {
                    Debug.LogError("ReihenParallelSchaltung: Component WashingMachineObject not found");
                }
            } else {
                Debug.LogError("ReihenParallelSchaltung: Object Textilwaschmaschine not found");
            }

            // Questmanager suchen
            newQuestManager = FindAnyObjectByType<Quests.QuestManager>();
            if (newQuestManager == null) {
                Debug.LogError("QuestManager not found");
            }
            // Die Quests des Pumpenkennlinien-Levels laden
            newQuestManager.SetQuestLine(QuestLines.ReihenParallelSchaltung);
        }

        private float timer = 0f;
        private float interval = 2f;

        // Update is called once per frame
        void Update() {
            if (newInteraction) {
                Debug.Log("newInteraction = true");
                //zeta = 1000f * Mathf.Exp(-0.092f*valvePosition);

                // Maschine ist aus und sperrt
                if (machineProgram == 0) {
                    Vdot = 0;
                    Debug.Log("Maschine aus:");

                    // Nur Pumpe 1 l�uft
                    if ((valvePosition1 == 100) && (valvePosition2 == 0) && (valvePosition3 == 0)) {
                        Debug.Log("\tNur Pumpe 1.");
                        p2 = p1 + 0.5f * Psi0 * pi * pi * D2 * D2 * rotationalSpeed1 * rotationalSpeed1 * rho;
                    }
                    // Nur Pumpe 2 l�uft
                    else if ((valvePosition1 == 0) && (valvePosition2 == 0) && (valvePosition3 == 100)) {
                        Debug.Log("\tNur Pumpe 2.");
                        p2 = p1 + 0.5f * Psi0 * pi * pi * D2 * D2 * rotationalSpeed2 * rotationalSpeed2 * rho;
                    }
                    // Reihenschaltung
                    else if ((valvePosition1 == 0) && (valvePosition2 == 100) && (valvePosition3 == 0)) {
                        Debug.Log("\tReihenschaltung.");
                        if (rotationalSpeed1 != rotationalSpeed2) {
                            Motor2_script.StartMotor();
                            Motor2_script.SetSpeedMotor(Mathf.RoundToInt(rotationalSpeed1 * 60f));
                        }
                        p2 = p1 + 0.5f * 2 * Psi0 * pi * pi * D2 * D2 * rotationalSpeed1 * rotationalSpeed1 * rho;
                    }
                    // Parallelschaltung
                    else if ((valvePosition1 == 100) && (valvePosition2 == 0) && (valvePosition3 == 100)) {
                        Debug.Log("\tParallelschaltung.");
                        if (rotationalSpeed1 != rotationalSpeed2) {
                            Motor2_script.StartMotor();
                            Motor2_script.SetSpeedMotor(Mathf.RoundToInt(rotationalSpeed1 * 60f));
                        }
                        p2 = p1 + 0.5f * Psi0 * pi * pi * D2 * D2 * rotationalSpeed1 * rotationalSpeed1 * rho;
                    }
                    // unzul�ssige Schaltung
                    else {
                        p2 = p1;
                    }
                }
                // Maschine l�uft
                else {
                    if (machineProgram == 1) {
                        Debug.Log("Maschine Programm 1:");
                        zeta = 0.2459f;
                    }
                    if (machineProgram == 2) {
                        Debug.Log("Maschine Programm 2:");
                        zeta = 11.8044f;
                    }

                    // Nur Pumpe 1 l�uft
                    if ((valvePosition1 == 100) && (valvePosition2 == 0) && (valvePosition3 == 0)) {
                        Debug.Log("\tNur Pumpe 1.");
                        Vdot = rotationalSpeed1 * Mathf.Sqrt(Psi0 / ((zeta / (A * A * pi * pi * D2 * D2)) +
                                                                     (16 * k * Psi0 / (pi * pi * pi * pi * D2 * D2 *
                                                                         D2 * D2 * D2 * D2))));
                        p2 = p1 + zeta * Vdot * Vdot * rho / (2 * A * A);
                    }
                    // Nur Pumpe 2 l�uft
                    else if ((valvePosition1 == 0) && (valvePosition2 == 0) && (valvePosition3 == 100)) {
                        Debug.Log("\tNur Pumpe 2.");
                        Vdot = rotationalSpeed2 * Mathf.Sqrt(Psi0 / ((zeta / (A * A * pi * pi * D2 * D2)) +
                                                                     (16 * k * Psi0 / (pi * pi * pi * pi * D2 * D2 *
                                                                         D2 * D2 * D2 * D2))));
                        p2 = p1 + zeta * Vdot * Vdot * rho / (2 * A * A);
                    }
                    // Reihenschaltung
                    else if ((valvePosition1 == 0) && (valvePosition2 == 100) && (valvePosition3 == 0)) {
                        Debug.Log("\tReihenschaltung.");
                        if (rotationalSpeed1 != rotationalSpeed2) {
                            Motor2_script.StartMotor();
                            Motor2_script.SetSpeedMotor(Mathf.RoundToInt(rotationalSpeed1 * 60f));
                        }
                        Vdot = rotationalSpeed1 * Mathf.Sqrt(2 * Psi0 / ((zeta / (A * A * pi * pi * D2 * D2)) +
                                                                         (16 * k * 2 * Psi0 / (pi * pi * pi * pi * D2 *
                                                                             D2 * D2 * D2 * D2 * D2))));
                        p2 = p1 + zeta * Vdot * Vdot * rho / (2 * A * A);
                    }
                    // Parallelschaltung
                    else if ((valvePosition1 == 100) && (valvePosition2 == 0) && (valvePosition3 == 100)) {
                        Debug.Log("\tParallelschaltung.");
                        if (rotationalSpeed1 != rotationalSpeed2) {
                            Motor2_script.StartMotor();
                            Motor2_script.SetSpeedMotor(Mathf.RoundToInt(rotationalSpeed1 * 60f));
                        }
                        Vdot = 2 * rotationalSpeed1 * Mathf.Sqrt(Psi0 / ((zeta / (A * A * pi * pi * D2 * D2)) +
                                                                         (16 * k * Psi0 / (pi * pi * pi * pi * D2 * D2 *
                                                                             D2 * D2 * D2 * D2))));
                        p2 = p1 + zeta * Vdot * Vdot * rho / (2 * A * A);
                    }
                    // unzul�ssige Schaltung
                    else {
                        Vdot = 0;
                        p2 = p1 * 1;
                    }

                    if (machineProgram == 1) {
                        if (Vdot > 400f / 3600f) {
                            Vdot = 400f / 3600f;
                            p2 = p1 + 60f * rho;
                        }
                    }
                    if (machineProgram == 2) {
                        if (Vdot > 100f / 3600f) {
                            Vdot = 100f / 3600f;
                            p2 = p1 + 180f * rho;
                        }
                    }
                }


                /*if (motor1isRunning)
                {
                    if (valvePosition1 < 1)
                    {
                        Vdot = 0;
                        p2 = p1 + 0.5f * Psi0 * pi * pi * D2 * D2 * rotationalSpeed1 * rotationalSpeed1 * rho;
                        //p2 = p1 + rotationalSpeed;
                    }
                    else
                    {
                        //Vdot = Mathf.Sqrt(Psi0 * Mathf.Pow(pi, 4) * Mathf.Pow(D2,6)* Mathf.Pow(rotationalSpeed, 2)/((zeta*pi*pi* Mathf.Pow(D2, 4)/(A*A))+16*k*Psi0));
                        Vdot = rotationalSpeed1 * Mathf.Sqrt(Psi0 / ((zeta / (A * A * pi * pi * D2 * D2)) + (16 * k * Psi0 / (pi * pi * pi * pi * D2 * D2 * D2 * D2 * D2 * D2))));
                        //Vdot  = valvePosition + rotationalSpeed;

                        p2 = p1 + zeta * Vdot * Vdot * rho / (2 * A * A);
                        //p2 = p1 + valvePosition + rotationalSpeed;
                    }
                }
                else
                {
                    Vdot = 0;
                    p2 = p1;
                }*/

                Sensor_p2_script.SetPressure(p2);
                Sensor_V_script.SetVolumeFlow(Vdot * 3600);

                Debug.Log(
                    $"[Pumpenkennlinie] Vdot={Vdot} m�/s | p2={p2} Pa | zeta={zeta} | n1={rotationalSpeed1} | n2={rotationalSpeed2}");
                newInteraction = false;
            }
        }
    }
}