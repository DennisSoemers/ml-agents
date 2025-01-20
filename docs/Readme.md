# Unity ML-Agents Toolkit for Deep Reinforcement Learning Experiments

## Project Overview
The DRL Unity Project integrates Unity's ML-Agents Toolkit with custom configurations to explore and analyze various DRL algorithms. The project focuses on:
- Comparing the performance of different configurations of the POCA algorithm.
- Documenting and sharing reproducible results with detailed experiment instructions.
- Evaluating training performance using metrics like ELO scores, running times, CPU/GPU usage, and memory consumption.

## Prerequisites
- Unity 2021.3 or later.
- Python 3.8+ installed with necessary dependencies.
- Unity ML-Agents Toolkit installed ([Installation Guide](https://github.com/Unity-Technologies/ml-agents)).
- TensorBoard installed (`pip install tensorboard`).
- Unity Profile Analyzer package installed (via Unity Package Manager).

## System Requirements
- **Operating System**: Windows, macOS, or Linux.
- **Development Tools**: Unity Editor, Visual Studio (or equivalent).
- **Dependencies**: TensorFlow or PyTorch, ML-Agents Toolkit.

---

## Reproducing Experiments
This project includes five experiments:
1. **Default POCA**  
2. **POCA with Enhanced Memory**  
3. **POCA with Reduced Network Size**  
4. **POCA with Increased Learning Rate**  
5. **Increased Concurrent Environments**

Each experiment has its corresponding configuration file stored in the directory `config/poca/`, named appropriately (e.g., `SoccerTwosDefault.yaml` for Default POCA). Follow the steps below to run the experiments.

### Running Experiments on macOS
1. **Open two terminals**:
   - **Terminal 1**: Start training with the following command:  
     ```bash
     mlagents-learn config/poca/SoccerTwosDefault.yaml --run-id=poca_default
     ```
     Replace `SoccerTwosDefault.yaml` with the appropriate file for other experiments.
   - Open Unity and press **"Play"** in the Unity Editor to start training.

2. **Profiler Setup in Unity**:
   - Go to **Window → Analysis → Profiler** in the Unity Editor.
   - Press the round recording button (next to "Play Mode") to begin collecting performance data for CPU, GPU, and memory.

3. **TensorBoard Setup**:
   - **Terminal 2**: Start TensorBoard for visualizing training results:
     ```bash
     tensorboard --logdir="<training-result-directory>" --port=6006
     ```
     Replace `<training-result-directory>` with the directory storing your training results. Open your browser and visit [http://localhost:6006](http://localhost:6006).

4. **Profile Analyzer**:
   - After training, save performance data from Unity Profiler:
     - Open **Window → Analysis → Profile Analyzer**.
     - Click **Pull Data**, then save the files `marker_table.csv` and `frame_table.csv`.
   - Run the `DataReader.py` Python script to analyze performance:
     ```bash
     python DataReader.py
     ```

5. **Switching Experiments**:
   - To run another experiment, replace the `.yaml` file in the `mlagents-learn` command and increment the TensorBoard port (e.g., use `6007` for the next run).

---

### Running Experiments on Windows
The steps are similar to macOS, with minor adjustments:
1. Open **Command Prompt** instead of Terminal.
2. Use the same `mlagents-learn` and TensorBoard commands.
3. For Unity, follow the same steps to set up the Profiler and Profile Analyzer.

---

## Configuration Files
The configuration files for each experiment are stored under the `config/poca/` folder. The configurations are:
- **Default POCA**: `SoccerTwosDefault.yaml`
- **POCA with Enhanced Memory**: `SoccerTwosEnhancedMemory.yaml`
- **POCA with Reduced Network Size**: `SoccerTwosReducedNetwork.yaml`
- **POCA with Increased Learning Rate**: `SoccerTwosIncreasedLR.yaml`
- **Increased Concurrent Environments**: `SoccerTwosConcurrentEnv.yaml`

Each configuration file specifies all necessary parameters for the experiment. Parameters like learning rate, memory size, or network layers are adjusted in these files, ensuring reproducibility without code changes.

---

## Notes for Reproducibility
- **Avoid manual code changes**: All parameter adjustments are encapsulated in configuration files.
- **Logging results**: Training results (ELO scores, running times) are logged in TensorBoard.
- **Profiler Analysis**: Performance metrics (CPU, GPU, memory) are saved using the Unity Profiler and analyzed via the Profile Analyzer tool.

By following the instructions above, anyone can reproduce the experiments documented in this repository.
