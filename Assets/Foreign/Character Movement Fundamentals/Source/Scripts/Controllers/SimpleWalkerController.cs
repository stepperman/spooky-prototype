using QTea;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CMF
{
    //A very simplified controller script;
	//This script is an example of a very simple walker controller that covers only the basics of character movement;
    public class SimpleWalkerController : Controller
    {
        private Mover mover;
        float currentVerticalSpeed = 0f;
        bool isGrounded;
        public float movementSpeed = 7f;
        public float jumpSpeed = 10f;
        public float gravity = 10f;
        public float movementDecay = 0.8f;

        Vector3 movementDirection;
		Vector3 lastVelocity = Vector3.zero;

		public Transform cameraTransform;
        PlayerKeyInput characterInput;
        Transform tr;

        private bool jumped = false;

        // Use this for initialization
        void Awake()
        {
            tr = transform;
            mover = GetComponent<Mover>();
            characterInput = GetComponent<PlayerKeyInput>();
            
        }

        protected void OnEnable()
        {
            characterInput.OnMove += OnMoveEvent;
            characterInput.OnJump += OnJumpEvent;
        }

        private void OnJumpEvent()
        {
            //Handle jumping;
            if ((characterInput != null) && isGrounded)
            {
                OnJumpStart();
                currentVerticalSpeed = jumpSpeed;
                isGrounded = false;
                jumped = true;
            }
        }

        private void OnMoveEvent(Vector2 moveVector)
        {
            movementDirection = moveVector;
        }

        void FixedUpdate()
        {
            //Run initial mover ground check;
            mover.CheckForGround();

            //If character was not grounded int the last frame and is now grounded, call 'OnGroundContactRegained' function;
            if(isGrounded == false && mover.IsGrounded() == true)
                OnGroundContactRegained(lastVelocity);

            //Check whether the character is grounded and store result;
            isGrounded = mover.IsGrounded() && !jumped;

            Vector3 _velocity = Vector3.zero;

            //Add player movement to velocity;
            _velocity += CalculateMovementDirection( movementDirection ) * movementSpeed;
            
            //Handle gravity;
            if (!isGrounded)
            {
                currentVerticalSpeed -= gravity * Time.deltaTime;
            }
            else
            {
                if (currentVerticalSpeed <= 0f)
                    currentVerticalSpeed = 0f;
            }

            //movementDirection *= movementDecay;

            //Add vertical velocity;
            _velocity += tr.up * currentVerticalSpeed;

			//Save current velocity for next frame;
			lastVelocity = _velocity;

            mover.SetExtendSensorRange(isGrounded);
            mover.SetVelocity(_velocity);
            jumped = false;
        }

        private Vector3 CalculateMovementDirection(Vector2 movementDirection)
        {
            //If no character input script is attached to this object, return no input;
			if(characterInput == null)
				return Vector3.zero;

			Vector3 _direction = Vector3.zero;

			//If no camera transform has been assigned, use the character's transform axes to calculate the movement direction;
			if(cameraTransform == null)
			{
				_direction += tr.right * movementDirection.x;
				_direction += tr.forward * movementDirection.y;
			}
			else
			{
				//If a camera transform has been assigned, use the assigned transform's axes for movement direction;
				//Project movement direction so movement stays parallel to the ground;
				_direction += Vector3.ProjectOnPlane(cameraTransform.right, tr.up).normalized * movementDirection.x;
				_direction += Vector3.ProjectOnPlane(cameraTransform.forward, tr.up).normalized * movementDirection.y;
			}

			//If necessary, clamp movement vector to magnitude of 1f;
			if(_direction.magnitude > 1f)
				_direction.Normalize();

			return _direction;
        }

        //This function is called when the controller has landed on a surface after being in the air;
		void OnGroundContactRegained(Vector3 _collisionVelocity)
		{
			//Call 'OnLand' delegate function;
			if(OnLand != null)
				OnLand(_collisionVelocity);
		}

        //This function is called when the controller has started a jump;
        void OnJumpStart()
        {
            //Call 'OnJump' delegate function;
            if(OnJump != null)
                OnJump(lastVelocity);
        }

        //Return the current velocity of the character;
        public override Vector3 GetVelocity()
        {
            return lastVelocity;
        }

        //Return only the current movement velocity (without any vertical velocity);
        public override Vector3 GetMovementVelocity()
        {
            return lastVelocity;
        }

        //Return whether the character is currently grounded;
        public override bool IsGrounded()
        {
            return isGrounded;
        }

    }

}

