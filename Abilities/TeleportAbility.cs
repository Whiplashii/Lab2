using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Teleport Ability", menuName = "Abilities/Teleport Ability")]
public class TeleportAbility : Ability
{
    [SerializeField]
    private float maxDistance = 5f; // Максимальная дистанция телепорта

    [SerializeField]
    private GameObject portalPrefab; // Префаб портала

    [SerializeField]
    private float portalLifetime = 3f; // Время жизни портала

    public override void Execute(Human user)
    {
        if (user != null)
        {
            Vector3 targetPosition = GetTargetPosition(user);

            // Телепортируем игрока
            user.transform.position = targetPosition;

            // Спавним портал на новом месте
            if (portalPrefab != null)
            {
                GameObject portal = Instantiate(portalPrefab, targetPosition, Quaternion.identity);
                Destroy(portal, portalLifetime);
            }
        }
    }

    private Vector3 GetTargetPosition(Human user)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Vector3 direction = (hit.point - user.transform.position).normalized;
            Vector3 destination = user.transform.position + direction * maxDistance;

            // Можно сделать так, чтобы игрок не застревал в стенах:
            destination.y = user.transform.position.y; // Оставляем высоту игрока
            return destination;
        }
        else
        {
            // Если не попали никуда, телепортируем просто вперед
            return user.transform.position + user.transform.forward * maxDistance;
        }
    }
}