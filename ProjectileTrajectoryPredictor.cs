using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using Helpers;
using UnityEngine;

/// <summary>
/// This class has methods for precomputing projectile trajectory with a given start velocity vector or for calculating start projectile velocity to hit given target
/// </summary>
public static class TrajectoryPredictor
{
    public struct Settings
    {
        public readonly bool useAirResistance;
        public readonly bool useMagnusForce;
        public readonly float bouncinessMultiplier;

        public Settings(bool useAir, bool useMagnus, float bounciness)
        {
            useAirResistance = useAir;
            useMagnusForce = useMagnus;
            bouncinessMultiplier = bounciness;
        }

    }

    // Simulation constants 
    private static float _simulationStep = 0.002f;
    private static float _simulationTime = 3f;
    private static float _projectileMass = 1f;
    private static float _projectileRadius = 0.035f;

    /// <summary>
    /// Returns precomputed Trajectory for a Projectile using default settings
    /// </summary>
    /// <param name="projectile">Projectile</param>
    /// <returns></returns>
    public static Trajectory GetProjectileTrajectory(Projectile projectile, bool visualize = false)
    {
        Settings settings = new Settings(true, true, 1f);
        return GetProjectileTrajectory(projectile.transform.position,projectile.velocity, projectile.angularVelocity, settings, visualize);
    }

    /// <summary>
    /// Returns precomputed Trajectory for a Projectile using provided settings
    /// </summary>
    /// <param name="projectile">Projectile</param>
    /// <returns></returns>
    public static Trajectory GetProjectileTrajectory(Vector3 startPosition, Vector3 velocity, Vector3 angularVelocity, Settings settings, bool visualize = false)
    {
        float t0 = Time.time;
        float timeStep = _simulationStep; 

        Trajectory.BouncePoint bouncePoint = new Trajectory.BouncePoint(0f,Vector3.zero, Vector3.zero, Vector3.zero);

        List<Vector3> predictedPoints = GetPredictedPoints(startPosition, velocity, angularVelocity, settings, ref bouncePoint);
        Trajectory trajectory = GetTrajectoryFromPredictedPoints(predictedPoints, t0, timeStep, bouncePoint);

        if (visualize)
            TrajectoryVisualizator.VisualizeProjectileTrajectory(trajectory);

        return trajectory;
    }

    ///ALL CALCULATIONS PRESENTEED AS PSEUDO CODE

    /// <summary>
    /// Method returns precalculated List of trajectory points with given initial parameters
    /// </summary>
    /// <param name="startPosition">Start position of the projectile</param>
    /// <param name="startVelocity">Start velocity vector of the projectile</param>
    /// <param name="angularVelocity">Start angular velocity vector of the projectile</param>
    /// <param name="settings">Start settings structure</param>
    /// <returns></returns>
    private static List<Vector3> GetPredictedPoints(Vector3 startPosition, Vector3 velocity, Vector3 angularVelocity, Settings settings, ref Trajectory.BouncePoint bouncePoint)
    {
        //Initializing Zdenal's magic slowing effect>
        float _speedMultiplier = CalculateInitialSpeedMultiplier(startPosition);

        //Setting up initial parameters and variables
        List<Vector3> predictedPoints = new List<Vector3>();

        float timer = Time.time;
        float timerBounce = 0f;
        float dt = Time.fixedDeltaTime;

        Vector3 currentPosition = startPosition;
        Vector3 lastPosition = startPosition;

        Vector3 startVelocity = velocity;
        Vector3 currentVelocity = velocity;
        Vector3 velocityDirection = startVelocity.normalized;
        Vector3 currentAngularVelocity = angularVelocity;

        Vector3 currentAcceleration = Physics.gravity;

        Vector3 velocityOnHit = Vector3.zero;
        Vector3 initialVelocityOnHit = Vector3.zero;

        bool bounceTimer = false;
        bool hitDetection = true;
        bool floorHitDetected = false;

        for (int i = 0; i < _simulationTime / _simulationStep; i++)
        {
            //When the ball hits the floor, we need to set up initial values for the motion equation
            if (floorHitDetected)
            {
                //Do on floor hit logic
                //Out values:
                //angularMultiplier
                //bouncePoint
            }

            //Setting up startVelocity to calculate trajectory before bounce
            startVelocity = currentVelocity;

            //Calculating the next position in spacetime according to the previous position
            Vector3 curPos = currentPosition;
            Vector3 newPos;

            if (bounceTimer)
            {
                newPos = CalculateNewPosition();
            }
            else
            {
                newPos = CalculateNewPosition();
            }
            lastPosition = curPos;
            currentPosition = newPos;
            velocityDirection = (newPos - lastPosition).normalized;

            //Calculating current velocity 
            if (bounceTimer)
            {
                currentVelocity = CalculateCurrentVelocity();
            }
            else
            {
                currentVelocity = CalculateCurrentVelocity();
            }

            //Calculating speed multiplier
            _speedMultiplier = CalculateSpeedMultiplier(currentPosition, _speedMultiplier, ref currentVelocity, ref currentAngularVelocity);

            //Calculating acceleration 
            currentAcceleration = Physics.gravity;

            if (settings.useAirResistance)
            {
                Vector3 airResistanceForce = PhysicsManager.AddAirFriction(currentVelocity, _projectileRadius);
                currentAcceleration += airResistanceForce / _projectileMass;
            }
            if (settings.useMagnusForce)
            {
                Vector3 magnusForce = PhysicsManager.AddMagnusForce(currentVelocity, currentAngularVelocity, _projectileRadius);
                currentAcceleration += magnusForce / _projectileMass;
            }

            //Add the position value to the array of precalculated trajectory points
            predictedPoints.Add(currentPosition);

            //handle timer and bounce timer
            timer += _simulationStep;
            if (bounceTimer)
            {
                timerBounce += _simulationStep;
            }

            //check for ground hit 
            if (currentPosition.y <= _projectileRadius && hitDetection)
            {
                floorHitDetected = true;
                hitDetection = false;
            }

            //zeroing the velocities because we only need them once to affect the calculations on start
            startVelocity = Vector3.zero;
            initialVelocityOnHit = Vector3.zero;
        }

        return predictedPoints;
    }

    public static Vector3 PrecalculateInitialVelocity(Vector3 startPosition, Vector3 targetPosition, float speedMagnitude, Vector3 angularVelocity)
    {
        Settings settings = new Settings(true, true, 1f);
        return PrecalculateInitialVelocity(startPosition, targetPosition, speedMagnitude, angularVelocity, settings);
    }

    /// <summary>
    /// This method returns start velocity vector needed to hit the given target
    /// </summary>
    /// <param name="startPosition">Start source position</param>
    /// <param name="targetPosition">Target position</param>
    /// <param name="speedMagnitude">Start speed magnitude in m/s</param>
    /// <param name="angularVelocity">Start angular velocity</param>
    /// <param name="settings">A sctructure of settings</param>
    /// <param name="verticalApproximationStep">Parameter for approximation</param>
    /// <param name="horizontalApproximationStep">Parameter for approximation</param>
    /// <param name="accuracy">Parameter for approximation</param>
    /// <returns></returns>
    public static Vector3 PrecalculateInitialVelocity(Vector3 startPosition, Vector3 targetPosition, float speedMagnitude, Vector3 angularVelocity, Settings settings, float verticalApproximationStep = 0.25f, float horizontalApproximationStep = 0.25f, float accuracy = 0.2f)
    {
        //initial position of a projectile
        Vector3 projectilePosition = startPosition;

        //current target position which changes as simulation goes further
        Vector3 simulationTargetPosition = targetPosition;

        //original target position, the point we want to hit
        Vector3 savedTargetPosition = targetPosition;

        //velocity magnitude
        float projectileSpeed = speedMagnitude;

        //array to store initial velocity solutions
        Vector3[] tempSolutions = new Vector3[2];

        //bounce position
        Vector3 bouncePos = Vector3.zero;

        float gravity = -Physics.gravity.y;

        //first iteration
        //the first step of the simulation, start velocity is calculated for the ideal environment, then the forces applied
        int numSolutions;
        numSolutions = TrajectorySolver.SolveBallisticTrajectory(projectilePosition, projectileSpeed, simulationTargetPosition, gravity, out tempSolutions[0], out tempSolutions[1]);
        Vector3 impulse = tempSolutions[0];
        PrecalculateTrajectory(sourcePosition, out bouncePos, impulse, angularVelocity, settings);
        bouncePos.y = 0f;

        //declaring delta values (delta -  a length between current bounce point and target point)

        float deltaX = Mathf.Abs(savedTargetPosition.x - bouncePos.x);
        float deltaZ = Mathf.Abs(savedTargetPosition.z - bouncePos.z);
        float bouncePointX = bouncePos.x;
        float bouncePointZ = bouncePos.z;

        //SIMULATION XZ-APPROXIMATION
        int simulationIterationNumber = 0;
        while (deltaX > accuracy || deltaZ > accuracy)
        {
            if (simulationIterationNumber > 20)
            {
                Debug.LogWarning("Simulation break");
                break;
            }

            //Do simulation
            //Output:
            //Final value for impulse

            simulationIterationNumber++;
        }
        return impulse;
    }

    public static Vector3 CalculateVelocityOnBounce(Vector3 currentPosition, Vector3 currentVelocity, float normalMultiplier,
        float parallelMultiplier)
    {
        //Do calculations
        //Output:
        //finalVelocity vector
        return _finalVelocity;
    }
    public static float CalculateInitialSpeedMultiplier(Vector3 source)
    {
        //Do calculations
        //Output:
        //speedMultiplier start value
        return _speedMultiplier;
    }
    public static float CalculateSpeedMultiplier(Vector3 currentPosition, float currentMultiplier, ref Vector3 currentVelocity, ref Vector3 currentAngularVelocity)
    {
        //Do calculations
        //Output:
        //speedMultiplier current value
        return _speedMultiplier;
    }

    private static Trajectory GetTrajectoryFromPredictedPoints(List<Vector3> predictedPoints, float t0, float timeStep, Trajectory.BouncePoint bouncePoint)
    {
        //Do calculations
        //Output:
        //trajectiry object
        return trajectory;
    }
}
