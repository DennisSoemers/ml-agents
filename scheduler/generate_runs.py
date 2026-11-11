import json
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SCHEDULER_DIR = Path(__file__).resolve().parent
CONFIG_FILE = SCHEDULER_DIR / "experiment_config.json"
YAML_GENERATOR = ROOT / "yaml_generator.py"
OUTPUT_FILE = SCHEDULER_DIR / "runs.json"

FLAGS = {
    "beta": "--beta",
    "epsilon": "--epsilon",
    "lambd": "--lambda",
    "gamma": "--gamma"
}

def generate_yaml(parameter, value):
    subprocess.run(
        [sys.executable, str(YAML_GENERATOR), FLAGS[parameter], str(value)], cwd=ROOT, check=True
    )
    filename = f"DungeonEscape_{parameter}_{float(value)}.yaml"
    return str(Path("yamls")/filename)


def create_runs():
    with open(CONFIG_FILE, "r", encoding="utf-8") as file:
        config = json.load(file)
    baseline = config["baseline"]
    hyperparameters = config["hyperparameters"]
    repetitions = config["repetitions"]

    runs = []

    baseline_yaml = generate_yaml("beta", baseline["beta"])

    #create baseline run repetitons
    for repetition in range(1, repetitions +1):
        runs.append({
            "run_id": f"baseline_rep{repetition:02d}",
            "config_id": "baseline",
            "hyperparameter": None,
            "value": None,
            "parameters": baseline.copy(),
            "repetition": repetition,
            "yaml_file": baseline_yaml,
            "status": "pending",
            "machine_id": None
            })

    #generate runs for other configurtions
    for parameter, values in hyperparameters.items():
        for value in values:
            if float(value) == float(baseline[parameter]):
                continue
            parameters = baseline.copy()
            parameters[parameter] = value

            yaml_file = generate_yaml(parameter, value)
            config_id = (f"{parameter}_{str(float(value)).replace('.', 'p')}")

            for repetition in range(1, repetitions+1):
                runs.append({
                    "run_id": f"{config_id}_rep{repetition:02d}",
                    "config_id": config_id,
                    "hyperparameter": parameter,
                    "value": value,
                    "parameters": parameters.copy(),
                    "repetition": repetition,
                    "yaml_file": yaml_file,
                    "status": "pending",
                    "machine_id": None
                    })

    with open(OUTPUT_FILE, "w", encoding="utf-8") as file:
        json.dump(runs, file, indent=4)
    print(f"\nGenerated {len(runs)} runs")
    print(f"Saved run list to {OUTPUT_FILE}")


if __name__ == "__main__":
    create_runs()
