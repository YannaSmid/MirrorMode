using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class PushButtonAgent : Agent
{


    [SerializeField] private Transform env;
    [SerializeField] private Transform target;
    [SerializeField] private Transform button;
    [SerializeField] private SpriteRenderer backgroundSpriteRenderer;
    [SerializeField] private float movementSpeed = 2f;

    private bool isButtonPushed;
    private bool interact;
    private int actionsReceived;

    private int nrEpisodes = 0;



    public override void OnEpisodeBegin()
    {
        nrEpisodes++;
        Debug.Log(nrEpisodes);
        transform.localPosition = new Vector3(Random.Range(-3.5f, -1.5f), Random.Range(-3.5f, 3.5f));
        target.localPosition = new Vector3(Random.Range(-3.5f, -1.5f), Random.Range(-3.5f, 3.5f));
        button.localPosition = new Vector3(Random.Range(1.5f, 3.5f), Random.Range(-3.5f, 3.5f));

        env.localRotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
        transform.rotation = Quaternion.identity;
        target.rotation = Quaternion.identity;
        button.rotation = Quaternion.identity;

        target.gameObject.SetActive(false);
        isButtonPushed = false;
        actionsReceived = 0;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation((Vector2)(transform.position - env.position));
        sensor.AddObservation((Vector2)(isButtonPushed ? target.position - env.position : button.position - env.position));
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        actionsReceived++;
        
        interact = actions.DiscreteActions[2] == 0 ? false : true;

        float moveX = actions.DiscreteActions[0] - 1;
        float moveY = actions.DiscreteActions[1] - 1;

        movementSpeed = 2f;

        transform.position += new Vector3(moveX, moveY) * Time.deltaTime * movementSpeed;

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;

        discreteActions[0] = Mathf.RoundToInt(Input.GetAxisRaw("Horizontal") + 1);
        discreteActions[1] = Mathf.RoundToInt(Input.GetAxisRaw("Vertical") + 1);

        discreteActions[2] = Input.GetButton("Jump") ? 1 : 0;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out Target target))
        {
            AddReward(15f);
            backgroundSpriteRenderer.color = Color.green;

            int timeRewoard = actionsReceived < 1000 ? (1000 - actionsReceived) / 50 : 0;
            // Debug.Log(timeRewoard);
            AddReward(timeRewoard);

            EndEpisode();
        }
        else if (collision.TryGetComponent(out Wall wall))
        {
            AddReward(-7f);
            backgroundSpriteRenderer.color = Color.red;
            EndEpisode();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (interact && !isButtonPushed && collision.name == "Button")
        {
            // Debug.Log("BUTTON PUSHED!!!");
            AddReward(10f);
            isButtonPushed = true;
            target.gameObject.SetActive(true);
        }
    }


}
