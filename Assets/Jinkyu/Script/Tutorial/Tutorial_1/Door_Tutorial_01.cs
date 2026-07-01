using DG.Tweening;
using System.Linq;
using UnityEngine;

public class Door_Tutorial_01 : ObjectBase
{

    [SerializeField]
    private GameObject board;
    private DoorScript.Door door;

    [SerializeField]
    private bool[] isSetting = new bool[4];// 실린더,세모,네모,하트 순
    public void SetPuzzle(GameObject puzzle)
    {
        var p = puzzle.GetComponent<Puzzle>();
        if (p == null) return;

        var puzzleType = p.puzzleType;

        isSetting[(int)puzzleType] = true;
        puzzle.transform.SetParent(board.transform, true);

        puzzle.GetComponent<Rigidbody>().isKinematic = true;
        puzzle.GetComponent<BoxCollider>().enabled = false;

        Vector3 pos = Vector3.zero;
        switch (puzzleType)
        {
            case Puzzle.PuzzleType.cylinder:
                pos = new Vector3(0.11986836f, 0.0312435627f, -0.00234311819f);
                break;
            case Puzzle.PuzzleType.triangle:
                pos = new Vector3(0.0337614492f, 0.0304191615f, -0.00292893429f);
                break;
            case Puzzle.PuzzleType.square:
                pos = new Vector3(-0.037701495f, 0.0297349617f, -0.00175737194f);
                break;
            case Puzzle.PuzzleType.heart:
                pos = new Vector3(-0.11912249f, 0.0289554242f, 0.00175738498f);
                break;

        }
        Sequence seq = DOTween.Sequence();
        seq.Append(puzzle.transform.DOLocalRotate(new Vector3(0, 0, 0), 1f));
        seq.Join(puzzle.transform.DOScale(new Vector3(1, 1, 1), 1f));
        seq.Join(puzzle.transform.DOLocalMove(pos, 1f).OnComplete(() => TryOpenDoor()));
        seq.Play();



    }

    public void TryOpenDoor()
    {
        if (isSetting.All(x => x))
        {
            door = GetComponentInParent<DoorScript.Door>();
            if (door != null)
                door.OpenDoor();
        }

    }
}
