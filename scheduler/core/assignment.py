from collections import defaultdict

def assign_runs(runs, machine_ids):
    if not machine_ids:
        raise ValueError("At least one machine required")
    if len(machine_ids)!=len(set(machine_ids)):
        raise ValueError("Machine IDs must be unique")

    run_ids = [run["run_id"] for run in runs]
    if len(run_ids) != len(set(run_ids)):
        raise ValueError("Run IDs must be unique")
    machines = list(machine_ids)

    workload = {machine: 0 for machine in machines}
    for run in runs:
        machine = run.get("machine_id")
        if machine is not None:
            if machine not in workload:
                raise ValueError(f"Unknown machine assigned to run {run['run_id']}: {machine}")
            workload[machine]+=1

    #group pending runs by configuration
    configs = defaultdict(list)
    for run in runs:
        if run["status"] == "pending":
            configs[run["config_id"]].append(run)

    next_machine = 0
    for config_runs in configs.values():
        for run in config_runs:
            already_used ={
                existing["machine_id"]
                for existing in runs
                if existing["config_id"] == run["config_id"] and existing["machine_id"] is not None
                }
            available = []
            for machine in machines:
                if machine not in already_used:
                    available.append(machine)
            if not available:
                available = machines

            lowest_workload = min(workload[machine] for machine in available)

            candidates = []
            for machine in available:
                if workload[machine] == lowest_workload:
                    candidates.append(machine)

            machine = next(
                machines[index] for offset in range(len(machines))
                for index in [(next_machine+offset) % len(machines)]
                if machines[index] in candidates
            )
            run["machine_id"] = machine
            run["status"] = "assigned"
            workload[machine]+=1
            next_machine = (machines.index(machine) + 1)%len(machines)
    return runs
