using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Project.Utility;
using BV;

public class NPCManager : MonoBehaviour
{
    public List<Vector3> path = new List<Vector3>();
    private float lastSynchronizationTime; // последнее время синхронизации
    private float syncDelay = 0f; // дельта между текущим временем и последней синхронизацией
    private float syncTime = 0f; // время синхронизации

    private Vector3 syncStartPosition = Vector3.zero; //начальная позиция интерполяции 
    private Vector3 syncEndPosition = Vector3.zero; // конечная позиция интерполяци
    private Quaternion syncStartRotation = Quaternion.identity; // начальный поворот интерполяции
    private Quaternion syncEndRotation = Quaternion.identity; // конечный поворот интерполяции
    bool isMove = false;

    void Start ()
    {
        syncEndPosition = gameObject.transform.position;
        syncEndRotation = gameObject.transform.rotation;
    }

    void FixedUpdate()
    {
        GameObject go = gameObject;
        InputHandler inputHandler = go.GetComponent<InputHandler>();

        syncTime += Time.fixedDeltaTime;
        go.transform.position = Vector3.Lerp(syncStartPosition, syncEndPosition, syncTime / syncDelay); // интерполяция перемещения
        go.transform.rotation = Quaternion.Lerp(syncStartRotation, syncEndRotation, syncTime / syncDelay); // интерполяция поворота

        inputHandler.vertical = isMove ? 0.4f : 0;
    }

    public void UpdateState(Vector3 positiom, Quaternion rotation, bool move)
    {
        GameObject go = gameObject;

        Vector3 syncVelocity = Vector3.zero;
        syncTime = 0f; // сбрасываем время синхронизации
        syncDelay = Time.time - lastSynchronizationTime; // получаем дельту предыдущей синхронизации
        lastSynchronizationTime = Time.time; // записываем новое время последней синхронизации

        syncEndPosition = positiom + syncVelocity * syncDelay; // конечная точка, в которую движется объект
        syncStartPosition = go.transform.position; // начальная точка равна текущей позиции

        syncEndRotation = rotation; // конечный поворот
        syncStartRotation = go.transform.rotation; // начальный поворот

        isMove = move;
    }
}