// 20.12.2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using System;
using UnityEditor;
using UnityEngine;

public class VisitorController : MonoBehaviour
{
    private Animator animator;

    private void Start()
    {
        // Получаем компонент Animator, прикрепленный к объекту Посетитель
        animator = GetComponent<Animator>();
    }

    public void ExitVisitor()
    {
        // Проверяем, есть ли компонент Animator
        if (animator != null)
        {
            // Запускаем анимацию ухода
            animator.SetTrigger("Exit");
        }
        else
        {
            Debug.LogError("Animator не найден на объекте Посетитель!");
        }
    }
}
