# Unity ML-Agents Toolkit - Multi-Modal AI Agents in Unity - Team AIML 6

**[Latest Release](https://github.com/Unity-Technologies/ml-agents/releases/latest)** | **[All Releases](https://github.com/Unity-Technologies/ml-agents/releases)**

## Overview

This project builds on the **Unity ML-Agents Toolkit** originally developed by Unity Technologies. Our focus is on incorporating **multi-modal sensory inputs**, specifically by adding **auditory sensors** to Unity's **Soccer Twos** environment. The primary goal is to create AI agents that can make decisions using both visual and auditory information, resulting in more realistic and adaptive behaviour, especially when objects like the soccer ball are not within the agent's line of sight.

We use advanced **Reinforcement Learning (RL)** algorithms like **Proximal Policy Optimization (PPO)** and **Multi-Agent Deep Deterministic Policy Gradient (MADDPG)**, which are already available within the ML-Agent toolkit, to train agents in both single-agent and multi-agent setups. These agents are evaluated on tasks such as scoring goals, defending, and coordinating in both competitive and cooperative settings.

Beyond Soccer Twos, we also evaluate how well these trained agents generalize to other Unity environments, including **Push Block**, **GridWorld**, and **Crawler**.

## Project Goals

This project aims to demonstrate how combining **visual and auditory inputs** can improve AI agent decision-making. Specifically, in Soccer Twos, auditory sensors enable agents to track the ball when it's out of sight, leading to smarter responses.

Our experiments include:

- **Comparing single-agent and multi-agent setups**: We look at cases where one agent controls both players versus cases where each player is controlled by a different agent.
  
- **Evaluating different RL algorithms**: We compare how well PPO and MADDPG handle complex multi-agent interactions.
  
- **Optimizing multi-sensory integration**: We explore how auditory and visual cues can be combined to improve overall agent performance.

We benchmark these multi-modal agents against traditional visual-only agents to highlight the advantages of integrating different sensory inputs.

## Resources and Links

For more information on the original ML-Agents Toolkit, the resources and original documentation is listed below:

- [ML-Agents Overview](https://github.com/Unity-Technologies/ml-agents/blob/main/docs/ML-Agents-Overview.md)
- [Unity Environment Control from Python](https://github.com/Unity-Technologies/ml-agents/blob/main/docs/Python-LLAPI.md)
- [Reinforcement Learning Algorithms](https://github.com/Unity-Technologies/ml-agents/blob/main/docs/Training-ML-Agents.md)
- [Latest Documentation](https://unity-technologies.github.io/ml-agents/)
- [Installation and Setup Instructions](https://github.com/Unity-Technologies/ml-agents/blob/main/docs/Installation.md)
