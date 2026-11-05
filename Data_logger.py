import csv
import math
import time
from functools import wraps
from pathlib import Path
from threading import RLock


FIELDS = (
    "epoch", "mean_reward", "mean_group_rewards", "ppo_beta", "ppo_epsilon", "ppo_lambda", "poca_strength", "poca_gamma", "simulation_runtime",
)
REWARD_KEYS = ("Environment/Cumulative Reward", "Environment/Group Cumulative Reward")


class CsvRecorder:
    def __init__(self, path, hyperparameters, reward_settings, clock=time.perf_counter):
        self.path = Path(path)
        if self.path.exists():
            raise FileExistsError(f"CSV already exists: {self.path}. Use a new run ID.")
        self.hyperparameters = hyperparameters
        self.reward_settings = reward_settings
        self.clock = clock
        self.started = clock()
        self.epochs = 0
        self.totals = [0.0, 0.0]
        self.counts = [0, 0]
        self.lock = RLock()
        self.file = None
        self.writer = None

    def observe(self, category, key, value):
        if category != "DungeonEscape" or key not in REWARD_KEYS:
            return
        value = float(value)
        if not math.isfinite(value):
            raise ValueError(f"Non-finite reward: {key}={value}")
        with self.lock:
            index = REWARD_KEYS.index(key)
            self.totals[index] += value
            self.counts[index] += 1

    def completed_update(self):
        with self.lock:
            if self.file is None:
                self.path.parent.mkdir(parents=True, exist_ok=True)
                self.file = self.path.open("x", newline="", encoding="utf-8")
                self.writer = csv.writer(self.file)
                self.writer.writerow(FIELDS)
                print(f"\nRecording CSV: {self.path.resolve()}")
            self.epochs += self.hyperparameters.num_epoch
            means = [total / count if count else ""
                     for total, count in zip(self.totals, self.counts)]
            self.writer.writerow((
                self.epochs, means[0], means[1], self.hyperparameters.beta,
                round(self.clock() - self.started, 6),
                self.hyperparameters.epsilon, self.hyperparameters.lambd,
                self.reward_settings.strength, self.reward_settings.gamma,
            ))
            self.file.flush()
            self.totals = [0.0, 0.0]
            self.counts = [0, 0]

    def close(self):
        with self.lock:
            if self.file is not None:
                self.file.close()


def install_update_hook(trainer_class, recorder):
    original = trainer_class._update_policy

    @wraps(original)
    def update_and_record(trainer):
        completed = original(trainer)
        if completed and trainer.stats_reporter.category == "DungeonEscape":
            recorder.completed_update()
        return completed

    trainer_class._update_policy = update_and_record
    return original


def main():
    from mlagents.trainers.learn import parse_command_line, run_cli
    from mlagents.trainers.poca.trainer import POCATrainer
    from mlagents.trainers.settings import RewardSignalType
    from mlagents.trainers.stats import StatsReporter, StatsWriter

    options = parse_command_line()
    checkpoint = options.checkpoint_settings
    if checkpoint.resume or checkpoint.inference:
        raise SystemExit("Use a fresh training run: --resume and --inference are unsupported.")
    settings = options.behaviors.get("DungeonEscape")
    if settings is None or settings.trainer_type != "poca":
        raise SystemExit("The configuration must contain DungeonEscape with trainer_type: poca.")
    hp = settings.hyperparameters
    if hp.num_epoch < 1 or hp.buffer_size < hp.batch_size:
        raise SystemExit("Require num_epoch >= 1 and buffer_size >= batch_size.")
    reward = settings.reward_signals.get(RewardSignalType.EXTRINSIC)
    if reward is None:
        raise SystemExit("DungeonEscape requires an extrinsic reward signal.")
    recorder = CsvRecorder(
        Path(checkpoint.write_path) / "DungeonEscape" / "training_data.csv", hp, reward
    )

    class RewardCollector(StatsWriter):
        def on_add_stat(self, category, key, value, aggregation=None):
            recorder.observe(category, key, value)

        def write_stats(self, category, values, step):
            # Native summary resets must not reset update-based window.
            pass

    collector = RewardCollector()
    StatsReporter.add_writer(collector)
    original = install_update_hook(POCATrainer, recorder)
    try:
        run_cli(options)
    finally:
        POCATrainer._update_policy = original
        if collector in StatsReporter.writers:
            StatsReporter.writers.remove(collector)
        recorder.close()
        if recorder.epochs == 0:
            print("No CSV rows: no complete DungeonEscape update was recorded.")


if __name__ == "__main__":
    main()
