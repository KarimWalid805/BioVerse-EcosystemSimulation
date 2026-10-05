# BioVerse Ecosystem Simulation

BioVerse is a Unity and C# agent-based simulation of a marine reef ecosystem. The project accompanies the thesis **A 3D Visual Representation of Marine Ecosystem Processes Inspired by Computational Ecology Formulae**. It turns ecological ideas into individual organism behaviors, then tracks how those local interactions shape whole-population trends.

The repository currently contains Unity C# scripts and `.meta` files, but no Unity project settings, scenes, prefabs, or art assets. It is therefore a source-code snapshot rather than a ready-to-open Unity project.

## Research background

The thesis investigates whether a real-time 3D simulation can make marine predator-prey dynamics easier to explore than laboratory observation alone. Its research focus is the interaction among environmental temperature, organism-level survival and reproduction, and population-level ecosystem change. The intended audience includes students, educators, and researchers using the simulation as an exploratory and educational tool.

The modeled reef food web has three principal population groups:

- **Benthic algae** are the primary producers and the energy source for clownfish.
- **Clownfish** are consumers and prey for lionfish when outside the protection of an anemone.
- **Lionfish** are predators whose food supply depends on clownfish.

Sea anemones provide a protected habitat for clownfish and organize their local social hierarchy. This adds spatial refuge and density-dependent effects to the simpler producer-consumer-predator chain.

## Mathematical implementation

### Lotka-Volterra as a conceptual reference

The thesis uses Lotka-Volterra predator-prey theory to frame the expected out-of-phase oscillations: prey increase when predator pressure is low, predators respond to prey availability, and increased predation then reduces prey. In the two-species form, this relationship can be expressed as:

```text
dC/dt = rC - aCL
dL/dt = eaCL - mL
```

Here, `C` is the clownfish population, `L` is the lionfish population, `r` is prey growth, `a` is the encounter/predation coefficient, `e` is the conversion of consumed prey into predator growth, and `m` is predator mortality. This is the theoretical reference model; the checked-in Unity scripts do **not** numerically integrate this differential-equation system. They implement discrete agents, energy thresholds, feeding, hunting, reproduction, and death. Population oscillations emerge from those rules and are compared with the theoretical pattern.

### Temperature stress curve

The implemented environmental control is temperature, adjustable from 20°C to 35°C, with an optimum at 27°C. Several scripts normalize deviation from this optimum and apply a power curve:

```text
s(T) = ((27 - T) / 7)^p,  for 20 <= T <= 27
s(T) = ((T - 27) / 8)^p,  for 27 < T <= 35
```

The Unity code clamps the normalized stress to the slider range. The exponent is typically `p = 2.5` for fish metabolism and algae spawning, and `p = 3` for egg development. The resulting stress value is interpolated into biological parameters: fish energy loss and reproductive cooldowns rise, algae spawning slows, egg incubation takes longer, and clutch sizes fall as temperature moves toward either extreme. Algae growth code also calculates a temperature-dependent rate, although the current implementation does not apply that calculated value to its growth field.

### Population statistics

The dashboard counts clownfish, lionfish, and algae at regular intervals and plots their population time series. It calculates the Shannon diversity index and Simpson dominance from population proportions `pᵢ`:

```text
Shannon diversity: H = -Σ pᵢ ln(pᵢ)
Simpson dominance: D =  Σ pᵢ²
```

The report generator exports an HTML report with the population chart and minimum, maximum, and average sampled populations, plus the current clownfish hierarchy. The chart uses Chart.js from a CDN.

## Biological behaviors represented

- **Clownfish:** energy-based foraging; predator avoidance and return to anemone refuge; growth and lifespan; Queen, King, Spare, and Baby ranks; sex transition after loss of the dominant female; egg laying, fertilization, temperature-dependent hatching, and juvenile recruitment.
- **Lionfish:** prey detection and pursuit; energy gain from feeding and starvation when food is unavailable; age, reproduction, courtship, drifting eggs, and temperature-dependent incubation and clutch size.
- **Algae:** spawning with an upper population limit, temperature-dependent spawning intervals, regrowth, and depletion when grazed by clownfish.
- **Sea anemones:** resident tracking, size-based clownfish hierarchy, a local capacity limit, and eviction of lower-ranking fish when the habitat is full.
- **Reef environment:** Unity NavMesh pathfinding, physics triggers and raycasts, obstacle avoidance, and terrain alignment support movement, foraging, predation, and habitat interactions.

The checked-in scripts implement a temperature slider and temperature responses. Although sunlight appears among the thesis's proposed environmental factors, no sunlight control is present in this repository snapshot.

## Thesis validation and findings

The thesis describes iterative two-species algae-clownfish and three-species algae-clownfish-lionfish simulations, with changes to reproduction, energy loss, algae availability, and temperature. Its validation combines theoretical comparison with ecological data and field literature:

- At **27°C**, the thesis reports recurring, out-of-phase population changes and compares their predator-prey phase lag with Atlantic cod and capelin time-series records from the Global Population Dynamics Database (GPDD). The thesis processes those records in R with `dplyr` and `ggplot2`.
- At **31°C**, it reports a trophic cascade in which increased lionfish metabolic demand contributes to predator starvation, releasing clownfish from predation and increasing grazing pressure on algae.
- At **20°C** and **35°C**, it reports severe reproductive stress and ecosystem collapse near thermal extremes.
- The thesis reports that the predator population still dies out after roughly 15–16 simulated minutes even under the 27°C baseline. It identifies population stability and performance at larger agent counts as limitations.

These are findings reported in the thesis, not guarantees that every run produces the same outcome. The simulation includes stochastic behavior and depends on scene configuration and parameter values.

## Technical structure

| Folder | Contents |
| --- | --- |
| `CoreLogic/` | Shared fish energy/lifespan behavior, spawning, time, and terrain alignment |
| `Algae Logic/` | Algae growth and spawning |
| `ClownFish/` | Clownfish foraging, fleeing, anemone movement, and reproduction |
| `LionFish/` | Lionfish hunting, reproduction, eggs, animation, and juvenile descent |
| `Heirarchy/` | Anemone resident hierarchy and capacity management |
| `DashBoard/` | Temperature and simulation controls, population metrics, graphing, and HTML export |
| `MainMenu/` | Main menu behavior |

The code uses Unity `MonoBehaviour` components and C# object-oriented design. Organism behavior is split across components and enabled or disabled as priorities change; navigation and physical sensing rely on Unity NavMesh and physics systems.

## Working with the source

The repository does not currently include an `Assets/` project tree, `ProjectSettings/`, `Packages/manifest.json`, Unity scene files, prefabs, or the referenced 3D models. To use these scripts, place the folders under `Assets/` in a compatible Unity project, add the required TextMeshPro and Unity UI components, and create the scene objects, tags, prefabs, colliders, NavMesh, and Inspector references expected by the scripts.

Important scene tags include `ClownFish`, `LionFish`, `RedAlgae`, and `Terrain`. The simulation also requires correctly configured anemone, fish, egg, and algae prefabs and dashboard UI references. Because these project assets and settings are not included here, a complete build or run cannot be reproduced from this repository alone.

## Scope and future work

The thesis proposes extensions including salinity and CO₂ controls, customizable starting populations, more trophic interactions, accessibility options, and Unity DOTS/ECS with the C# Job System to support larger populations. These are proposed future improvements, not features documented in the current scripts.

## License

No license file is included. Contact the repository owner for reuse and distribution terms.
