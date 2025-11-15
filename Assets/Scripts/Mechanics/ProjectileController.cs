using UnityEngine;

namespace Platformer.Mechanics
{
    public class ProjectileController : MonoBehaviour
    {
        public float speed = 8f;       // velocidade do projétil
        public float lifetime = 2f;    // tempo de vida
        private Rigidbody2D rb;

        void Start()
        {
            rb = GetComponent<Rigidbody2D>();

            // movimento físico na direção local (respeita rotação)
            rb.linearVelocity = transform.right * speed;

            // destrói o objeto depois de X segundos
            Destroy(gameObject, lifetime);
        }

        void OnTriggerEnter2D(Collider2D collision)
        {
            // Evita destruir ao colidir com o Player
            if (collision.CompareTag("Player")) return;

            // Se atingir um inimigo
            if (collision.CompareTag("Enemy"))
            {
                Debug.Log("💥 Inimigo atingido: " + collision.name);

                // Se o inimigo tiver um script de Health, aplica dano
                var health = collision.GetComponent<Health>();
                if (health != null)
                {
                    health.Decrement(); // método padrão do projeto Platformer Microgame
                }
                else
                {
                    // Se não tiver sistema de vida, apenas destrói o inimigo
                    Destroy(collision.gameObject);
                }

                Destroy(gameObject); // destrói o projetil
                return;
            }

            // Se bater em parede ou outro obstáculo, destrói o projetil
            if (collision.CompareTag("Wall"))
            {
                Debug.Log("💥 Poder colidiu com parede!");
                Destroy(gameObject);
            }
        }

    }
}
