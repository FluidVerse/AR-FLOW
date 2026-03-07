using System.Collections.Generic;
using static Quests.QuestObject;
using static Quests.QuestInteractionTypes;

namespace Quests {
    /// <summary>
    /// Static class to hold all the <see cref="QuestLine"/> instances, i.e. all the different levels in the game.
    /// </summary>
    public static class QuestLines {

        public static QuestLine TestLevel =>
            new QuestLine("Testlevel", "Testbeschreibung!", new List<QuestStage> {
                new QuestStage("Ebene 1",
                    "Willkommen im Testlevel! Hier lernen Sie die Grundlagen des Spiels kennen.",
                    new List<Quest> {
                        new Quest("Überblick verschaffen",
                            "Schauen Sie sich die Kupplung und den Elektromotor im Detail an.",
                            new List<IQuestTarget> {
                                new QuestTarget<object>(Kupplung, UseDetailView),
                                new QuestTarget<object>(Elektromotor1, UseDetailView),
                            }),
                        new Quest("Ventil anschauen",
                            "Schauen Sie sich das Ventil im Detail an.",
                            new List<IQuestTarget> {
                                new QuestTarget<object>(Kugelhahn1, UseDetailView)
                            },
                            isOptional: true),
                        new Quest("Ventil einstellen", "Stellen sie das Ventil korrekt ein.",
                            new List<IQuestTarget> {
                                new QuestTarget<int>(Kugelhahn1, SetValvePosition, value => value > 50)
                            },
                            new List<IQuestInteraction> {
                                new QuestInteraction<int>(Kugelhahn1, SetValvePosition, 100)
                            }),
                        new Quest("Ventil anschauen (doppelt)",
                            "Schauen Sie sich das Ventil im Detail an.",
                            new List<IQuestTarget> {
                                new QuestTarget<object>(Kugelhahn1, UseDetailView)
                            },
                            isOptional: true),
                        new Quest("Ventil anschauen (dreifach)",
                            "Schauen Sie sich das Ventil im Detail an.",
                            new List<IQuestTarget> {
                                new QuestTarget<object>(Kugelhahn1, UseDetailView)
                            },
                            isOptional: true)
                    }),
                new QuestStage("Ebene 2",
                    "Teilen Sie dieses Spiel, wenn Sie es so weit geschafft haben!",
                    new List<Quest> {
                        new Quest("Ventil anschauen",
                            "Schauen Sie sich das Ventil im Detail an.",
                            new List<IQuestTarget> {
                                new QuestTarget<object>(Kugelhahn1, UseDetailView)
                            })
                    }),
                new QuestStage("Ebene 3",
                    "Copy-Paste, um die UI besser zu testen.",
                    new List<Quest> {
                        new Quest("Ventil erneut anschauen",
                            "Schauen Sie sich das Ventil im Detail an.",
                            new List<IQuestTarget> {
                                new QuestTarget<object>(Kugelhahn1, UseDetailView)
                            })
                    })
            });


        public static QuestLine Pumpenkennlinie =>
            new QuestLine("Pump Curve", "Welcome!\n\n" +
                                        "In this level, you have a centrifugal pump as the supply unit and a ball valve as the load.\n" +
                                        "Your task is to determine the pump characteristic curve for two different rotational speeds.\n\n" +
                                        "First, get an overview, take a closer look at the components, and then determine the curves.\n\n" +
                                        "\n\nGood luck!",
                new List<QuestStage> {
                    new QuestStage("Overview",
                        "Get an overview.",
                        new List<Quest> {
                            new Quest("Centrifugal Pump",
                                "Take a closer look at the centrifugal pump.",
                                new List<IQuestTarget> {
                                    new QuestTarget<object>(Pumpe1, UseDetailView)
                                }),
                            new Quest("Electric Motor",
                                "Take a closer look at the electric motor.",
                                new List<IQuestTarget> {
                                    new QuestTarget<object>(Elektromotor1, UseDetailView)
                                }),
                            new Quest("Pressure Sensor p1",
                                "Take a closer look at pressure sensor p1.",
                                new List<IQuestTarget> {
                                    new QuestTarget<object>(DruckSensor1, UseDetailView)
                                }),
                            new Quest("Pressure Sensor p2",
                                "Take a closer look at pressure sensor p2.",
                                new List<IQuestTarget> {
                                    new QuestTarget<object>(DruckSensor2, UseDetailView)
                                }),
                            new Quest("Ball Valve",
                                "Take a closer look at the ball valve.",
                                new List<IQuestTarget> {
                                    new QuestTarget<object>(Kugelhahn1, UseDetailView)
                                })
                        }),

                    new QuestStage("Measure Point 1",
                        "Record the first measurement point.",
                        new List<Quest> {
                            new Quest("Set Valve",
                                "Set the valve to 0%.",
                                new List<IQuestTarget> {
                                    new QuestTarget<int>(Kugelhahn1, SetValvePosition, value => value == 0)
                                }),
                            new Quest("Set Speed",
                                "Set the motor speed to 1000 rpm.",
                                new List<IQuestTarget> {
                                    new QuestTarget<int>(Elektromotor1, SetMotorSpeed, value => value == 1000)
                                },
                                new List<IQuestInteraction> {
                                    new QuestInteraction<object>(Elektromotor1, QuestInteractionTypes.BlockMotorSpeed)
                                }),
                            new Quest("Start Motor",
                                "Turn on the electric motor.",
                                new List<IQuestTarget> {
                                    new QuestTarget<object>(Elektromotor1, StartMotor)
                                }),
                            new Quest("Read Pressure",
                                "Read the static pressure p2.",
                                new List<IQuestTarget> {
                                    new QuestTarget<object>(DruckSensor2, QuestInteractionTypes.OpenInteractionMenu)
                                }),
                            new Quest("Read Flow Rate",
                                "Read the volume flow rate.",
                                new List<IQuestTarget> {
                                    new QuestTarget<object>(VolumenstromSensor1,
                                        QuestInteractionTypes.OpenInteractionMenu)
                                })
                        }),

                    new QuestStage("Measure Point 2",
                        "Record the next measurement point.",
                        new List<Quest> {
                            new Quest("Set Valve",
                                "Set the valve to 40%.",
                                new List<IQuestTarget> {
                                    new QuestTarget<int>(Kugelhahn1, SetValvePosition, value => value == 40)
                                }),
                            new Quest("Read Pressure",
                                "Read the static pressure p2.",
                                new List<IQuestTarget> {
                                    new QuestTarget<object>(DruckSensor2, QuestInteractionTypes.OpenInteractionMenu)
                                }),
                            new Quest("Read Flow Rate",
                                "Read the volume flow rate.",
                                new List<IQuestTarget> {
                                    new QuestTarget<object>(VolumenstromSensor1,
                                        QuestInteractionTypes.OpenInteractionMenu)
                                })
                        }),

                    new QuestStage("Measure Point 3",
                        "Record the next measurement point.",
                        new List<Quest> {
                            new Quest("Set Valve",
                                "Set the valve to 50%.",
                                new List<IQuestTarget> {
                                    new QuestTarget<int>(Kugelhahn1, SetValvePosition, value => value == 50)
                                }),
                            new Quest("Read Pressure",
                                "Read the static pressure p2.",
                                new List<IQuestTarget> {
                                    new QuestTarget<object>(DruckSensor2, QuestInteractionTypes.OpenInteractionMenu)
                                }),
                            new Quest("Read Flow Rate",
                                "Read the volume flow rate.",
                                new List<IQuestTarget> {
                                    new QuestTarget<object>(VolumenstromSensor1,
                                        QuestInteractionTypes.OpenInteractionMenu)
                                })
                        }),

                    new QuestStage("Measure Point 4",
                        "Record the next measurement point.",
                        new List<Quest> {
                            new Quest("Set Valve",
                                "Set the valve to 100%.",
                                new List<IQuestTarget> {
                                    new QuestTarget<int>(Kugelhahn1, SetValvePosition, value => value == 100)
                                }),
                            new Quest("Read Pressure",
                                "Read the static pressure p2.",
                                new List<IQuestTarget> {
                                    new QuestTarget<object>(DruckSensor2, QuestInteractionTypes.OpenInteractionMenu)
                                }),
                            new Quest("Read Flow Rate",
                                "Read the volume flow rate.",
                                new List<IQuestTarget> {
                                    new QuestTarget<object>(VolumenstromSensor1,
                                        QuestInteractionTypes.OpenInteractionMenu)
                                })
                        }),

                    new QuestStage("Measure Point 5",
                        "Record the first measurement point at the new speed.",
                        new List<Quest> {
                            new Quest("Set Valve",
                                "Set the valve to 0%.",
                                new List<IQuestTarget> {
                                    new QuestTarget<int>(Kugelhahn1, SetValvePosition, value => value == 0)
                                }),
                            new Quest("Set Speed",
                                "Set the motor speed to 2000 rpm.",
                                new List<IQuestTarget> {
                                    new QuestTarget<int>(Elektromotor1, SetMotorSpeed, value => value == 2000)
                                }),
                            new Quest("Read Pressure",
                                "Read the static pressure p2.",
                                new List<IQuestTarget> {
                                    new QuestTarget<object>(DruckSensor2, QuestInteractionTypes.OpenInteractionMenu)
                                }),
                            new Quest("Read Flow Rate",
                                "Read the volume flow rate.",
                                new List<IQuestTarget> {
                                    new QuestTarget<object>(VolumenstromSensor1,
                                        QuestInteractionTypes.OpenInteractionMenu)
                                })
                        },
                        new List<IQuestInteraction> {
                            new QuestInteraction<object>(Elektromotor1, UnblockMotorSpeed)
                        }),

                    new QuestStage("Measure Point 6",
                        "Record the next measurement point.",
                        new List<Quest> {
                            new Quest("Set Valve",
                                "Set the valve to 40%.",
                                new List<IQuestTarget> {
                                    new QuestTarget<int>(Kugelhahn1, SetValvePosition, value => value == 40)
                                }),
                            new Quest("Read Pressure",
                                "Read the static pressure p2.",
                                new List<IQuestTarget> {
                                    new QuestTarget<object>(DruckSensor2, QuestInteractionTypes.OpenInteractionMenu)
                                }),
                            new Quest("Read Flow Rate",
                                "Read the volume flow rate.",
                                new List<IQuestTarget> {
                                    new QuestTarget<object>(VolumenstromSensor1,
                                        QuestInteractionTypes.OpenInteractionMenu)
                                })
                        }),

                    new QuestStage("Measure Point 7",
                        "Record the next measurement point.",
                        new List<Quest> {
                            new Quest("Set Valve",
                                "Set the valve to 50%.",
                                new List<IQuestTarget> {
                                    new QuestTarget<int>(Kugelhahn1, SetValvePosition, value => value == 50)
                                }),
                            new Quest("Read Pressure",
                                "Read the static pressure p2.",
                                new List<IQuestTarget> {
                                    new QuestTarget<object>(DruckSensor2, QuestInteractionTypes.OpenInteractionMenu)
                                }),
                            new Quest("Read Flow Rate",
                                "Read the volume flow rate.",
                                new List<IQuestTarget> {
                                    new QuestTarget<object>(VolumenstromSensor1,
                                        QuestInteractionTypes.OpenInteractionMenu)
                                })
                        }),

                    new QuestStage("Measure Point 8",
                        "Record the next measurement point.",
                        new List<Quest> {
                            new Quest("Set Valve",
                                "Set the valve to 100%.",
                                new List<IQuestTarget> {
                                    new QuestTarget<int>(Kugelhahn1, SetValvePosition, value => value == 100)
                                }),
                            new Quest("Read Pressure",
                                "Read the static pressure p2.",
                                new List<IQuestTarget> {
                                    new QuestTarget<object>(DruckSensor2, QuestInteractionTypes.OpenInteractionMenu)
                                }),
                            new Quest("Read Flow Rate",
                                "Read the volume flow rate.",
                                new List<IQuestTarget> {
                                    new QuestTarget<object>(VolumenstromSensor1,
                                        QuestInteractionTypes.OpenInteractionMenu)
                                })
                        })
                });

        public static QuestLine ReihenParallelSchaltung =>
            new QuestLine("Pump Circuit",
                "Welcome!\n\nIn this level, you have two identical centrifugal pumps available to operate a textile washing machine. The washing machine has two different programs for different wash cycles.\n" +
                "You can configure different pump arrangements using the three ball valves.\n\n" +
                "The pump controllers detect invalid valve positions and block the flow if necessary. In series and parallel configurations, the rotational speed of pump 1 is automatically applied to the other pump. The washing machine automatically limits the flow rate according to the selected program.\n\n" +
                "First, get an overview, then set up both a series and a parallel configuration and determine the load characteristic of the washing machine.\n\nGood luck!",
                new List<QuestStage> {
                    new QuestStage("Overview",
                        "Get an overview.",
                        new List<Quest> {
                            new Quest("Centrifugal Pumps",
                                "Take a closer look at both centrifugal pumps.",
                                new List<IQuestTarget> {
                                    new QuestTarget<object>(Pumpe1, OpenInteractionMenu),
                                    new QuestTarget<object>(Pumpe2, OpenInteractionMenu)
                                }),
                            new Quest("Ball Valves",
                                "Take a closer look at all ball valves.",
                                new List<IQuestTarget> {
                                    new QuestTarget<object>(Kugelhahn1, OpenInteractionMenu),
                                    new QuestTarget<object>(Kugelhahn2, OpenInteractionMenu),
                                    new QuestTarget<object>(Kugelhahn3, OpenInteractionMenu)
                                }),
                            new Quest("Pressure Sensors",
                                "Take a closer look at both pressure sensors.",
                                new List<IQuestTarget> {
                                    new QuestTarget<object>(DruckSensor1, OpenInteractionMenu),
                                    new QuestTarget<object>(DruckSensor2, OpenInteractionMenu)
                                }),
                            new Quest("Flow Meter",
                                "Take a closer look at the flow meter.",
                                new List<IQuestTarget> {
                                    new QuestTarget<object>(VolumenstromSensor1, OpenInteractionMenu)
                                })
                        }),

                    new QuestStage("Parallel Config.",
                        "Set up a parallel pump configuration.",
                        new List<Quest> {
                            new Quest("Valve Settings",
                                "Configure a parallel arrangement of the pumps using the valves.",
                                new List<IQuestTarget> {
                                    new QuestTarget<int>(Kugelhahn1, SetValvePosition, value => value == 100),
                                    new QuestTarget<int>(Kugelhahn2, SetValvePosition, value => value == 0),
                                    new QuestTarget<int>(Kugelhahn3, SetValvePosition, value => value == 100)
                                })
                        }),

                    new QuestStage("Series Config.",
                        "Set up a series pump configuration.",
                        new List<Quest> {
                            new Quest("Valve Settings",
                                "Configure a series arrangement of the pumps using the valves.",
                                new List<IQuestTarget> {
                                    new QuestTarget<int>(Kugelhahn1, SetValvePosition, value => value == 0),
                                    new QuestTarget<int>(Kugelhahn2, SetValvePosition, value => value == 100),
                                    new QuestTarget<int>(Kugelhahn3, SetValvePosition, value => value == 0)
                                })
                        }),

                    new QuestStage("Measure Point 1",
                        "Record the first measurement point.",
                        new List<Quest> {
                            new Quest("Set Speed",
                                "Set the rotational speed of both electric motors to 500 rpm and turn them on.",
                                new List<IQuestTarget> {
                                    new QuestTarget<int>(Elektromotor1, SetMotorSpeed, value => value == 500),
                                    new QuestTarget<int>(Elektromotor2, SetMotorSpeed, value => value == 500),
                                    new QuestTarget<object>(Elektromotor1, StartMotor),
                                    new QuestTarget<object>(Elektromotor2, StartMotor)
                                }),
                            new Quest("Start Machine",
                                "Start washing program 2.",
                                new List<IQuestTarget> {
                                    new QuestTarget<object>(Maschine1, QuestInteractionTypes.StartProgram2)
                                }),
                            new Quest("Read Pressure",
                                "Read the static pressure p2.",
                                new List<IQuestTarget> {
                                    new QuestTarget<object>(DruckSensor2, QuestInteractionTypes.OpenInteractionMenu)
                                }),
                            new Quest("Read Flow Rate",
                                "Read the volume flow rate.",
                                new List<IQuestTarget> {
                                    new QuestTarget<object>(VolumenstromSensor1,
                                        QuestInteractionTypes.OpenInteractionMenu)
                                })
                        }),

                    new QuestStage("Measure Point 2",
                        "Record the next measurement point.",
                        new List<Quest> {
                            new Quest("Set Speed",
                                "Set the rotational speed of both electric motors to 1000 rpm.",
                                new List<IQuestTarget> {
                                    new QuestTarget<int>(Elektromotor1, SetMotorSpeed, value => value == 1000),
                                    new QuestTarget<int>(Elektromotor2, SetMotorSpeed, value => value == 1000)
                                }),
                            new Quest("Read Pressure",
                                "Read the static pressure p2.",
                                new List<IQuestTarget> {
                                    new QuestTarget<object>(DruckSensor2, QuestInteractionTypes.OpenInteractionMenu)
                                }),
                            new Quest("Read Flow Rate",
                                "Read the volume flow rate.",
                                new List<IQuestTarget> {
                                    new QuestTarget<object>(VolumenstromSensor1,
                                        QuestInteractionTypes.OpenInteractionMenu)
                                })
                        }),

                    new QuestStage("Measure Point 3",
                        "Record the next measurement point.",
                        new List<Quest> {
                            new Quest("Set Speed",
                                "Set the rotational speed of both electric motors to 1500 rpm.",
                                new List<IQuestTarget> {
                                    new QuestTarget<int>(Elektromotor1, SetMotorSpeed, value => value == 1500),
                                    new QuestTarget<int>(Elektromotor2, SetMotorSpeed, value => value == 1500)
                                }),
                            new Quest("Read Pressure",
                                "Read the static pressure p2.",
                                new List<IQuestTarget> {
                                    new QuestTarget<object>(DruckSensor2, QuestInteractionTypes.OpenInteractionMenu)
                                }),
                            new Quest("Read Flow Rate",
                                "Read the volume flow rate.",
                                new List<IQuestTarget> {
                                    new QuestTarget<object>(VolumenstromSensor1,
                                        QuestInteractionTypes.OpenInteractionMenu)
                                })
                        }),

                    new QuestStage("Measure Point 4",
                        "Record the next measurement point.",
                        new List<Quest> {
                            new Quest("Set Speed",
                                "Set the rotational speed of both electric motors to 2000 rpm.",
                                new List<IQuestTarget> {
                                    new QuestTarget<int>(Elektromotor1, SetMotorSpeed, value => value == 2000),
                                    new QuestTarget<int>(Elektromotor2, SetMotorSpeed, value => value == 2000)
                                }),
                            new Quest("Read Pressure",
                                "Read the static pressure p2.",
                                new List<IQuestTarget> {
                                    new QuestTarget<object>(DruckSensor2, QuestInteractionTypes.OpenInteractionMenu)
                                }),
                            new Quest("Read Flow Rate",
                                "Read the volume flow rate.",
                                new List<IQuestTarget> {
                                    new QuestTarget<object>(VolumenstromSensor1,
                                        QuestInteractionTypes.OpenInteractionMenu)
                                })
                        }),

                    new QuestStage("Measure Point 5",
                        "Record the next measurement point.",
                        new List<Quest> {
                            new Quest("Set Speed",
                                "Set the rotational speed of both electric motors to 2500 rpm.",
                                new List<IQuestTarget> {
                                    new QuestTarget<int>(Elektromotor1, SetMotorSpeed, value => value == 2500),
                                    new QuestTarget<int>(Elektromotor2, SetMotorSpeed, value => value == 2500)
                                }),
                            new Quest("Read Pressure",
                                "Read the static pressure p2.",
                                new List<IQuestTarget> {
                                    new QuestTarget<object>(DruckSensor2, QuestInteractionTypes.OpenInteractionMenu)
                                }),
                            new Quest("Read Flow Rate",
                                "Read the volume flow rate.",
                                new List<IQuestTarget> {
                                    new QuestTarget<object>(VolumenstromSensor1,
                                        QuestInteractionTypes.OpenInteractionMenu)
                                })
                        })
                });

        // TODO commented out the TODO quests for now 
        public static QuestLine FarbfadenLevel =>
            new QuestLine("Reynoldscher Farbfadenversuch",
                "Willkommen im Rohr!\n\nHier hat Herr Reynolds einst Versuche durchgeführt, um die Einflüsse von Trägheit und Reibung in einem Rohr zu untersuchen. \nIm Gegensatz zu ihm kannst Du hier einfach die Reynolds-Zahl per Schieberegler verändern und dir den Übergang von laminarer zu turbulenter Strömung anschauen.\n\nDie moderne Technik ermöglicht es uns außerdem, die Geschwindigkeitsprofile im Rohr und in Wandnähe zu visualisieren! Da es im turbulenten keine analytische Lösung gibt, haben wir euch hier zwei verschiedene Modelle für die mittleren Geschwindigkeitsprofile mitgebracht (Salama und Power Law).\n\n Viel Spaß!",
                new List<QuestStage> {
                    // Stage 1: Reynolds-Zahl einstellen
                    new QuestStage("Reynolds-Zahl", "<unbenutzt>", new List<Quest> {
                        new Quest("Kritische Re-Zahl finden",
                            "Stelle mit dem Schieberegler die kritische Reynolds-Zahl ein, bei der die Strömung von laminar zu turbulent wechselt (Re_krit ≈ 2300).",
                            new List<IQuestTarget> {
                                new QuestTarget<float>(ReynoldsSlider, SetReynoldsNumber)
                            })
                    }),
                    // Stage 2: k/D = 0.05
                    new QuestStage("k/D = 0.05", "<unbenutzt>", new List<Quest> {
                        new Quest("k/D = 0.05",
                            "Rufe das Moody-Diagramm über die Taste X/Y auf und zeichne für k/D = 0.05 den Punkt ein, ab dem sich das Geschwindigkeitsprofil nicht mehr ändert",
                            new List<IQuestTarget> {
                                new QuestTarget<object>(MoodyDiagram, ClickedCorrectly)
                            })
                    }),
                    // Stage 3: k/D = 0.1
                    new QuestStage("k/D = 0.1", "<unbenutzt>", new List<Quest> {
                        new Quest("k/D = 0.1",
                            "Rufe das Moody-Diagramm über die Taste X/Y auf und zeichne für k/D = 0.1 den Punkt ein, ab dem sich das Geschwindigkeitsprofil nicht mehr ändert",
                            new List<IQuestTarget> {
                                new QuestTarget<object>(MoodyDiagram, ClickedCorrectly)
                            })
                    }),
                    // Stage 4: k/D = 0.03
                    new QuestStage("k/D = 0.03", "<unbenutzt>", new List<Quest> {
                        new Quest("k/D = 0.03",
                            "Rufe das Moody-Diagramm über die Taste X/Y auf und zeichne für k/D = 0.03 den Punkt ein, ab dem sich das Geschwindigkeitsprofil nicht mehr ändert",
                            new List<IQuestTarget> {
                                new QuestTarget<object>(MoodyDiagram, ClickedCorrectly)
                            })
                    }),
                    // Stage 5: k/D = 0.08
                    new QuestStage("k/D = 0.08", "<unbenutzt>", new List<Quest> {
                        new Quest("k/D = 0.08",
                            "Rufe das Moody-Diagramm über die Taste X/Y auf und zeichne für k/D = 0.08 den Punkt ein, ab dem sich das Geschwindigkeitsprofil nicht mehr ändert",
                            new List<IQuestTarget> {
                                new QuestTarget<object>(MoodyDiagram, ClickedCorrectly)
                            })
                    }),
                    // Stage 6: Erklärung (Platzhalter)
                    /*new QuestStage("Erklärung", "<unbenutzt>", new List<Quest> {
                        new Quest("Erklärung", "TODO_EXPLANATION", new List<IQuestTarget> {
                            new QuestTarget<object>(MoodyDiagram, ClickedCorrectly)
                        })
                    }),
                    // Stage 7: Frage (Eingabefeld)
                    new QuestStage("Frage", "<unbenutzt>", new List<Quest> {
                        new Quest("Frage", "TODO_QUESTION", new List<IQuestTarget> {
                            new QuestTarget<string>(InputField, AnsweredCorrectly)
                        }, requiresInput: true)
                    }),
                    // Stage 8: Durchmesser berechnen
                    new QuestStage("Durchmesser", "<unbenutzt>", new List<Quest> {
                        new Quest("Durchmesser", "Berechne den Durchmesser des Rohres", new List<IQuestTarget> {
                            new QuestTarget<string>(InputField, AnsweredCorrectly)
                        }, requiresInput: true)
                    })*/
                });

        public static QuestLine ARScene =>
            new QuestLine("AR-Szene", "TODO AR", new List<QuestStage>(0));

        public static QuestLine PumpenlevelAR =>
            new QuestLine("Centrifugal Pump",
                "Welcome to the centrifugal pump!\n\nRotate and zoom the model to explore the inner structure in 3D.\nYou can hide the casing and impeller parts to take a closer look inside.\n\nVisualize pressure, absolute velocity and relative velocity as interactive 3D fields based on real CFD simulation data.\n\nDiscover how energy is transferred from the rotating impeller to the fluid and observe how flow and pressure develop throughout the pump.\n\nHave fun exploring!",
                new List<QuestStage>(0));

        public static QuestLine Potential =>
            new QuestLine("Potential",
                "Welcome to the potential level! Try out our sandbox-like tool to visualize potential and flow functions!",
                new List<QuestStage>(0));

        public static QuestLine Kreuzstrom =>
            new QuestLine("Crossflow Valve",
                "Welcome to crossflow!\n\nTap the ends of the crossflow valve to set them as inlets or outlets.\nYou can visualize all important flow quantities to better understand what is happening inside the system.\n\nExperiment with different configurations and observe how the flow paths change and interact.\n\nHave fun exploring!",
                new List<QuestStage>(0));
    }
}