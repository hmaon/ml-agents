using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball3DAgent : Agent
{
    [Header("Specific to Ball3D")]
    public GameObject ball;

    public Vector3 savedLocalPos = Vector3.zero;
    public float collisions = 0;
    public float lastCollisionTime = 0;

    public override List<float> CollectState()
    {
        List<float> state = new List<float>();
        state.Add(gameObject.transform.rotation.z);
        state.Add(gameObject.transform.rotation.x);
        RaycastHit info;
        bool hit = Physics.Raycast(ball.transform.position, Vector3.down, out info);
        state.Add((ball.transform.position.x - gameObject.transform.position.x));
        //state.Add((ball.transform.position.y - gameObject.transform.position.y));
        if (hit)
        {
            state.Add(info.distance);
        }
        else
        {
            state.Add(-9999f);
        }
        state.Add((ball.transform.position.z - gameObject.transform.position.z));
        state.Add(ball.transform.GetComponent<Rigidbody>().velocity.x);
        state.Add(ball.transform.GetComponent<Rigidbody>().velocity.y);
        state.Add(ball.transform.GetComponent<Rigidbody>().velocity.z);
        state.Add(ball.transform.rotation.x);
        state.Add(ball.transform.rotation.y);
        state.Add(ball.transform.rotation.z);
        state.Add(ball.transform.rotation.w);
        state.Add(gameObject.transform.rotation.y);
        state.Add(gameObject.transform.rotation.w);
        state.Add(Vector3.Distance(gameObject.transform.localPosition, savedLocalPos));
        state.Add(ball.transform.GetComponent<Rigidbody>().angularVelocity.x);
        //state.Add(ball.transform.GetComponent<Rigidbody>().angularVelocity.y); // irrelevant rotation about long axis?
        state.Add(ball.transform.GetComponent<Rigidbody>().angularVelocity.z);

        state.Add(Time.time - lastCollisionTime);

        return state;
    }

    // to be implemented by the developer
    public override void AgentStep(float[] act)
    {
        if (brain.brainParameters.actionSpaceType == StateType.continuous)
        {
            float action_z = act[0];
            action_z = Mathf.Clamp(action_z, -7, 7); // why not clamp? -gv
            //if ((gameObject.transform.rotation.z < 0.55f && action_z > 0f) || 
            //    (gameObject.transform.rotation.z > -0.55f && action_z < 0f))
            {
                // let it spin!!
                gameObject.transform.Rotate(Vector3.forward, action_z, Space.World);
            }
            float action_x = act[1];
            action_x = Mathf.Clamp(action_x, -7, 7);
            //if ((gameObject.transform.rotation.x < 0.55f && action_x > 0f) ||
            //    (gameObject.transform.rotation.x > -0.55f && action_x < 0f))
            {
                gameObject.transform.Rotate(Vector3.right, action_x, Space.World);
            }

            float action_y = act[2];
            action_x = Mathf.Clamp(action_y, -7, 7);
            //if ((gameObject.transform.rotation.x < 0.55f && action_x > 0f) ||
            //    (gameObject.transform.rotation.x > -0.55f && action_x < 0f))
            {
                gameObject.transform.Rotate(Vector3.up, action_y, Space.Self);
            }

            Vector3 move = new Vector3(act[3], act[4], act[5]);
            move = Vector3.ClampMagnitude(move, 0.2f);

            Vector3 newPos = gameObject.transform.localPosition + move;
            Vector3 offset = newPos - savedLocalPos;
            offset = Vector3.ClampMagnitude(offset, 1.5f);
            gameObject.transform.localPosition = savedLocalPos + offset;

            if (done == false)
            {
                if (collisions > 0)
                {
                    reward = 0.1f * (float)System.Math.Tanh(0.1f * (-2f + (ball.transform.position.y - gameObject.transform.position.y))); // Modified to get ball as high as possible at any cost -gv
                    reward = Mathf.Clamp(reward, 0.001f, 0.1f);
                }

                //reward = 0.01f;
                //if (ball.GetComponent<Rigidbody>().velocity.y > 0)
                //{
                //    reward += 0.001f;
                //}

                if (collisions > 0)
                {
                    reward += .5f * Mathf.Clamp01(Mathf.Abs(Vector3.Dot(Vector3.up, ball.transform.up)) - 0.80f); // reward for pointing up?
                }

                if (collisions > 0)
                {
                    // reward hang time??
                    reward += Mathf.Clamp01(Time.time - lastCollisionTime) * 0.1f;
                }
                //var a = gameObject.transform.position;
                //var b = ball.transform.position;
                //a.y = 0;
                //b.y = 0;
                //if ((a - b).sqrMagnitude < 0.25f) reward += .01f; // keep in the center?
                if (collisions == 0)
                {
                    reward = 0;
                }
            }
        }
        else
        {
            int action = (int)act[0];
            if (action == 0 || action == 1)
            {
                action = (action * 2) - 1;
                float changeValue = action * 2f;
                if ((gameObject.transform.rotation.z < 0.25f && changeValue > 0f) ||
                    (gameObject.transform.rotation.z > -0.25f && changeValue < 0f))
                {
                    gameObject.transform.Rotate(new Vector3(0, 0, 1), changeValue);
                }
            }
            if (action == 2 || action == 3)
            {
                action = ((action - 2) * 2) - 1;
                float changeValue = action * 2f;
                if ((gameObject.transform.rotation.x < 0.25f && changeValue > 0f) ||
                    (gameObject.transform.rotation.x > -0.25f && changeValue < 0f))
                {
                    gameObject.transform.Rotate(new Vector3(1, 0, 0), changeValue);
                }
            }
            if (done == false)
            {
                reward = 0.1f;
            }
        }
        if ((ball.transform.position.y - gameObject.transform.position.y) < -2f ||
            Mathf.Abs(ball.transform.position.x - gameObject.transform.position.x) > 3f ||
            Mathf.Abs(ball.transform.position.z - gameObject.transform.position.z) > 3f)
        {
            done = true;
            reward = -1f;
        }

    }

    // to be implemented by the developer
    public override void AgentReset()
    {
        gameObject.transform.rotation = Quaternion.identity;
        gameObject.transform.Rotate(new Vector3(1, 0, 0), Random.Range(-10f, 10f));
        gameObject.transform.Rotate(new Vector3(0, 0, 1), Random.Range(-10f, 10f));
        ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
        ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        ball.transform.position = new Vector3(Random.Range(-1.5f, 1.5f), 4f, Random.Range(-1.5f, 1.5f)) + gameObject.transform.position;
        ball.transform.rotation = Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
        gameObject.transform.localPosition = savedLocalPos + Random.insideUnitSphere;
        collisions = 0;

        GetComponent<Renderer>().material.color = new Color(Random.value / 2, Random.value / 2, Random.value / 2, 1);
        lastCollisionTime = -9999;
    }

    public override void InitializeAgent()
    {
        base.InitializeAgent();

        savedLocalPos = gameObject.transform.localPosition;
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject == ball)
        {
            collisions++;
            lastCollisionTime = Time.time;
        }
    }
}
