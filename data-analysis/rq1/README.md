# RQ1 model
## Command-line usage
To run the script simply type this command in the terminal:
>python train_time_predictor.py python train_time_predictor.py <path_to_config_file>

## Config file structure
The config file is a simple yaml file with following structure:
```yaml
rq1:
  data_file: "../combined_all_data.csv"
  output_plot: "time_prediction_results.png"
  random_state: 42

  preprocessing:
    min_time: 20
    max_time: 15000

  features:
    hardware:
      - physical_cores
      - system_ram_gb
    hyperparameters:
      - batch_size
      - learning_rate
      - max_steps
      - buffer_size
      - hidden_units
      - num_layers

  random_forest:
    n_estimators: 200
    max_depth: null
    min_samples_leaf: 1

  gradient_boosting:
    n_estimators: 300
    max_depth: 6
    learning_rate: 0.05

  per_game:
    min_samples: 300
```

