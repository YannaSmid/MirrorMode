using System;
using UnityEngine;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Demonstrations;

public class DemoRecorderDummyAgent : Agent 
{
    public DemonstrationRecorder demoRecorder;
    public AugmentationManager augmentationManager;
    public bool startTraining = false;

    void Awake()
    {
        if (demoRecorder.Record && !augmentationManager.enableAugmentation)
        {
            Debug.LogWarning("Recording while augmentation is disabled! Augmentation is enabled automatically now!");
            augmentationManager.enableAugmentation = true;
        }

        if (startTraining && demoRecorder.Record)
        {
            Debug.LogWarning("Recording while training! Recording is disabled now!");
            demoRecorder.Record = false;
        }

        if (startTraining && augmentationManager.enableAugmentation)
        {
            Debug.LogWarning("Augmentation enabled during training! Augmentation is disabled automatically now!");
            augmentationManager.enableAugmentation = false;
        }

    }
    public override void OnActionReceived(ActionBuffers actions) 
    {
        // Do nothing - this is just a placeholder
    }
    
    public override void Heuristic(in ActionBuffers actionsOut) 
    {
        // Do nothing
    }
}
