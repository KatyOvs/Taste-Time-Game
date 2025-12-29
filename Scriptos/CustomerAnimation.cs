// 17.12.2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using System;
using UnityEditor;
using UnityEngine;

public class CustomerAnimation : MonoBehaviour
{
    public Vector3 startPosition = new Vector3(10f, 0f, 0f); // Начальная позиция (за пределами экрана справа)
    public Vector3 targetPosition = new Vector3(0f, 0f, 0f); // Целевая позиция (рядом с панелью)
    public float moveSpeed = 2f; // Скорость движения

    private bool isEntering = false;
    private bool isExiting = false;

    void Start()
    {
        // Устанавливаем начальную позицию спрайта
        transform.position = startPosition;
    }

    void Update()
    {
        if (isEntering)
        {
            // Двигаем спрайт к целевой позиции
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            // Останавливаем анимацию, когда спрайт достигает целевой позиции
            if (transform.position == targetPosition)
            {
                isEntering = false;
            }
        }

        if (isExiting)
        {
            // Двигаем спрайт вниз, чтобы он "провалился"
            transform.position += Vector3.down * moveSpeed * Time.deltaTime;

            // Можно добавить условие для остановки анимации после достижения определенной точки
        }
    }

    public void StartEnteringAnimation()
    {
        isEntering = true;
    }

    public void StartExitingAnimation()
    {
        isExiting = true;
    }
}
