# Mirror Mode: Your Biggest Enemy is Yourself

## Master's Thesis by Yanna E. Smid



This is a research project to investigate the possibilities of Imitation Learning and Reinforcement Learning techniques in imitating player's strategies. Unity's ML-Agent package is applied for this, and offers the required algorithms in Python. The ML-Agent toolkit v3.0.0 package is installed in the Unity editor and is developed by A. Juliani et al. \[15]. 

Two game modes are implemented: Standard Mode and Mirror Mode. Both are based on Nintendo's mobile strategy game Fire Emblem Heroes.



### Installation steps

In order to use the created game scenes, a series of steps are required. For more detailed installation steps, check out the official [Unity ML-Agents toolkit github](https://github.com/Unity-Technologies/ml-agents/blob/release_22_docs/docs/Installation.md).



1. Install Unity project editor version 2023.2.13f1.
2. Create a virtual environment in the Unity project directory. This can either be done in via Conda, or the local Command Prompt on Windows. I used Command Prompt in Windows. For Command Prompt the following steps can be followed to create a virtual environment:

&nbsp;	1. Navigate to the Unity project directory in Command Prompt.

&nbsp;	2. Create a venv folder by running the following command python -m venv venv.

&nbsp;	3. Activate the environment with: venv \\ scripts \\ activate.

4\. Ensure that Python version 3.10.11 is installed and active within the virtual environment.

5\. Upgrade pip through python -m pip install –upgrade pip.

6\. Install the ML-Agents toolkit with pip install mlagents.

7\. Install PyTorch version 2.2.1 using pip3 install torch==2.2.1.



The mlagents-learn commands can now be used effectively to communicate with the Unity environment. To check if the ML-Agents toolkit is installed

correctly, the following command can be ran: mlagents-learn –help.



### Required Application Versions

For compatibility between Unity’s ML-Agent package and PyTorch tools, it is necessary to acquire the specific application versions

* Unity Editor 2023.2.13f1
* ML-Agent toolit package 3.0.0 (release 22)
* Python 3.10.11
* PyTorch 2.2.1
* Pip 25.0.1
* Windows 11



## How to play

Two main game scenes are created: one for Standard Mode, and one for Mirror Mode. Both can be played regardless of collecting data to train agents. For the official training mechanisms, and model usage follow the next descriptions.



#### Standard Mode

Standard Mode is used to collect player demonstrations in order to train agents through Imitation Learning.

In the game scene, make sure to follow these steps:

1. Check enable augmentation in Augmentation Manager
2. Check record in Demonstration Recorder in the Demonstration Recorder Manager
3. Set behavior type to *heuristic* *only* in Behavior Parameters in Demo Recorder Manager
4. Uncheck start training in Demo Recorder Dummy in the Demonstration Recorder Manager



In Demonstration Recorder in the Demonstration Recorder Manager you can set a directory to save the Demonstration files.



Standard Mode is also used to train agents, based on player demonstrations.

To start training, follow these steps:

1. Set directory in MirrorAgent.yaml in config folder, to the directory of your demonstrations.
2. In Unity uncheck enable augmentation in Augmentation Manager
3. Set behavior type to *default* in Behavior Parameters in Demo Recorder Manager
4. uncheck record in Demonstration Recorder
5. Check start training in Demo Recorder Manager
6. Open the project directory in your terminal and activate your created virtual environment linked to the unity project. 
7. Run the following comman: mlagents-learn config/MirrorAgent.yaml –run-id=ID-Name-Run
8. Play the Unity Standard Mode Scene





#### Mirror Mode

In Mirror Mode, you can compete against agents that follow the trained model. 

1. Find your trained model in the results folder of your run-ID
2. Copy the model to the model folder in Unity Asset folder
3. Insert this model in the Model parameter in Behavior Parameters of each enemy object



### Acknowledgement

This project would not have been possible without the help of [Code Monkey](https://www.youtube.com/@CodeMonkeyUnity)'s youtube tutorials.

Furthermore, all UI and character sprites are collected from [Fandom Community Fire Emblem Heroes Wiki](https://feheroes.fandom.com/wiki/Game_assets_collection#UI_Sprite_sheets).



### Contact

If you stumble upon any problems or have any questions feel free to send me an email to y.e.smid@umail.leidenuniv.nl















