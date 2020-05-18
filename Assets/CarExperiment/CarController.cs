using UnityEngine;
using System.Collections;
using SharpNeat.Phenomes;
using Boo.Lang;
using System;

public class CarController : UnitController {

    public float Speed = 5f;
    public float TurnSpeed = 180f;
    public int Lap = 1;
    public int CurrentPiece, LastPiece;
    bool MovingForward = true;
    bool IsRunning;
    public float SensorRange = 10;
    int WallHits; 
    IBlackBox box;

    private float[] sensorHeadings = new float[5];
    //Maximum sensor rotation(+ or -), in degrees, from start orientation
    public float maxSensorRotation = 22.5f;
    public float sensorTurnSpeed = 180f;

    // Use this for initialization
    void Start () {
	    
	}
	
	// Update is called once per frame
    void FixedUpdate()
    {
        //grab the input axes
        //var steer = Input.GetAxis("Horizontal");
        //var gas = Input.GetAxis("Vertical");

        ////if they're hittin' the gas...
        //if (gas != 0)
        //{
        //    //take the throttle level (with keyboard, generally +1 if up, -1 if down)
        //    //  and multiply by speed and the timestep to get the distance moved this frame
        //    var moveDist = gas * speed * Time.deltaTime;

        //    //now the turn amount, similar drill, just turnSpeed instead of speed
        //    //   we multiply in gas as well, which properly reverses the steering when going 
        //    //   backwards, and scales the turn amount with the speed
        //    var turnAngle = steer * turnSpeed * Time.deltaTime * gas;

        //    //now apply 'em, starting with the turn           
        //    transform.Rotate(0, turnAngle, 0);

        //    //and now move forward by moveVect
        //    transform.Translate(Vector3.forward * moveDist);
        //}

        // Five sensors: Front, left front, left, right front, right 

        if (IsRunning)
        {
            float[] sensors = new float[5];

            // Get new sensor direction
            Vector3 sensorsLocation = transform.position + transform.forward * 0.5f;
            Vector3 frontSensorDir = transform.TransformDirection(new Vector3(0, 0, 1).normalized);
            Vector3 rightFrontSensorDir = transform.TransformDirection(new Vector3(0.5f, 0, 1).normalized);
            Vector3 rightSensorDir = transform.TransformDirection(new Vector3(1, 0, 0).normalized);
            Vector3 leftFrontSensorDir = transform.TransformDirection(new Vector3(-0.5f, 0, 1).normalized);
            Vector3 leftSensorDir = transform.TransformDirection(new Vector3(-1, 0, 0).normalized);
            Vector3[] sensorDirs = new[] { frontSensorDir, rightFrontSensorDir, rightSensorDir, leftFrontSensorDir, leftSensorDir };
            for (int i = 0; i < sensorDirs.Length; ++i)
                sensorDirs[i] = Quaternion.AngleAxis(sensorHeadings[i], transform.up) * sensorDirs[i];

            // Sensor raycasts
            int layerMask = ~(1 << gameObject.layer);
            ISignalArray inputArr = box.InputSignalArray;
            for (int i = 0; i < sensors.Length; ++i)
            {
                RaycastHit hit;
                if (Physics.Raycast(sensorsLocation, sensorDirs[i], out hit, SensorRange, layerMask))
                {
                    sensors[i] = 1 - hit.distance / SensorRange;
                    Color color = Color.green;
                    Debug.DrawLine(sensorsLocation, hit.point, color);
                }
                else
                {
                    Color color = Color.red;
                    Debug.DrawRay(sensorsLocation, sensorDirs[i] * SensorRange, color);
                }

                inputArr[i] = sensors[i];
                inputArr[sensors.Length + i] = sensorHeadings[i] / maxSensorRotation;
            }

            // Activate neural network
            box.Activate();
            ISignalArray outputArr = box.OutputSignalArray;

            // Gas and Steering from brain
            var steer = (float)outputArr[0] * 2 - 1;
            var gas = (float)outputArr[1] * 2 - 1;

            var moveDist = gas * Speed * Time.fixedDeltaTime;
            var turnAngle = steer * TurnSpeed * Time.fixedDeltaTime * gas;

            transform.Rotate(new Vector3(0, turnAngle, 0));
            transform.Translate(Vector3.forward * moveDist);

            // Sensor orientation steering from brain
            for (int i = 0; i < sensorHeadings.Length; ++i)
            {
                float sensorSteer = (float)outputArr[i + 2] * 2 - 1;
                sensorHeadings[i] += sensorSteer * sensorTurnSpeed * Time.fixedDeltaTime;
                sensorHeadings[i] = Mathf.Clamp(sensorHeadings[i], -maxSensorRotation, maxSensorRotation);
            }
        }
    }

    public override void Stop()
    {
        this.IsRunning = false;
    }

    public override void Activate(IBlackBox box)
    {
        this.box = box;
        this.IsRunning = true;

        // Zero out sensor orientations
        for (int i = 0; i < sensorHeadings.Length; ++i)
            sensorHeadings[i] = 0;
    }

    public void NewLap()
    {        
        if (LastPiece > 2 && MovingForward)
        {
            Lap++;            
        }
    }

    public override float GetFitness()
    {
        if (Lap == 1 && CurrentPiece == 0)
        {
            return 0;
        }
        int piece = CurrentPiece;
        if (CurrentPiece == 0)
        {
            piece = 17;
        }
        float fit = 100 + Lap * piece - WallHits * 0.5f;
      //  print(string.Format("Piece: {0}, Lap: {1}, Fitness: {2}", piece, Lap, fit));
        if (fit > 0)
        {
            return fit;
        }
        return 0;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag.Equals("Road"))
        {
            RoadPiece rp = collision.collider.GetComponent<RoadPiece>();
          //  print(collision.collider.tag + " " + rp.PieceNumber);
            
            if ((rp.PieceNumber != LastPiece) && (rp.PieceNumber == CurrentPiece + 1 || (MovingForward && rp.PieceNumber == 0)))
            {
                LastPiece = CurrentPiece;
                CurrentPiece = rp.PieceNumber;
                MovingForward = true;                
            }
            else
            {
                MovingForward = false;
            }
            if (rp.PieceNumber == 0)
            {
                CurrentPiece = 0;
            }
        }
        else if (collision.collider.tag.Equals("Wall"))
        {
            WallHits++;
        }
        else if (collision.collider.tag.Equals("Car"))
        {
            WallHits++;
        }
    }



    //void OnGUI()
    //{
    //    GUI.Button(new Rect(10, 200, 100, 100), "Forward: " + MovingForward + "\nPiece: " + CurrentPiece + "\nLast: " + LastPiece + "\nLap: " + Lap);
    //}
    
}
