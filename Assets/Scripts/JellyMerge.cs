using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class JellyMerge : Singleton<JellyMerge>
{
    [HideInInspector] public List<JellyBlock> jellyBlocks;
    [SerializeField] private float minSwipe = 150f;
    private Vector2 swipeStart, swipeEnd;
    private Vector3Int moveDirection;

    private bool waiting;

    // Start is called before the first frame update
    void Start()
    {
        jellyBlocks = new List<JellyBlock>(FindObjectsOfType<JellyBlock>());
    }

    // Update is called once per frame
    void Update()
    {
        if (waiting) return;

        if (Input.GetMouseButtonDown(0))
            swipeStart = Input.mousePosition;
        else if (Input.GetMouseButtonUp(0))
        {
            swipeEnd = Input.mousePosition;
            if (Vector2.Distance(swipeStart, swipeEnd) < minSwipe) return;
            Vector2 direction = (swipeEnd - swipeStart).normalized;
            moveDirection = GetSwipeDirection(direction);
            MoveBlocks();
        }
    }

    private Vector3Int GetSwipeDirection(Vector2 dir)
    {
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            return dir.x > 0 ? Vector3Int.right : Vector3Int.left;
        else return dir.y > 0 ? Vector3Int.forward : Vector3Int.back;
    }

    public void RemoveBlock(JellyBlock block)
    {
        jellyBlocks.Remove(block);
        Destroy(block.gameObject, 0.1f);
    }

    private void MoveBlocks()
    {
        // Where(...): loai bo cac tham chieu den b bi Destroy tranh loi
        jellyBlocks = jellyBlocks.Where(b => b != null).
            OrderBy(b => b.ComparePosition(moveDirection)).ToList();
        bool changed = false;

        for (int i = 0; i < jellyBlocks.Count; i++)// sd foreach se loi vi jellyBlocks bi thay doi
        {
            var block = jellyBlocks[i];
            changed |= block.MoveBlock(moveDirection);

            if (block.remove)
            {
                RemoveBlock(block);
                i--;// update lai i
            }
        }

        if (changed)
        {
            StartCoroutine(WaitForChanges());
            Invoke(nameof(CheckWin), 0.2f);
        }
    }

    private IEnumerator WaitForChanges()
    {
        SoundManager13.Instance.PlaySound(4);
        waiting = true;
        yield return new WaitForSeconds(0.1f);
        waiting = false;
    }

    private void CheckWin()
    {
        Dictionary<Color, int> colorCounts = new Dictionary<Color, int>();

        foreach (var block in jellyBlocks)
        {
            if (colorCounts.ContainsKey(block.blockColor))
                colorCounts[block.blockColor]++;
            else colorCounts[block.blockColor] = 1;
        }

        bool isWin = true;
        foreach (var count in colorCounts.Values)
        {
            if (count != 1)
            {
                isWin = false;
                break;
            }
        }

        if (isWin) GameManager13.Instance.GameWin();
    }
}
