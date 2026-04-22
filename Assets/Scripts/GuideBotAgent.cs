using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

[RequireComponent(typeof(Rigidbody))]
public class GuideBotAgent : Agent
{
    public GuideBotTargetManager targetManager;
    public Transform startPoint;

    [Header("Mode")]
    public bool trainingMode = true;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float turnSpeed = 40f;
    public float reachDistance = 3f;
    public float obstacleRayDistance = 4f;

    [Header("Episode Limits")]
    public int maxStepsPerEpisode = 4000;

    private Rigidbody rb;
    private float previousDistance;
    private int stepCounter;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        transform.position = startPoint.position;
        transform.rotation = startPoint.rotation;

        stepCounter = 0;

        if (trainingMode)
        {
            
            targetManager.SetRandomTarget();
        }
        else
        {
            
            targetManager.ResetToFirst();
        }

        Transform target = targetManager.GetCurrentTarget();
        previousDistance = target ? Vector3.Distance(transform.position, target.position) : 0f;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Transform target = targetManager.GetCurrentTarget();

        if (target == null)
        {
            sensor.AddObservation(Vector3.zero); // 3
            sensor.AddObservation(0f);           // 1
        }
        else
        {
            Vector3 toTarget = target.position - transform.position;
            Vector3 localDir = transform.InverseTransformDirection(toTarget.normalized);

            sensor.AddObservation(localDir);                  
            sensor.AddObservation(toTarget.magnitude / 100f); 
        }

        sensor.AddObservation(transform.forward); 
        sensor.AddObservation(rb.linearVelocity.x); 
        sensor.AddObservation(rb.linearVelocity.z); 

        sensor.AddObservation(RayValue(transform.forward));                                
        sensor.AddObservation(RayValue(Quaternion.Euler(0, -30, 0) * transform.forward)); 
        sensor.AddObservation(RayValue(Quaternion.Euler(0, 30, 0) * transform.forward));  
    }

    private float RayValue(Vector3 dir)
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, obstacleRayDistance))
        {
            return hit.distance / obstacleRayDistance;
        }

        return 1f;
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        stepCounter++;

        
        float moveInput = Mathf.Clamp01((actions.ContinuousActions[0] + 1f) * 0.5f);
        float turnInput = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);

        transform.Rotate(Vector3.up, turnInput * turnSpeed * Time.deltaTime);
        rb.MovePosition(rb.position + transform.forward * moveInput * moveSpeed * Time.deltaTime);

        Transform target = targetManager.GetCurrentTarget();
        if (target == null) return;

        float currentDistance = Vector3.Distance(transform.position, target.position);

        
        AddReward(-0.001f);

       
        AddReward(-Mathf.Abs(turnInput) * 0.001f);

        
        float progress = previousDistance - currentDistance;
        AddReward(progress * 0.01f);

        
        Vector3 toTarget = (target.position - transform.position).normalized;
        float alignment = Vector3.Dot(transform.forward, toTarget);
        AddReward(alignment * 0.001f);

        previousDistance = currentDistance;

        
        if (currentDistance < reachDistance)
        {
            AddReward(1.0f);

            if (trainingMode)
            {
               
                EndEpisode();
            }
            else
            {
               
                targetManager.AdvanceTarget();

                Transform nextTarget = targetManager.GetCurrentTarget();
                previousDistance = nextTarget
                    ? Vector3.Distance(transform.position, nextTarget.position)
                    : 0f;
            }
        }

        
        if (transform.position.y < -2f)
        {
            AddReward(-1f);
            EndEpisode();
        }

       
        if (trainingMode && stepCounter > maxStepsPerEpisode)
        {
            AddReward(-1f);
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActions = actionsOut.ContinuousActions;

        
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            continuousActions[0] = 1f;
        else
            continuousActions[0] = -1f; 

       
        continuousActions[1] = Input.GetAxis("Horizontal");
    }

    private void OnCollisionEnter(Collision collision)
    {
        AddReward(-0.01f);
    }
}