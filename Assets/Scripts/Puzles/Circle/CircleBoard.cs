using UnityEngine;

public class CircleBoard : MonoBehaviour
{
    [SerializeField] private DiskPuzzle diskA;
    [SerializeField] private DiskPuzzle diskB;
    [SerializeField] private DiskPuzzle diskC;
    
    public DiskPuzzle DiskA => diskA;
    public DiskPuzzle DiskB => diskB;
    public DiskPuzzle DiskC => diskC;
}
