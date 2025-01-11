import yaml
import os
import subprocess
from datetime import datetime

"""
The script will ask the user for the learning rate and model type, generate a run ID, create a configuration file, and start training.
Keep in mind that this script assumes that you have the ml-agents package installed. If you want to upload the results to the Hugging Face hub, you will also need to add your HF api key.
"""

def create_config_and_run():
    # Ask the user for the learning rate
    learning_rate = input("Enter the learning rate (e.g., 0.001, 0.01, 0.1): ")

    try:
        # Convert the learning rate to a float
        learning_rate = float(learning_rate)
    except ValueError:
        print("Invalid input. Please enter a valid numeric learning rate.")
        return

    # Ask the user for model type
    try:
        model_type_prompt = (
            "1. Forward and backward raycast\n"
            "2. Sound and view\n"
            "3. Only forward raycast\n"
            "Enter the model type (e.g., 1): \n"
        )
        model_type = int(input(model_type_prompt))
        if model_type not in [1, 2, 3]:
            raise ValueError
    except ValueError:
        print("Invalid input. Please enter a number between 1 and 3.")
        return

    # Ask the user if they're going to name the run ID (optional)
    run_id_name = input("Do you want to name the run ID? (Y/N): ").strip().lower()
    if run_id_name == 'y':
        run_id = input("Enter the run ID: ")
    else:
        # Generate run ID using date, time, and learning rate
        run_id = f"{datetime.now().strftime('%Y-%m-%d_%H-%M')}_lr={learning_rate}-model_type={model_type}"

    print(f"Run ID: {run_id}")

    # Load the template configuration file
    template_path = "training_scripts/template_config.yaml"  # Path to the template configuration file
    with open(template_path, 'r') as file:
        config = yaml.safe_load(file)

    # Update the learning rate in the configuration
    config['behaviors']['SoccerTwos']['hyperparameters']['learning_rate'] = learning_rate

    # Ensure the results directory exists
    os.makedirs("./results", exist_ok=True)

    # Ensure the training scripts/''run_id'' directory exists
    os.makedirs(f"./training_scripts/{run_id}", exist_ok=True)

    # Generate the configuration file name
    config_filename = f"./training_scripts/{run_id}/config.yaml"

    # Save the updated configuration to a new file
    with open(config_filename, 'w') as file:
        yaml.safe_dump(config, file)

    print(f"Configuration file '{config_filename}' created successfully.")

    # Check if the run already exists
    results_dir = f"./results/{run_id}"
    if os.path.exists(results_dir):
        # If it exists, add the --resume flag
        train_command = f"mlagents-learn {config_filename} --run-id={run_id} --resume"
    else:
        # If it doesn't exist, use the --run-id flag
        train_command = f"mlagents-learn {config_filename} --run-id={run_id}"

    # Execute the training command
    print("\nStarting training...")
    subprocess.run(train_command, shell=True)

    # Ask the user if they want to push to Hugging Face hub
    push_to_hf = input("Do you want to push to Hugging Face hub? (Y/N): ").strip().lower()
    if push_to_hf == 'y':
        # Ask the user for the Hugging Face username
        hf_username = input("Enter your Hugging Face username: ").strip()
        # Push to Hugging Face
        push_command = (
            f"mlagents-push-to-hf --run-id=\"{run_id}\" --local-dir=\"{results_dir}\" "
            f"--repo-id=\"{hf_username}/SoccerTwos-{run_id}\" --commit-message=\"First Push\""
        )
        print("\nPushing results to Hugging Face...")
        subprocess.run(push_command, shell=True)
    else:
        print("\nSkipping push to Hugging Face.")

    # Launch TensorBoard
    tensorboard_command = f"tensorboard --logdir results/{run_id}"
    print("\nLaunching TensorBoard...")
    subprocess.run(tensorboard_command, shell=True)

# Run the function
create_config_and_run()
