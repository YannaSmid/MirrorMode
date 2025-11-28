using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class DiscreteMoves : Agent
{

    [SerializeField] private Transform env;
    [SerializeField] private Transform target;
    [SerializeField] private SpriteRenderer backgroundSpriteRenderer;

    private bool isButtonPushed;
    private bool interact;
    private int actionsReceived;



    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3(Random.Range(-3.5f, -1.5f), Random.Range(-3.5f, 3.5f));
        target.localPosition = new Vector3(Random.Range(-3.5f, -1.5f), Random.Range(-3.5f, 3.5f));
        env.localRotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
        transform.rotation = Quaternion.identity;
        target.rotation = Quaternion.identity;

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation((Vector2)(transform.position - env.position));
       
    }

    public override void OnActionReceived(ActionBuffers actions)
    {

        float moveX = actions.DiscreteActions[0] - 1;
        float moveY = actions.DiscreteActions[1] - 1;

        float movementSpeed = 1f;

        transform.position += new Vector3(moveX, moveY) * Time.deltaTime * movementSpeed;

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;

        discreteActions[0] = Mathf.RoundToInt(Input.GetAxisRaw("Horizontal") + 1);
        discreteActions[1] = Mathf.RoundToInt(Input.GetAxisRaw("Vertical") + 1);

    }

    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name == "Target")
        {
            AddReward(10f);
            backgroundSpriteRenderer.color = Color.green;
            EndEpisode();
        }
        else if (collision.transform.parent.name == "Walls")
        {
            AddReward(-2f);
            backgroundSpriteRenderer.color = Color.red;
            EndEpisode();   
        }
    }


}