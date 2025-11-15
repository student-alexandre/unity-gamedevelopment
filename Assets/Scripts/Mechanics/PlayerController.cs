using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;
using Platformer.Model;
using Platformer.Core;
using UnityEngine.InputSystem;

namespace Platformer.Mechanics
{
    /// <summary>
    /// This is the main class used to implement control of the player.
    /// It is a superset of the AnimationController class, but is inlined to allow for any kind of customisation.
    /// </summary>
    public class PlayerController : KinematicObject
    {
        public AudioClip jumpAudio;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;

        public LayerMask wallLayer;
        public float wallGrabDistance = 0.1f;
        public float wallSlideSpeed = 1.5f;
        public bool isOnWall;    // está colado
        public bool isGrabbing;  // está grudado

        public float wallClimbSpeed = 3f;

        public GameObject projectilePrefab;

        /// <summary>
        /// Max horizontal speed of the player.
        /// </summary>
        public float maxSpeed = 7;
        /// <summary>
        /// Initial jump velocity at the start of a jump.
        /// </summary>
        public float jumpTakeOffSpeed = 7;

        public JumpState jumpState = JumpState.Grounded;
        private bool stopJump;
        /*internal new*/
        public Collider2D collider2d;
        /*internal new*/
        public AudioSource audioSource;
        public Health health;
        public bool controlEnabled = true;

        bool jump;
        public bool isInvulnerable = false; // 👈 novo estado
        Vector2 move;
        SpriteRenderer spriteRenderer;
        internal Animator animator;
        readonly PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        private InputAction m_MoveAction;
        private InputAction m_JumpAction;

        public Bounds Bounds => collider2d.bounds;

        void Awake()
        {
            health = GetComponent<Health>();
            audioSource = GetComponent<AudioSource>();
            collider2d = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();

            m_MoveAction = InputSystem.actions.FindAction("Player/Move");
            m_JumpAction = InputSystem.actions.FindAction("Player/Jump");

            m_MoveAction.Enable();
            m_JumpAction.Enable();
        }

        void FireProjectile()
        {
            if (projectilePrefab == null) return;

            Vector3 spawnPos = transform.position + new Vector3(spriteRenderer.flipX ? -0.5f : 0.5f, 0.2f, 0);
            Quaternion rotation = spriteRenderer.flipX ? Quaternion.Euler(0, 180, 0) : Quaternion.identity;

            Instantiate(projectilePrefab, spawnPos, rotation);
        }
        protected override void Update()
        {
            if (controlEnabled)
            {
                move.x = m_MoveAction.ReadValue<Vector2>().x;
                if (jumpState == JumpState.Grounded && m_JumpAction.WasPressedThisFrame())
                    jumpState = JumpState.PrepareToJump;
                else if (m_JumpAction.WasReleasedThisFrame())
                {
                    stopJump = true;
                    Schedule<PlayerStopJump>().player = this;
                }
            }
            else
            {
                move.x = 0;
            }

            float dir = move.x != 0 ? Mathf.Sign(move.x) : (spriteRenderer.flipX ? -1f : 1f);

            // Origem do raio no meio do corpo
            Vector2 rayOrigin = (Vector2)transform.position + Vector2.up * 0.3f;

            // Raycast real
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * dir, wallGrabDistance, wallLayer);
            isOnWall = hit.collider != null;

            // Linha vermelha pra debug
            Debug.DrawRay(rayOrigin, Vector2.right * dir * wallGrabDistance, Color.red);

            // if (isOnWall)
            //     Debug.Log("🔥 Encostando na parede!");
            // else
            //     Debug.Log("⛔ Não detectou parede!");
            // --- GRUDAR NA PAREDE ---
            if (isOnWall && !IsGrounded && Keyboard.current.spaceKey.isPressed)
            {
                // Se apertar espaço enquanto encosta na parede, gruda
                isGrabbing = true;
                gravityModifier = 0;
                velocity = Vector2.zero;
            }

            // --- ENQUANTO ESTIVER GRUDADO ---
            if (isGrabbing)
            {
                // Mantém o personagem travado na parede
                targetVelocity = Vector2.zero;
                gravityModifier = 0;

                // --- ESCALAR ---
                float climbDir = 0f;
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
                    climbDir = 1f;
                else if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
                    climbDir = -1f;

                if (climbDir != 0f)
                    velocity.y = climbDir * wallClimbSpeed;
                else
                    velocity.y = 0f; // parado se não estiver subindo/descendo

                velocity.x = 0f; // trava horizontal

                // --- SOLTAR COM G ---
                if (Keyboard.current.gKey.wasPressedThisFrame)
                {
                    isGrabbing = false;
                    gravityModifier = 1;
                }

                // --- WALL JUMP ---
                if (Keyboard.current.spaceKey.wasPressedThisFrame)
                {
                    isGrabbing = false;
                    gravityModifier = 1;

                    // pula pro lado oposto da parede
                    float jumpDir = spriteRenderer.flipX ? -1 : 1;
                    velocity = new Vector2(jumpDir * 7f, jumpTakeOffSpeed);

                    // vira o sprite pro novo lado
                    spriteRenderer.flipX = jumpDir < 0;
                }
            }

            // Sai automaticamente se encostar no chão
            if (IsGrounded && !isOnWall)
            {
                isGrabbing = false;
                gravityModifier = 1;
            }

            if (Keyboard.current.fKey.wasPressedThisFrame)
            {
                FireProjectile();
            }
            UpdateJumpState();
            base.Update();
        }

        void UpdateJumpState()
        {
            jump = false;
            switch (jumpState)
            {
                case JumpState.PrepareToJump:
                    jumpState = JumpState.Jumping;
                    jump = true;
                    stopJump = false;
                    break;
                case JumpState.Jumping:
                    if (!IsGrounded)
                    {
                        Schedule<PlayerJumped>().player = this;
                        jumpState = JumpState.InFlight;
                    }
                    break;
                case JumpState.InFlight:
                    if (IsGrounded)
                    {
                        Schedule<PlayerLanded>().player = this;
                        jumpState = JumpState.Landed;
                    }
                    break;
                case JumpState.Landed:
                    jumpState = JumpState.Grounded;
                    break;
            }
        }

        protected override void ComputeVelocity()
        {
            if (jump && IsGrounded)
            {
                velocity.y = jumpTakeOffSpeed * model.jumpModifier;
                jump = false;
            }
            else if (stopJump)
            {
                stopJump = false;
                if (velocity.y > 0)
                {
                    velocity.y = velocity.y * model.jumpDeceleration;
                }
            }

            if (move.x > 0.01f)
                spriteRenderer.flipX = false;
            else if (move.x < -0.01f)
                spriteRenderer.flipX = true;

            animator.SetBool("grounded", IsGrounded);
            animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / maxSpeed);
            if (isGrabbing)
            {
                gravityModifier = 0;

                // sobe/desce com W / S ou setas
                float climbDir = 0f;
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
                    climbDir = 1f;
                else if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
                    climbDir = -1f;

                if (climbDir != 0f)
                {
                    velocity.y = climbDir * wallClimbSpeed;   // sobe ou desce
                    Debug.Log("🧗 Escalando parede!");
                }
                else
                {
                    velocity.y = 0f; // parado quando não aperta nada
                }

                velocity.x = 0f; // impede deslizar da parede
                targetVelocity = Vector2.zero;
            }
            else if (!isGrabbing && gravityModifier == 0)
            {
                gravityModifier = 1;
            }

            targetVelocity = isGrabbing ? Vector2.zero : move * maxSpeed;
        }

        public void TakeDamage()
        {
            if (isInvulnerable) return; // 👈 ignora se já estiver invulnerável

            if (health == null) return;
            health.Decrement();

            if (ouchAudio != null)
                AudioSource.PlayClipAtPoint(ouchAudio, transform.position);

            StartCoroutine(InvulnerabilityCoroutine());
        }

        IEnumerator InvulnerabilityCoroutine()
        {
            isInvulnerable = true;
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            Color originalColor = sr.color;

            float elapsed = 0f;
            float duration = 1.5f;
            float flashInterval = 0.1f;

            while (elapsed < duration)
            {
                // Alterna entre branco e vermelho, mantendo opacidade
                sr.color = (sr.color == originalColor)
                    ? new Color(1f, 0.4f, 0.4f, 1f)
                    : originalColor;

                yield return new WaitForSeconds(flashInterval);
                elapsed += flashInterval;
            }

            sr.color = originalColor;
            isInvulnerable = false;
        }

        public enum JumpState
        {
            Grounded,
            PrepareToJump,
            Jumping,
            InFlight,
            Landed
        }
    }
}