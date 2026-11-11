import sys
from pathlib import Path

# this is platfrom specific, might want to not have it as a set value
PATH_TO_YAMLS = Path(__file__).resolve().parent / "yamls"
PATH_TO_YAMLS.mkdir(parents=True, exist_ok=True)
if(len(sys.argv)< 3):
    print("Usage: yaml_generator --[hyperparameter] [value]")
    sys.exit(-1)

hyperparameter = sys.argv[1]
#check whether second arg is numerical
value = sys.argv[2]

try:
    value = float(value)
except ValueError:
    print("Value must be a number")
    sys.exit(-1)

valid_hyperparameters = ["--beta", "--epsilon", "--lambda", "--strength", "--gamma"]
if hyperparameter not in valid_hyperparameters:
    print(f"hyperparameter shoud be in {valid_hyperparameters}")
    sys.exit(-1)


changed_param = ""

beta = 0.01
epsilon = 0.2
lambd = 0.95
strenght = 1.0
gamma = 0.99

match hyperparameter:
    case "--beta":
        beta = value
        changed_param = "beta"
    case "--epsilon":
        epsilon = value
        changed_param = "epsilon"
    case "--lambda":
        lambd = value
        changed_param = "lambd"
    case "--strength":
        strenght = value
        changed_param = "strength"
    case "--gamma":
        gamma = value
        changed_param = "gamma"


mod_file = f"""behaviors:
  DungeonEscape:
    trainer_type: poca
    hyperparameters:
      batch_size: 1024
      buffer_size: 10240
      learning_rate: 0.0003
      beta: {beta}
      epsilon: {epsilon}
      lambd: {lambd}
      num_epoch: 3
      learning_rate_schedule: constant
    network_settings:
      normalize: false
      hidden_units: 256
      num_layers: 2
      vis_encode_type: simple
    reward_signals:
      extrinsic:
        gamma: {gamma}
        strength: {strenght}
    keep_checkpoints: 5
    max_steps: 20000000
    time_horizon: 64
    summary_freq: 60000
"""

filename= PATH_TO_YAMLS / f"DungeonEscape_{changed_param}_{value}.yaml"

with open(filename, "w") as f:
    f.write(mod_file)
    print(f"created file at: {filename}")
