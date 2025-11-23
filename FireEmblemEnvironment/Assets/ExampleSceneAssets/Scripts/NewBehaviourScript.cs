using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class NewBehaviourScript : Agent
{
    
    
    [SerializeField] private Transform target;
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    
    public override void OnEpisodeBegin()
    {
        transform.position = new Vector3(Random.Range(-3.5f, -1.5f), Random.Range(-3.5f, 3.5f));
        target.position = new Vector3(Random.Range(1.5f, 3.5f), Random.Range(-3.5f, 3.5f));
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        Debug.Log((Vector2)transform.localPosition);
        sensor.AddObservation((Vector2)transform.localPosition);
        sensor.AddObservation((Vector2)target.localPosition);
    }


    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveY = actions.ContinuousActions[1];
        float moveSpeed = 5f;
        transform.localPosition += new Vector3(moveX, moveY) * Time.deltaTime * moveSpeed;
    }
    
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out Target targ))
        {
            AddReward(10f);
            spriteRenderer.color = Color.green;
            EndEpisode();
        }
        else if (collision.TryGetComponent(out Wall wall))
        {
            AddReward(-2f);
            spriteRenderer.color = Color.red;
            EndEpisode();
        }
    }
}