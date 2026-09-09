using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class DiskPuzzle : MonoBehaviour
{
    [Header("Disc Settings")]
    [SerializeField] private int segments = 8;

    [SerializeField] private int currentIndex;

    [SerializeField] private float rotateDuration = 0.25f;

    [SerializeField] private Ease rotateEase = Ease.OutBack;

    [Header("Linked Discs")]
    [SerializeField] private List<LinkedDisc> linkedDiscs = new List<LinkedDisc>();

    [Header("Debug")]
    [SerializeField] private bool canRotate = true;

    private float anglePerSegment;

    private bool isRotating;

    private void Awake()
    {
        anglePerSegment = 360f / segments;
        ApplyRotationInstant();
    }

    private void Start()
    {
        GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;
    }

    public void SetInitialPos(int pos)
    {
        currentIndex = pos;
        ApplyRotationInstant();
    }

    public void RotateClockwise() { Rotate(1); }

    public void RotateCounterClockwise() { Rotate(-1); }

    public void Rotate(int direction)
    {
        if (!canRotate) return;
        if (isRotating) return;

        RotateInternal(direction, true);
        
        HudManager.Instance.CheckWin();
        
    }

    private void RotateInternal(int amount, bool propagate)
    {
        currentIndex += amount;
        NormalizeIndex();

        float targetAngle = currentIndex * anglePerSegment;
        isRotating = true;
        transform.DOLocalRotate(new Vector3(0, 0, targetAngle), rotateDuration).SetEase(rotateEase).OnComplete(() => { isRotating = false; });

        if (propagate) RotateLinked(amount);
    }

    private void RotateLinked(int amount)
    {
        foreach (LinkedDisc linked in linkedDiscs)
        {
            if (linked.disc == null) continue;

            int finalAmount = amount * linked.multiplier;
            linked.disc.RotateFromExternal(finalAmount);
        }
    }

    public void RotateFromExternal(int amount)
    {
        RotateInternal(amount, false);
    }

    private void NormalizeIndex()
    {
        currentIndex %= segments;
        if (currentIndex < 0) currentIndex += segments;
    }

    private void ApplyRotationInstant()
    {
        float angle = currentIndex * anglePerSegment;
        transform.localRotation = Quaternion.Euler(0, 0, angle);
    }

    public bool IsAtIndex(int target) { return currentIndex == target; }
    public int GetIndex() { return currentIndex; }

}

[Serializable]
public class LinkedDisc
{
    public DiskPuzzle disc;
    public int multiplier = 1;
}
