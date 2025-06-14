using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy1 : Enemy
{
    // Override MoveTowardsPlayer to remove the range check
    protected override void MoveTowardsPlayer(float speed, bool isHurt, GameObject player, float range, Animator animator)
    {
        if (_health._isDead) // Accessing protected member _health
        {
            _rb2d.velocity = Vector2.zero; // Accessing protected member _rb2d
            return;
        }

        if (isHurt) return;

        // Removed the range check: Vector2.Distance(transform.position, player.transform.position) <= range
        if (player != null)
        {
            Vector2 direction = (player.transform.position - transform.position).normalized;
            Vector2 targetPosition = direction * speed * Time.fixedDeltaTime + _rb2d.position;
            _rb2d.MovePosition(targetPosition); // Accessing protected member _rb2d
            animator.SetBool("isMove", true); // Using the animator parameter
            FlipSprite(direction.x > 0, _spriteRenderer); // Accessing protected member _spriteRenderer and calling protected method FlipSprite
        }
        else
        {
            animator.SetBool("isMove", false); // Using the animator parameter
        }
    }
}
