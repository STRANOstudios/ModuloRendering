using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;

namespace Project.Runtime.Logic.Player
{
    [HideMonoScript]
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        #region Serialized Fields ---------------------------------------------

        [BoxGroup("Settings")]
        [Tooltip("The root transform of the camera")]
        [SerializeField, Required, SceneObjectsOnly]
        private Transform m_cameraRoot;

        [BoxGroup("Settings")]
        [Tooltip("The animator component")]
        [SerializeField, Required, ChildGameObjectsOnly]
        private Animator m_animator;

        [FoldoutGroup("Settings/Parameters")]
        [Tooltip("The speed when walking"), Unit(Units.MetersPerSecond)]
        [SerializeField] 
        private float m_walkSpeed = 2f;

        [FoldoutGroup("Settings/Parameters")]
        [Tooltip("The speed when running"), Unit(Units.MetersPerSecond)]
        [SerializeField]
        private float m_runSpeed = 5f;

        [FoldoutGroup("Settings/Parameters")]
        [Tooltip("The jump force"), Unit(Units.MetersPerSecondSquared)]
        [SerializeField]
        private float m_jumpForce = 6f;

        [FoldoutGroup("Settings/Parameters")]
        [Tooltip("Gravity"), Unit(Units.MetersPerSecondSquared)]
        [SerializeField]
        private float m_gravity = -9.81f;

        [FoldoutGroup("Settings/Parameters")]
        [Tooltip("The rotation speed"), Unit(Units.DegreesPerSecond)]
        [SerializeField]
        private float m_rotationSpeed = 10f;

        [BoxGroup("Settings")]
        [Tooltip("[<color=yellow>Test</color>] The afterimage system")]
        [SerializeField]
        private AfterimageSystem m_afterimageSystem;

        #endregion

        #region Private Fields ------------------------------------------------

        private PlayerInput m_playerInput;
        private InputAction m_moveAction;
        private InputAction m_lookAction;
        private InputAction m_jumpAction;
        private InputAction m_runAction;
        private InputAction m_crouchAction;

        private Vector2 m_moveInput;
        private Vector2 m_lookInput;
        private bool m_jumpPressed;
        private bool m_isRunning;
        private bool m_isCrouching;

        private CharacterController m_controller;
        private Vector3 m_velocity;
        private bool m_isGrounded;

        private Vector3 m_lastAfterimagePosition;

        #endregion

        #region Unity Events -------------------------------------------------

        private void Awake()
        {
            m_controller = GetComponent<CharacterController>();
            InitializeInput();
        }

        private void OnEnable()
        {
            m_moveAction?.Enable();
            m_lookAction?.Enable();
            m_jumpAction?.Enable();
            m_runAction?.Enable();
            m_crouchAction?.Enable();
        }

        private void OnDisable()
        {
            m_moveAction?.Disable();
            m_lookAction?.Disable();
            m_jumpAction?.Disable();
            m_runAction?.Disable();
            m_crouchAction?.Disable();
        }

        private void Update()
        {
            ReadInput();
            HandleMovement();
            HandleRotation();
            UpdateAnimator();
        }

        #endregion

        #region Initialization and State Setup -------------------------------

        private void InitializeInput()
        {
            m_playerInput = GetComponent<PlayerInput>();
            m_moveAction = m_playerInput.actions["Move"];
            m_lookAction = m_playerInput.actions["Look"];
            m_jumpAction = m_playerInput.actions["Jump"];
            m_runAction = m_playerInput.actions["Run"];
            m_crouchAction = m_playerInput.actions["Crouch"];
        }

        #endregion

        #region Movement and Rotation Logic ----------------------------------

        private void ReadInput()
        {
            m_moveInput = m_moveAction.ReadValue<Vector2>();
            m_lookInput = m_lookAction.ReadValue<Vector2>();
            m_jumpPressed = m_jumpAction.WasPressedThisFrame();
            m_isRunning = m_runAction.IsPressed();
            m_isCrouching = m_crouchAction.IsPressed();
        }

        private void HandleMovement()
        {
            m_isGrounded = m_controller.isGrounded;

            if (m_isGrounded && m_velocity.y < 0)
                m_velocity.y = -2f;

            Vector3 inputDirection = new Vector3(m_moveInput.x, 0f, m_moveInput.y);

            Vector3 worldDirection = Quaternion.Euler(0, m_cameraRoot.eulerAngles.y, 0) * inputDirection;
            worldDirection.Normalize();

            float currentSpeed = m_isRunning ? m_runSpeed : m_walkSpeed;
            if (m_isCrouching)
                currentSpeed *= 0.5f;

            Vector3 move = worldDirection * currentSpeed;

            if (m_isGrounded && m_jumpPressed)
                m_velocity.y = m_jumpForce;

            m_velocity.y += m_gravity * Time.deltaTime;

            m_controller.Move((move + m_velocity) * Time.deltaTime);

            if (Vector3.Distance(transform.position, m_lastAfterimagePosition) >= 0.5f)
            {
                m_afterimageSystem?.SpawnAfterImage(transform);
                m_lastAfterimagePosition = transform.position;
            }

        }

        private void HandleRotation()
        {
            if (m_moveInput.y <= 0.1f) return;

            Vector3 lookDir = new Vector3(m_lookInput.x, 0f, m_lookInput.y);
            if (lookDir.sqrMagnitude < 0.1f) return;

            Vector3 forward = Quaternion.Euler(0, m_cameraRoot.eulerAngles.y, 0) * Vector3.forward;
            Quaternion targetRotation = Quaternion.LookRotation(forward, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, m_rotationSpeed * Time.deltaTime);
        }

        #endregion

        #region Animator Sync ------------------------------------------------

        private void UpdateAnimator()
        {
            Vector3 localVelocity = transform.InverseTransformDirection(m_controller.velocity);
            float speed = new Vector2(localVelocity.x, localVelocity.z).magnitude;

            m_animator.SetFloat("Grounded", m_isGrounded ? 1f : 0f);
            m_animator.SetFloat("Speed-Y", m_velocity.y);
            m_animator.SetFloat("Speed-X", localVelocity.x);
            m_animator.SetFloat("Speed-Z", localVelocity.z);
            m_animator.SetFloat("Speed", speed);
            m_animator.SetFloat("Stand", m_isCrouching ? 0.5f : 1f);
        }

        #endregion
    }
}
