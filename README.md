# ![AR FLOW](Assets/Images/Logo_Entwurf.png)

AR FLOW is an educational mobile game designed for interactive fluid mechanics education. Using virtual laboratories and a
variety of other levels, the application enables students to conduct interactive flow experiments and simulate
pump-piping systems using Android smartphones or tablets.

## Download

This repository contains the full Unity project, source code, and assets for AR FLOW. For building this project on your
own, see [Installation & Setup](#installation--setup) below.

A prebuilt APK for Android devices is available for download in
the [sciebo cloud](https://tu-dortmund.sciebo.de/s/cPm64BMDnzETW6T).

## Academic Publications

The development and educational impact of AR FLOW have been documented in the following academic publications:

### Innovative Fluid Mechanics Education Through Augmented Reality and Interactive Learning

```bibtex
@article{https://doi.org/10.1002/pamm.70085,
    author = {Behr, Alexander S. and Aurich, Daniel and Figge, Daniel and Bredberg, Oscar and Finke, Jacqueline and Neumann, Jakub and Boettcher, Konrad and Brümmer, Andreas},
    title = {Innovative Fluid Mechanics Education Through Augmented Reality and Interactive Learning},
    journal = {PAMM},
    volume = {26},
    number = {1},
    pages = {e70085},
    doi = {https://doi.org/10.1002/pamm.70085},
    url = {https://onlinelibrary.wiley.com/doi/abs/10.1002/pamm.70085},
    eprint = {https://onlinelibrary.wiley.com/doi/pdf/10.1002/pamm.70085},
    abstract = {Traditional pedagogical setups in laboratories are often outdated and do not provide the necessary support for effective learning. Teaching fluid mechanics poses additional significant challenges, particularly due to the need to visualize invisible properties, like velocity vector fields (velocity) or scalar pressure fields in time-dependent 3D spaces. In this context, the application constructive alignment (CA) promises a more holistic approach to achieving better learning success. Here, the intended learning outcomes (ILOs), the teaching-learning activities (TLAs), and the learning outcome monitoring (LOM), are taken as an iterative process and the three parts have to be perfectly aligned. To rate the ILOs, a cognitive taxonomy should be used. In an engineering context, we prefer the structure of observed learning outcome (SOLO) taxonomy as it helps to clarify, whether an ILO addresses surface understanding, or deep understanding. This work features a work in progress modular augmented reality (AR) mobile application called the AR FLOW for interactive fluid mechanic experiments. This application allows students to access worksheets via QR codes and explore various levels of learning. Furthermore, they can later access the laboratories that are set up in an interactive level system that helps them discover all the features of a level. The easy implementation of this application into existing lectures and courses is supported by accompanying teaching materials based on the principles of CA, featuring clear ILOs, and assessment tasks (ATs) rephrased according to SOLO taxonomy.},
    year = {2026}
}
```

### Evaluating Effectiveness and Appeal of a Virtual Laboratory in an Undergraduate Fluid Mechanics Course

```bibtex
@article{Aurich_Sommer-Behr_Neumann_Boettcher_Brümmer_2025, 
    title={Evaluating Effectiveness and Appeal of a Virtual Laboratory in an Undergraduate Fluid Mechanics Course}, 
    volume={15}, 
    url={https://online-journals.org/index.php/i-jep/article/view/59067}, 
    DOI={10.3991/ijep.v15i8.59067}, 
    abstractNote={This study investigates the integration of a smartphone-based virtual laboratory into a fourth-semester undergraduate fluid mechanics class on pump–piping systems. The virtual laboratory is designed according to constructive alignment and the SOLO taxonomy to foster deep learning. Students interact with realistic 3D system models, adjust component parameters, and receive real-time feedback based on physical simulations. To identify the effectiveness, a pre- and post-test with 26 paired responses showed a small overall improvement in general knowledge, with medium-to-large gains in specific methodological knowledge and selfassessed competence in handling real fluid systems. Student feedback was collected to assess the appeal of the teaching method. Students rate it highly positive (mean rating = 4.42/5), highlighting increased motivation, engagement, and active participation compared to conventional teaching. Future work will expand the app with additional levels targeting diverse learning objectives in fluid mechanics.}, 
    number={8}, 
    journal={International Journal of Engineering Pedagogy (iJEP)}, 
    author={Aurich, Daniel and Sommer-Behr, Alexander and Neumann, Jakub and Boettcher, Konrad E.R. and Brümmer, Andreas}, 
    year={2025}, 
    month={Dec.}, 
    pages={pp. 65–75} 
}
```

### AR-Flow: A Unity-Based Platform for Structured, Level-Oriented Learning in Fluid Mechanics

To be published soon.

## Levels

AR FLOW features a variety of levels, each designed to teach specific concepts in fluid mechanics.

- **Potential Flow**: Explore the concept of potential flow and visualize potential and flow functions.
- **3D Centrifugal Pump**: Visualize pressure and velocity fields in a 3D centrifugal pump model.
- **Pump Curve**: Determine the characteristic pump curve for different rotational speeds in a virtual lab setup.
- **Pump Circuit**: Determine the load characteristic curve of the washing machine in a virtual lab setup.
- **Reynolds' Dye Experiment**: Visualize laminar and turbulent flow regimes using a virtual dye injection experiment.
- **Crossflow Valve**: Visualize flow patterns and pressure distribution inside a crossflow valve.

Thanks to the modular architecture of the application, new levels can be easily added in the future to cover additional
topics in fluid mechanics or related fields.

## Installation & Setup

### Prerequisites

* **Unity 6000.0.60f1** with **Android Build Support**
* An Android mobile device compatible with ARCore (for the AR level)

### Getting Started

1. **Clone the Repository**
   ```bash
   git clone https://github.com/FluidVerse/AR-FLOW.git
   ```
2. **Download additional third party assets**
    * Some assets used in this project are not included in the repository due to licensing restrictions.
      See [THIRDPARTY-INSTALLATION.md](THIRDPARTY-INSTALLATION.md) for instructions on how to obtain and integrate these
      assets into the project.
3. **Open in Unity**
    * Launch Unity Hub.
    * Click "Add" and select the project folder.
    * Open the project (this may take some time as Unity imports assets).
4. **Build to Device**
    * Go to `File > Build Profiles`.
    * Select Android as your target platform and click `Switch Platform`.
    * Ensure the required scenes are in the scene list.
    * Click `Build` or connect your device and click `Build and Run`.

## License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

## Third Party Assets

This project utilizes several third-party assets and libraries.

Please refer to [THIRDPARTY.md](THIRDPARTY.md) for a full list of attributions and licenses.

**Important:** Some assets are excluded from this repository due to licensing issues. You **must** install them manually
to run the project.
**See [THIRDPARTY-INSTALLATION.md](THIRDPARTY-INSTALLATION.md) for installation instructions.**

## Contact

This project was developed as a collaboration between the **Department of Mechanical Engineering, Chair of Fluidics**
and **Department of Biochemical and Chemical Engineering, Laboratory of Equipment Design** at the **Technical University
of Dortmund**.

For inquiries, please refer to the contact information in the associated research papers.
